using Confluent.Kafka;
using MetricsCollection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;


namespace ReaderEventAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReaderEventController : ControllerBase
    {
        private readonly ILogger<ReaderEventController> _logger;
        private readonly InfluxClient _influxClient;

        public ReaderEventController(ILogger<ReaderEventController> logger)
        {
            _logger = logger;
            _influxClient = new InfluxClient();
        }

        // POST api/<ReaderEvent>
        [HttpPost]
        public ActionResult<int> Post([FromBody] IEnumerable<TagRead> tagReads)
        {
            //_logger.LogTrace($"ReaderEventController Post called with {value}");
            try
            {
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
                    //One entry in the stream per tag read
                    foreach (var tagRead in tagReads)
                    {
                        producer.Produce("time_entries", new Message<Null, string> { Value = JsonConvert.SerializeObject(tagRead) }, DeliveryHandler);
                    }
                    
                    producer.Flush(TimeSpan.FromSeconds(10));
                }

                _influxClient.SendMetric("reader_event_api_post", tagReads.Count());

                return Ok(tagReads.Count());
            }
            catch (Exception ex)
            {
                _influxClient.SendMetric("reader_event_api_error", 1);

                throw ex;
            }
        }

        private void DeliveryHandler(DeliveryReport<Null, string> report)
        {
            Console.WriteLine(report.Error.Reason);
        }

    }
}
