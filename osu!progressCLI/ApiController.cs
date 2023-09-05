﻿using Newtonsoft.Json;
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
    internal class ApiController
    {

        private string clientid;
        private string clientsecret;
        public ApiController() {

            clientid = Crendtials.Instance.GetClientId();
            clientsecret = Crendtials.Instance.GetClientSecret();

            if (Crendtials.Instance.GetAccessToken() == null)
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
                //Console.WriteLine(responseContent);

                TokenResponse tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseContent);

                Crendtials.Instance.SetAccessToken(tokenResponse.access_token);

            }
            else
            {
                //retireve beatmap info next time a 
                Console.WriteLine($"HTTP Error: {response.StatusCode}, beatmap info will only be limited available");
            }

            httpClient.Dispose();

        }

        public async Task<JObject> getExpandedBeatmapinfo(string id)
        {
            JObject beatmap;
            //Console.WriteLine($"{DateTime.Now:T} Error reading {e.propPath}{Environment.NewLine}");
            string beatmapEndpoint = $"https://osu.ppy.sh/api/v2/beatmaps/lookup?id={id}";

            var client = new HttpClient();

            client.BaseAddress = new Uri(beatmapEndpoint);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Crendtials.Instance.GetAccessToken());
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


            HttpResponseMessage reponse = await client.GetAsync(beatmapEndpoint);

            if (reponse.IsSuccessStatusCode)
            {
                string responseBody = await reponse.Content.ReadAsStringAsync();
                Console.WriteLine("Beatmap info Recieved.");

                beatmap = JObject.Parse(responseBody);
            }
            else
            {
                Console.WriteLine($"Request failed with status code {reponse.StatusCode}");
                return null;
            }


            return beatmap;
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
