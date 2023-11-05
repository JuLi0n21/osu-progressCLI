using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using osu1progressbar.Game.Database;
using System.Collections.Specialized;
using System.Net;
using System.Text;

namespace osu_progressCLI.server
{
    /// <summary>
    /// Model View Design (Partialy implementet).
    /// </summary>
    internal class Reqreshelper
    {
        DatabaseController controller;

        public Reqreshelper()
        {
            controller = new DatabaseController();
        }

        public void defaultpage(HttpListenerRequest request, HttpListenerResponse response)
        {
            WriteResponse(response, PageGenerator.Instance.generatepage(Credentials.Instance.GetConfig().userid, "osu", controller.GetWeekCompare()), "text/html");
        }

        public void getAllBeatmapScroes(HttpListenerRequest request, HttpListenerResponse response)
        {
            DateTime to = DateTime.Now;
            DateTime from = to.Subtract(TimeSpan.FromDays(30000)); //around 100 years

            string beatmapstring = GetBeatmapData(from, to);

            WriteResponse(response, beatmapstring, "application/json");
        }

        public void getBeatmapsinTimeSpan(HttpListenerRequest request, HttpListenerResponse response, DateTime from, DateTime to)
        {
            string beatmapstring = GetBeatmapData(from, to);

            WriteResponse(response, beatmapstring, "application/json");
        }

        public void getAllTimeWasted(HttpListenerRequest request, HttpListenerResponse response)
        {
            DateTime to = DateTime.Now;
            DateTime from = to.Subtract(TimeSpan.FromDays(30000)); //around 100 years

            string beatmapstring = GetTimeWasted(from, to);

            WriteResponse(response, beatmapstring, "application/json");
        }

        public void getTimeWastedinTimeSpan(HttpListenerRequest request, HttpListenerResponse response, DateTime from, DateTime to)
        {
            //create db query firts
        }

        public void getAllBanchoTime(HttpListenerRequest request, HttpListenerResponse response)
        {
            DateTime to = DateTime.Now;
            DateTime from = to.Subtract(TimeSpan.FromDays(30000)); //around 100 years

            string beatmapstring = GetBanchoTime(from, to);

            WriteResponse(response, beatmapstring, "application/json");
        }

        public void getBanchoTimebyday(HttpListenerRequest request, HttpListenerResponse response)
        {
            DateTime to = DateTime.Now;
            DateTime from = to.Subtract(TimeSpan.FromDays(30000)); //around 100 years

            WriteResponse(response, System.Text.Json.JsonSerializer.Serialize(controller.GetBanchoTimebyDay(from, to)), "application/json");
        }

        public void getTimeWastedbyday(HttpListenerRequest request, HttpListenerResponse response)
        {
            DateTime to = DateTime.Now;
            DateTime from = to.Subtract(TimeSpan.FromDays(30000)); //around 100 years

            WriteResponse(response, System.Text.Json.JsonSerializer.Serialize(controller.GetTimeWastedByDay(from, to)), "application/json");
        }

        public void getBanchoTimeinTineSpan(HttpListenerRequest request, HttpListenerResponse response, DateTime from, DateTime to)
        {
            //create db query firts
        }

        public void search(HttpListenerRequest request, HttpListenerResponse response, NameValueCollection parameters)
        {
            DateTime to = DateTime.Now;
            DateTime from = to.Subtract(TimeSpan.FromDays(30000)); //around 100 years

            WriteResponse(response, System.Text.Json.JsonSerializer.Serialize(controller.GetScoreSearch(from, to, QueryParser.Filter(parameters[0].ToString()))), "application/json");
        }

        public void Simulateplay(HttpListenerRequest request, HttpListenerResponse response, NameValueCollection parameters)
        {
            Logger.Log(Logger.Severity.Info, Logger.Framework.Server, @$"Request PP calc for {parameters.Count}");
            if (parameters["Beatmapid"] != null)
            {
                //implement
            }
        }

        public async void user(HttpListenerRequest request, HttpListenerResponse response, NameValueCollection parameters)
        {
            Logger.Log(Logger.Severity.Debug, Logger.Framework.Server, $@"{parameters["userid"]} - {DifficultyAttributes.ModeConverter(int.Parse(parameters["mode"]))}");
            WriteResponse(response, JsonConvert.SerializeObject(await ApiController.Instance.getuser(parameters["userid"], DifficultyAttributes.ModeConverter(int.Parse(parameters["mode"])))), "application/json");
        }

