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


//add button to get credentials
namespace osu_progressCLI.server
{
    public sealed class Webserver
    {
        private Webserver instance;

        private HttpListener listener;

        private DatabaseController databaseController;

        private reqreshelper helper;

        private string ip = "127.0.0.1";
        private string port = Credentials.Instance.GetConfig().port;
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

        public async Task listen()
        {
            HttpListenerContext context = await listener.GetContextAsync();
            await HandleRequest(context);
        }


        private async Task HandleRequest(HttpListenerContext context)
        {

            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;


            // Console.WriteLine($"queryparams: {request.Url.Query}");

            string path = request.Url.AbsolutePath;
            NameValueCollection queryparams = HttpUtility.ParseQueryString(request.Url.Query);

            if (path.EndsWith(".jpg") || path.EndsWith(".jpeg") || path.EndsWith(".png") || path.EndsWith(".gif") || path.EndsWith(".bmp") || path.EndsWith(".tiff") || path.EndsWith(".ico") || path.EndsWith(".webp") || path.EndsWith(".svg"))
            {
                helper.serveimage(request, response, path);
                return;
            }

            if (path.EndsWith(".html"))
            {
                helper.servehtml(request, response, path);
                return;
            }

            if (path.EndsWith(".js"))
            {
                helper.servejs(request, response, path);
                return;
            }

            if (path.EndsWith(".css"))
            {
                helper.servecss(request, response, path);
                return;
            }

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
            else if (path == "/api/beatmaps/averages")
            {
                helper.getScoreAverages(request, response, queryparams);
            }
            else if (path == "/api/beatmaps/score" && queryparams["id"] != null)
            {
                Console.WriteLine("score requested: " + queryparams[0]);
                helper.getScore(request, response, queryparams);
            }
            else if (path == "/api/beatmaps/search" && queryparams["searchquery"] != null)
            {

                //convert timespan 
                DateTime from = DateTime.Now;
                DateTime to = DateTime.Now;
                helper.getBanchoTimeinTineSpan(request, response, from, to);
            }
            else if (path == "/api/banchotime" && request.HttpMethod == "GET")
            {
                helper.getAllBanchoTime(request, response);
            }
            else if (path == "/api/banchotimebyday" && request.HttpMethod == "GET")
            {
                helper.getBanchoTimebyday(request, response);
            }
            else if (path == "/api/timewasted" && request.HttpMethod == "GET")
            {
                helper.getAllTimeWasted(request, response);
            }
            else if (path == "/api/timewastedbyday" && request.HttpMethod == "GET")
            {
                helper.getTimeWastedbyday(request, response);
            }
            else if (path == "/api/save" && request.HttpMethod == "POST")
            {
                helper.save(request, response);
            }
            else
            {
                Console.WriteLine("Not found: " + path);
                response.StatusCode = 404;
                response.OutputStream.Close();
            }

        }

        public void stop()
        {
            listener.Stop();
        }

        ~Webserver()
        {
            listener.Stop();
        }
    }
}
