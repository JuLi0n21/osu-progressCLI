using Fluid;
using osu1progressbar.Game.Database;
using System.Net;

namespace osu_progressCLI.Webserver.Server
{
    internal class RequestHandler
    {
        DatabaseController controller = new DatabaseController();
        public RequestHandler()
        {

        }

        public async void route(HttpListenerRequest request, HttpListenerResponse response, FluidParser parser)
        {
            string path = request.Url.AbsolutePath;
              
            if(path == "/")
            {
                var template = FluidRenderer.templates.Find(item => item.Key.Equals("Homepage.liquid"));

                await ApiController.Instance.getuser(Credentials.Instance.GetConfig().userid, Credentials.Instance.GetConfig().mode);
                WeekCompare week = controller.GetWeekCompare();

                string playtimethisweek = (week.ThisWeek / 3600).ToString().PadRight(5).Substring(0, 5);
                string diffrencetolastweek = ((week.ThisWeek - week.LastWeek) / week.LastWeek * 100).ToString().PadRight(6).Substring(0, 6);

                var context = new TemplateContext(week);
                context.SetValue("thisweek", playtimethisweek);
                context.SetValue("lastweek", diffrencetolastweek);
                context.SetValue( "week", week);
                context.SetValue("config", Credentials.Instance.GetConfig());
                context.SetValue( "List", controller.GetScoresInTimeSpan(DateTime.Now.AddDays(-100), DateTime.Now));

                Webserver.Instance().WriteResponse(response, template.Value.Render(context), "text/html");
            }

            response.StatusCode = 404;
            response.OutputStream.Close();
            return;
        }
    }
}
