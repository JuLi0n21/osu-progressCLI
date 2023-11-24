using Fluid;
using osu_progressCLI.Datatypes;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace osu_progressCLI.Webserver.Server
{

    internal class SSEstream
    {
        private static List<HttpListenerResponse> SSEconnections = new List<HttpListenerResponse>();
        private static object lockObject = new object();
        private static FluidRenderer renderer = new FluidRenderer();


        public void Setup(HttpListenerRequest request, HttpListenerResponse response, FluidParser parser) {


            response.ContentType = "text/event-stream";
            response.Headers.Add("Cache-Control", "no-cache");
            response.Headers.Add("Connection", "keep-alive");


            lock (lockObject)
            {
                SSEconnections.Add(response);
                SendEvent("message", "RANDOM TEXT NON FORMATTED!");
            }
        }

        public static void SendEvent(string eventName, string eventData)
        {
            string eventMessage = $"event: {eventName}\ndata: {eventData}\n\n";
            byte[] buffer = Encoding.UTF8.GetBytes(eventMessage);

            lock (lockObject)
            {
                foreach (var client in SSEconnections)
                {
                    try
                    {
                        client.OutputStream.Write(buffer, 0, buffer.Length);
                        client.OutputStream.Flush();
                    }
                    catch (Exception e)  
                    {
                        Logger.Log(Logger.Severity.Warning, Logger.Framework.Server, e.Message);
                    }
                }
            }
        }

        public static void SendScore(Score score)
        {

            var template = FluidRenderer.templates.Find(item => item.Key.Equals("scoreoneliner.liquid"));

            var context = new TemplateContext(score);
            context.SetValue("score", score);

            string eventMessage = $"event: score\ndata: {template.Value.Render(context)}\n\n";
            
            byte[] buffer = Encoding.UTF8.GetBytes(eventMessage);

            lock (lockObject)
            {
                foreach (var client in SSEconnections)
                {
                    try
                    {
                        client.OutputStream.Write(buffer, 0, buffer.Length);
                        client.OutputStream.Flush();
                    }
                    catch (Exception e)
                    {
                        Logger.Log(Logger.Severity.Warning, Logger.Framework.Server, e.Message);
                    }
                }
            }
        }
    }
}
