using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace osu_progressCLI
{
    /// <summary>
    /// Handels Api reqests to offical osu api / other apis
    /// </summary>
    public sealed class ApiController
    {

        private static ApiController instance;

        private string clientid;
        private string clientsecret;

        private JObject usercache = null;
        private DateTime userTimestamp;
        private ApiController()
        {

            clientid = Credentials.Instance.GetClientId();
            clientsecret = Credentials.Instance.GetClientSecret();

            if (Credentials.Instance.GetAccessToken() == null)
            {
                getAccessToken();
            }
        }

        /// <summary>
        /// fetches Api token.
        /// the token active for 24H.
        /// </summary>
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


        /// <summary>
        /// Should not be used doesnt work!
        /// </summary>
        /// <param name="clientid"></param>
        /// <param name="clientsecret"></param>
        public async void updateapitokken(string clientid, string clientsecret)
        {
            this.clientid = clientid;
            this.clientsecret = clientsecret;
            getAccessToken();
        }

        /// <summary>
        /// Fetches Beatmap info from osu api
        /// </summary>
        /// <param name="id">
        /// Beatmapid to be fetched
        /// </param>
        /// <returns>Returns Beatmap in Json format</returns>
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

        /// <summary>
        /// Osu Api search
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="query"></param>
        /// <returns>response Json</returns>
        public async Task<JObject> getSearch(string mode, string query)
        {
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

        /// <summary>
        /// Gets user on ID or Username
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="mode"></param>
        /// <returns>response Json</returns>
        public async Task<JObject> getuser(string userid, string mode)
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
                    //Console.WriteLine(responseBody);

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
            else
            {
                Logger.Log(Logger.Severity.Debug, Logger.Framework.Network, $"Serving Cached user: {usercache["username"]} | Refresh in {(userTimestamp - DateTime.Now.AddMinutes(-5)).TotalSeconds.ToString().Substring(0, 3)} Seconds");
                return usercache;
            }
            return usercache;
        }


        /// <summary>
        /// Fetches Most played Beatmap for Userid
        /// </summary>
        /// <param name="userid">Userid</param>
        /// <param name="offset">offset of the request</param>
        /// <param name="count">amount max is 50</param>
        /// <param name="downloadmissingbeatmaps"> if u want to download the missing beatmaps</param>
        /// <returns>List of Beatmaps</returns>

        private async Task<JArray> getMostPlayedMaps(string userid, int offset = 0, int count = 1, bool downloadmissingbeatmaps = false)
        {

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
                        await DownloadBeatmapset(client, int.Parse(beatmap["beatmap"]["beatmapset_id"].ToString()));
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
      
        /// <summary>
        /// Downloads Beatmap
        /// </summary>
        /// <param name="client">client used for the Operation</param>
        /// <param name="beatmapsetid">beatmap to be downloaded</param>
        /// <param name="unzip">if it shoudl be unzipped</param>
        /// <param name="folderpath">overwrite default save path (Songs Folder given in config.)</param>
        /// <returns>relative foldername/zipname </returns>
        public async Task<string> DownloadBeatmapset(HttpClient client, int beatmapsetid, bool unzip = true, string folderpath = null)
        {
            try
            {
                Logger.Log(Logger.Severity.Info, Logger.Framework.Scoreimporter, $"Checking if Beatmapset Exists: {beatmapsetid}");

                if (folderpath == null)
                {
                    folderpath = Credentials.Instance.GetConfig().songfolder;
                }
                else
                {
                    if (!Directory.Exists(folderpath))
                    {
                        Directory.CreateDirectory(folderpath);
                        Logger.Log(Logger.Severity.Warning, Logger.Framework.Scoreimporter, $"{folderpath} not found creating new Folder!");
                    }
                }


                string[] files = Directory.GetDirectories(folderpath);

                bool doesexist = false;
                for (int i = 0; i < files.Length; i++)
                {
                    if (files[i].Contains($"{beatmapsetid} ")) //avoids 2345 counting for 234 
                    {
                        Logger.Log(Logger.Severity.Info, Logger.Framework.Scoreimporter, $"Beatmap Set with ID: {beatmapsetid} Exists! Canceling Download...");

                        return Path.GetFileName(files[i]);
                    }
                }


                if (!doesexist)
                {
                    {
                        try
                        {
                            Logger.Log(Logger.Severity.Info, Logger.Framework.Scoreimporter, $"Starting Download for:  {beatmapsetid}");
                            Stopwatch time = Stopwatch.StartNew();

                            byte[] mapFile = client.GetByteArrayAsync($"https://api.chimu.moe/v1/download/{beatmapsetid}").Result;
                            string mapNameJson = client.GetStringAsync($"https://api.chimu.moe/v1/set/{beatmapsetid}").Result;
                            dynamic mapNameData = JsonConvert.DeserializeObject(mapNameJson);
                            // Console.WriteLine(mapNameJson);
                            string artist = mapNameData.Artist;
                            string title = mapNameData.Title;

                            string unfilterd = $"{mapNameData.SetId} {artist} - {title}";
                            string sanitizedMapName = Regex.Replace(unfilterd, "[<>\":/\\|?*]", "");

                            string filePath = Path.Combine(folderpath, $"{sanitizedMapName}.osz");
                            string dirPath = Path.Combine(folderpath, $"{sanitizedMapName}");


                            using (FileStream fileStream = File.OpenWrite(filePath))
                            {
                                fileStream.Write(mapFile, 0, mapFile.Length);
                            }

                            if (unzip)
                            {
                                ZipFile.ExtractToDirectory(filePath, dirPath);
                            }

                            Logger.Log(Logger.Severity.Info, Logger.Framework.Scoreimporter, $"Beatmap Set with ID: {beatmapsetid} Downloaded Succesfully in {time.Elapsed.TotalSeconds}");
                            return sanitizedMapName;
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(Logger.Severity.Error, Logger.Framework.Scoreimporter, $"{ex.Message}");
                            return null;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.Log(Logger.Severity.Error, Logger.Framework.Network, $"{ex.Message}");
                return null;
            }

            return null;
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
