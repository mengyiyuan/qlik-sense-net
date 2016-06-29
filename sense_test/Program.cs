using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace sense_test
{
    class Program
    {
        const string baseUrl = "https://c4w17062.itcs.hpecorp.net:4242";

        static void Main(string[] args)
        {
            // Test get all sense users
            foreach (SenseUser user in GetAllSenseUsers())
            {
                Console.WriteLine(string.Format("User ID: {0}\tUser Directory: {1}\tName: {2}\n", user.UserID, user.UserDirectory, user.Name));
            }

            // Test create new stream
            //SenseStream testStream = CreateNewStream("API test3");
            //Console.Write(testStream.id);

            // Test get all streams
            //GetAllStreams();

            Console.ReadKey();
        }

        
        private static string GetXrfKey(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).
                Select(s => s[random.Next(s.Length)]).ToArray());
        }
        
        private static List<SenseUser> GetAllSenseUsers()
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            // Execute the request
            var client = new RestClient(baseUrl);
            client.ClientCertificates = new X509CertificateCollection();
            client.ClientCertificates.Add(RetrieveQsCert());

            IRestResponse response = client.Execute(GenerateQsRequest("/qrs/user", "&filter=userDirectory ne 'INTERNAL'", Method.GET));
            var content = response.Content;
            List<SenseUser> allUsers = JsonConvert.DeserializeObject<List<SenseUser>>(content.ToString());

            return allUsers;
        }

        private static X509Certificate2 RetrieveQsCert()
        {
            // Retrieve Qlik Sense certificate
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);
            X509Certificate2 certificate_ = store.Certificates.Cast<X509Certificate2>().FirstOrDefault(c => c.FriendlyName == "QlikClient");
            store.Close();

            return certificate_;
        }

        private static RestRequest GenerateQsRequest(string apiPath, string optionalQueryString, Method method)
        {
            // Generate Xrf key as required by calling Qlik Sense APIs
            string requestXrfKey = GetXrfKey(16);
            string resourceString = apiPath + "?xrfkey=" + requestXrfKey + optionalQueryString;

            var request = new RestRequest(resourceString, method);

            // Add headers
            request.AddHeader("X-Qlik-Xrfkey", requestXrfKey);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Accept", "application/json");
            request.AddHeader("X-Qlik-User", @"UserDirectory=internal;UserId=sa_repository");
            
            return request;
        }

        private static List<SenseStream> GetAllStreams()
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            // Execute the request
            var client = new RestClient(baseUrl);
            client.ClientCertificates = new X509CertificateCollection();
            client.ClientCertificates.Add(RetrieveQsCert());

            IRestResponse response = client.Execute(GenerateQsRequest("/qrs/stream", "", Method.GET));
            var content = response.Content;

            List<SenseStream> streamList = JsonConvert.DeserializeObject<List<SenseStream>>(content.ToString());

            return streamList;
        }
        
        private static SenseStream CreateNewStream(string streamName)
        {            
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            // Generate Xrf key as required by calling Qlik Sense APIs
            string requestXrfKey = GetXrfKey(16);
            var request = new RestRequest("/qrs/stream?xrfkey=" + requestXrfKey, Method.POST);

            // Add headers
            request.AddHeader("X-Qlik-Xrfkey", requestXrfKey);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Accept", "application/json");
            request.AddHeader("X-Qlik-User", @"UserDirectory=internal;UserId=sa_repository");

            // Create POST body
            SenseStream newStream = new SenseStream();
            newStream.id = "00000000-0000-0000-0000-000000000000";
            newStream.name = streamName;
            request.RequestFormat = DataFormat.Json;
            request.AddBody(newStream);

            // Execute the request
            var client = new RestClient(baseUrl + ":4242");
            client.ClientCertificates = new X509CertificateCollection();
            client.ClientCertificates.Add(RetrieveQsCert());
            IRestResponse response = client.Execute(request);
            var content = response.Content;

            newStream = JsonConvert.DeserializeObject<SenseStream>(content.ToString());

            return newStream;
        }

        
        
    }
}
