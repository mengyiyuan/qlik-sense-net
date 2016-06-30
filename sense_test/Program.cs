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
            //foreach (SenseUser user in GetAllSenseUsers())
            //{
            //    Console.WriteLine(string.Format("User ID: {0}\tUser Directory: {1}\tName: {2}\n", user.UserID, user.UserDirectory, user.Name));
            //}

            // Test create new stream
            //SenseStream testStream = CreateNewStream("API test3");
            //Console.Write(testStream.id);

            // Test get all streams
            //GetAllStreams();

            // Test get all custom properties
            //GetAllCustomProperty();

            // Test update choice values in custom property
            UpdateCustomProperty("API test", "StreamCategory");

            Console.ReadKey();
        }

        
        private static string GetXrfKey(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).
                Select(s => s[random.Next(s.Length)]).ToArray());
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

        private static RestRequest GenerateQsRequest(string apiPath, string optionalQueryString, object body, Method method)
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

            // If method is POST or PUT, we need a body
            if (method == Method.POST || method == Method.PUT)
            {
                request.RequestFormat = DataFormat.Json;
                request.AddBody(body);
            }
            
            return request;
        }

        private static string ExecuteQsRequest(RestRequest request)
        {
            var client = new RestClient(baseUrl);
            client.ClientCertificates = new X509CertificateCollection();
            client.ClientCertificates.Add(RetrieveQsCert());
            IRestResponse response = client.Execute(request);
            return response.Content.ToString();
        }

        private static List<SenseUser> GetAllSenseUsers()
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            var request = GenerateQsRequest("/qrs/user", "&filter=userDirectory ne 'INTERNAL'", null, Method.GET);
            List<SenseUser> allUsers = JsonConvert.DeserializeObject<List<SenseUser>>(ExecuteQsRequest(request));            

            return allUsers;
        }

        private static List<SenseStream> GetAllStreams()
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            // Execute the request
            var client = new RestClient(baseUrl);
            client.ClientCertificates = new X509CertificateCollection();
            client.ClientCertificates.Add(RetrieveQsCert());

            IRestResponse response = client.Execute(GenerateQsRequest("/qrs/stream", "", null, Method.GET));
            var content = response.Content;

            List<SenseStream> streamList = JsonConvert.DeserializeObject<List<SenseStream>>(content.ToString());

            return streamList;
        }
        
        private static SenseStream CreateNewStream(string streamName)
        {            
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                       
            // Create POST body
            SenseStream newStream = new SenseStream();
            newStream.id = "00000000-0000-0000-0000-000000000000";
            newStream.name = streamName;
            var request = GenerateQsRequest("/qrs/stream", "", newStream, Method.POST);

            // Execute the request and deserialize results
            newStream = JsonConvert.DeserializeObject<SenseStream>(ExecuteQsRequest(request));

            return newStream;
        }        

        private static List<SenseCustomProperty> GetAllCustomProperty()
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            // Execute the request
            var client = new RestClient(baseUrl);
            client.ClientCertificates = new X509CertificateCollection();
            client.ClientCertificates.Add(RetrieveQsCert());

            IRestResponse response = client.Execute(GenerateQsRequest("/qrs/custompropertydefinition/full", "", null, Method.GET));
            var content = response.Content;

            List<SenseCustomProperty> customPropertyList = JsonConvert.DeserializeObject<List<SenseCustomProperty>>(content.ToString());

            return customPropertyList;
        }

        //private static string GetResourceIdByName(string resourceName)
        //{

        //}

        private static SenseCustomProperty UpdateCustomProperty(string newChoiceValue, string propertyName)
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            // Create PUT body
            SenseCustomProperty updatedProperty = new SenseCustomProperty();
            
            List<SenseCustomProperty> customPropertyList = GetAllCustomProperty();
            
            foreach (SenseCustomProperty property in customPropertyList)
            {
                if (property.name == propertyName)
                {
                    updatedProperty = property;
                    break;
                }
            }

            updatedProperty.choiceValues.Add(newChoiceValue);
            //string body = JsonConvert.SerializeObject(updatedProperty);

            var request = GenerateQsRequest("/qrs/custompropertydefinition/" + updatedProperty.id, "", updatedProperty, Method.PUT);

            updatedProperty = JsonConvert.DeserializeObject<SenseCustomProperty>(ExecuteQsRequest(request));
            return updatedProperty;
        }

    }
}
