using Confluent.Kafka;
using MetricsCollection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using TimeEntryProcessor.Processor;

namespace TimeEntryProcessor
{
    public class Worker : BackgroundService
    {
        public const string METRIC_PREFIX = "time_entry_processor";
        private readonly InfluxClient _influxClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<Worker> _logger;
        private const string KAFKA_TOPIC = "time_entries";

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _configuration = configuration;
            _logger = logger;
            _influxClient = new InfluxClient(_configuration["InfluxClientToken"]);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //var currentRace = Race.GetCurrentRace();

            _influxClient.SendMetric($"{METRIC_PREFIX}_execute", 1);
            var config = GetConsumerConfig();

            using (var consumer = new ConsumerBuilder<Ignore, string>(config).Build())
            {
                consumer.Subscribe(KAFKA_TOPIC);

                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                    //Pull from Kafka to get available reader events
                    var consumeResult = consumer.Consume();

                    if (!string.IsNullOrWhiteSpace(consumeResult.Message.Value))
                    {
                        _influxClient.SendMetric($"{METRIC_PREFIX}_gotmessage", 1);
                        //Got a message, do something with it now
                        var timeEntry = JsonConvert.DeserializeObject<TimeEntry>(consumeResult.Message.Value);
                        var processor = new TimeEntryProcessingFacade(_configuration);
                        processor.ProcessTimeEntry(timeEntry);
                        
                    }

                    await Task.Delay(1000, stoppingToken);
                }

            }
            
            
        }

        private ConsumerConfig GetConsumerConfig()
        {
            return new ConsumerConfig
            {
                BootstrapServers = "pkc-epwny.eastus.azure.confluent.cloud:9092",
                ClientId = Dns.GetHostName(),
                SaslMechanism = SaslMechanism.Plain,
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslUsername = "DL7ZEA3FYCX4RS3Z",
                SaslPassword = "sYtS2HnvacXEwVh6thsvujyKCdZbfrKaGRSKWYyzoFD9jD6OhlA+0fDy+Fifef7j",
                GroupId = "TimeEntryFactory"
            };
        }
    }
}
