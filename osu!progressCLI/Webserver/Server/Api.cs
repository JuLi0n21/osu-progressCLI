using Fluid;
using osu1progressbar.Game.Database;
using osu_progressCLI.Datatypes;
using System.Net;
using System.Web;
using WebSocketSharp;

namespace osu_progressCLI.Webserver.Server
{
    internal class Api
    {
        DatabaseController controller;

        public Api()
        {
            controller = new DatabaseController();
        }
		
        public async void Route(HttpListenerRequest request, HttpListenerResponse response, FluidParser parser)
        {

            string path = request.Url.AbsolutePath;

            var queryparams = HttpUtility.ParseQueryString(request.Url.Query);

            if (path == "/api/dogfood")
            {
                
                
            }
            else if (path == "/api/beatmaps" && request.HttpMethod == "GET")
            {
                DateTime to = DateTime.Now;
                DateTime from = to.Subtract(TimeSpan.FromDays(30000)); //around 100 years

                if (queryparams["from"] != null && queryparams["to"] != null)
                {
                    if (!DateTime.TryParse(queryparams["to"].ToString(), out to) || !DateTime.TryParse(queryparams["from"].ToString(), out from))
                    {
                        Webserver.Instance().WriteResponse(response, "Something went wrong check parameters from and to", "text/plain");
                        return;
                    }
                }

                List<Score> scores = controller.GetScoresInTimeSpan(from, to);
                var template = FluidRenderer.templates.Find(item => item.Key.Equals("Scores.liquid"));

                var context = new TemplateContext(scores);
                context.SetValue("List", scores);
                Webserver.Instance().WriteResponse(response, template.Value.Render(context), "text/html");
            }
            else if (path == "/api/beatmaps/search" && request.HttpMethod == "GET")
            {

                using (Stream body = request.InputStream)
                {
                    using (StreamReader reader = new StreamReader(body, request.ContentEncoding))
                    {
                        string bodyData = reader.ReadToEnd();
                        Console.WriteLine("Received data: " + bodyData);
                    }
                }


                DateTime to = DateTime.Now;
                DateTime from = to.Subtract(TimeSpan.FromDays(30000)); //around 100 years

                Console.WriteLine(queryparams["query"]);

                if (!queryparams["from"].IsNullOrEmpty() && !queryparams["to"].IsNullOrEmpty())
                {
                    if (!DateTime.TryParse(queryparams["to"].ToString(), out to) || !DateTime.TryParse(queryparams["from"].ToString(), out from))
                    {
                        Webserver.Instance().WriteResponse(response, "Something went wrong check parameters from and to", "text/plain");
                        return;
                    }
                }

                List<Score> scores = controller.GetScoreSearch(from, to, QueryParser.Filter(queryparams["query"].ToString()));
                var template = FluidRenderer.templates.Find(item => item.Key.Equals("Scores.liquid"));

                var context = new TemplateContext(scores);
                context.SetValue("List", scores);
                Webserver.Instance().WriteResponse(response, template.Value.Render(context), "text/html");
            }
            else if (path == "/api/beatmaps/averages" && request.HttpMethod == "GET")
            {

            }
            else if (path == "/api/beatmaps/scores" && request.HttpMethod == "GET")
            {

            }
            else if (path == "/api/beatmaps/search" && request.HttpMethod == "GET")
            {

            }
            else if (path == "/api/banchotime" && request.HttpMethod == "GET")
            {
                //optional arguement for "bancho time by day"
            }
            else if (path == "/api/timewasted" && request.HttpMethod == "GET")
            {
                //optional arguement for "time wasted by day"
            }
            else if (path == "/api/user" && request.HttpMethod == "GET")
            {

            }
            else if (path == "/api/save" && request.HttpMethod == "POST")
            {

            }
            else if (path == "/api/run" && request.HttpMethod == "POST")
            {

            }
            else if (path == "/api/upload" && request.HttpMethod == "POST")
            {

            }

            response.StatusCode = 404;
            response.OutputStream.Close();
        }
    }
}
