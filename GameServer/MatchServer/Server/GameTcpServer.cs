
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Server;

namespace Game.Http
{
    /// <summary>
    /// 游戏服务器连接方法，启动，关闭，查询日志，测试，补发奖励
    /// </summary>
    public class GameTcpServer
    {
        private TcpServer tcpService;
        private Dictionary<string, ClientData> dicPack = null;
        public TcpServer GetService() => tcpService;

        public GameTcpServer() { dicPack = new Dictionary<string, ClientData>(); }

        //启动服务器
        public void EnableSerer()
        {
            var address = ServerConifg.GetAddressServer();
            tcpService = TcpServer.ins.AwakeAction(this.Connected, this.Disconnected)
                .StartListen(address.ip, address.port);
        }

        private void Disconnected(Client obj)
        {
            this.CloseClient(obj);
        }

        private void Connected(Client obj)
        {
            this.AddClient(obj);
        }

        private void AddClient(Client socket)
        {
            lock (typeof(GameTcpServer))
            {
                if (!dicPack.ContainsKey(socket.ipAddress))
                {
                    this.tcpService.Logger.Info($"服务器接收到客户端 {socket.ipAddress} 链接");
                    dicPack[socket.ipAddress] = new ClientData(socket);
                }
                else
                {
                    dicPack[socket.ipAddress].AddFromTcpSerer(socket);
                }
            }

        }

        public void PushClient(uint roleId, short cmd, PushMessageHandle messageHandle)
        {
            foreach (var item in dicPack.Values)
            {
                if (item != null&&item.roleId == roleId)
                {
                    item.PushClient(cmd, messageHandle);
                }
            }
        }

        //关闭每一个客户端
        private void CloseClient(Client socket)
        {
            if (dicPack.ContainsKey(socket.ipAddress))
            {
                dicPack[socket.ipAddress].Close();
                dicPack.Remove(socket.ipAddress);
            }
        }
    
        /// <summary>
        /// 广播踢人下线
        /// </summary>
        /// <param name="IPaddress">地址格式：127.0.0.1:9001</param>
        public void RemoveClient(params string[] ipArray)
        {
            Task.Run(async () =>
            {
                var len = ipArray.Length;
                for (int i = 0; i < len; i++)
                {
                    var ipAddress = ipArray[i];
                    if (dicPack.ContainsKey(ipAddress))
                    {
                        dicPack[ipAddress].Close();
                        dicPack.Remove(ipAddress);
                    }
                    await Task.Delay(1);
                }
            });
        }

        //停服
        public void CloseServer()
        {
            if (this.tcpService != null)
            {
                if (dicPack.Count > 0)
                {
                    dicPack.Values.ToList().ForEach(p => p.Close());
                    dicPack.Clear();
                }
                this.tcpService.Stop();
            }
        }

        //启动和关闭日志
        public void SetEnable(bool isEnable)
        {
            if (this.tcpService != null)
            {
                this.tcpService.Logger.IsEnable = isEnable;
            }
        }
    }
}
