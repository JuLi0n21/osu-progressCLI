using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace osu_progressCLI
{
    public sealed class Crendtials
    {
        private static Crendtials instance;

        private string access_token { get; set; }
        private JsonDataHelper dataHelper { get; set; }

        private Crendtials() {
            dataHelper = new JsonDataHelper();
            try {
                string jsonString = File.ReadAllText("Credentials/Credentials.json");

                dataHelper = JsonSerializer.Deserialize<JsonDataHelper>(jsonString);
                //Console.WriteLine(jsonString);
            }
            catch (Exception exption) {
                Console.WriteLine(exption.Message);
            }
        }

        public void SetAccessToken(string token) {
            access_token = token;
        }

        public string GetAccessToken()
        {
            return access_token;
        }

        public string GetClientSecret() {
            return dataHelper.client_secret;
        }

        public string GetClientId()
        {
            return dataHelper.client_id;
        }

        public static Crendtials Instance { 
            get {
                if (instance == null) { 
                    instance = new Crendtials();
                }
                return instance; 
            } 
        }

    }

    class JsonDataHelper
    {
        public string? client_id { get; set; }
        public string? client_secret { get; set; }
    }
}
