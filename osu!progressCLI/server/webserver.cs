﻿using Newtonsoft.Json;
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
using Fleck;
using System.Net.WebSockets;
using System.Collections.Concurrent;


//add button to get credentials
namespace osu_progressCLI.server
{
    public sealed class Webserver
    {
        private static Webserver instance;

        private HttpListener listener;
        private readonly object lockObject = new object();
        private DateTime lastMessageSentTime = DateTime.MinValue;

        private reqreshelper helper;
        private ConcurrentBag<WebSocket> connectedSockets = new ConcurrentBag<WebSocket>();

        private string ip = "127.0.0.1";
        private string port = Credentials.Instance.GetConfig().port;
        private Webserver()
        {
            listener = new HttpListener();
            listener.Prefixes.Add($"http://{ip}:{port}/");
            listener.IgnoreWriteExceptions = true;
            listener.Start();
            Logger.Log(Logger.Severity.Debug, Logger.Framework.Server, $"Starting Weberver on localhost:{port}/");

            Console.WriteLine($"You can view ur Stats on localhost:{port}/");
            helper = new reqreshelper();

        }

        public static Webserver Instance()
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
            if (context.Request.IsWebSocketRequest)
            {
                WebSocketRequest(context);
            }

            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            string path = request.Url.AbsolutePath;
            NameValueCollection queryparams = HttpUtility.ParseQueryString(request.Url.Query);

            if (path.EndsWith(".jpg") || path.EndsWith(".jpeg") || path.EndsWith(".png") || path.EndsWith(".gif") || path.EndsWith(".bmp") || path.EndsWith(".tiff") || path.EndsWith(".ico") || path.EndsWith(".webp") || path.EndsWith(".svg"))
            {
                Logger.Log(Logger.Severity.Debug, Logger.Framework.Server, $"Serving {path}");

                helper.serveimage(request, response, path);
                return;
            }

            if (path.EndsWith(".html"))
            {
                Logger.Log(Logger.Severity.Debug, Logger.Framework.Server, $"Serving {path}");
                helper.servehtml(request, response, path);
                return;
            }

            if (path.EndsWith(".js"))
            {
                Logger.Log(Logger.Severity.Debug, Logger.Framework.Server, $"Serving {path}");

                helper.servejs(request, response, path);
                return;
            }

            if (path.EndsWith(".css"))
            {
                Logger.Log(Logger.Severity.Debug, Logger.Framework.Server, $"Serving {path}");

                helper.servecss(request, response, path);
                return;
            }

