using SocketDemo.Core;
using SocketDemo.Core.Message;
using System;
using System.Collections.Generic;
using System.IO;
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
                string[] columns = line.Split(new char[] { '\t', ' ', '\r', '\n' }, 2);
                if(columns.Length == 2)
                {
                    switch (columns[0].Trim())
                    {
                        case "text":
                            Message message = new Message();
                            message.Body = Encoding.UTF8.GetBytes(columns[1]);
                            client.Send(message.Serialize());
                            break;

                        case "file":
                            FileMessage fileMessage = new FileMessage();
                            FileInfo file = new FileInfo(columns[1]);
                            fileMessage.FileName = file.Name;
                            if (file.Exists)
                            {
                                byte[] buf = new byte[1024];
                                using (MemoryStream ms = new MemoryStream())
                                {
                                    using (FileStream stream = File.OpenRead(columns[1]))
                                    {
                                        int len = stream.Read(buf, 0, buf.Length);
                                        while (len > 0)
                                        {
                                            ms.Write(buf, 0, len);
                                            len = stream.Read(buf, 0, buf.Length);
                                        }
                                    }

                                    fileMessage.Data = ms.ToArray();
                                }

                                client.Send(fileMessage.Serialize());
                            }
                            else
                            {
                                Console.WriteLine("no such file or directory: {0}", columns[1]);
                            }
                            break;
                    }
                }

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
