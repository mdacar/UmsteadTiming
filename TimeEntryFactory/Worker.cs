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
                        _influxClient.SendMetric("time_entry_factory_gotmessage", 1);
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
                            _influxClient.SendMetric("time_entry_factory_time_entry_created", 1);
                        }

                    }
                    
                }

                consumer.Close();
            }
        }

        private void DeliveryHandler(DeliveryReport<Null, string> report)
        {
            if (report.Error.IsError)
                _influxClient.SendMetric("time_entry_factory_stream_error", 1);
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
                    _influxClient.SendMetric("time_entry_factory_time_entry_create_runner_not_found", 1);
                    return null;
                }
                var timeEntry = new TimeEntry()
                {
                    ID = Guid.NewGuid(),
                    RaceXRunnerID = runner.ID,
                    ElapsedTime = GetElapsedTime(currentRace, tagRead.TimeStamp),
                    AbsoluteTime = GetAbsoluteTime(tagRead.TimeStamp),
                    ReaderTimestamp = long.Parse(tagRead.TimeStamp),
                    TimeEntrySource = GetTimeEntrySource(currentRace),
                    TimeEntryStatusID = 5
                };
                return timeEntry;
            }
            catch (Exception ex)
            {
                _influxClient.SendMetric("time_entry_factory_time_entry_create_error", 1);
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
                BootstrapServers = "pkc-epwny.eastus.azure.confluent.cloud:9092",
                ClientId = Dns.GetHostName(),
                SaslMechanism = SaslMechanism.Plain,
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslUsername = "DL7ZEA3FYCX4RS3Z",
                SaslPassword = "sYtS2HnvacXEwVh6thsvujyKCdZbfrKaGRSKWYyzoFD9jD6OhlA+0fDy+Fifef7j",
                GroupId = "TimeEntryFactory"
            };
        }

        private ProducerConfig GetProducerConfig()
        {
            return new ProducerConfig
            {
                BootstrapServers = "pkc-epwny.eastus.azure.confluent.cloud:9092",
                ClientId = Dns.GetHostName(),
                SaslMechanism = SaslMechanism.Plain,
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslUsername = "DL7ZEA3FYCX4RS3Z",
                SaslPassword = "sYtS2HnvacXEwVh6thsvujyKCdZbfrKaGRSKWYyzoFD9jD6OhlA+0fDy+Fifef7j",
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
