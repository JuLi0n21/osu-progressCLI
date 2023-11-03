using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OsuMemoryDataProvider.OsuMemoryModels.Abstract;
using System.Data.Entity.Core.Mapping;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO.Compression;
using System.Web;

namespace osu_progressCLI
{
    public sealed class ApiController
    {

        private static ApiController instance;

        private string clientid;
        private string clientsecret;

        private JObject usercache = null;
        private DateTime userTimestamp;
        private ApiController() {

            clientid = Credentials.Instance.GetClientId();
            clientsecret = Credentials.Instance.GetClientSecret();

            if(Credentials.Instance.GetAccessToken() == null)
            {
                getAccessToken();
            }
        }



        private async void getAccessToken()
        {
            Logger.Log(Logger.Severity.Debug, Logger.Framework.Network, $"Getting AccessToken");

            string access_token = null;

            string oauthTokenEndpoint = "https://osu.ppy.sh/oauth/token";
            string grantType = "client_credentials";
            string scope = "public";

            var httpClient = new HttpClient();
            var requestContent = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("client_id", clientid),
            new KeyValuePair<string, string>("client_secret", clientsecret),
            new KeyValuePair<string, string>("grant_type", grantType),
            new KeyValuePair<string, string>("scope", scope),
        });

            HttpResponseMessage response;

            try
            {
                response = await httpClient.PostAsync(oauthTokenEndpoint, requestContent);
            }
            catch (HttpRequestException ex)
            {
                Logger.Log(Logger.Severity.Error, Logger.Framework.Network, $"HTTP Request Exception: {ex.Message}");

                return;
            }

            if (response.IsSuccessStatusCode)
            {
                Logger.Log(Logger.Severity.Info, Logger.Framework.Network, "Recieved Accesstoken, expanded Beatmap data should be availabe now.");

                var responseContent = await response.Content.ReadAsStringAsync();

                TokenResponse tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseContent);
                Credentials.Instance.SetAccessToken(tokenResponse.access_token);

