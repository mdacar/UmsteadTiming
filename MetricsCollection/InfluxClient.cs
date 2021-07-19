using System;
using System.Collections.Generic;
using System.Text;
using InfluxDB;
using InfluxDB.Client;
using InfluxDB.Client.Writes;

namespace MetricsCollection
{
    public class InfluxClient
    {

        private InfluxDBClient _client;
        const string token = "fi6ptjLmhCtlGbvVpElJylV9D_0WP7CCxbkyrkkbZnYKEDAgO8v3qVlajBP9MZI3As7csczLYh1cWYGyAFi-hg==";
        const string bucket = "umsteadtiming";
        const string org = "michaeldacar@hotmail.com";
        const string influxUrl = "https://eastus-1.azure.cloud2.influxdata.com";


        public InfluxClient()
        {
            // You can generate a Token from the "Tokens Tab" in the UI
            

            
            _client = InfluxDBClientFactory.Create(influxUrl, token.ToCharArray());
            
        }

        public void SendMetric(string metricName, int value)
        {
            using (var writeApi = _client.GetWriteApi())
            {
                
                var point = PointData.Measurement(metricName)
                    .Field("value", value)
                    .Timestamp(DateTime.UtcNow, InfluxDB.Client.Api.Domain.WritePrecision.Ms);
                writeApi.WritePoint(bucket, org, point);
            }
        }

    }
}
