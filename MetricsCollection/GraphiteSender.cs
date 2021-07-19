using System;
using System.Net.Http;
using System.Text;

namespace MetricsCollection
{
    public class GraphiteSender
    {
        private string _password;

        public Uri GraphiteUrl { get; set; }
        public string UserName { get; set; }



        public GraphiteSender(Uri graphiteEndpoint, string userName, string password)
        {
            GraphiteUrl = graphiteEndpoint;
            UserName = userName;
            _password = password;
        }

        public void SendMetric()
        {
            var client = GetHttpConnection();
            client.PostAsync(GraphiteUrl.AbsoluteUri, null);
        }

        private HttpClient GetHttpConnection()
        {
            HttpClient client = new HttpClient(new HttpClientHandler());
            var basicAuthBytes = Encoding.ASCII.GetBytes($"{UserName}:{_password}");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(basicAuthBytes));

            return client;
        }
    }
}
