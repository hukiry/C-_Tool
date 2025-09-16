using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    public class TcpServer {
        Socket socketListen;
        private int BufferLength = 1024 * 64;
        private Dictionary<string, Client> SocketClients = new Dictionary<string, Client>();
        private int m_maxCount = 20000;
        private bool m_disposedValue;

        public Action<Client> Connected;
        public Action<Client> Disconnected;
        public static TcpServer ins { get; } = new TcpServer();
        public Logger Logger { get; } = new Logger();
        public TcpServer StartListen(string ip="localhost",int port = 1234)
        {
            socketListen = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socketListen.ReceiveBufferSize = this.BufferLength;
            socketListen.SendBufferSize = this.BufferLength;
            socketListen.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            socketListen.NoDelay = true;

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            socketListen.Bind(endPoint);
            socketListen.Listen(m_maxCount);
            m_disposedValue = false;

            Log($"{endPoint} Listen Server...", ConsoleColor.Green);
            SocketAsyncEventArgs e = new SocketAsyncEventArgs();
            e.UserToken = socketListen;
            e.Completed += this.Args_Completed;
            if (!socketListen.AcceptAsync(e))
            {
                this.OnAccepted(e);
            }
            return this;
        }

        public TcpServer AwakeAction(Action<Client> Connected, Action<Client> Disconnected)
        {
            this.Connected = Connected;
            this.Disconnected = Disconnected;
            return this;
        }

        public void Stop()
        {
            foreach (var item in SocketClients.Values)
            {
                item.Close();
            }
            SocketClients.Clear();
            m_disposedValue = true;
            socketListen.Shutdown(SocketShutdown.Both);
            socketListen.Close();
        }

        private void OnAccepted(SocketAsyncEventArgs e)
        {
            if (!this.m_disposedValue)
            {
                if (e.SocketError == SocketError.Success && e.AcceptSocket != null)
                {
                    Socket socket = e.AcceptSocket;
                    socket.ReceiveBufferSize = this.BufferLength;
                    socket.SendBufferSize = this.BufferLength;


                    if (this.SocketClients.Count > this.m_maxCount)
                    {
                        //this.Logger.Warning(this, "连接客户端数量已达到设定最大值");
                        socket.Close();
                        socket.Dispose();
                    }
                    else
                    {
                        //缓存远程链接的客户端 :链接成功
                        this.SocketInit(socket);
                    }
                }
                e.AcceptSocket = null;
                try
                {
                    if (!((Socket)e.UserToken).AcceptAsync(e))
                    {
                        this.OnAccepted(e);
                    }
                }
                catch
                {
                    e.Dispose();
                    return;
                }
            }
        }

        private void Args_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation == SocketAsyncOperation.Accept)
            {
                this.OnAccepted(e);
            }
        }

        private void SocketInit(Socket socket)
        {
            try
            {
                socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
                var client = new Client(socket, this, this.SocketClients.Count);
                client.BeginReceive();
                Log($"收到客户端链接：{client.ipAddress}......{this.SocketClients.Count}", ConsoleColor.Green);
                this.SocketClients[client.ipAddress] = client;

            }
            catch (Exception ex)
            {
                Log("接收新连接错误" + ex, ConsoleColor.DarkGreen);
            }
        }

        public void RemoveClient(string ipAddress)
        {
            if (SocketClients.ContainsKey(ipAddress))
            {
                SocketClients.Remove(ipAddress);
            }
        }


        private void Log(string error, ConsoleColor consoleColor)
        {
            Console.ForegroundColor = consoleColor;
            Console.WriteLine(error);
        }
    }

}