            //default page
            if (path == "/")
            {
                Logger.Log(Logger.Severity.Debug, Logger.Framework.Server, $"/ call");
                helper.defaultpage(request, response);
            } //get all beatmaps
            else if (path == "/api/beatmaps" && request.HttpMethod == "GET")
            {
                Logger.Log(Logger.Severity.Debug, Logger.Framework.Server, $"/api/beatmaps call");
                helper.getAllBeatmapScroes(request, response);
            }
            else if (path == "/api/beatmapsintimespan" && queryparams["timespan"] != null && request.HttpMethod == "GET")
            {
                Logger.Log(Logger.Severity.Debug, Logger.Framework.Server, $"/api/beatmapsintimespan call with {queryparams["timespan"]}");
                //convert timespan 
                DateTime from = DateTime.Now;
                DateTime to = DateTime.Now;
                helper.getBeatmapsinTimeSpan(request, response, from, to);
            }
            else if (path == "/api/beatmaps/search" && queryparams["searchquery"] != null) {

                Logger.Log(Logger.Severity.Debug, Logger.Framework.Server, $"/api/beatmaps/search call with {queryparams["timespan"]}");
                helper.search(request, response, queryparams);
            }
            else if (path == "/api/beatmaps/averages")
            {
                Logger.Log(Logger.Severity.Debug, Logger.Framework.Server, $"/api/beatmaps/averages call");
                helper.getScoreAverages(request, response, queryparams);
            }
            else if (path == "/api/beatmaps/score" && queryparams["id"] != null)
            {
                Logger.Log(Logger.Severity.Info, Logger.Framework.Server, $"Score Requested {queryparams["id"]}");
                helper.getScore(request, response, queryparams);
            }
            else if (path == "/api/beatmaps/search" && queryparams["searchquery"] != null)
            {
                Logger.Log(Logger.Severity.Debug, Logger.Framework.Server, $"/api/beatmaps/search call with {queryparams["searchquery"]}");
                //convert timespan 
                DateTime from = DateTime.Now;
                DateTime to = DateTime.Now;
                helper.getBanchoTimeinTineSpan(request, response, from, to);
            }
            else if (path == "/api/banchotime" && request.HttpMethod == "GET")
            {
                Logger.Log(Logger.Severity.Debug, Logger.Framework.Server, $"/api/banchotime call");
                helper.getAllBanchoTime(request, response);
            }
            else if (path == "/api/banchotimebyday" && request.HttpMethod == "GET")
            {
                Logger.Log(Logger.Severity.Debug, Logger.Framework.Server, $"/api/banchotimebyday call");
                helper.getBanchoTimebyday(request, response);
            }
            else if (path == "/api/timewasted" && request.HttpMethod == "GET")
            {
                Logger.Log(Logger.Severity.Debug, Logger.Framework.Server, $"/api/timewasted call");
                helper.getAllTimeWasted(request, response);
            }
            else if (path == "/api/timewastedbyday" && request.HttpMethod == "GET")
            {
                Logger.Log(Logger.Severity.Debug, Logger.Framework.Server, $"/api/timewastedbyday call");
                helper.getTimeWastedbyday(request, response);
            }
            else if (path == "/api/user" && queryparams["userid"] != null && queryparams["mode"] != null && request.HttpMethod == "GET")
            {
                Logger.Log(Logger.Severity.Debug, Logger.Framework.Server, $"/api/user call");
                helper.user(request, response, queryparams);
            }
            else if (path == "/api/save" && request.HttpMethod == "POST")
            {
                Logger.Log(Logger.Severity.Debug, Logger.Framework.Server, $"/api/save call");
                helper.save(request, response);
            }
            else if (path == "/api/run" && request.HttpMethod == "POST")
            {
                Logger.Log(Logger.Severity.Debug, Logger.Framework.Server, $"/api/run call");
                helper.run(request, response);
            }
            else
            {
                response.StatusCode = 404;
                response.OutputStream.Close();

                Logger.Log(Logger.Severity.Warning, Logger.Framework.Server, $"Not found: {path}. (Can be Ignored if everything works fine!)");
            }

        }
        private async Task WebSocketRequest(HttpListenerContext context)
        {
            HttpListenerWebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null);

            WebSocket webSocket = webSocketContext.WebSocket;
            connectedSockets.Add(webSocket);
            try
            {
                byte[] buffer = new byte[1024];

                while (webSocket.State == WebSocketState.Open)
                {
                    var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                        connectedSockets.TryTake(out webSocket);
                    }
                    else
                    {
                        // Handle received data here
                        string receivedMessage = System.Text.Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
                        Console.WriteLine($"Received: {receivedMessage}");

                        // Send a response (you can customize this part)
                        string responseMessage = $"Received: {receivedMessage}";
                        byte[] responseBytes = System.Text.Encoding.UTF8.GetBytes(responseMessage);
                        await webSocket.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(Logger.Severity.Error, Logger.Framework.Server, $"WebSocket error: {ex.Message}");
            }
            finally
            {
                connectedSockets.TryTake(out webSocket);
            }

        }

        public async Task SendData(string type, object data)
        {
            lock (lockObject)
            {
                if (DateTime.UtcNow - lastMessageSentTime < TimeSpan.FromSeconds(0.4))
                {
                    // Less than one second has passed since the last message was sent
                    return; // Skip sending this message
                }

                // Update the last message sent time
                lastMessageSentTime = DateTime.UtcNow;
            }

            Logger.Log(Logger.Severity.Debug, Logger.Framework.Server, $"Send Websocket Data of Type: {type}");

            var wrapper = new
            {
                Type = type,
                Data = data
            };

            byte[] dataBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(wrapper));

            foreach (var socket in connectedSockets)
            {
                if (socket.State == WebSocketState.Open)
                {
                    await socket.SendAsync(new ArraySegment<byte>(dataBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                }
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
