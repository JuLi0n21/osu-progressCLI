using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebSocketSharp;

namespace osu_progressCLI
{
    /// <summary>
    /// Handels Api reqests to offical osu api / other apis
    /// </summary>
    public sealed class ApiController
    {
        private static ApiController instance;

        private JObject usercache = null;
        private DateTime userTimestamp;
    
        private double ppcutoffcache;
        private DateTime ppcutoffTimestamp;
        private ApiController() { }

        private async Task<JObject> MakeRequest(string Query)
        {
            Logger.Log(Logger.Severity.Debug, Logger.Framework.Network, $"Requesting: {Query}");

            string beatmapEndpoint = $"https://osu.ppy.sh/api/v2/{Query}";

            LoginHelper login = Credentials.Instance.GetLoginHelper();

            if (login is not null && login.CreatedAt.HasValue && login.expires_in.HasValue)
            {
                if (login.CreatedAt.Value < DateTime.Now.AddSeconds(-login.expires_in.Value))
                {
                    Logger.Log(
                        Logger.Severity.Debug,
                        Logger.Framework.Network,
                        $"Saved Token No Longer Valid, Requesting new Token"
                    );
                    await getAccessToken(login.refresh_token);
                }
            }

            var client = new HttpClient();

            client.BaseAddress = new Uri(beatmapEndpoint);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                Credentials.Instance.GetAccessToken()
            );
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );

            try
            {
                HttpResponseMessage reponse = await client.GetAsync(beatmapEndpoint);

                if (reponse.IsSuccessStatusCode)
                {
                    string responseBody = await reponse.Content.ReadAsStringAsync();
                    Logger.Log(
                        Logger.Severity.Info,
                        Logger.Framework.Network,
                        $"Recived Answer for: {Query}"
                    );

                    return JObject.Parse(responseBody);
                }
                else
                {
                    Logger.Log(
                        Logger.Severity.Warning,
                        Logger.Framework.Network,
                        $"Request for {Query} Failed: {reponse.StatusCode}"
                    );

                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                client.Dispose();
            }
        }

        /// <summary>
        /// fetches Api token.
        /// the token active for 24H.
        /// </summary>
        private async void getAccessToken()
        {
            Logger.Log(Logger.Severity.Debug, Logger.Framework.Network, $"Getting AccessToken");

            if (!Credentials.Instance.GetAccessToken().IsNullOrEmpty())
                return;

            string oauthTokenEndpoint = "https://osu.ppy.sh/oauth/token";
            string grantType = "client_credentials";
            string scope = "public";

            var httpClient = new HttpClient();
            var requestContent = new FormUrlEncodedContent(
                new[]
                {
                    new KeyValuePair<string, string>(
                        "client_id",
                        Credentials.Instance.GetClientId()
                    ),
                    new KeyValuePair<string, string>(
                        "client_secret",
                        Credentials.Instance.GetClientSecret()
                    ),
                    new KeyValuePair<string, string>("grant_type", grantType),
                    new KeyValuePair<string, string>("scope", scope),
                }
            );

            HttpResponseMessage response;

            try
            {
                response = await httpClient.PostAsync(oauthTokenEndpoint, requestContent);
            }
            catch (HttpRequestException ex)
            {
                Logger.Log(
                    Logger.Severity.Error,
                    Logger.Framework.Network,
                    $"HTTP Request Exception: {ex.Message}"
                );

                return;
            }

            if (response.IsSuccessStatusCode)
            {
                Logger.Log(
                    Logger.Severity.Info,
                    Logger.Framework.Network,
                    "Recieved Accesstoken, expanded Beatmap data should be availabe now."
                );

                var responseContent = await response.Content.ReadAsStringAsync();

                TokenResponse tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(
                    responseContent
                );
                Credentials.Instance.SetAccessToken(tokenResponse.access_token);
                Console.WriteLine(tokenResponse.access_token);
            }
            else
            {
                Logger.Log(
                    Logger.Severity.Warning,
                    Logger.Framework.Network,
                    $"HTTP Error: {response.StatusCode}, beatmap info will only be limited available, Check if ur Clientcredentials are correct and u have a working Internet Connection!"
                );
            }

            httpClient.Dispose();
        }

        public async Task getAccessToken(string refresh_token)
        {
            Logger.Log(Logger.Severity.Debug, Logger.Framework.Network, $"Getting AccessToken");

            string oauthTokenEndpoint =
                $"https://osu-progress-oauth-helper.vercel.app/refresh?port={Credentials.Instance.GetConfig().port}&refresh_token={refresh_token}";

            var client = new HttpClient();

            client.BaseAddress = new Uri(oauthTokenEndpoint);
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );

            try
            {
                HttpResponseMessage reponse = await client.GetAsync(oauthTokenEndpoint);

                if (reponse.IsSuccessStatusCode)
                {
                    string responseBody = await reponse.Content.ReadAsStringAsync();
                    TokenResponse tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(
                        responseBody
                    );
                    if (tokenResponse.access_token != null)
                    {
                        Credentials.Instance.SetAccessToken(tokenResponse.access_token);

                        LoginHelper ton = new();
                        ton.access_token = tokenResponse.access_token;
                        ton.expires_in = tokenResponse.expires_in;
                        ton.refresh_token = tokenResponse.refresh_token;
                        ton.CreatedAt = DateTime.Now;

                        Logger.Log(
                            Logger.Severity.Info,
                            Logger.Framework.Network,
                            $"Recieved new Access_Token"
                        );

                        try
                        {
                            await File.WriteAllTextAsync(
                                Credentials.loginwithosuFilePath,
                                JsonConvert.SerializeObject(ton, Formatting.Indented)
                            );
                        }
                        catch (Exception e)
                        {
                            Logger.Log(Logger.Severity.Error, Logger.Framework.Misc, e.Message);
                        }
                    }
                    else
                    {
                        Logger.Log(
                            Logger.Severity.Info,
                            Logger.Framework.Network,
                            $"Something went wrong please log in again!"
                        );
                    }
                }
                else
                {
                    Logger.Log(
                        Logger.Severity.Warning,
                        Logger.Framework.Network,
                        $"Request for Failed: {reponse.StatusCode}"
                    );
                }
            }
            catch (Exception ex)
            {
                Logger.Log(
                    Logger.Severity.Warning,
                    Logger.Framework.Network,
                    $"Request for Failed: {ex.Message}"
                );
            }
            finally
            {
                client.Dispose();
            }
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
            Logger.Log(
                Logger.Severity.Debug,
                Logger.Framework.Network,
                $"Requesting Beatmap info for: {id}"
            );

            string Query = $"beatmaps/lookup?id={id}";

            return await MakeRequest(Query);
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

            string Query = $"search?mode={mode}&query={query}";