               //await getMostPlayedMaps("14100399",10150,50, true);
            }
            else
            {
                Logger.Log(Logger.Severity.Warning, Logger.Framework.Network, $"HTTP Error: {response.StatusCode}, beatmap info will only be limited available, Check if ur Clientcredentials are correct and u have a working Internet Connection!");

            }

            httpClient.Dispose();

        }

        public async void updateapitokken(string clientid, string clientsecret) {
            this.clientid = clientid;
            this.clientsecret = clientsecret;
            getAccessToken();
        }

        public async Task<JObject> getExpandedBeatmapinfo(string id)
        {
            Logger.Log(Logger.Severity.Debug, Logger.Framework.Network, $"Requesting Beatmap info for: {id}");

            JObject beatmap = null;
            string beatmapEndpoint = $"https://osu.ppy.sh/api/v2/beatmaps/lookup?id={id}";

            var client = new HttpClient();

            client.BaseAddress = new Uri(beatmapEndpoint);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Credentials.Instance.GetAccessToken());
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


            HttpResponseMessage reponse = await client.GetAsync(beatmapEndpoint);

            if (reponse.IsSuccessStatusCode)
            {
                string responseBody = await reponse.Content.ReadAsStringAsync();
                Logger.Log(Logger.Severity.Info, Logger.Framework.Network, $"Rechieved Beatmap info for: {id}");

                //Console.WriteLine(responseBody);
                beatmap = JObject.Parse(responseBody);
            }
            else
            {
                Logger.Log(Logger.Severity.Warning, Logger.Framework.Network, $"Beatmap Request failed with status code {reponse.StatusCode}");

                return beatmap;
            }

            client.Dispose();
            return beatmap;
        }

        public async Task<JObject> getSearch(string mode, string query) {
            Logger.Log(Logger.Severity.Info, Logger.Framework.Network, $"Api Search Request");

            JObject search = null;

            string searchEndpoint = $"https://osu.ppy.sh/api/v2/search?mode={mode}&query={query}";

            var client = new HttpClient();

            client.BaseAddress = new Uri(searchEndpoint);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Credentials.Instance.GetAccessToken());
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


            HttpResponseMessage reponse = await client.GetAsync(searchEndpoint);

            if (reponse.IsSuccessStatusCode)
            {
                Logger.Log(Logger.Severity.Info, Logger.Framework.Misc, $"Search Recieved for: {query}");

                string responseBody = await reponse.Content.ReadAsStringAsync();
                //Console.WriteLine(responseBody);

                search = JObject.Parse(responseBody);
            }
            else
            {
                Logger.Log(Logger.Severity.Warning, Logger.Framework.Misc, $"Search Request failed with status code {reponse.StatusCode}");

                return search;
            }

            client.Dispose();

            return search;
        }

        public async Task<JObject> getuser(string userid , string mode)
        {
            Logger.Log(Logger.Severity.Debug, Logger.Framework.Misc, $"Requesting User info for: {userid}, {mode}");
            if (usercache == null || userTimestamp <= DateTime.Now.AddMinutes(-5))
            {
                userTimestamp = DateTime.Now;
                string searchEndpoint = $"https://osu.ppy.sh/api/v2/users/{userid}/{mode}";
                Logger.Log(Logger.Severity.Debug, Logger.Framework.Misc, $"Request {searchEndpoint}");
                var client = new HttpClient();

                client.BaseAddress = new Uri(searchEndpoint);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Credentials.Instance.GetAccessToken());
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("key", "username");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


                HttpResponseMessage reponse = await client.GetAsync(searchEndpoint);

                if (reponse.IsSuccessStatusCode)
                {
                    Logger.Log(Logger.Severity.Info, Logger.Framework.Network, $"Recieved User info for: {userid}");

                    string responseBody = await reponse.Content.ReadAsStringAsync();
                    Console.WriteLine(responseBody);

                    usercache = JObject.Parse(responseBody);
                }
                else
                {
                    Logger.Log(Logger.Severity.Warning, Logger.Framework.Network, $"User Request failed with status code {reponse.StatusCode}");

                    client.Dispose();
                    return usercache;
                }

                client.Dispose();
            }
            else {
                Logger.Log(Logger.Severity.Debug, Logger.Framework.Network, $"Serving Cached user: {usercache["username"]} | Refresh in {(userTimestamp - DateTime.Now.AddMinutes(-5)).TotalSeconds.ToString().Substring(0,3)} Seconds");
                return usercache;
            }
            return usercache;
        }

        private async Task<JArray> getMostPlayedMaps(string userid, int offset = 0, int count = 1, bool downloadmissingbeatmaps = false) {
            
            Logger.Log(Logger.Severity.Debug, Logger.Framework.Misc, $"Requesting MostplayedMaps for: {userid} offset: {offset}, count: {count}, download?={downloadmissingbeatmaps}");
            JArray beatmaps = null;

            string MostPlayedEndpoint = $"https://osu.ppy.sh/api/v2/users/{userid}/beatmapsets/most_played?limit={count}&offset={offset}";
            Logger.Log(Logger.Severity.Debug, Logger.Framework.Misc, $"Request {MostPlayedEndpoint}");
            var client = new HttpClient();

            client.BaseAddress = new Uri(MostPlayedEndpoint);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Credentials.Instance.GetAccessToken());
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


            HttpResponseMessage reponse = await client.GetAsync(MostPlayedEndpoint);


                if (reponse.IsSuccessStatusCode)
                {

                    string responseBody = await reponse.Content.ReadAsStringAsync();
                    //Console.WriteLine(responseBody);

                beatmaps = JArray.Parse(responseBody);
                Logger.Log(Logger.Severity.Info, Logger.Framework.Network, $"Recieved Mostplayed for: {userid} Count:{beatmaps.Count}");

                if (downloadmissingbeatmaps)
                    {
                        foreach (JObject beatmap in beatmaps)
                        {
                            await DownloadBeatmapset(int.Parse(beatmap["beatmap"]["beatmapset_id"].ToString()));
                        }
                     Logger.Log(Logger.Severity.Warning, Logger.Framework.Network, $"Successfully Downloaded All Requested Beatmaps");

                    }
                }
                else
                {
                Logger.Log(Logger.Severity.Warning, Logger.Framework.Network, $"User Request failed with status code {reponse.StatusCode}");

                client.Dispose();
                    return beatmaps;
                }
                
                client.Dispose();
                return beatmaps;
        }

        private async Task<bool> DownloadBeatmapset(int beatmapsetid, string folderpath = null, bool unzip = true) {
            
            Logger.Log(Logger.Severity.Info, Logger.Framework.Network, $"Checking if Beatmapset Exists: {beatmapsetid}");

            if (folderpath == null) {
                folderpath = Credentials.Instance.GetConfig().songfolder;
            }

            string[] files = Directory.GetDirectories(folderpath);
            
            bool doesexist = false;
            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].Contains($"{beatmapsetid} ")) //avoids 2345 counting for 234 
                {
                    Logger.Log(Logger.Severity.Info, Logger.Framework.Network, $"Beatmap Set with ID: {beatmapsetid} Exists! Canceling Download...");

                    return false;
                }
            }


            if (!doesexist)
            {
                {
                    try {
                        Logger.Log(Logger.Severity.Info, Logger.Framework.Network, $"Starting Download for:  {beatmapsetid}");

                        using HttpClient client = new HttpClient();

                        byte[] mapFile = client.GetByteArrayAsync($"https://api.chimu.moe/v1/download/{beatmapsetid}").Result;
                        string mapNameJson = client.GetStringAsync($"https://api.chimu.moe/v1/set/{beatmapsetid}").Result;
                        dynamic mapNameData = JsonConvert.DeserializeObject(mapNameJson);
                        // Console.WriteLine(mapNameJson);
                        string artist = mapNameData.Artist;
                        string title = mapNameData.Title;

                        string sanitizedMapName = $"{mapNameData.SetId} {artist} - {title}";

                        string filePath = Path.Combine(folderpath, $"{sanitizedMapName}.osz");
                        string dirPath = Path.Combine(folderpath, $"{sanitizedMapName}");


                        using (FileStream fileStream = File.OpenWrite(filePath))
                        {
                            fileStream.Write(mapFile, 0, mapFile.Length);
                        }

                        if (unzip) {
                            ZipFile.ExtractToDirectory(filePath, dirPath);
                        }

                        Logger.Log(Logger.Severity.Info, Logger.Framework.Network, $"Beatmap Set with ID: {beatmapsetid} Downloaded Succesfully");
                        return true;
                    } catch (Exception ex)
                    {
                    Logger.Log(Logger.Severity.Info, Logger.Framework.Network, $"{ex.Message}");
                        return false;
                    }
                }

            } 

            return false;
        }

        public static ApiController Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ApiController();
                }
                return instance;
            }
        }

    }

    public class TokenResponse
    {
        public string? token_type { get; set; }
        public string? access_token { get; set; }
        public int? expires_in { get; set; }
        public string? refresh_token { get; set; }

    }

}
