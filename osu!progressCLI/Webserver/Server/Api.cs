using System.Collections.Specialized;
using System.Net;
using System.Web;
using Fluid;
using Fluid.Ast;
using Newtonsoft.Json;
using osu1progressbar.Game.Database;
using osu_progressCLI.Datatypes;
using Parlot.Fluent;

namespace osu_progressCLI.Webserver.Server
{
    internal class Api
    {
        DatabaseController controller;

        public Api()
        {
            controller = new DatabaseController();
        }

        public async void Route(
            HttpListenerRequest request,
            HttpListenerResponse response,
            FluidParser parser
        )
        {
            try
            {
                //parsing parameters
                string path = request.Url.AbsolutePath;
                Logger.Log(Logger.Severity.Info, Logger.Framework.Server, path);

                var queryparams = HttpUtility.ParseQueryString(request.Url.Query);
                //Console.WriteLine(queryparams.ToString());
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
                    if (
                        !DateTime.TryParse(queryparams["to"].ToString(), out to)
                        || !DateTime.TryParse(queryparams["from"].ToString(), out from)
                    )
                        ;
                }

                if (!String.IsNullOrEmpty(queryparams["Beatmapid"]))
                {
                    queryparams["query"] += $" BeatmapId=={queryparams["Beatmapid"]}";
                }

                if (!String.IsNullOrEmpty(queryparams["Osufilename"]))
                {
                    queryparams["query"] += $" {queryparams["Osufilename"]}";
                }

                //routing
                if (path == "/api/beatmaps" && request.HttpMethod == "GET")
                {
                    var template = FluidRenderer.templates.Find(item =>
                        item.Key.Equals("Scores.liquid")
                    );

                    List<Score> scores = new();
                    var context = new TemplateContext(scores);

                    if (
                        !String.IsNullOrEmpty(queryparams["offset"])
                        && !String.IsNullOrEmpty(queryparams["limit"])
                    )
                    {
                        scores = controller.GetScoresInTimeSpan(
                            from,
                            to,
                            int.Parse(queryparams["limit"]),
                            int.Parse(queryparams["offset"])
                        );
                        context.SetValue("limit", int.Parse(queryparams["limit"]));

                        if (scores.Count == int.Parse(queryparams["limit"]))
                        {
                            context.SetValue("limit", int.Parse(queryparams["limit"]));
                            context.SetValue(
                                "offset",
                                int.Parse(queryparams["offset"]) + int.Parse(queryparams["limit"])
                            );
                        }
                    }
                    else
                    {
                        scores = controller.GetScoresInTimeSpan(from, to);
                    }

                    context.SetValue("List", scores);
                    try
                    {
                        Webserver
                            .Instance()
                            .WriteResponse(response, template.Value.Render(context), "text/html");
                    }
                    catch (Exception ex)
                    {
                        Webserver
                            .Instance()
                            .WriteResponse(
                                response,
                                "Something went wrong" + ex.Message,
                                "text/html"
                            );
                    }
                    return;
                }
                else if (path == "/api/beatmaps/search" && request.HttpMethod == "GET")
                {
                    if (request.Headers["HX-Request"] != null)
                    {
                        List<Score> scores = new();

                        var template = FluidRenderer.templates.Find(item =>
                            item.Key.Equals("Scores.liquid")
                        );

                        var context = new TemplateContext(scores);

                        if (!String.IsNullOrEmpty(queryparams["Osufilename"]))
                        {
                            scores = controller.GetScoreSearch(
                                from,
                                to,
                                QueryParser.Filter(
                                    queryparams["query"].ToString(),
                                    queryparams["Osufilename"].ToString()
                                )
                            );
                        }
                        else
                        {
                            if (
                                !String.IsNullOrEmpty(queryparams["offset"])
                                && !String.IsNullOrEmpty(queryparams["limit"])
                            )
                            {
                                scores = controller.GetScoreSearch(
                                    from,
                                    to,
                                    QueryParser.Filter(queryparams["query"].ToString()),
                                    int.Parse(queryparams["limit"]),
                                    int.Parse(queryparams["offset"])
                                );

                                if (scores.Count == int.Parse(queryparams["limit"]))
                                {
                                    context.SetValue("limit", int.Parse(queryparams["limit"]));
                                    context.SetValue(
                                        "offset",
                                        int.Parse(queryparams["offset"])
                                            + int.Parse(queryparams["limit"])
                                    );
                                    context.SetValue("query", queryparams["query"].ToString());
                                    context.SetValue("form", queryparams["from"]?.ToString());
                                    context.SetValue("to", queryparams["to"].ToString());
                                }
                            }
                            else
                            {
                                scores = controller.GetScoreSearch(
                                    from,
                                    to,
                                    QueryParser.Filter(queryparams["query"].ToString())
                                );
                            }
                        }

                        context.SetValue("List", scores);
                        Webserver
                            .Instance()
                            .WriteResponse(response, template.Value.Render(context), "text/html");
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(queryparams["Osufilename"]))
                        {
                            Webserver
                                .Instance()
                                .WriteResponse(
                                    response,
                                    System.Text.Json.JsonSerializer.Serialize(
                                        controller.GetScoreSearch(
                                            from,
                                            to,
                                            QueryParser.Filter(
                                                queryparams["query"].ToString(),
                                                queryparams["Osufilename"].ToString()
                                            )
                                        )
                                    ),
                                    "application/json"
                                );
                        }
                        else
                        {
                            Webserver
                                .Instance()
                                .WriteResponse(
                                    response,
                                    System.Text.Json.JsonSerializer.Serialize(
                                        controller.GetScoreSearch(
                                            from,
                                            to,
                                            QueryParser.Filter(queryparams["query"].ToString())
                                        )
                                    ),
                                    "application/json"
                                );
                        }
                    }
                    return;
                }
                else if (path == "/api/beatmaps/averages" && request.HttpMethod == "GET")
                {
                    Webserver
                        .Instance()
                        .WriteResponse(
                            response,
                            System.Text.Json.JsonSerializer.Serialize(
                                controller.GetScoreAveragesbyDay(from, to)
                            ),
                            "application/json"
                        );
                }
                else if (path == "/api/banchotime" && request.HttpMethod == "GET")
                {
                    Webserver
                        .Instance()
                        .WriteResponse(
                            response,
                            System.Text.Json.JsonSerializer.Serialize(
                                controller.GetBanchoTime(from, to)
                            ),
                            "application/json"
                        );
                }
                else if (path == "/api/banchotimebyday" && request.HttpMethod == "GET")
                {
                    Webserver
                        .Instance()
                        .WriteResponse(
                            response,
                            System.Text.Json.JsonSerializer.Serialize(
                                controller.GetBanchoTimebyDay(from, to)
                            ),
                            "application/json"
                        );
                }
                else if (path == "/api/timewasted" && request.HttpMethod == "GET")
                {
                    Webserver
                        .Instance()
                        .WriteResponse(
                            response,
                            System.Text.Json.JsonSerializer.Serialize(
                                controller.GetTimeWasted(from, to)
                            ),
                            "application/json"
                        );
                }
                else if (path == "/api/timewastedbyday" && request.HttpMethod == "GET")
                {
                    Webserver
                        .Instance()
                        .WriteResponse(
                            response,
                            System.Text.Json.JsonSerializer.Serialize(
                                controller.GetTimeWastedByDay(from, to)
                            ),
                            "application/json"
                        );
                }
                else if (path == "/api/user" && request.HttpMethod == "GET")
                {
                    Webserver
                        .Instance()
                        .WriteResponse(
                            response,
                            System.Text.Json.JsonSerializer.Serialize(
                                await ApiController.Instance.getuser(
                                    queryparams["userid"],
                                    queryparams["mode"]
                                )
                            ),
                            "application/json"
                        );
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
                        Webserver
                            .Instance()
                            .WriteResponse(
                                response,
                                "<span class=\"text-red-500 hide-me\">Something Went Wrong!</span>",
                                "text/html"
                            );
                        return;
                    }

