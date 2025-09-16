
using System;
using System.Collections;
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
        private SocketTimer httpTimer;

        public string ipAddress;
        private TcpServer tcpLister;
        private int index = 0;

        public Action<Packet> ReceivePacket;

        public Client(Socket socket, TcpServer tcpLister, int index)
        {
            this.index = index;
            this.tcpLister = tcpLister;
            this.mSocket = socket;
            this.buffer = new byte[1024 * 64];
            this.ipAddress = socket.RemoteEndPoint.ToString();

            this.httpTimer = new SocketTimer(20, this.OnLoopUpdate);
            this.httpTimer.Start();

            tcpLister?.Connected(this);
        }

        public bool IsConnected()
        {
            return mSocket != null && mSocket.Connected;
        }

        public void BeginReceive()
        {
            //设置timeout
            this.mSocket.SendTimeout = 30;
            this.mSocket.ReceiveTimeout = 30;
            //开始监听
            mSocket.BeginReceive(this.buffer, 0, BUFF_SIZE, SocketFlags.None, new AsyncCallback(RecvCallback), null);
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
            index++;
            Log($"收到{ipAddress}：cmd:{cmd} ,size：{buffer.Length}===={index}", ConsoleColor.Green);
            Packet packet = new Packet(cmd, buffer);
            //this.SendPacket(packet.ToBytes());
            ReceivePacket?.Invoke(packet);
        }

        public void SendPacket(byte[] msgdata)
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
            if (!IsConnected())
            {
                Log($"-------客户端断开链接----------{this.ipAddress}", ConsoleColor.DarkBlue);
                this.Close();
            }

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

            if (httpTimer != null)
            {
                httpTimer.Stop();
                httpTimer = null;
            }

            tcpLister?.RemoveClient(ipAddress);
            tcpLister?.Disconnected(this);
        }

        private void Log(string error, ConsoleColor consoleColor)
        {
            Console.ForegroundColor = consoleColor;
            Console.WriteLine(error);
        }
    }
}
