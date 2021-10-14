using Confluent.Kafka;
using MetricsCollection;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using TimeEntryProcessor.Processor.Models;

namespace TimeEntryProcessor.Processor
{
    internal class TimeEntryProcessingFacade
    {
        private readonly IConfiguration _configuration;
        private readonly InfluxClient _influxClient;
        private Race _currentRace;
        private readonly RaceRepository _raceRepo;

        public TimeEntryProcessingFacade(IConfiguration configuration)
        {
            _configuration = configuration;
            _influxClient = new InfluxClient(_configuration["InfluxClientToken"]);

            _raceRepo = new RaceRepository(_configuration["UltimateTimingDBConnection"]);
            //_raceRepo = new RaceRepository("Server=MikeD-Desktop2;Database=UltimateTiming;Integrated Security=true;");
            _currentRace = _raceRepo.GetCurrentRace();
        }

        public void ProcessTimeEntry(TimeEntry entry)
        {
            //Save to the DB
            SaveTimeEntry(entry);

            //Add it to the race to see if we need to send a notification
            var response = AddTimeEntryToRace(entry);

            var runner = response.Runner;
            var split = response.NewSplit;

            //only have a split if the time entry was deemed to be valid
            if (split != null)
            {
                if (split.SendNotifications)
                {
                    //drop the info into the notification stream
                    var notificationRequest = new NotificationRequest();
                    notificationRequest.RaceXRunnerID = runner.RaceXRunnerID;
                    notificationRequest.Message = split.NotificationMessageFormat
                        .Replace("{{RunnerName}}", $"{runner.FirstName} {runner.LastName}")
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

        private AddTimeEntryResponse AddTimeEntryToRace(TimeEntry entry)
        {
            return _currentRace.AddTimeEntry(entry);
        }

        private void SaveTimeEntry(TimeEntry entry)
        {
            _raceRepo.SaveTimeEntry(entry);
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
