using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace osu_progressCLI.server
{
    internal class OsuWebSocket
    {
        private static OsuWebSocket instance;
        private readonly List<WebSocket> connections;
        private HttpListener listener;

        private OsuWebSocket()
        {
            connections = new List<WebSocket>();

            Task.Run(() => StartWebSocketServer());
        }

        private async Task StartWebSocketServer()
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add($"http://127.0.0.1:{Credentials.Instance.GetConfig().port}/");
            listener.Start();

            while (true)
            {
                try
                {
                    var context = await listener.GetContextAsync();
                    if (context.Request.IsWebSocketRequest)
                    {
                        WebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null);
                        WebSocket webSocket = webSocketContext.WebSocket;
                        Console.WriteLine($"WebSocket connection open: {webSocket.GetHashCode()}");
                        connections.Add(webSocket);

                        await HandleWebSocket(webSocket);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error accepting WebSocket client: {ex.Message}");
                }
            }
        }

        private async Task HandleWebSocket(WebSocket webSocket)
        {
            byte[] buffer = new byte[1024];

            while (webSocket.State == WebSocketState.Open)
            {
                try
                {
                    WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Console.WriteLine($"WebSocket connection closed: {webSocket.GetHashCode()}");
                        connections.Remove(webSocket);
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by the server", CancellationToken.None);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error receiving WebSocket message: {ex.Message}");
                }
            }
        }

        public static OsuWebSocket Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new OsuWebSocket();
                }
                return instance;
            }
        }

        public async Task SendData(object data)
        {
            var dataJson = JsonConvert.SerializeObject(data);
            var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(dataJson));

            foreach (var webSocket in connections)
            {
                try
                {
                    await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending WebSocket data: {ex.Message}");
                }
            }
        }
    }
}
