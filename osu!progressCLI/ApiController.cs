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
                Console.WriteLine($"HTTP Request Exception: {ex.Message}");
                return;
            }

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Recieved Accesstoken, expanded Beatmap data should be availabe now.");

                TokenResponse tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseContent);

                Credentials.Instance.SetAccessToken(tokenResponse.access_token);

            }
            else
            {
                Console.WriteLine($"HTTP Error: {response.StatusCode}, beatmap info will only be limited available");
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
            Console.WriteLine($"Requesting Beatmap info for: {id}");
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
                Console.WriteLine($"Rechieved Beatmap info for: {id}");
                Console.WriteLine(responseBody);
                beatmap = JObject.Parse(responseBody);
            }
            else
            {
                Console.WriteLine($"Request failed with status code {reponse.StatusCode}");
                return beatmap;
            }

            client.Dispose();
            return beatmap;
        }

        public async Task<JObject> getSearch(string mode, string query) {

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
                string responseBody = await reponse.Content.ReadAsStringAsync();
                Console.WriteLine("search Recieved.");
                //Console.WriteLine(responseBody);

                search = JObject.Parse(responseBody);
            }
            else
            {
                Console.WriteLine($"Request failed with status code {reponse.StatusCode}");
                return search;
            }

            client.Dispose();

            return search;
        }

        public async Task<JObject> getuser(string userid, string mode)
        {

            Console.WriteLine($"Requesting User info for: {userid}");
            JObject userJson = null;

            string searchEndpoint = $"https://osu.ppy.sh/api/v2/users/{userid}/{mode}";

            var client = new HttpClient();

            client.BaseAddress = new Uri(searchEndpoint);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Credentials.Instance.GetAccessToken());
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


            HttpResponseMessage reponse = await client.GetAsync(searchEndpoint);

            if (reponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"Recieved User info for: {userid}");
                string responseBody = await reponse.Content.ReadAsStringAsync();
                //Console.WriteLine(responseBody);

                userJson = JObject.Parse(responseBody);
            }
            else
            {
                Console.WriteLine($"Request failed with status code {reponse.StatusCode}");
                return userJson;
            }

            client.Dispose();

            return userJson;
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
