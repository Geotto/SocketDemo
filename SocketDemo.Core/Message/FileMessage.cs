using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SocketDemo.Core.Message
{
    /// <summary>
    /// 文件消息
    /// </summary>
    [Serializable]
    public class FileMessage
    {
        /// <summary>
        /// 文件名
        /// </summary>
        private string fileName;

        /// <summary>
        /// 文件数据
        /// </summary>
        private byte[] data;

        /// <summary>
        /// 嵌套消息
        /// </summary>
        private Message message;

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName
        {
            get { return this.fileName; }
            set { this.fileName = value; }
        }

        /// <summary>
        /// 文件数据
        /// </summary>
        public byte[] Data
        {
            get { return this.data; }
            set { this.data = value; }
        }

        public FileMessage() : base()
        {
            message = new Message();
            message.Header.MsgType = MessageType.File;
        }

        public FileMessage(Message message)
        {
            this.message = message;
            using (MemoryStream ms = new MemoryStream(message.Body))
            {
                int fileNameLen = Message.ReadInt(ms);
                byte[] fileNameData = new byte[fileNameLen];
                ms.Read(fileNameData, 0, fileNameData.Length);
                this.fileName = Encoding.UTF8.GetString(fileNameData);
                int dataLen = Message.ReadInt(ms);
                byte[] data = new byte[dataLen];
                ms.Read(data, 0, data.Length);
                this.data = data;
            }
        }

        public byte[] Serialize()
        {
            byte[] body = new byte[0];
            using (MemoryStream stream = new MemoryStream())
            {
                byte[] buf = Encoding.UTF8.GetBytes(fileName);
                Message.WriteInt(stream, buf.Length);
                stream.Write(buf, 0, buf.Length);
                Message.WriteInt(stream, data.Length);
                stream.Write(data, 0, data.Length);
                body = stream.ToArray();
            }

            message.Body = body;
            return message.Serialize();
        }
    }
}
