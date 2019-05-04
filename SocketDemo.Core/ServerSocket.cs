using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SocketDemo.Core
{
    /// <summary>
    /// 服务端socket
    /// </summary>
    public class ServerSocket
    {
        /// <summary>
        /// 套接字
        /// </summary>
        private Socket socket;

        /// <summary>
        /// 绑定的端口地址
        /// </summary>
        private EndPoint bind;

        public ServerSocket(String host, int port)
        {
            IPAddress address = IPAddress.Parse(host);

            this.bind = new IPEndPoint(address, port);
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        /// <summary>
        /// 开始监听
        /// </summary>
        public void Listen()
        {
            this.socket.Bind(bind);
            this.socket.Listen(1000);
            this.socket.BeginAccept(OnAccepted, this.socket);
        }

        /// <summary>
        /// 处理接受连接事件
        /// </summary>
        /// <param name="ar"></param>
        private void OnAccepted(IAsyncResult ar)
        {
            if (ar.AsyncState is Socket)
            {
                Socket socket = ar.AsyncState as Socket;
                Socket clientSocket = socket.EndAccept(ar);
                ClientSocket client = new ClientSocket(clientSocket);
                ClientSocketManager.Instance.Add(client);

                socket.BeginAccept(OnAccepted, this.socket);
            }
        }
    }
}
