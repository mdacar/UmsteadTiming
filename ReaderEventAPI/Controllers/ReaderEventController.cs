using Confluent.Kafka;
using MetricsCollection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ReaderEventAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReaderEventController : ControllerBase
    {
        private readonly ILogger<ReaderEventController> _logger;
        private readonly InfluxClient _influxClient;
        private readonly IConfiguration _configuration;

        public ReaderEventController(ILogger<ReaderEventController> logger, IConfiguration configuration)
        {
            _configuration = configuration;
            _logger = logger;
            _influxClient = new InfluxClient(_configuration["InfluxClientToken"]);
            
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
                    BootstrapServers = _configuration["ConfluentKafkaServer"],
                    ClientId = Dns.GetHostName(),
                    SaslMechanism = SaslMechanism.Plain,
                    SecurityProtocol = SecurityProtocol.SaslSsl,
                    SaslUsername = _configuration["ConfluentKafkaUsername"],
                    SaslPassword = _configuration["ConfluentKafkaPassword"],
                };

                using (var producer = new ProducerBuilder<Null, string>(config).Build())
                {
                    //One entry in the stream per tag read
                    Parallel.ForEach(tagReads, tagRead =>
                    {
                        producer.Produce("reader_events", new Message<Null, string> { Value = JsonConvert.SerializeObject(tagRead) }, DeliveryHandler);
                    });
                    
                    
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
            if (report.Error.IsError)
                _influxClient.SendMetric("reader_event_stream_error", 1);

            Console.WriteLine(report.Error.Reason);
        }

    }
}