        public void run(HttpListenerRequest request, HttpListenerResponse response)
        {
            try
            {
                string requestData = null;
                using (Stream body = request.InputStream)
                {
                    StreamReader reader = new StreamReader(body);
                    requestData = reader.ReadToEnd();
                }

                JObject parameters = JObject.Parse(requestData);
                Logger.Log(Logger.Severity.Info, Logger.Framework.Server, $"{parameters}");

                if (parameters == null)
                {
                    Logger.Log(Logger.Severity.Warning, Logger.Framework.Server, $"No Parameters for Programm Request");

                    return;
                }

                if (parameters["programm"].ToString() == "OsuMissAnalyzer" && parameters["id"] != null)
                {
                    DifficultyAttributes.StartMissAnalyzer(int.Parse(parameters["id"].ToString()));
                }
            }
            catch (Exception e)
            {
                Logger.Log(Logger.Severity.Error, Logger.Framework.Server, $@"{e.Message}");
            }
        }

        public void getScore(HttpListenerRequest request, HttpListenerResponse response, NameValueCollection parameters)
        {
            WriteResponse(response, System.Text.Json.JsonSerializer.Serialize(controller.GetScore(int.Parse(parameters[0]))), "application/json");
        }

        public void getScoreAverages(HttpListenerRequest request, HttpListenerResponse response, NameValueCollection parameters)
        {
            DateTime to = DateTime.Now;
            DateTime from = to.Subtract(TimeSpan.FromDays(30000)); //around 100 years

            WriteResponse(response, GetScoreAverages(from, to), "application/json");
        }

        public void Save(HttpListenerRequest request, HttpListenerResponse response)
        {
            try
            {
                string requestData;
                using (Stream body = request.InputStream)
                using (StreamReader reader = new StreamReader(body))
                {
                    requestData = reader.ReadToEnd();
                }

                if (string.IsNullOrWhiteSpace(requestData))
                {
                    WriteResponse(response, "Request data is empty", "application/json", 400); // Bad Request
                    return;
                }

                if (!TryParseJson(requestData, out JObject parameters))
                {
                    WriteResponse(response, "Invalid JSON data", "application/json", 400); // Bad Request
                    return;
                }

                if (!TryUpdateConfig(parameters, out string errorMessage))
                {
                    WriteResponse(response, errorMessage, "application/json", 500); // Internal Server Error
                    return;
                }

                WriteResponse(response, " { \"message\":\"Settings Saved\"}", "application/json");
            }
            catch (Exception ex)
            {
                WriteResponse(response, ex.Message, "application/json", 500); // Internal Server Error
            }
        }

        private bool TryParseJson(string json, out JObject parsedJson)
        {
            try
            {
                parsedJson = JObject.Parse(json);
                return true;
            }
            catch (JsonReaderException)
            {
                parsedJson = null;
                return false;
            }
        }

