using SocketDemo.Core;
using SocketDemo.Core.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace SocketDemo.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ClientSocket client = new ClientSocket(socket);
            client.Connect("127.0.0.1", 8888);
            client.OnMessage += OnMessage;
            client.BeginReceive();

            Console.Write("> ");
            String line = Console.ReadLine();
            while (line != "exit")
            {
                Message req = new Message();
                req.Body = Encoding.UTF8.GetBytes(line);
                client.Send(req.Serialize());

                Console.Write("> ");
                line = Console.ReadLine();
            }
        }

        private static void OnMessage(ClientSocket socket, Message message)
        {
            String data = Encoding.UTF8.GetString(message.Body);
            Console.WriteLine(data);
        }
    }
}
