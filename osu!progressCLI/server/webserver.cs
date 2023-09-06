using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using osu1progressbar.Game.Database;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Text.Json;

namespace osu_progressCLI.server
{
    public sealed class Webserver
    {
        private Webserver instance;

        private HttpListener listener;

        private DatabaseController databaseController;

        private string ip = "127.0.0.1";
        private string port = "42069";
        public Webserver()
        {
            listener = new HttpListener();
            listener.Prefixes.Add($"http://{ip}:{port}/");
            listener.Start();
            Console.WriteLine($"you can view ur Stats on localhost:{port}/");
            databaseController = new DatabaseController();
        }
        public Webserver Instance()
        {

            if (instance == null)
            {
                instance = new Webserver();
                return instance;
            }
            return instance;
        }

        public async void listen()
        {
            HttpListenerContext context = await listener.GetContextAsync();
            HandleRequest(context);
        }

        private void HandleRequest(HttpListenerContext context)
        {

            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            Console.WriteLine(request.Url.Query);

            string path = request.Url.AbsolutePath;
            NameValueCollection queryparams = HttpUtility.ParseQueryString(request.Url.Query);


            if (path == "/")
            {
                ServeStaticFile(response, "server/html/index.html", "text/html");
            }
            else if (path == "/api/beatmaps.json" && request.HttpMethod == "GET")
            {
                DateTime to = DateTime.Now;
                DateTime from = to.Subtract(TimeSpan.FromDays(30000));

                string beatmapstring = GetBeatmapData(from, to);

                byte[] buffer = Encoding.UTF8.GetBytes(beatmapstring);

                response.ContentType = "application/json";
                response.ContentLength64 = buffer.Length;

                response.OutputStream.Write(buffer, 0, buffer.Length);
                response.OutputStream.Close();
                Console.WriteLine("beatmaps send?");
            }
            else
            {
                Console.WriteLine("Invalid request");
                // Handle a 404 - Not Found response for unknown routes
                response.StatusCode = 302; // 302 Found (Temporary Redirect)
                response.AddHeader("Location", "/");
                response.OutputStream.Close();
            }

        }

        static void ServeStaticFile(HttpListenerResponse response, string filePath, string contentType)
        {
            if (File.Exists(filePath))
            { 
                string content = File.ReadAllText(filePath);
                WriteResponse(response, content, contentType);
            }
            else
            {
                response.StatusCode = 404;
                string responseString = $"404 - Not Found: File not found at {filePath}";
                WriteResponse(response, responseString, "text/plain");
            }
        }

        static void WriteResponse(HttpListenerResponse response, string responseString, string contentType)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);

            response.ContentType = contentType;
            response.ContentLength64 = buffer.Length;

            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
        }

        private string GetBeatmapData(DateTime from, DateTime to) {

            //make database maybe single ton aswell or save accestoken somewhere instead of inside the object.
            // databaseController.GetScoresInTimeSpan(from, to);
            string jsondata = System.Text.Json.JsonSerializer.Serialize(databaseController.GetScoresInTimeSpan(from, to));
            return jsondata;
         
        }

        ~Webserver()
        {
            listener.Stop();
        }
    }
}
