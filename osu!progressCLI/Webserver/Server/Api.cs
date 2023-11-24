using Fluid;
using osu_progressCLI.Datatypes;
using osu1progressbar.Game.Database;
using System.Collections.Specialized;
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
            try
            {
                //parsing parameters
                string path = request.Url.AbsolutePath;
                Console.WriteLine(path);

                var queryparams = HttpUtility.ParseQueryString(request.Url.Query);

                string requestData;
                using (Stream bodystream = request.InputStream)
                using (StreamReader reader = new StreamReader(bodystream))
                {
                    requestData = reader.ReadToEnd();
                }
                NameValueCollection PostData = HttpUtility.ParseQueryString(requestData);

                foreach (string key in PostData.AllKeys)
                {
                    string value = PostData[key];
                    Console.WriteLine($"Key: {key}, Value: {value}");
                }

                DateTime to = DateTime.Now;
                DateTime from = to.Subtract(TimeSpan.FromDays(30000)); //around 100 years

                if (queryparams["from"] != null && queryparams["to"] != null)
                {
                    if (!DateTime.TryParse(queryparams["to"].ToString(), out to) || !DateTime.TryParse(queryparams["from"].ToString(), out from)) ;

                }

                if (!queryparams["Beatmapid"].IsNullOrEmpty())
                {
                    queryparams["query"] += $" BeatmapId=={queryparams["Beatmapid"]}";
                    Console.WriteLine(queryparams["query"]);
                }

                //routing
                if (path == "/api/beatmaps" && request.HttpMethod == "GET")
                {

                    List<Score> scores = controller.GetScoresInTimeSpan(from, to);
                    var template = FluidRenderer.templates.Find(item => item.Key.Equals("Scores.liquid"));

                    var context = new TemplateContext(scores);
                    context.SetValue("List", scores);
                    try
                    {
                        Webserver.Instance().WriteResponse(response, template.Value.Render(context), "text/html");
                    }
                    catch (Exception ex)
                    {

                        Webserver.Instance().WriteResponse(response, "Something went wrong" + ex.Message, "text/html");
                    }
                    return;
                }
                else if (path == "/api/beatmaps/search" && request.HttpMethod == "GET")
                {
                    if (request.Headers["HX-Request"] != null)
                    {
                        List<Score> scores = controller.GetScoreSearch(from, to, QueryParser.Filter(queryparams["query"].ToString()));
                        Console.WriteLine(scores.Count);
                        var template = FluidRenderer.templates.Find(item => item.Key.Equals("Scores.liquid"));

                        var context = new TemplateContext(scores);
                        context.SetValue("List", scores);
                        Webserver.Instance().WriteResponse(response, template.Value.Render(context), "text/html");
                    }
                    else
                    {
                        Webserver.Instance().WriteResponse(response, System.Text.Json.JsonSerializer.Serialize(controller.GetScoreSearch(from, to, QueryParser.Filter(queryparams["query"].ToString()))), "application/json");
                    }
                    return;
                }
                else if (path == "/api/beatmaps/averages" && request.HttpMethod == "GET")
                {
                    Webserver.Instance().WriteResponse(response, System.Text.Json.JsonSerializer.Serialize(controller.GetScoreAveragesbyDay(from, to)), "application/json");
                }
                else if (path == "/api/banchotime" && request.HttpMethod == "GET")
                {
                    Webserver.Instance().WriteResponse(response, System.Text.Json.JsonSerializer.Serialize(controller.GetBanchoTime(from, to)), "application/json");
                }
                else if (path == "/api/banchotimebyday" && request.HttpMethod == "GET")
                {
                    Webserver.Instance().WriteResponse(response, System.Text.Json.JsonSerializer.Serialize(controller.GetBanchoTimebyDay(from, to)), "application/json");
                }
                else if (path == "/api/timewasted" && request.HttpMethod == "GET")
                {
                    Webserver.Instance().WriteResponse(response, System.Text.Json.JsonSerializer.Serialize(controller.GetTimeWasted(from, to)), "application/json");
                }
                else if (path == "/api/timewastedbyday" && request.HttpMethod == "GET")
                {
                    Webserver.Instance().WriteResponse(response, System.Text.Json.JsonSerializer.Serialize(controller.GetTimeWastedByDay(from, to)), "application/json");
                }
                else if (path == "/api/user" && request.HttpMethod == "GET")
                {
                    Webserver.Instance().WriteResponse(response, System.Text.Json.JsonSerializer.Serialize(await ApiController.Instance.getuser(queryparams["userid"], queryparams["mode"])), "application/json");
                }
                else if (path == "/api/save" && request.HttpMethod == "POST")
                {
                    try
                    {
                        Credentials.Instance.UpdateConfig(
                            osufolder: PostData["osufolder"],
                            songfolder: PostData["songfolder"],
                            localconfig: PostData["localconfig"],
                            username: PostData["username"],
                            userid: PostData["userid"],
                            rank: PostData["rank"],
                            countryrank: PostData["countryrank"],
                            country: PostData["country"],
                            countrycode: PostData["countrycode"],
                            avatar_url: PostData["avatarurl"],
                            cover_url: PostData["coverurl"],
                            port: PostData["port"],
                            mode: PostData["mode"]
                        );
                    }
                    catch
                    {
                        Webserver.Instance().WriteResponse(response, "<span class=\"text-red-500 hide-me\">Something Went Wrong!</span>", "text/html");
                        return;
                    }

                    Webserver.Instance().WriteResponse(response, "<span class=\"text-green-600 hide-me\">Settings Updated!</span>", "text/html");

                }
                else if (path == "/api/run" && request.HttpMethod == "POST")
                {

                }
                else if (path == "/api/uploadstatus" && request.HttpMethod == "POST")
                {
                    bool allAreFalse = ScoreImporter.Instance.getScoreFileTracker().All(obj => obj.running == false);

                    if (allAreFalse)
                    {
                        Webserver.Instance().WriteResponse(response, "<span> All Scores Imported </span>", "text/html");
                    }
                    else
                    {

                        string output = $"<div>";

                        if (ScoreImporter.Instance.getScoreFileTracker() != null)
                        {
                            foreach (ScoreFileTracker list in ScoreImporter.Instance.getScoreFileTracker())
                            {
                                int percentage = (list.index + 1) * 100 / list.amountoffscores;
                                output += $"<div class=\" text--pink m-2 flex justify-between\">" +
                                    $"<p>{Path.GetFileName(list.filename)}</p> <p>{list.index + 1}|{list.amountoffscores}</p></div>" +
                                    $"<div class=\"bg-pink-900 border\" style=\"width:{percentage}%\">{percentage}%</div>" +
                                    $"</div>";
                            }
                            output += "</div>";

                            Webserver.Instance().WriteResponse(response, output, "text/html");
                        }
                    }

                }
                else if (path == "/api/import" && request.HttpMethod == "POST")
                {

                    Webserver.Instance().WriteResponse(response, "<span  class=\" text--pink text-3xl \"> Folder to Put in the score.db or score.csv file should open!</span>", "text/html");
                    DifficultyAttributes.Startshell(@"start explorer.exe Importer\imports");
                    return;
                }
                else if (path == "/api/importfiles" && request.HttpMethod == "POST")
                {

                    try
                    {
                        string output = "<div class=\"flex flex-col\">";
                        string[] files = Directory.GetFiles("Importer/imports");

                        for (int i = 0; i < files.Length; i++)
                        {
                            output += $"<span>{Path.GetFileName(files[i])}</span>";
                        }
                        output += "</div>";

                        Webserver.Instance().WriteResponse(response, output, "text/html");
                    }
                    catch (Exception e)
                    {
                        Webserver.Instance().WriteResponse(response, $"<span> {e.Message} </span>", "text/html");
                    }
                }
                else if (path == "/api/startimport" && request.HttpMethod == "POST")
                {
                    string[] files = Directory.GetFiles("imports");

                    for (int i = 0; i < files.Length; i++)
                    {
                        if (File.Exists("Importer/imports/osu!.db"))
                        {
                            File.Move("Importer/imports/osu!.db", "Importer/cache/osu!.db");
                        }
                        else if (File.Exists("Importer/cache/osu!.db"))
                        {
                            //osudb exists dont do anything
                        }
                        else
                        {
                            Webserver.Instance().WriteResponse(response, $"<span class=\" text-red-600 text-3xl hide-me\"> Add a Copy off your osu!.db into the Folder, scores will not be imported otherwise! </span>", "text/html");
                            return;
                        }
                    }

                    if (files.Length > 0)
                    {
                        Webserver.Instance().WriteResponse(response, $"<span class=\" text--pink text-3xl hide-me\"> Starting Scoreimporter it will Take a few Seconds to Update! </span>", "text/html");
                        Task.Run(() => ScoreImporter.Instance.StartImporting());
                        return;
                    }
                    Webserver.Instance().WriteResponse(response, $"<span class=\" text-red-600 text-3xl hide-me\"> Please Add Scores to Import </span>", "text/html");
                    return;
                }

            }
            catch (Exception e) { 
                
            }
            finally
            {
                response.StatusCode = 404;
                response.OutputStream.Close();
            }
        }
    }
}