            return await MakeRequest(Query);
        }

        /// <summary>
        /// Gets user on ID or Username
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="mode"></param>
        /// <returns>response Json</returns>
        public async Task<JObject> getuser(string userid, string mode)
        {
            Logger.Log(
                Logger.Severity.Debug,
                Logger.Framework.Misc,
                $"Requesting User info for: {userid}, {mode}"
            );

            if (userid.IsNullOrEmpty())
            {
                return null;
            }
            if (usercache == null || userTimestamp <= DateTime.Now.AddMinutes(-5))
            {
                userTimestamp = DateTime.Now;
                string Query = $"users/{userid}/{mode}";

                usercache = await MakeRequest(Query);

                if (Credentials.Instance.GetConfig().Local == "False" && usercache is not null)
                {
                    await Task.Run(
                        () =>
                            Credentials.Instance.UpdateConfig(
                                username: usercache["username"]?.ToString(),
                                rank: usercache["statistics"]["global_rank"]?.ToString(),
                                countryrank: usercache["statistics"]["country_rank"]?.ToString(),
                                country: usercache["country"]["name"]?.ToString(),
                                countrycode: usercache["country"]["code"]?.ToString().ToLower(),
                                cover_url: usercache["cover_url"]?.ToString(),
                                avatar_url: usercache["avatar_url"]?.ToString()
                            )
                    );
                }
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

        private async Task<JArray> getMostPlayedMaps(
            string userid,
            int offset = 0,
            int count = 1,
            bool downloadmissingbeatmaps = false
        )
        {
            Logger.Log(
                Logger.Severity.Debug,
                Logger.Framework.Misc,
                $"Requesting MostplayedMaps for: {userid} offset: {offset}, count: {count}, download?={downloadmissingbeatmaps}"
            );
            JArray beatmaps = null;

            string MostPlayedEndpoint =
                $"https://osu.ppy.sh/api/v2/users/{userid}/beatmapsets/most_played?limit={count}&offset={offset}";
            Logger.Log(
                Logger.Severity.Debug,
                Logger.Framework.Misc,
                $"Request {MostPlayedEndpoint}"
            );
            var client = new HttpClient();

            client.BaseAddress = new Uri(MostPlayedEndpoint);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                Credentials.Instance.GetAccessToken()
            );
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );

            HttpResponseMessage reponse = await client.GetAsync(MostPlayedEndpoint);

            if (reponse.IsSuccessStatusCode)
            {
                string responseBody = await reponse.Content.ReadAsStringAsync();
                //Console.WriteLine(responseBody);

                beatmaps = JArray.Parse(responseBody);
                Logger.Log(
                    Logger.Severity.Info,
                    Logger.Framework.Network,
                    $"Recieved Mostplayed for: {userid} Count:{beatmaps.Count}"
                );

                if (downloadmissingbeatmaps)
                {
                    foreach (JObject beatmap in beatmaps)
                    {
                        await DownloadBeatmapset(
                            client,
                            int.Parse(beatmap["beatmap"]["beatmapset_id"].ToString())
                        );
                    }
                    Logger.Log(
                        Logger.Severity.Warning,
                        Logger.Framework.Network,
                        $"Successfully Downloaded All Requested Beatmaps"
                    );
                }
            }
            else
            {
                Logger.Log(
                    Logger.Severity.Warning,
                    Logger.Framework.Network,
                    $"User Request failed with status code {reponse.StatusCode}"
                );

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
        public async Task<string> DownloadBeatmapset(
            HttpClient client,
            int beatmapsetid,
            bool unzip = true,
            string folderpath = null
        )
        {
            try
            {
                Logger.Log(
                    Logger.Severity.Info,
                    Logger.Framework.Scoreimporter,
                    $"Checking if Beatmapset Exists: {beatmapsetid}"
                );

                if (folderpath == null)
                {
                    folderpath = Credentials.Instance.GetConfig().songfolder;
                }
                else
                {
                    if (!Directory.Exists(folderpath))
                    {
                        Directory.CreateDirectory(folderpath);
                        Logger.Log(
                            Logger.Severity.Warning,
                            Logger.Framework.Scoreimporter,
                            $"{folderpath} not found creating new Folder!"
                        );
                    }
                }

                string[] files = Directory.GetDirectories(folderpath);

                bool doesexist = false;
                for (int i = 0; i < files.Length; i++)
                {
                    if (files[i].Contains($"{beatmapsetid} ")) //avoids 2345 counting for 234
                    {
                        Logger.Log(
                            Logger.Severity.Info,
                            Logger.Framework.Scoreimporter,
                            $"Beatmap Set with ID: {beatmapsetid} Exists! Canceling Download..."
                        );

                        return Path.GetFileName(files[i]);
                    }
                }

                if (!doesexist)
                {
                    {
                        try
                        {
                            Logger.Log(
                                Logger.Severity.Info,
                                Logger.Framework.Scoreimporter,
                                $"Starting Download for:  {beatmapsetid}"
                            );
                            Stopwatch time = Stopwatch.StartNew();

                            byte[] mapFile = client
                                .GetByteArrayAsync(
                                    $"https://api.chimu.moe/v1/download/{beatmapsetid}"
                                )
                                .Result;
                            string mapNameJson = client
                                .GetStringAsync($"https://api.chimu.moe/v1/set/{beatmapsetid}")
                                .Result;
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
                                File.Delete(filePath);
                            }

                            Logger.Log(
                                Logger.Severity.Info,
                                Logger.Framework.Scoreimporter,
                                $"Beatmap Set with ID: {beatmapsetid} Downloaded Succesfully in {time.Elapsed.TotalSeconds}"
                            );
                            return sanitizedMapName;
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(
                                Logger.Severity.Error,
                                Logger.Framework.Scoreimporter,
                                $"{ex.Message}"
                            );
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

        public async Task<JArray> getTopScores()
        {
            String Query =
                $"users/{Credentials.Instance.GetConfig().userid}/scores/best?include_fails=0&mode=osu&limit=100&offset=0";

            Logger.Log(Logger.Severity.Debug, Logger.Framework.Network, $"Requesting: {Query}");

            string beatmapEndpoint = $"https://osu.ppy.sh/api/v2/{Query}";

            var client = new HttpClient();

            client.BaseAddress = new Uri(beatmapEndpoint);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                Credentials.Instance.GetAccessToken()
            );
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );

            try
            {
                HttpResponseMessage reponse = await client.GetAsync(beatmapEndpoint);

                if (reponse.IsSuccessStatusCode)
                {
                    string responseBody = await reponse.Content.ReadAsStringAsync();
                    Logger.Log(
                        Logger.Severity.Info,
                        Logger.Framework.Network,
                        $"Recived Answer for: {Query}"
                    );

                    return JArray.Parse(responseBody);
                }
                else
                {
                    Logger.Log(
                        Logger.Severity.Warning,
                        Logger.Framework.Network,
                        $"Request for {Query} Failed: {reponse.StatusCode}"
                    );

                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                client.Dispose();
            }
        }

        public async Task<double> getppcutoffpoint()
        {
            if (ppcutoffcache == null || ppcutoffTimestamp <= DateTime.Now.AddMinutes(-5))
            {
                ppcutoffTimestamp = DateTime.Now;
              JArray toplays = await getTopScores();  
               ppcutoffcache = Convert.ToDouble(toplays.ElementAt(10)["pp"]);
            }
                  

            return ppcutoffcache;
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
        public DateTime? expires_at { get; set; }
    }
}
