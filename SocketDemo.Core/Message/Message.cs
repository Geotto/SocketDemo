using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace SocketDemo.Core.Message
{
    /// <summary>
    /// 消息
    /// </summary>
    [Serializable]
    public class Message
    {
        /// <summary>
        /// 消息开始标记
        /// </summary>
        public const byte StartTag = 0x01;

        /// <summary>
        /// 消息头
        /// </summary>
        public Header Header { get; set; }

        /// <summary>
        /// 消息体
        /// </summary>
        public byte[] Body;

        public Message()
        {
            this.Header = new Header();
        }

        /// <summary>
        /// 序列化当前消息
        /// </summary>
        /// <returns>序列化后的字节数组</returns>
        public byte[] Serialize()
        {
            byte bodyCrc = 0;
            for (int i = 0; i < Body.Length; i++)
            {
                bodyCrc += Body[i];
            }

            this.Header.StartTag = StartTag;
            this.Header.BodyCrc = bodyCrc;
            this.Header.BodyLen = Body.Length;

            using (MemoryStream stream = new MemoryStream())
            {
                stream.WriteByte(this.Header.StartTag);
                stream.WriteByte(this.Header.BodyCrc);
                WriteInt(stream, this.Header.BodyLen);
                stream.Write(this.Body, 0, this.Body.Length);

                return stream.ToArray();
            }
        }

        /// <summary>
        /// 向输出流写入int值（按网络字节序）
        /// </summary>
        /// <param name="stream">输出流</param>
        /// <param name="value">需要写入的值</param>
        public static void WriteInt(MemoryStream stream, int value)
        {
            int val = IPAddress.HostToNetworkOrder(value);
            byte[] data = new byte[4];
            data[0] = (byte)((val >> 24) & 0xFF);
            data[1] = (byte)((val >> 16) & 0xFF);
            data[2] = (byte)((val >> 8) & 0xFF);
            data[3] = (byte)(val & 0xFF);

            stream.Write(data, 0, data.Length);
        }
    }
}
