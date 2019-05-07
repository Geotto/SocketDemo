using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using SocketDemo.Core.Message;

namespace SocketDemo.Core
{
    /// <summary>
    /// 客户端连接管理器
    /// </summary>
    public class ClientSocketManager
    {
        /// <summary>
        /// 文件存储目录
        /// </summary>
        private static string FileStoreDir = ConfigurationManager.AppSettings["FileStoreDir"];

        /// <summary>
        /// 客户端套接字映射
        /// </summary>
        private Dictionary<int, ClientSocket> dict = new Dictionary<int, ClientSocket>();

        /// <summary>
        /// 同步锁
        /// </summary>
        private object syncRoot = new object();

        /// <summary>
        /// 获取单例
        /// </summary>
        public static ClientSocketManager Instance
        {
            get { return InstanceHolder.instance; }
        }

        private ClientSocketManager() { }

        /// <summary>
        /// 添加客户端套接字
        /// </summary>
        /// <param name="socket"></param>
        public void Add(ClientSocket socket)
        {
            lock (syncRoot)
            {
                socket.OnMessage += OnMessage;
                socket.OnError += OnError;
                socket.BeginReceive();
                dict[socket.Handle] = socket;
            }

            Console.WriteLine("client socket accepted, ip: {0}", socket.IP);
        }

        /// <summary>
        /// 异常处理函数
        /// </summary>
        /// <param name="socket">套接字</param>
        /// <param name="error">异常</param>
        private void OnError(ClientSocket socket, Exception error)
        {
            Console.WriteLine(error.ToString());
            lock (syncRoot)
            {
                dict.Remove(socket.Handle);
                Console.WriteLine("移除socket连接");
            }
        }

        /// <summary>
        /// 消息处理函数
        /// </summary>
        /// <param name="message">消息</param>
        private void OnMessage(ClientSocket socket, Message.Message req)
        {
            switch (req.Header.MsgType)
            {
                case MessageType.Text:
                    Message.Message resp = new Message.Message();
                    resp.Body = req.Body;

                    socket.Send(resp.Serialize());
                    break;

                case MessageType.File:
                    if (!Directory.Exists(FileStoreDir))
                    {
                        Directory.CreateDirectory(FileStoreDir);
                    }

                    FileMessage message = new FileMessage(req);
                    using (FileStream fs = File.OpenWrite(FileStoreDir + Path.DirectorySeparatorChar + message.FileName))
                    {
                        fs.Write(message.Data, 0, message.Data.Length);
                    }

                    Console.WriteLine("{0} saved to {1}", message.FileName, FileStoreDir);
                    break;
            }
        }

        private static class InstanceHolder
        {
            public static ClientSocketManager instance = new ClientSocketManager();
        }
    }
}
