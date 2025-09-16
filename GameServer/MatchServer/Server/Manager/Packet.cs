//using Server;
//using System;
//using System.IO;
//using System.Net;

//namespace Game.Http
//{
//    /// <summary>
//    /// 1, 数据解析
//    /// 2，数据封装
//    /// 3，数据接收处理
//    /// 4，协议号
//    /// </summary>
//    public sealed class Packet
//    {
//        //空包
//        public static Packet Empty = new Packet(0, new byte[0]);
//        //消息头大小
//        public const short HEAD_LENGTH = 5;

//        //包最大的大小
//        public const short MSG_MAX_SIZE = Int16.MaxValue;
//        //当前包流水号
//        public short serialNumber = 0;
//        //当前包版本
//        public byte version = 0;
//        public Packet(short cmd, byte[] msgData, byte version=0)
//        {
//            this.cmd = cmd;
//            this.Size = msgData.Length;
//            this.version = version;
//            this.msgBody = msgData;
//        }

//        /**
//         * 获取消息命令
//         */
//        public short cmd { get; private set; }

//        /**
//         * 获取消息体长度
//         */
//        public int Size { get; private set; }

//        //消息体
//        private byte[] msgBody;

//        /// <summary>
//        /// 获取数据,位置重置为 ：读取消息的包
//        /// </summary>
//        public void WirteByteBlock(ByteBlock byteBlock)
//        {
//            byteBlock.Write(msgBody);
//            byteBlock.Position = 0;
//        }

//        /// <summary>
//        /// 发送消息头为4个字节
//        /// </summary>
//        /// <returns></returns>
//        public byte[] ToBytes()
//        {
            
//            short msgLen = (short)(HEAD_LENGTH-1 + this.Size);
//            byte[] msgdata = new byte[msgLen];

//            //总长度：消息头+消息体的 4个字节
//            byte[] totalbytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(msgLen));
//            Buffer.BlockCopy(totalbytes, 0, msgdata, 0, totalbytes.Length);

//            //消息cmd  2个字节
//            byte[] cmdbytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(cmd));
//            Buffer.BlockCopy(cmdbytes, 0, msgdata, 2, cmdbytes.Length);

//            //拷贝到消息体
//            Buffer.BlockCopy(msgBody, 0, msgdata, HEAD_LENGTH-1, msgBody.Length);

//            return msgdata;
//        }

//        /// <summary>
//        /// 收到消息头为5个字节
//        /// </summary>
//        public static Packet Decode(byte[] msg, int recvLength)
//        {
//            if (recvLength >= HEAD_LENGTH)
//            {
//                //消息头+消息体的长度
//                short msgLength = IPAddress.HostToNetworkOrder(BitConverter.ToInt16(msg, 0));
//                //消息cmd
//                byte version = msg[2];
//                //消息cmd
//                short cmd = IPAddress.HostToNetworkOrder(BitConverter.ToInt16(msg, 3));

//                int bodyLen = msgLength - HEAD_LENGTH;
//                //非法消息
//                if (msgLength < HEAD_LENGTH || msgLength > MSG_MAX_SIZE)
//                {
//                    GameCenter.Log(LogType.Error, string.Format("接收到非法消息  cmd:{0}, msgLen:{1}", cmd, bodyLen),null);
//                    return null;
//                }
//                //合法的消息包
//                else if (recvLength >= msgLength)
//                {
//                    MemoryStream body = new MemoryStream(msg, HEAD_LENGTH, bodyLen);
//                    if (body != null)
//                    {
//                        return new Packet(cmd, body.ToArray(), version);
//                    }
//                }
//                else
//                {
//                    GameCenter.Log(LogType.Error, "接收到非法消息，小于包头  cmd:" + cmd,null);
//                    return null;
//                }
//            }
//            return Packet.Empty;
//        }

//        /// <summary>
//        /// 获取到包的长度
//        /// </summary>
//        public static int GetPacketSize(byte[] msg, int recvLength)
//        {
//            if (recvLength >= HEAD_LENGTH)
//            {
//                return IPAddress.HostToNetworkOrder(BitConverter.ToInt16(msg, 0));
//            }
//            return HEAD_LENGTH;
//        }
//    }
//}
