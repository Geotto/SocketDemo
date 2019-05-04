﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocketDemo.Core.Message
{
    /// <summary>
    /// 消息头
    /// </summary>
    public class Header
    {
        /// <summary>
        /// 消息开始标记
        /// </summary>
        public byte StartTag;

        /// <summary>
        /// 校验和
        /// </summary>
        public byte BodyCrc;

        /// <summary>
        /// 消息体长度
        /// </summary>
        public int BodyLen;
    }
}
