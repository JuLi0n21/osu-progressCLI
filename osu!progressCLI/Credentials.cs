using Newtonsoft.Json;

//fix its broken
namespace osu_progressCLI
{
    /// <summary>
    /// Used for Handling Config and Api Credentials
    /// </summary>
    public sealed class Credentials
    {
        private static Credentials instance;

        private string access_token { get; set; }
        private JsonCredentials dataHelper { get; set; }

        private JsonConfig config { get; set; }

        string credentialsFilePath = "credentials.json";


        private Credentials()
        {

            try
            {

                if (!File.Exists(credentialsFilePath))
                {
                    UpdateApiCredentials("", "");
                }
                else
                {
                    using (StreamReader credentialsReader = new StreamReader(credentialsFilePath))
                    {
                        string jsonCredentialsString = credentialsReader.ReadToEnd();
                        dataHelper = JsonConvert.DeserializeObject<JsonCredentials>(jsonCredentialsString);
                    }

                }

                string configFilePath = "config.json";
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
            catch (Exception e)
            {
                Logger.Log(Logger.Severity.Error, Logger.Framework.Misc, $"{e.Message}");
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

        public JsonConfig GetConfig()
        {
            return config;
        }

        public bool UpdateApiCredentials(string clientid, string clientsecret)
        {
            if (dataHelper == null)
            {
                dataHelper = new JsonCredentials();
            }

            try
            {

                if (dataHelper.client_secret != clientsecret || dataHelper.client_id != clientid)
                {
                    if (!string.IsNullOrEmpty(clientid))
                        dataHelper.client_id = clientid;

                    if (!string.IsNullOrEmpty(clientsecret))
                        dataHelper.client_secret = clientsecret;


                    ApiController.Instance.updateapitokken(dataHelper.client_id, dataHelper.client_secret);
                }

                File.WriteAllText(credentialsFilePath, JsonConvert.SerializeObject(dataHelper));

                return true;
            }
            catch (Exception e)
            {
                Logger.Log(Logger.Severity.Error, Logger.Framework.Misc, $"{e.Message}");

                return false;
            }

        }

        /// <summary>
        /// updates config or returns default config
        /// Sets Missanalyzer config aswell
        /// </summary>
        /// <param name="osufolder"></param>
        /// <param name="songfolder"></param>
        /// <param name="localconfig"></param>
        /// <param name="username"></param>
        /// <param name="rank"></param>
        /// <param name="country"></param>
        /// <param name="cover_url"></param>
        /// <param name="avatar_url"></param>
        /// <param name="port"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        public bool UpdateConfig(string osufolder = "C:\\", string songfolder = "C:\\", string localconfig = "False", string username = "", string rank = "", string country = "", string cover_url = "", string avatar_url = "", string port = "4200", string userid = "")
        {
            if (config == null)
            {
                config = new JsonConfig();
            }

            try
            {
                string filePath = "config.json";

                if (!string.IsNullOrEmpty(localconfig))
                    config.Localconfig = localconfig;

                if (!string.IsNullOrEmpty(port))
                    config.port = port;

                if (!string.IsNullOrEmpty(osufolder))
                    config.osufolder = @osufolder;

                if (!string.IsNullOrEmpty(songfolder))
                    config.songfolder = @songfolder;

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

                if (!string.IsNullOrEmpty(osufolder) && !string.IsNullOrEmpty(songfolder))
                    updateOsuMissAnalyzer(osufolder, songfolder);

                return true;
            }
            catch (Exception e)
            {
                Logger.Log(Logger.Severity.Error, Logger.Framework.Misc, $"{e.Message}");

                return false;
            }
        }

        public static void updateOsuMissAnalyzer(string osufolder, string songsfolder)
        {
            string filepath = "OsuMissAnalyzer/options.cfg";
            try
            {
                string[] lines = File.ReadAllLines(filepath);

                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].StartsWith("OsuDir="))
                    {
                        Logger.Log(Logger.Severity.Debug, Logger.Framework.Misc, $"Replaced {lines[i]} with OsuDir={@osufolder}");
                        lines[i] = @$"OsuDir={osufolder}";
                    }
                    if (lines[i].StartsWith("SongsDir="))
                    {
                        Logger.Log(Logger.Severity.Debug, Logger.Framework.Misc, $"Replaced {lines[i]} with SongsDir={@songsfolder}");
                        lines[i] = @$"SongsDir={songsfolder}";
                    }
                }
                File.WriteAllLines(filepath, lines);
            }
            catch (Exception e)
            {
                Logger.Log(Logger.Severity.Error, Logger.Framework.Misc, $"Error: {e.Message}");

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
        public string? Localconfig { get; set; } = "False";
        public string? port { get; set; } = "4200";
        public string? username { get; set; } = String.Empty;
        public string? rank { get; set; } = String.Empty;
        public string? country { get; set; } = String.Empty;
        public string? cover_url { get; set; } = String.Empty;
        public string? avatar_url { get; set; } = String.Empty;
        public string? userid { get; set; } = "2";
        public string? osufolder { get; set; } = @$"C:\";
        public string? songfolder { get; set; } = @$"C:\";
    }
}
