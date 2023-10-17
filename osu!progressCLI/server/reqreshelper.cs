﻿using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using osu1progressbar.Game.Database;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace osu_progressCLI.server
{
    internal class reqreshelper
    {
        HttpListenerRequest request;
        HttpListenerResponse response;

        DatabaseController controller;

        public reqreshelper () {
            controller = new DatabaseController ();
        }

        //defautl page
        public void defaultpage(HttpListenerRequest request, HttpListenerResponse response) {

            WriteResponse(response, PageGenerator.Instance.generatepage(Credentials.Instance.GetConfig().userid, "osu"), "text/html");
            //ServeStaticFile(response, "server/html/index.html", "text/html");
        }

        public void getAllBeatmapScroes(HttpListenerRequest request, HttpListenerResponse response) {
            
            DateTime to = DateTime.Now;
            DateTime from = to.Subtract(TimeSpan.FromDays(30000)); //around 100 years

            string beatmapstring = GetBeatmapData(from, to);

            WriteResponse(response, beatmapstring, "application/json");
        }
        //get all beatmap scores

        //get beatmaps in timespan (from, to)
        public void getBeatmapsinTimeSpan(HttpListenerRequest request, HttpListenerResponse response, DateTime from, DateTime to) {

            string beatmapstring = GetBeatmapData(from, to);

            WriteResponse(response, beatmapstring, "application/json");
        }

        //time wasted 

        public void getAllTimeWasted(HttpListenerRequest request, HttpListenerResponse response)
        {
            DateTime to = DateTime.Now;
            DateTime from = to.Subtract(TimeSpan.FromDays(30000)); //around 100 years

            string beatmapstring = GetTimeWasted(from, to);

            WriteResponse(response, beatmapstring, "application/json");

        }

        public void getTimeWastedinTimeSpan(HttpListenerRequest request, HttpListenerResponse response,DateTime from, DateTime to) {
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

            string beatmapstring = GetBanchoTime(from, to);

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

        public void search(HttpListenerRequest request, HttpListenerResponse response, NameValueCollection parameters) {

            //Console.WriteLine("searchquery: " + parameters[0].ToString());
            DateTime to = DateTime.Now;
            DateTime from = to.Subtract(TimeSpan.FromDays(30000)); //around 100 years

           

            WriteResponse(response, System.Text.Json.JsonSerializer.Serialize(controller.GetScoreSearch(from, to, QueryParser.Filter(parameters[0].ToString()))) ,"application/json");
        }

        public void Simulateplay(HttpListenerRequest request, HttpListenerResponse response, NameValueCollection parameters) {

            Logger.Log(Logger.Severity.Info, Logger.Framework.Server, @$"Request PP calc for {parameters.Count}");
            if (parameters["Beatmapid"] != null) { 
            
            }
            
        }        

        public void user(HttpListenerRequest request, HttpListenerResponse response, NameValueCollection parameters) {

            //why does this shit take so long pls help
            Logger.Log(Logger.Severity.Error, Logger.Framework.Server, $@"{parameters["userid"]} - {parameters["mode"]}");
            WriteResponse(response, "shit", "application/json");
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
                    Logger.Log(Logger.Severity.Warning, Logger.Framework.Server, $"No Parameters for MissAnalyzer Request");

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
        

        public void getScore(HttpListenerRequest request, HttpListenerResponse response, NameValueCollection parameters) {

            WriteResponse(response, System.Text.Json.JsonSerializer.Serialize(controller.GetScore(int.Parse(parameters[0]))), "application/json");
        }

        public void getScoreAverages(HttpListenerRequest request, HttpListenerResponse response, NameValueCollection parameters) {
            DateTime to = DateTime.Now;
            DateTime from = to.Subtract(TimeSpan.FromDays(30000)); //around 100 years

            WriteResponse(response, GetScoreAverages(from, to), "application/json");
        }

        public void save(HttpListenerRequest request, HttpListenerResponse response) {

            string requestData = null;
            using (Stream body = request.InputStream)
            {
                StreamReader reader = new StreamReader(body);
                requestData = reader.ReadToEnd();
            }

            JObject parameters = JObject.Parse(requestData);

            Credentials.Instance.UpdateApiCredentials(parameters["clientId"].ToString(), parameters["clientSecret"].ToString());

            Credentials.Instance.UpdateConfig(parameters["localsettings"].ToString(), parameters["username"].ToString(), parameters["rank"].ToString(), parameters["country"].ToString(), parameters["coverUrl"].ToString(), parameters["avatarUrl"].ToString(), parameters["port"].ToString(), parameters["userid"].ToString());

            string message = "Saved";
            WriteResponse(response, message, "application/json");
        }

        private string GetBeatmapData(DateTime from, DateTime to)
        {

            //make database maybe single ton aswell or save accestoken somewhere instead of inside the object.
            // databaseController.GetScoresInTimeSpan(from, to);
            string jsondata = System.Text.Json.JsonSerializer.Serialize(controller.GetScoresInTimeSpan(from, to));
            return jsondata;

        }

        private string GetBanchoTime(DateTime from, DateTime to)
        {
            string jsondata = System.Text.Json.JsonSerializer.Serialize(controller.GetBanchoTime(from, to));
            return jsondata;
        }

        private string GetTimeWasted(DateTime from, DateTime to) {

            string jsondata = System.Text.Json.JsonSerializer.Serialize(controller.GetTimeWasted(from, to));
            return jsondata;

        }

        private string GetScoreAverages(DateTime from, DateTime to)
        {
            string jsondata = System.Text.Json.JsonSerializer.Serialize(controller.GetScoreAveragesbyDay(from, to));
            return jsondata;
        }

        private string GetScore(int id)
        {
            string jsondata = System.Text.Json.JsonSerializer.Serialize(controller.GetScore(id));
            return jsondata;
        }

        public void serveimage(HttpListenerRequest request, HttpListenerResponse response, string filepath) {

            //testign needed
            //Console.WriteLine("image/" + filepath.Split(".")[1]);
            ServeStaticImage(response, "public/img" + filepath);
        }

        public void servehtml(HttpListenerRequest request, HttpListenerResponse response, string filepath)
        {
            string htmlFilePath = "public/html" + filepath;

            ServeStaticFile(response, htmlFilePath, "text/html");
        }

        public void servejs(HttpListenerRequest request, HttpListenerResponse response, string filepath) { 
            
            string jsFilePath = "public/js" + filepath;
            
            ServeStaticFile(response, jsFilePath, "text/javascript");
        }

        public void servecss(HttpListenerRequest request, HttpListenerResponse response, string filepath)
        {

            string jsFilePath = "public/css" + filepath;

            ServeStaticFile(response, jsFilePath, "text/css");
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

            }
        }

    }
}
