using Confluent.Kafka;
using MetricsCollection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TimeEntryFactory
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly InfluxClient _influxClient;
        

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
            _influxClient = new InfluxClient();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var currentRace = Race.GetCurrentRace();

            _influxClient.SendMetric("time_entry_factory_execute", 1);
            var config = new ConsumerConfig
            {
                BootstrapServers = "pkc-epwny.eastus.azure.confluent.cloud:9092",
                ClientId = Dns.GetHostName(),
                SaslMechanism = SaslMechanism.Plain,
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslUsername = "DL7ZEA3FYCX4RS3Z",
                SaslPassword = "sYtS2HnvacXEwVh6thsvujyKCdZbfrKaGRSKWYyzoFD9jD6OhlA+0fDy+Fifef7j",
                GroupId = "TimeEntryFactory"
            };

            using (var consumer = new ConsumerBuilder<Ignore, string>(config).Build())
            {
                consumer.Subscribe("time_entries");
                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                    await Task.Delay(1000, stoppingToken);

                    //Pull from Kafka to get available reader events
                    var consumeResult = consumer.Consume();
                    _logger.LogDebug($"Got message {consumeResult.Message}");

                    //Convert this message to a TagRead so I can convert it to a TimeEntry
                    var tagRead = JsonConvert.DeserializeObject<TagRead>(consumeResult.Message.Value);
                    var runner = currentRace.Runners.Where(r => r.TagID == tagRead.TagId.Substring(tagRead.TagId.Length - 3)).First();

                    var timeEntry = new TimeEntry()
                    {
                        ID = Guid.NewGuid(),
                        RaceXRunnerID = runner.ID,
                        ElapsedTime = GetElapsedTime(tagRead.TimeStamp),
                        AbsoluteTime = GetAbsoluteTime(tagRead.TimeStamp),
                        ReaderTimestamp = long.Parse(tagRead.TimeStamp)
                    };

                    if (!string.IsNullOrWhiteSpace(consumeResult.Message.Value))
                    {
                        _influxClient.SendMetric("time_entry_factory_gotmessage", 1);
                    }

                    //Throw it to the processor queue

                }

                consumer.Close();
            }
        }

        private DateTime GetAbsoluteTime(string timeStamp)
        {
            throw new NotImplementedException();
        }

        private long GetElapsedTime(string timeStamp)
        {
            throw new NotImplementedException();
        }
    }
}
