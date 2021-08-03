using Confluent.Kafka;
using MetricsCollection;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace TimeEntryProcessor.Processor
{
    internal class TimeEntryProcessingFacade
    {
        private readonly IConfiguration _configuration;
        private readonly InfluxClient _influxClient;

        public TimeEntryProcessingFacade(IConfiguration configuration)
        {
            _configuration = configuration;
            _influxClient = new InfluxClient(_configuration["InfluxClientToken"]);
        }

        public void ProcessTimeEntry(TimeEntry entry)
        {
            
            SaveTimeEntry(entry);

            var split = GetSplit(entry);

            if (split != null)
            {
                if (split.SendNotifications)
                {
                    //drop the info into the notification queue
                    var notificationRequest = new NotificationRequest();
                    notificationRequest.RaceXRunnerID = split.Runner.RaceXRunnerID;
                    notificationRequest.Message = split.NotificationMessageFormat
                        .Replace("{{RunnerName}}", $"{split.Runner.FirstName} {split.Runner.LastName}")
                        .Replace("{{Distance}}", $"{split.Distance}")
                        .Replace("{{CheckPointName}}", split.CheckPointName)
                        .Replace("{{SplitTime}}", split.SplitTimeFormatted);
                   
                    SendNotificationRequestTostream(notificationRequest);

                }
            }
        }

        private void SendNotificationRequestTostream(NotificationRequest notificationRequest)
        {
            var producerConfig = GetProducerConfig();
            using (var producer = new ProducerBuilder<Null, string>(producerConfig).Build())
            {
                producer.Produce("notification_requests", new Message<Null, string> { Value = JsonConvert.SerializeObject(notificationRequest) }, DeliveryHandler);
                producer.Flush(TimeSpan.FromSeconds(10));
                _influxClient.SendMetric($"{Worker.METRIC_PREFIX}_notification_request_created", 1);
            }
        }

        private void DeliveryHandler(DeliveryReport<Null, string> report)
        {
            if (report.Error.IsError)
                _influxClient.SendMetric($"{Worker.METRIC_PREFIX}_stream_error", 1);
            Console.WriteLine(report.Error.Reason);
        }

        private Split GetSplit(TimeEntry entry)
        {
            var repo = new RaceRepository(_configuration["UltimateTimingDBConnection"]);
            var race = repo.GetCurrentRace();
            var response = race.AddTimeEntry(new UltimateTiming.DomainModel.TimeEntryRequest()
            {

            });
            var split = response.NewSplit;
            return new Split();
        }

        private void SaveTimeEntry(TimeEntry entry)
        {
            RaceRepository repo = new RaceRepository(_configuration["UltimateTimingDBConnection"]);
            repo.SaveTimeEntry(entry);
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
    }
}
