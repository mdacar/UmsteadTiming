using Confluent.Kafka;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ReaderEventAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReaderEventController : ControllerBase
    {
        private readonly ILogger<ReaderEventController> _logger;

        public ReaderEventController(ILogger<ReaderEventController> logger)
        {
            _logger = logger;
        }

        // POST api/<ReaderEvent>
        [HttpPost]
        public void Post([FromBody] TimeEntry value)
        {
            _logger.LogTrace($"ReaderEventController Post called with {value}");

            var config = new ProducerConfig
            {
                BootstrapServers = "pkc-epwny.eastus.azure.confluent.cloud:9092",
                ClientId = Dns.GetHostName(),
                SaslMechanism = SaslMechanism.Plain,
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslUsername = "DL7ZEA3FYCX4RS3Z",
                SaslPassword = "sYtS2HnvacXEwVh6thsvujyKCdZbfrKaGRSKWYyzoFD9jD6OhlA+0fDy+Fifef7j",
            };

            using (var producer = new ProducerBuilder<Null, string>(config).Build())
            {
                producer.Produce("time_entries", new Message<Null, string> { Value = value.ToString() }, DeliveryHandler);
                producer.Flush(TimeSpan.FromSeconds(10));
            }
        }

        private void DeliveryHandler(DeliveryReport<Null, string> report)
        {
            Console.WriteLine(report.Error.Reason);
        }

    }
}
