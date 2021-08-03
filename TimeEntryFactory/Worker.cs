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
using Microsoft.Extensions.Configuration;

namespace TimeEntryFactory
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly InfluxClient _influxClient;
        private readonly IConfiguration _configuration;
        public const string METRIC_PREFIX = "time_entry_factory";

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _influxClient = new InfluxClient();
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var currentRace = Race.GetCurrentRace(_configuration["UltimateTimingDBConnection"]);

            _influxClient.SendMetric($"{Worker.METRIC_PREFIX}_time_entry_factory_execute", 1);
            var config = GetConsumerConfig();

            using (var consumer = new ConsumerBuilder<Ignore, string>(config).Build())
            {
                consumer.Subscribe("reader_events");
                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                    await Task.Delay(1000, stoppingToken);

                    //Pull from Kafka to get available reader events
                    var consumeResult = consumer.Consume();
                    _logger.LogDebug($"Got message {consumeResult.Message}");

                    
                    if (!string.IsNullOrWhiteSpace(consumeResult.Message.Value))
                    {
                        _influxClient.SendMetric($"{Worker.METRIC_PREFIX}_gotmessage", 1);
                    }

                    //Convert this message to a TagRead so I can convert it to a TimeEntry

                    var timeEntry = GetTimeEntry(currentRace, consumeResult.Message.Value);

                    if (timeEntry != null)
                    {
                        //Throw it to the processor queue
                        var producerConfig = GetProducerConfig();


                        using (var producer = new ProducerBuilder<Null, string>(producerConfig).Build())
                        {
                            producer.Produce("time_entries", new Message<Null, string> { Value = JsonConvert.SerializeObject(timeEntry) }, DeliveryHandler);
                            producer.Flush(TimeSpan.FromSeconds(10));
                            _influxClient.SendMetric($"{Worker.METRIC_PREFIX}_time_entry_created", 1);
                        }

                    }
                    
                }

                consumer.Close();
            }
        }

        private void DeliveryHandler(DeliveryReport<Null, string> report)
        {
            if (report.Error.IsError)
                _influxClient.SendMetric($"{Worker.METRIC_PREFIX}_stream_error", 1);
            Console.WriteLine(report.Error.Reason);
        }

        private TimeEntry GetTimeEntry(Race currentRace, string message)
        {
            try
            {
                var tagRead = JsonConvert.DeserializeObject<TagRead>(message);
                var runner = currentRace.Runners.Where(r => r.TagID == tagRead.TagId.Substring(tagRead.TagId.Length - 3)).FirstOrDefault();
                if (runner == null)
                {
                    _influxClient.SendMetric($"{Worker.METRIC_PREFIX}_time_entry_create_runner_not_found", 1);
                    return null;
                }
                var timeEntry = new TimeEntry()
                {
                    ID = Guid.NewGuid(),
                    RaceXRunnerID = runner.ID,
                    ElapsedTime = GetElapsedTime(currentRace, tagRead.TimeStamp),
                    AbsoluteTime = GetAbsoluteTime(tagRead.TimeStamp),
                    ReaderTimestamp = long.Parse(tagRead.TimeStamp),
                    TimeEntrySource = GetTimeEntrySource(currentRace)
                };
                return timeEntry;
            }
            catch (Exception ex)
            {
                _influxClient.SendMetric($"{Worker.METRIC_PREFIX}_time_entry_create_error", 1);
                throw ex;
            }
        }

        private int? GetTimeEntrySource(Race currentRace)
        {
            var teSource = currentRace.TimeEntrySources.Where(t => t.Description == "RFID Reader").FirstOrDefault();
            if (teSource != null)
                return teSource.ID;
            else
                return null;
        }

        private ConsumerConfig GetConsumerConfig()
        {
            return new ConsumerConfig
            {
                BootstrapServers = _configuration["ConfluentKafkaServer"],
                ClientId = Dns.GetHostName(),
                SaslMechanism = SaslMechanism.Plain,
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslUsername = _configuration["ConfluentKafkaUsername"],
                SaslPassword = _configuration["ConfluentKafkaPassword"],
                GroupId = "TimeEntryFactory"
            };
        }

        private ProducerConfig GetProducerConfig()
        {
            return new ProducerConfig
            {
                BootstrapServers = _configuration["ConfluentKafkaServer"],
                ClientId = Dns.GetHostName(),
                SaslMechanism = SaslMechanism.Plain,
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslUsername = _configuration["ConfluentKafkaUsername"],
                SaslPassword = _configuration["ConfluentKafkaPassword"],
            };
        }

        private DateTime GetAbsoluteTime(string timeStamp)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddMilliseconds(Convert.ToDouble(timeStamp) / 1000);
            return dtDateTime;
        }

        private long GetElapsedTime(Race currentRace, string timeStamp)
        {
            var absoluteTime = GetAbsoluteTime(timeStamp);
            return (long)absoluteTime.Subtract(currentRace.StartDate.ToUniversalTime()).TotalMilliseconds;
            
        }
    }
}