        private bool TryUpdateConfig(JObject parameters, out string errorMessage)
        {
            try
            {
                Credentials.Instance.UpdateConfig(
                    parameters["osufolder"]?.ToString(),
                    parameters["songfolder"]?.ToString(),
                    parameters["localsettings"]?.ToString(),
                    parameters["username"]?.ToString(),
                    parameters["rank"]?.ToString(),
                    parameters["country"]?.ToString(),
                    parameters["coverUrl"]?.ToString(),
                    parameters["avatarUrl"]?.ToString(),
                    parameters["port"]?.ToString(),
                    parameters["userid"]?.ToString()
                );
                errorMessage = null;
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        private void WriteResponse(HttpListenerResponse response, string message, string contentType, int statusCode = 200)
        {
            response.StatusCode = statusCode;
            response.ContentType = contentType;

            using (var writer = new StreamWriter(response.OutputStream))
            {
                writer.Write(message);
            }
        }


        private string GetBeatmapData(DateTime from, DateTime to)
        {
           return System.Text.Json.JsonSerializer.Serialize(controller.GetScoresInTimeSpan(from, to));
        }

        private string GetBanchoTime(DateTime from, DateTime to)
        {
            return System.Text.Json.JsonSerializer.Serialize(controller.GetBanchoTime(from, to));
        }

        private string GetTimeWasted(DateTime from, DateTime to)
        {
            return System.Text.Json.JsonSerializer.Serialize(controller.GetTimeWasted(from, to));
        }

        private string GetScoreAverages(DateTime from, DateTime to)
        {
            return System.Text.Json.JsonSerializer.Serialize(controller.GetScoreAveragesbyDay(from, to));
        }

        private string GetScore(int id)
        {
           return System.Text.Json.JsonSerializer.Serialize(controller.GetScore(id));
        }

        public void serveimage(HttpListenerRequest request, HttpListenerResponse response, string filepath)
        {
            if (File.Exists("public/img" + filepath))
            {
                ServeStaticImage(response, "public/img" + filepath);
            }
            else if (File.Exists($"{Credentials.Instance.GetConfig().songfolder}{filepath}"))
            {
                ServeStaticImage(response, WebUtility.UrlDecode($@"{Credentials.Instance.GetConfig().songfolder}{filepath}"));
            }
        }

        public void servehtml(HttpListenerRequest request, HttpListenerResponse response, string filepath)
        {
            string htmlFilePath = "public/html" + filepath;

            ServeStaticFile(response, htmlFilePath, "text/html");
        }

        public void servejs(HttpListenerRequest request, HttpListenerResponse response, string filepath)
        {
            string jsFilePath = "public/js" + filepath;

            ServeStaticFile(response, jsFilePath, "text/javascript");
        }

        public void serveosr(HttpListenerRequest request, HttpListenerResponse response, string filepath)
        {
            string jsFilePath = $"{Credentials.Instance.GetConfig().osufolder}/Data/r/{filepath}";

            ServeStaticFile(response, jsFilePath, "application/osr");
        }

        public void servecss(HttpListenerRequest request, HttpListenerResponse response, string filepath)
        {
            string jsFilePath = "public/css" + filepath;

            ServeStaticFile(response, jsFilePath, "text/css");
        }

        public async Task<bool> upload(HttpListenerRequest request, HttpListenerResponse response)
        {
            try
            {
                string uploadDir = "imports";
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }

                string fileName = request.Headers["filename"];
                bool download = bool.Parse(request.Headers["download"]);
                bool import = bool.Parse(request.Headers["import"]);

                Console.WriteLine($"download: {download}, import: {import}");
                string filePath = Path.Combine(uploadDir, fileName);

                using (Stream inputStream = request.InputStream)
                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await inputStream.CopyToAsync(fileStream);
                }

                if (fileName.EndsWith(".csv"))
                    //    Task.Run(() => ScoreImporter.ImportScores(fileName));
                    Logger.Log(Logger.Severity.Info, Logger.Framework.Server, $"Received and saved: {fileName}");

                WriteResponse(response, "{\"message\":\"Upload Successful!\"}", "application/json");
                return true;
            }
            catch (Exception ex)
            {
                WriteResponse(response, $"{{ \"message\":\"{ex.Message}\"}}", "application/json");
                return false;
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

        static void ServeStaticImage(HttpListenerResponse response, string imagePath)
        {
            try
            {
                if (File.Exists(imagePath))
                {
                    string contentType = GetContentType(Path.GetExtension(imagePath));

                    byte[] imageBytes = File.ReadAllBytes(imagePath);
                    response.ContentType = contentType;
                    response.ContentLength64 = imageBytes.Length;
                    response.OutputStream.Write(imageBytes, 0, imageBytes.Length);
                    response.OutputStream.Close();
                }
                else
                {
                    response.StatusCode = 404;
                    string responseString = $"404 - Not Found: Image not found at {imagePath}";
                    WriteResponse(response, responseString, "text/plain");
                }
            }
            catch (Exception e)
            {
                Logger.Log(Logger.Severity.Error, Logger.Framework.Server, e.Message);
            }
        }

        static string GetContentType(string fileExtension)
        {
            switch (fileExtension.ToLower())
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".gif":
                    return "image/gif";
                case ".bmp":
                    return "image/bmp";
                case ".ico":
                    return "image/x-icon";
                default:
                    return "application/octet-stream";
            }
        }

        static void WriteResponse(HttpListenerResponse response, string responseString, string contentType)
        {
            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(responseString);

                response.ContentType = contentType;
                response.ContentLength64 = buffer.Length;

                Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
            }
            catch (Exception e)
            {
                Logger.Log(Logger.Severity.Error, Logger.Framework.Server, e.Message);
            }
        }

    }
}
