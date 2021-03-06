using Microsoft.Azure.KeyVault;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace TimeEntrySimulator
{
    class Program
    {
        async static Task Main(string[] args)
        {
            Random randomizer = new Random();

            string clientId = "13673b99-e9d8-437f-84df-3a0a6558fb54";
            string secretUrl = "https://umsteadtimingkeys.vault.azure.net/";
            string clientSecret = "5RWXSMvSHx6YC.j3A2h_M0K21.-UA4eJx_";
            //string tenantId = "8f70917d-2628-45d3-a2fa-5a768a3e00ea";

            var client = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(
                async (string auth, string res, string scope) =>
                {
                    var authContext = new AuthenticationContext(auth);
                    var creds = new ClientCredential(clientId, clientSecret);
                    AuthenticationResult result = await authContext.AcquireTokenAsync(res, creds);
                    if (result == null)
                        throw new Exception("Failed to retrieve token");

                    return result.AccessToken;
                }
                ));

            var secretConnString = await client.GetSecretAsync(secretUrl, "UltimateTimingDBConnection");

            SqlConnection conn = new SqlConnection(secretConnString.Value);
            conn.Open();
            string sql = @"SELECT rdr.ReaderName, rte.ReaderTimestamp, 1 AS 'AntennaNumber', rxr.TagID
                            FROM RunnerTimeEntry rte INNER JOIN RFIDReader rdr ON rte.RFIDReaderID = rdr.ID
	                            INNER JOIN RaceXRunner rxr ON rte.RaceXRunnerID = rxr.ID
                            WHERE rxr.RaceID = 'CB2F8692-FD4A-460E-B325-8C5EFD3CC9C8'
	                            AND TimeEntryStatusID IN (1, 5)
                            ORDER BY ReaderTimestamp";

            SqlCommand cmd = new SqlCommand(sql, conn);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable table = new DataTable();
            da.Fill(table);
            int delayCounter = 0;
            var tagReads = new List<TagRead>();

            //Always send between 1 and 10 reads in a batch
            int numberOfReadsToSend = randomizer.Next(1, 10);

            foreach (DataRow row in table.Rows)
            {
                //create the tag read
                var tagRead = new TagRead()
                {
                    ReaderName = row.Field<string>("ReaderName"),
                    AntennaNumber = row.Field<int>("AntennaNumber"),
                    TimeStamp = row.Field<long>("ReaderTimestamp").ToString(),
                    TagId = $"300833B2DDD90140{DateTime.Now.Year}{row.Field<string>("TagId").PadLeft(4, '0')}" ,
                    TagType = "P",
                    TimeEntrySource = "RFID Reader"
                };
                tagReads.Add(tagRead);
                
                //Hit the batch size so send
                if (++delayCounter == numberOfReadsToSend)
                {
                    //send
                    SendToReaderEventApi(tagReads);
                    //clear the list so we don't send dupes
                    tagReads.Clear();
                    //wait for a few seconds
                    Thread.Sleep(randomizer.Next(1, 10) * 1000);
                    //start the process over again with new numbers
                    delayCounter = 0;
                    numberOfReadsToSend = randomizer.Next(1, 10);
                }
            }
            Console.ReadLine();
        }


        private static void SendToReaderEventApi(IList<TagRead> tagReads)
        {
            RestClient client = new RestClient();

            RestRequest req = new RestRequest("https://readerevent.azurewebsites.net/api/readerevent", Method.POST);
            req.AddJsonBody(tagReads);
            var response = client.Execute(req);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine($"Failed to post: {response.Content}");
            }

        }
    }
}
