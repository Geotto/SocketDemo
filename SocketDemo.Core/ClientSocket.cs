using SocketDemo.Core.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SocketDemo.Core
{
    /// <summary>
    /// 客户端socket消息处理函数
    /// </summary>
    /// <param name="socket">客户端套接字</param>
    /// <param name="message">消息</param>
    public delegate void ClientSocketMessageHandler(ClientSocket socket, Message.Message message);

    /// <summary>
    /// 异常处理函数
    /// </summary>
    /// <param name="socket">客户端套接字</param>
    /// <param name="error">异常</param>
    public delegate void ErrorHandler(ClientSocket socket, Exception error);

    /// <summary>
    /// 客户端套接字
    /// </summary>
    public class ClientSocket
    {
        /// <summary>
        /// 消息处理函数
        /// </summary>
        public ClientSocketMessageHandler OnMessage;

        /// <summary>
        /// 异常处理函数
        /// </summary>
        public ErrorHandler OnError;

        /// <summary>
        /// 套接字
        /// </summary>
        private Socket socket;

        /// <summary>
        /// 消息缓冲区
        /// </summary>
        private byte[] buffer;

        /// <summary>
        /// 消息处理器
        /// </summary>
        private MessageParser parser;

        public ClientSocket(Socket socket)
        {
            this.socket = socket;
            this.buffer = new byte[1024];
            this.parser = new MessageParser();
            this.parser.OnMessage += MessageReceived;
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="data">数据</param>
        public void Send(byte[] data)
        {
            this.socket.Send(data);
        }

        /// <summary>
        /// 连接到远端
        /// </summary>
        /// <param name="host">远端地址</param>
        /// <param name="port">远端端口</param>
        public void Connect(String host, int port)
        {
            IPAddress address = IPAddress.Parse(host);
            EndPoint endPoint = new IPEndPoint(address, port);
            this.socket.Connect(endPoint);
        }

        /// <summary>
        /// 消息处理函数
        /// </summary>
        /// <param name="message">消息</param>
        private void MessageReceived(Message.Message message)
        {
            if (OnMessage != null)
            {
                OnMessage.Invoke(this, message);
            }
        }

        /// <summary>
        /// 开始接收消息
        /// </summary>
        public void BeginReceive()
        {
            this.socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, OnReceive, this.socket);
        }

        /// <summary>
        /// 处理接收事件
        /// </summary>
        /// <param name="ar">异步调用返回</param>
        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                if (ar.AsyncState is Socket)
                {
                    Socket socket = ar.AsyncState as Socket;
                    int len = socket.EndReceive(ar);
                    if (len == 0)
                    {
                        if (OnError != null)
                        {
                            OnError.Invoke(this, new Exception("can not read data from socket."));
                        }
                    }
                    else
                    {
                        parser.Add(buffer, len);
                        BeginReceive();
                    }
                }
            }
            catch (Exception ex)
            {
                if (OnError != null)
                {
                    OnError.Invoke(this, ex);
                }
            }
        }

        /// <summary>
        /// 获取客户端句柄
        /// </summary>
        public int Handle
        {
            get { return this.socket.Handle.ToInt32(); }
        }

        public IPAddress IP
        {
            get { return ((IPEndPoint)this.socket.RemoteEndPoint).Address; }
        }
    }
}
