using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace TimeEntryFactory
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
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
                    Console.WriteLine(consumeResult.Message);
                }

                consumer.Close();
            }
        }
    }
}
