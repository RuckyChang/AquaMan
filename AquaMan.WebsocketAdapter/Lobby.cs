using System.Net.Sockets;
using System.Net;
using System;

namespace AquaMan.WebsocketAdapter
{
    public class Lobby
    {
        public Lobby()
        {
            TcpListener server = new TcpListener(IPAddress.Parse("127.0.0.1"), 80);

            server.Start();
            Console.WriteLine("Server has started on 127.0.0.1:80.{0}Waiting for a connection...", Environment.NewLine);
            TcpClient client = server.AcceptTcpClient();

            Console.WriteLine("A client connected.");
        }
    }
}
