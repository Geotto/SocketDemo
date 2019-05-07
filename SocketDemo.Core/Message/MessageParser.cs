using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace SocketDemo.Core.Message
{
    /// <summary>
    /// 消息处理函数
    /// </summary>
    /// <param name="message">消息</param>
    public delegate void MessageHandler(Message message);

    /// <summary>
    /// 消息解析器
    /// </summary>
    public class MessageParser
    {
        /// <summary>
        /// 消息处理函数
        /// </summary>
        public MessageHandler OnMessage;

        /// <summary>
        /// 消息缓冲区列表
        /// </summary>
        private List<byte[]> bufferList = new List<byte[]>();

        /// <summary>
        /// 消息缓冲区列表中所有缓冲区加起来的字节数
        /// </summary>
        private int totalSize = 0;

        /// <summary>
        /// 当前读取位置在消息缓冲区列表所有字节中的偏移位置
        /// </summary>
        private int offset = 0;

        /// <summary>
        /// 将新数据添加到缓冲区列表
        /// </summary>
        /// <param name="buffer">缓冲区</param>
        /// <param name="len">有效的字节数</param>
        public void Add(byte[] buffer, int len)
        {
            if (buffer == null || len <= 0 || buffer.Length < len)
                return;

            byte[] copy = new byte[len];
            Array.Copy(buffer, copy, len);

            bufferList.Add(copy);
            totalSize += len;

            Parse();
        }

        /// <summary>
        /// 进行消息解析
        /// </summary>
        private void Parse()
        {
            while (totalSize > 0)
            {
                byte val = GetByteAt(offset, 0);
                if (val == Message.StartTag)
                {
                    //遇到开始标记
                    if (offset + 6 <= totalSize)
                    {
                        //长度足够，读取消息头
                        int pos = 1;
                        byte msgType = GetByteAt(offset, pos++);
                        byte bodyCrc = GetByteAt(offset, pos++);                        
                        byte[] data = new byte[4];
                        data[0] = GetByteAt(offset, pos++);
                        data[1] = GetByteAt(offset, pos++);
                        data[2] = GetByteAt(offset, pos++);
                        data[3] = GetByteAt(offset, pos++);
                        int len = data[0] << 24 | data[1] << 16 | data[2] << 8 | data[3];
                        int bodyLen = IPAddress.NetworkToHostOrder(len);

                        Header header = new Header();
                        header.StartTag = val;
                        header.MsgType = (MessageType)msgType;
                        header.BodyCrc = bodyCrc;
                        header.BodyLen = bodyLen;

                        if (offset + 6 + bodyLen < totalSize)
                        {
                            //长度足够，读取消息体
                            byte[] body = new byte[bodyLen];
                            FillByteArray(offset + 6 + 1, body, bodyLen);

                            //计算crc
                            byte crc = 0;
                            for (int i = 0; i < body.Length; i++)
                            {
                                crc += body[i];
                            }

                            if (header.BodyCrc == crc)
                            {
                                //校验通过
                                Message message = new Message();
                                message.Header = header;
                                message.Body = body;

                                //处理消息
                                if (OnMessage != null)
                                {
                                    OnMessage.Invoke(message);
                                }

                                RemoveBefore(offset + 6 + message.Body.Length + 1);
                            }
                            else
                            {
                                //校验未通过
                                RemoveBefore(offset + 1);
                            }
                        }
                        else
                        {
                            //长度不足，直接返回
                            return;
                        }
                    }
                    else
                    {
                        //长度不足，直接返回
                    }
                }
                else
                {
                    //不是开始标记，读取下一个
                    RemoveBefore(offset + 1);
                }
            }
        }

        /// <summary>
        /// 移除指定位置之前的数据
        /// </summary>
        /// <param name="pos">结束位置</param>
        private void RemoveBefore(int pos)
        {
            int size = pos;
            for (int i = 0;;)
            {
                if (i < 0 || i >= bufferList.Count)
                    break;

                byte[] buffer = bufferList[i];
                if (buffer.Length < pos)
                {
                    bufferList.RemoveAt(i);
                    pos -= buffer.Length;
                }
                else
                {
                    if (buffer.Length - pos == 0)
                    {
                        bufferList.RemoveAt(i);
                    }
                    else
                    {
                        byte[] newBuffer = new byte[buffer.Length - pos];
                        Array.Copy(buffer, pos, newBuffer, 0, newBuffer.Length);
                        bufferList[i] = newBuffer;
                    }

                    offset = 0;
                    totalSize -= size;
                    return;
                }
            }
        }

        /// <summary>
        /// 复制到目标字节数组
        /// </summary>
        /// <param name="begin">开始位置</param>
        /// <param name="body">目标位置</param>
        /// <param name="len">复制长度</param>
        private void FillByteArray(int begin, byte[] dest, int len)
        {
            int sum = 0;
            int pos = 0;
            for (int i = 0; i < bufferList.Count; i++)
            {
                if (sum + bufferList[i].Length > begin)
                {
                    if (len - pos <= bufferList[i].Length)
                    {
                        int start = sum < begin ? begin - sum : 0;
                        Array.Copy(bufferList[i], start, dest, pos, len - pos);
                        return;
                    }
                    else
                    {
                        int start = sum < begin ? begin - sum : 0;
                        Array.Copy(bufferList[i], start, dest, pos, bufferList[i].Length - start);
                        pos += bufferList[i].Length - start;
                    }
                }

                sum += bufferList[i].Length;
            }

            throw new ArgumentOutOfRangeException();
        }

        /// <summary>
        /// 获取从指定偏移量开始之后某个位置的字节
        /// </summary>
        /// <param name="offset">偏移量</param>
        /// <param name="pos">位置</param>
        /// <returns>指定位置的值</returns>
        private byte GetByteAt(int offset, int pos)
        {
            int sum = 0;
            for (int i = 0; i < bufferList.Count; i++)
            {
                if (sum + bufferList[i].Length > offset + pos)
                {
                    byte val = bufferList[i][offset + pos - sum];
                    return val;
                }
                else
                {
                    sum += bufferList[i].Length;
                }
            }

            throw new ArgumentOutOfRangeException();
        }
    }
}
