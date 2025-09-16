using Hukiry.Socket;
using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    public class Client
    {
        //接收队列
        private const int BUFF_SIZE = 32 * 1024;
        private int msgLength;
        private byte[] msgbuff = new byte[Packet.MSG_MAX_SIZE];
        private Queue mRecvQueue = Queue.Synchronized(new Queue());
        private Socket mSocket;
        private byte[] buffer;
        private HttpTimer httpTimer;

        public Client()
        {
            this.buffer = new byte[BUFF_SIZE];
            this.httpTimer = new HttpTimer(20, this.OnLoopUpdate);
            this.httpTimer.Start();
           
        }

        public void Connect(string ip, int port)
        {
            if (this.IsConnected())
            {
                this.Close();
            }
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            mSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            mSocket.Blocking = false;
            mSocket.ReceiveTimeout = 3000;
            mSocket.BeginConnect(endPoint, FinishConnect, mSocket);
        }

        private void FinishConnect(IAsyncResult async)
        {
            try
            {
                Socket handler = (Socket)async.AsyncState;
                handler.EndConnect(async);
                Log($" 链接成功", ConsoleColor.Cyan);
                //设置timeout
                this.mSocket.SendTimeout = 30;
                this.mSocket.ReceiveTimeout = 30;
                //开始监听
                mSocket.BeginReceive(this.buffer, 0, BUFF_SIZE, SocketFlags.None, new AsyncCallback(RecvCallback), null);
            }
            catch (Exception e)
            {
                Log($" 链接失败", ConsoleColor.Red);
            }
        }

        public bool IsConnected()
        {
            return mSocket != null && mSocket.Connected;
        }

        private void RecvCallback(IAsyncResult async)
        {
            if (mSocket == null || !mSocket.Connected) return;
            try
            {
                int recvLength = mSocket.EndReceive(async);
                if (recvLength > 0)
                {
                    //解析数据包
                    if (msgLength + recvLength <= Packet.MSG_MAX_SIZE)
                    {
                        Buffer.BlockCopy(this.buffer, 0, msgbuff, msgLength, recvLength);//收到的数据添加到末尾
                        msgLength += recvLength;//收到消息的总长度

                        while (msgLength >= Packet.GetPacketSize(msgbuff, msgLength))
                        {
                            Packet newPacket = Packet.Decode(msgbuff, msgLength);
                            if (newPacket == null || newPacket == Packet.Empty)
                            {
                                Console.WriteLine("接收到非法消息  cmd:未知");
                                break;
                            }
                            else
                            {
                                mRecvQueue.Enqueue(newPacket);//装包
                            }

                            int packLen = newPacket.Size + Packet.HEAD_LENGTH;//当前装包的长度
                            msgLength = msgLength - packLen; //剩下的消息长度
                            if (msgLength > 0)
                            {
                                Buffer.BlockCopy(msgbuff, packLen, msgbuff, 0, msgLength); //重新定位剩下的数据
                            }
                            else if (msgLength < 0)
                            {
                                Log("msgLength not equip packet.size" + msgLength + ", " + newPacket.Size, ConsoleColor.Red);
                            }
                        }
                    }
                    else
                    {   //非法包丢掉
                        if (msgLength >= Packet.HEAD_LENGTH)
                        {
                            //body长度
                            int msglength = BitConverter.ToInt16(msgbuff, 0);
                            //协议号
                            int cmdId = BitConverter.ToInt16(msgbuff, 2);
                            Log(string.Format("接收到非法消息 ,包体长度大于限定长度 cmd:{0}, bodylen:{1}", cmdId, msglength), ConsoleColor.Red);
                            msgLength -= msglength - recvLength;
                        }
                    }
                }
                else if (recvLength < 0)
                {
                    this.Close();//接受错误
                    return;
                }

                mSocket.BeginReceive(this.buffer, 0, BUFF_SIZE, SocketFlags.None, new AsyncCallback(RecvCallback), null);
            }
            catch (Exception e)
            {
                //网络错误
                this.Close();
            }
        }

        private void OnReceivePacket(byte[] buffer, short cmd)
        {
            string txt = System.Text.Encoding.UTF8.GetString(buffer);
            Log($"cmd:{cmd} ,收到：{txt}" , ConsoleColor.Green);
        }

        public void SendPacket(string msg)
        {
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(msg);
            Packet packet = new Packet(2001, buffer);
            Log($"发送消息：{packet.cmd} 长度：{packet.Size}", ConsoleColor.Cyan);
            this.SendPacket(packet.ToBytes());
        }

        private void SendPacket(byte[] msgdata)
        {
            //网络还未连接,提示警告
            if (!IsConnected()) { Log("conection does't work now!", ConsoleColor.Red); return; }
           
            mSocket.BeginSend(msgdata, 0, msgdata.Length, SocketFlags.None, new AsyncCallback(SendCallback), null);
        }

        /// <summary>
        /// 发送等待
        /// </summary>
        void SendCallback(IAsyncResult async)
        {
            try
            {
                mSocket.EndSend(async);
            }
            catch (Exception e)
            {
                this.Close();
            }
        }

        /// <summary>
        /// 每20毫秒更新
        /// </summary>
        /// <param name="SignalTime">信号时间</param>
        private void OnLoopUpdate(DateTime SignalTime)
        {
            int len = mRecvQueue.Count;
            //消息队列
            while (len > 0)
            {
                len--;
                Packet pk = mRecvQueue.Dequeue() as Packet;
                if (pk == null)
                {
                    break;
                }
                this.OnReceivePacket(pk.MsgBody, pk.cmd);
            }
        }

        public void Close()
        {
            this.msgLength = 0;
            mRecvQueue.Clear();
            if (this.mSocket != null)
            {
                if (this.mSocket.Connected)
                {
                    this.mSocket.Shutdown(SocketShutdown.Both);
                }
                this.mSocket.Close();
                this.mSocket = null;
            }
        }

        private void Log(string error, ConsoleColor consoleColor)
        {
            Console.ForegroundColor = consoleColor;
            Console.WriteLine(error);
        }
    }
}