                    Webserver
                        .Instance()
                        .WriteResponse(
                            response,
                            "<span class=\"text-green-600 hide-me\">Settings Updated!</span>",
                            "text/html"
                        );
                }
                else if (path == "/api/run" && request.HttpMethod == "POST")
                {
                    DifficultyAttributes.StartMissAnalyzer(int.Parse(queryparams["id"].ToString()));
                }
                else if (path == "/api/uploadstatus" && request.HttpMethod == "POST")
                {
                    bool allAreFalse = ScoreImporter
                        .Instance.getScoreFileTracker()
                        .All(obj => obj.index >= obj.amountoffscores);

                    if (allAreFalse)
                    {
                        Webserver
                            .Instance()
                            .WriteResponse(
                                response,
                                "<span> All Scores Imported </span>",
                                "text/html"
                            );
                    }
                    else
                    {
                        string output = $"<div>";

                        if (ScoreImporter.Instance.getScoreFileTracker() != null)
                        {
                            foreach (
                                ScoreFileTracker list in ScoreImporter.Instance.getScoreFileTracker()
                            )
                            {
                                int percentage = (list.index + 1) * 100 / list.amountoffscores;
                                output +=
                                    $"<div class=\" text--pink m-2 flex justify-between\">"
                                    + $"<p>{Path.GetFileName(list.filename)}</p> <p>{list.index}|{list.amountoffscores}</p></div>"
                                    + $"<div class=\"bg-pink-900 border\" style=\"width:{percentage}%\">{percentage}%</div>"
                                    + $"</div>";
                            }
                            output += "</div>";

                            Webserver.Instance().WriteResponse(response, output, "text/html");
                        }
                    }
                }
                else if (path == "/api/import" && request.HttpMethod == "POST")
                {
                    Webserver
                        .Instance()
                        .WriteResponse(
                            response,
                            "<span  class=\" text--pink text-3xl \"> Folder to Put in the score.db or score.csv file should open!</span>",
                            "text/html"
                        );
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
                        Webserver
                            .Instance()
                            .WriteResponse(response, $"<span> {e.Message} </span>", "text/html");
                    }
                }
                else if (path == "/api/startimport" && request.HttpMethod == "POST")
                {
                    string[] files = Directory.GetFiles(ScoreImporter.IMPORT_LOCATION);

                    if (files.Length > 0)
                    {
                        Webserver
                            .Instance()
                            .WriteResponse(
                                response,
                                $"<span class=\" text--pink text-3xl hide-me\"> Starting Scoreimporter it will Take a few Seconds to Update! </span>",
                                "text/html"
                            );
                        Task.Run(() => ScoreImporter.Instance.StartImporting());
                        return;
                    }
                    Webserver
                        .Instance()
                        .WriteResponse(
                            response,
                            $"<span class=\" text-red-600 text-3xl hide-me\"> Please Add Scores to Import </span>",
                            "text/html"
                        );
                    return;
                }
                else if (path == "/api/callback" && request.HttpMethod == "GET")
                {
                    if (!String.IsNullOrEmpty(queryparams["access_token"]))
                    {
                        Credentials.Instance.SetAccessToken(queryparams["access_token"]);

                        Credentials.Instance.GetLoginHelper().access_token = queryparams[
                            "access_token"
                        ];
                        Credentials.Instance.GetLoginHelper().expires_in = int.Parse(
                            queryparams["expires_in"]
                        );
                        Credentials.Instance.GetLoginHelper().refresh_token = queryparams[
                            "refresh_token"
                        ];
                        Credentials.Instance.GetLoginHelper().CreatedAt = DateTime.Now;

                        try
                        {
                            await File.WriteAllTextAsync(
                                Credentials.loginwithosuFilePath,
                                JsonConvert.SerializeObject(
                                    Credentials.Instance.GetLoginHelper(),
                                    Formatting.Indented
                                )
                            );
                        }
                        catch (Exception e)
                        {
                            Logger.Log(Logger.Severity.Error, Logger.Framework.Misc, e.Message);
                        }
                    }

                    Webserver.Instance().Redirect(response, $"/");
                    return;
                }
                else if (path == "/api/potentialfcs")
                {
                    List<Score> scores = null;

                    scores = controller.GetPotentcialtopplays(
                        await ApiController.Instance.getppcutoffpoint()
                    );

                    var template = FluidRenderer.templates.Find(item =>
                        item.Key.Equals("Scores.liquid")
                    );

                    var context = new TemplateContext(scores);
                    context.SetValue("List", scores);
                    Webserver
                        .Instance()
                        .WriteResponse(response, template.Value.Render(context), "text/html");
                }
            }
            catch (Exception e) { }
            finally
            {
                response.StatusCode = 404;
                response.OutputStream.Close();
            }
        }
    }
}
