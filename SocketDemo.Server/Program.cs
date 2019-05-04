using SocketDemo.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocketDemo.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            ServerSocket server = new ServerSocket("0.0.0.0", 8888);
            server.Listen();

            Console.ReadLine();
        }
    }
}
