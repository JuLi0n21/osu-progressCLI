using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using static Program;

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

            }
            else
            {
                Logger.Log(Logger.Severity.Warning, Logger.Framework.Network, $"HTTP Error: {response.StatusCode}, beatmap info will only be limited available");

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
                Logger.Log(Logger.Severity.Warning, Logger.Framework.Network, $"Request failed with status code {reponse.StatusCode}");

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
                Logger.Log(Logger.Severity.Warning, Logger.Framework.Misc, $"Request failed with status code {reponse.StatusCode}");

                return search;
            }

            client.Dispose();

            return search;
        }

        public async Task<JObject> getuser(string userid, string mode)
        {
            Logger.Log(Logger.Severity.Debug, Logger.Framework.Misc, $"Requesting User info for: {userid}");

            if (usercache == null || userTimestamp <= DateTime.Now.AddMinutes(-1))
            {
                userTimestamp = DateTime.Now;
                string searchEndpoint = $"https://osu.ppy.sh/api/v2/users/{userid}/{mode}";

                var client = new HttpClient();

                client.BaseAddress = new Uri(searchEndpoint);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Credentials.Instance.GetAccessToken());
                client.DefaultRequestHeaders.Accept.Clear();
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
                    Logger.Log(Logger.Severity.Warning, Logger.Framework.Network, $"Request failed with status code {reponse.StatusCode}");

                    client.Dispose();
                    return usercache;
                }

                client.Dispose();
            }
            return usercache;
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
