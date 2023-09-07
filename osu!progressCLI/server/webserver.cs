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

        private reqreshelper helper;

        private string ip = "127.0.0.1";
        private string port = "42069";
        public Webserver()
        {
            listener = new HttpListener();
            listener.Prefixes.Add($"http://{ip}:{port}/");
            listener.IgnoreWriteExceptions = true;
            listener.Start();
            Console.WriteLine($"you can view ur Stats on localhost:{port}/");
            databaseController = new DatabaseController();
            helper = new reqreshelper();
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


            Console.WriteLine($"queryparams: {request.Url.Query}");

            string path = request.Url.AbsolutePath;
            NameValueCollection queryparams = HttpUtility.ParseQueryString(request.Url.Query);


            //default page
            if (path == "/")
            {
                helper.defaultpage(request, response);
            } //get all beatmaps
            else if (path == "/api/beatmaps" && request.HttpMethod == "GET")
            {
                helper.getAllBeatmapScroes(request, response);
            }
            else if (path == "/api/beatmapsintimespan" && queryparams["timespan"] != null && request.HttpMethod == "GET")
            {
                //convert timespan 
                DateTime from = DateTime.Now;
                DateTime to = DateTime.Now;
                helper.getBeatmapsinTimeSpan(request, response, from, to);
            }
            else if (path == "/api/beatmaps/search" && queryparams["searchquery"] != null) {

                helper.search(request, response, queryparams);
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

        ~Webserver()
        {
            listener.Stop();
        }
    }
}
