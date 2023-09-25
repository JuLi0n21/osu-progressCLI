﻿using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using osu1progressbar.Game.Database;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Metrics;
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
            Console.WriteLine("beatmaps send?");
        }
        //get all beatmap scores

        //get beatmaps in timespan (from, to)
        public void getBeatmapsinTimeSpan(HttpListenerRequest request, HttpListenerResponse response, DateTime from, DateTime to) {

            string beatmapstring = GetBeatmapData(from, to);

            WriteResponse(response, beatmapstring, "application/json");
            Console.WriteLine("all beatmaps send?");
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
        public void getBanchoTimeinTineSpan(HttpListenerRequest request, HttpListenerResponse response, DateTime from, DateTime to)
        {
            //create db query firts
        }

        public void search(HttpListenerRequest request, HttpListenerResponse response, NameValueCollection parameters) {


            //create db query frisrt
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
            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(responseString);

                response.ContentType = contentType;
                response.ContentLength64 = buffer.Length;

                Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
            }
            catch (Exception e) {
            
            }
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

    }
}