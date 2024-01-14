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
        private LoginHelper helper { get; set; }
        private JsonCredentials dataHelper { get; set; }

        private JsonConfig config { get; set; }

        string credentialsFilePath = "credentials.json";
        public static string loginwithosuFilePath = "loginWithOsu!.json";


        private Credentials()
        {

            try
            {
                Logger.Log(Logger.Severity.Info, Logger.Framework.Database, "Loading Login with Osu!");
                if (File.Exists(loginwithosuFilePath))
                {
                    Logger.Log(Logger.Severity.Info, Logger.Framework.Database, "File found try Parsing");
                    helper = new LoginHelper();

                    using (StreamReader configReader = new StreamReader(loginwithosuFilePath))
                    {
                        string jsonConfigString = configReader.ReadToEnd();
                        helper = JsonConvert.DeserializeObject<LoginHelper>(jsonConfigString);
                    }

                    if (helper == null)
                    {
                        Logger.Log(Logger.Severity.Info, Logger.Framework.Database, "Parsing Failed");
                    }
                    else
                    {
                        if (helper.CreatedAt.Value > DateTime.Now.AddSeconds(-helper.expires_in.Value))
                        {
                            Logger.Log(Logger.Severity.Info, Logger.Framework.Database, "Token not Outdated (reusing)");
                            SetAccessToken(helper.access_token);
                        } else
                        {
                            Logger.Log(Logger.Severity.Info, Logger.Framework.Database, "Token Outdated (requested when needed)");
                        }
                    }


                }

                Logger.Log(Logger.Severity.Info, Logger.Framework.Database, "Loading Credentials.json");
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

        public LoginHelper GetLoginHelper()
        {
            return helper;
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

                }

                File.WriteAllText(credentialsFilePath, JsonConvert.SerializeObject(dataHelper, Formatting.Indented));

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
        public bool UpdateConfig(string osufolder = "", string songfolder = "", string localconfig = "False", string username = "", string userid = "", string rank = "", string countryrank = "", string country = "", string countrycode = "", string mode = "", string cover_url = "", string avatar_url = "", string port = "4200")
        {
            if (config == null)
            {
                config = new JsonConfig();
            }

            try
            {
                string filePath = "config.json";

                if (string.IsNullOrEmpty(localconfig) || localconfig == "False")
                {
                    config.Local = "False";
                }
                else { 
                    config.Local = "True";
                }

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

                if (!string.IsNullOrEmpty(countryrank))
                    config.country_rank = countryrank;

                if (!string.IsNullOrEmpty(countrycode))
                    config.country_code = countrycode;

                if (!string.IsNullOrEmpty(country))
                    config.country = country;

                if (!string.IsNullOrEmpty(mode))
                    config.mode = mode;

                if (!string.IsNullOrEmpty(cover_url))
                    config.cover_url = cover_url;

                if (!string.IsNullOrEmpty(avatar_url))
                    config.avatar_url = avatar_url;

                if (!string.IsNullOrEmpty(userid))
                    config.userid = userid;

                File.WriteAllText(filePath, JsonConvert.SerializeObject(config, Formatting.Indented));

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

        public void updateOsuMissAnalyzer(string osufolder, string songsfolder)
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

    public class LoginHelper
    {
        public string? access_token { get; set; } = String.Empty;
        public string? refresh_token { get; set; } = String.Empty;
        public double? expires_in { get; set; } = null;
        public DateTime? CreatedAt { get; set; } = null;
    }

    public class JsonConfig
    {
        public string? Local { get; set; } = "False";
        public string? port { get; set; } = "4200";
        public string? username { get; set; } = String.Empty;
        public string? rank { get; set; } = String.Empty;
        public string? country_rank{ get; set; } = String.Empty;
        public string? country { get; set; } = String.Empty;
        public string? country_code { get; set; } = String.Empty;
        public string? mode { get; set; } = "osu";
        public string? cover_url { get; set; } = String.Empty;
        public string? avatar_url { get; set; } = String.Empty;
        public string? userid { get; set; } = "2";
        public string? osufolder { get; set; } = @$"C:\";
        public string? songfolder { get; set; } = @$"C:\";
    }
}
