using System;
using System.IO;
using System.Net;

namespace Hukiry.Socket
{
    //消息包
    public class Packet
    {
		//空包
		public static Packet Empty = new Packet(0, new byte[0]);

        //消息头大小
        public const short HEAD_LENGTH = 4;

        //包最大的大小
        public const short MSG_MAX_SIZE = Int16.MaxValue;

		//当前包版本
		public byte version = 0;

        /// <summary>
        /// 获取消息命令
        /// </summary>
        public short cmd { get; private set;}

        /// <summary>
        /// 获取消息长度
        /// </summary>
        public int Size { get; private set; }

        /// <summary>
        /// 消息体
        /// </summary>
        private byte[] msgBody;

        /// <summary>
        /// 获取数据
        /// </summary>
        public byte[] MsgBody => msgBody;

       
        /// <summary>
        /// 创建包
        /// </summary>
        /// <param name="cmd">消息号</param>
        /// <param name="msgData">数据</param>
        /// <param name="version">版本号</param>
        public Packet(short cmd, byte[] msgData)
        {
            this.cmd = cmd;
            this.Size = msgData.Length;
            this.msgBody = msgData;
        }

        /// <summary>
        /// 转换成字节码：发送消息头5个字节
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            short msgLen = (short)(HEAD_LENGTH /*+1*/+ this.Size);
            byte[] msgdata = new byte[msgLen];

            //总长度：消息头4个字节+消息体的长度
            byte[] totalbytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(msgLen));
            Buffer.BlockCopy(totalbytes, 0, msgdata, 0, totalbytes.Length);

            //消息版本
            //byte[] version = new byte[1] { this.version };
            //Buffer.BlockCopy(version, 0, msgdata, 2, version.Length);

            //消息cmd
            byte[] cmdbytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(cmd));
			Buffer.BlockCopy(cmdbytes, 0, msgdata, 2, cmdbytes.Length);

            //拷贝到消息体
            Buffer.BlockCopy(msgBody, 0, msgdata, HEAD_LENGTH/*+1*/, msgBody.Length);

			return msgdata;
        }

        /// <summary>
        /// 消息包解密：收到消息头4个字节
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="recvLength"></param>
        /// <returns></returns>
        public static Packet Decode(byte[] msg, int recvLength)
        {
            if (recvLength >= HEAD_LENGTH)
            {
                //消息头+消息体的长度
                short msgLength = IPAddress.HostToNetworkOrder(BitConverter.ToInt16(msg, 0));
                //消息cmd
                short cmd = IPAddress.HostToNetworkOrder(BitConverter.ToInt16(msg, 2));

                int bodyLen = msgLength - HEAD_LENGTH;
				//非法消息
				if (msgLength < HEAD_LENGTH || msgLength > MSG_MAX_SIZE)
                {
                    Log(string.Format("接收到非法消息  cmd:{0}, msgLen:{1}", cmd, bodyLen), ConsoleColor.Red);
					return null;
                }
                //合法的消息包
                else if(recvLength >= msgLength)
                {
					MemoryStream body = new MemoryStream(msg, HEAD_LENGTH, bodyLen);
					if(body != null) {
						return new Packet(cmd, body.ToArray());
					}
				}else {
                    Log("接收到非法消息，小于包头  cmd:" + cmd, ConsoleColor.Red);
					return null;
				}
            }
            return Packet.Empty;
        }
       
        /// <summary>
        /// 获取到包的长度
        /// </summary>
        public static int GetPacketSize(byte[] msg, int recvLength) {
			if (recvLength >= HEAD_LENGTH) {
				return IPAddress.HostToNetworkOrder(BitConverter.ToInt16(msg, 0));
			}
			return HEAD_LENGTH;
		}
     
        private static void Log(string error, ConsoleColor consoleColor)
        {
            Console.ForegroundColor = consoleColor;
            Console.WriteLine(error);
        }
    }
}
