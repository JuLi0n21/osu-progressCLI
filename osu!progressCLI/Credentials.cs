using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

//fix its broken
namespace osu_progressCLI
{
    public sealed class Credentials
    {
        private static Credentials instance;

        //add access_token to shasre across the programm
        private string access_token { get; set; }
        private JsonCredentials dataHelper { get; set; }

        private JsonConfig config { get; set; }


        private Credentials()
        {
            dataHelper = new JsonCredentials();
            config = new JsonConfig();

            try
            {
                string credentialsFilePath = "Credentials/Credentials.json";
                if (!File.Exists(credentialsFilePath))
                {
                    UpdateApiCredentials("","");
                }
                else
                {
                    using (StreamReader credentialsReader = new StreamReader(credentialsFilePath))
                    {
                        string jsonCredentialsString = credentialsReader.ReadToEnd();
                        dataHelper = JsonConvert.DeserializeObject<JsonCredentials>(jsonCredentialsString);
                    }
                }

                string configFilePath = "Config/Config.json";
                if (!File.Exists(configFilePath))
                {
                    UpdateConfig();
                }
                else
                {
                    using (StreamReader configReader = new StreamReader(configFilePath))
                    {
                        string jsonConfigString = configReader.ReadToEnd();
                        config = JsonConvert.DeserializeObject<JsonConfig>(jsonConfigString);
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        public void SetAccessToken(string token)
        {
            access_token = token;
        }

        public string GetAccessToken()
        {
            return access_token;
        }

        public string GetClientSecret()
        {
            return dataHelper.client_secret;
        }

        public string GetClientId()
        {
            return dataHelper.client_id;
        }

        public JsonConfig GetConfig() {
            return config;
        }

        public bool UpdateApiCredentials(string clientid, string clientsecret)
        {

            try
            {
                string filePath = "Credentials/Credentials.json";

                if (!string.IsNullOrEmpty(clientid))
                    dataHelper.client_id = clientid;

                if (!string.IsNullOrEmpty(clientsecret))
                    dataHelper.client_secret = clientsecret;

                ApiController.Instance.updateapitokken(dataHelper.client_id, dataHelper.client_secret);

                File.WriteAllText(filePath, JsonConvert.SerializeObject(dataHelper));

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }

            
        
        }

        public bool UpdateConfig(string localconfig = "false", string username = "", string rank = "", string country = "", string cover_url = "", string avatar_url = "", string port = "4200", string userid = "")
        {

            try
            {
                string filePath = "config.json";

                if (!string.IsNullOrEmpty(localconfig))
                    config.Localconfig = localconfig;

                if (!string.IsNullOrEmpty(port))
                    config.port = port;

                if (!string.IsNullOrEmpty(username))
                    config.username = username;

                if (!string.IsNullOrEmpty(rank))
                    config.rank = rank;

                if (!string.IsNullOrEmpty(country))
                    config.country = country;

                if (!string.IsNullOrEmpty(cover_url))
                    config.cover_url = cover_url;

                if (!string.IsNullOrEmpty(avatar_url))
                    config.avatar_url = avatar_url;

                if (!string.IsNullOrEmpty(userid))
                    config.userid = userid;

                File.WriteAllText(filePath, JsonConvert.SerializeObject(config));

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        public static Credentials Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Credentials();
                }
                return instance;
            }
        }

    }

    class JsonCredentials
    {
        public string? client_id { get; set; } = String.Empty;
        public string? client_secret { get; set; } = String.Empty;
    }

    public class JsonConfig
    {
        public string? Localconfig { get ; set; } = "false";
        public string? port { get; set; } = "4200";
        public string? username { get; set; } = String.Empty;
        public string? rank { get; set; } = String.Empty;
        public string? country { get; set; } = String.Empty;
        public string? cover_url { get; set; } = String.Empty;
        public string? avatar_url { get; set; } = String.Empty;
        public string? userid { get;set; } = "14100399";   
    }
}
