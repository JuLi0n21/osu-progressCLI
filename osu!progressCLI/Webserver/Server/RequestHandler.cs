using System.Net;
using System.Web;
using Fluid;
using osu1progressbar.Game.Database;
using osu_progressCLI.Datatypes;

namespace osu_progressCLI.Webserver.Server
{
    internal class RequestHandler
    {
        DatabaseController controller = new DatabaseController();

        public RequestHandler() { }

        public async void route(
            HttpListenerRequest request,
            HttpListenerResponse response,
            FluidParser parser
        )
        {
            string path = request.Url.AbsolutePath;
            var queryparams = HttpUtility.ParseQueryString(request.Url.Query);

            //Logger.Log(Logger.Severity.Info, Logger.Framework.Server, $"Requsted: {path}");

            if (path == "/")
            {
                var template = FluidRenderer.templates.Find(item =>
                    item.Key.Equals("Homepage.liquid")
                );

                await ApiController.Instance.getuser(
                    Credentials.Instance.GetConfig().userid,
                    Credentials.Instance.GetConfig().mode
                );
                WeekCompare week = controller.GetWeekCompare();

                string playtimethisweek = (week.ThisWeek / 3600)
                    .ToString()
                    .PadRight(5)
                    .Substring(0, 5);
                string diffrencetolastweek = ((week.ThisWeek - week.LastWeek) / week.LastWeek * 100)
                    .ToString()
                    .PadRight(6)
                    .Substring(0, 6);

                var context = new TemplateContext(week);

                context.SetValue("isAuth", !String.IsNullOrEmpty(Credentials.Instance.GetAccessToken()));
                context.SetValue("count", controller.scorecount());
                context.SetValue("thisweek", playtimethisweek);
                context.SetValue("lastweek", diffrencetolastweek);
                context.SetValue("week", week);
                context.SetValue("config", Credentials.Instance.GetConfig());
                context.SetValue(
                    "List",
                    controller.GetScoresInTimeSpan(DateTime.Now.AddDays(-100), DateTime.Now)
                );

                Webserver
                    .Instance()
                    .WriteResponse(response, template.Value.Render(context), "text/html");
            }
            else if (path == "/score")
            {
                var template = FluidRenderer.templates.Find(item =>
                    item.Key.Equals("Scorepage.liquid")
                );

                Score score = controller.GetScore(int.Parse(queryparams["id"]));

                if (score != null)
                {
                    var context = new TemplateContext(score);
                    context.SetValue("score", score);
                    context.SetValue(
                        "player",
                        await ApiController.Instance.getuser(
                            Credentials.Instance.GetConfig().userid,
                            Credentials.Instance.GetConfig().mode
                        )
                    );

                    Webserver
                        .Instance()
                        .WriteResponse(response, template.Value.Render(context), "text/html");
                }
            }
            else if (path == "/importer")
            {
                var template = FluidRenderer.templates.Find(item =>
                    item.Key.Equals("Importer.liquid")
                );

                var context = new TemplateContext();

                Webserver
                    .Instance()
                    .WriteResponse(response, template.Value.Render(context), "text/html");
            }

            response.StatusCode = 404;
            response.OutputStream.Close();
            return;
        }
    }
}
