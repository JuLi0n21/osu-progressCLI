using System;
using System.Collections.Generic;
using Fleck;

namespace osu_progressCLI.server
{
    internal class WebSocketServer
    {
        private static WebSocketServer instance;
        private readonly List<IWebSocketConnection> connections;
        private Fleck.WebSocketServer server;

        private WebSocketServer()
        {
            connections = new List<IWebSocketConnection>();
            server = new Fleck.WebSocketServer($"ws://localhost:{Credentials.Instance.GetConfig().port}");

            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    Console.WriteLine($"WebSocket connection open: {socket.ConnectionInfo.Id}");
                    connections.Add(socket);
                };

                socket.OnClose = () =>
                {
                    Console.WriteLine($"WebSocket connection closed: {socket.ConnectionInfo.Id}");
                    connections.Remove(socket);
                };
            });
        }

        public static WebSocketServer Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new WebSocketServer();
                }
                return instance;
            }
        }

        public void SendDataToClients(string data)
        {
            foreach (var socket in connections)
            {
                socket.Send(data);
            }
        }
    }
}
