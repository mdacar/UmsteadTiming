using Confluent.Kafka;
using MetricsCollection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace TimeEntryProcessor.Processor
{
    internal class TimeEntryProcessingFacade
    {
        private readonly InfluxClient _influxClient;

        public TimeEntryProcessingFacade()
        {
            _influxClient = new InfluxClient();
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
            var repo = new RaceRepository();
            var race = repo.GetCurrentRace();
            var response = race.AddTimeEntry(new UltimateTiming.DomainModel.TimeEntryRequest()
            {

            });
            var split = response.NewSplit;
            return new Split();
        }

        private void SaveTimeEntry(TimeEntry entry)
        {
            RaceRepository repo = new RaceRepository();
            repo.SaveTimeEntry(entry);
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
    }
}
