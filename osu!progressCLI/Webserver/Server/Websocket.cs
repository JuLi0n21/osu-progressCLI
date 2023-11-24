using Fleck;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Security.Policy;

class WebSocket
{
    static List<IWebSocketConnection> allSockets = new List<IWebSocketConnection>();

    static Dictionary<IWebSocketConnection, int> Phonebook = new();

    static WebSocket instance;

    static WebSocketServer server;

    private WebSocket() {
        server = new Fleck.WebSocketServer("ws://0.0.0.0:9001");
    }

    public static WebSocket Instance()
    {
        if(instance == null)
        {
            instance = new WebSocket();
        } 
        return instance;
    }

    public static void start()
    {
      

        server.Start(socket =>
        {
            socket.OnOpen = () =>
            {
                allSockets.Add(socket);
                socket.Send($"Websocket Connection established: {socket.ConnectionInfo.Id}");
            };

            socket.OnClose = () =>
            {
                allSockets.Remove(socket);
            };

            socket.OnMessage = message =>
            {
                Console.WriteLine($"Received message: {message}");

               
            };
        });

        Console.WriteLine("WebSocket server listening on ws://0.0.0.0:9001");
    }
    public static void BroadcastMessage(string message, IWebSocketConnection socket)
    {
        try
        {
            int sender = Phonebook[socket];
            foreach (var s in allSockets)
            {
                s.Send($"{sender}: {message}");
            }

        }
        catch (Exception e)
        {

        }
    }

    public static void BroadcastMessage(string message)
    {
        try
        {
            foreach (var s in allSockets)
            {
                s.Send(message);
            }

        }
        catch (Exception e)
        {
            //
        }
    }
}
