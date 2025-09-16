
using Hukiry.Protobuf;
using Server;
using System;
using System.Collections.Generic;

namespace Game.Http
{
    /*
	 * 1, 服务断开连接时，缓存到静态类中，当前数据不清空。
	 * 2，如果时间超出，未被连接上，直接清空处理
	 * 3，根据登陆协议返回的类型，处理是否要重新走一遍协议
	 */
    public class ClientData
    {
		private const int DELAY_TIME = 20;//延时毫秒
		private bool isEnable = true;
		private int msgLength;
        private byte[] msgbuff;

		private Client socketClient;
		private Queue<Packet> packetQueue = null;
		private Dictionary<short, string> receivePb = null;
		private Dictionary<short, IMessage> messageDic = null;
		public uint roleId;

        public string IpAddress => this.socketClient?.ipAddress;//获取ip和端口
		public ClientData(Client socketClient)
        {
			this.AddFromTcpSerer( socketClient);
            msgLength = 0;
            msgbuff = new byte[Packet.MSG_MAX_SIZE];
			packetQueue = new Queue<Packet>();
			messageDic = new Dictionary<short, IMessage>();

			receivePb = new Dictionary<short, string>();
			receivePb.Add(10, nameof(login_message_Pb));
			receivePb.Add(11, nameof(mail_message_Pb));
			receivePb.Add(12, nameof(activity_message_Pb));
			receivePb.Add(13, nameof(common_message_Pb));
			receivePb.Add(14, nameof(shop_message_Pb));
			receivePb.Add(15, nameof(friend_message_Pb));
			receivePb.Add(16, nameof(meta_message_Pb));
			receivePb.Add(19, nameof(error_message_Pb));
			this.isEnable = true;
		}

		//客户端重连的时候
		public void AddFromTcpSerer(Client socketClient)
		{
			this.socketClient = socketClient;
			this.socketClient.ReceivePacket = OnReceivePacket;
		}

        private void OnReceivePacket(Packet newPacket)
        {
			var message = this.CreateMessage(newPacket.cmd);
			if (message != null && message.Receive(newPacket, this.IpAddress))
			{
				if (newPacket.cmd == protocal.msg_1001)
				{

					this.SendClient(message, protocal.msg_1002);
				}
				else
				{
					//响应消息包
					this.SendClient(message, newPacket.cmd);
				}

				if (message.RoleID > 0)
				{
					this.roleId = message.RoleID;
				}
			}
			else
			{
				//this.SendError(newPacket.cmd);
			}
		}

		private void SendError(short reqCmd, byte errorCode=0)
		{
			try
			{
				var message = this.CreateMessage(protocal.msg_1901);
				var pack = message?.GetBufferData(protocal.msg_1901, proto => {
					var msg = proto as msg_1901;
					msg.cmd = (ushort)reqCmd;
					msg.code = errorCode;
				});

				if (pack != null)
				{
					this.socketClient?.SendPacket(pack.ToBytes());
				}
			}
			catch (Exception ex)
			{
				GameCenter.Log(LogType.Warning, this, "发送消息给客户端失败", ex);
			}
		}

		/// <summary>
		/// 推送数据
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="messageHandle"></param>
		public void PushClient(short cmd, PushMessageHandle messageHandle)
		{
			try
			{
				var msg = this.CreateMessage(cmd);
				var pack = msg.GetBufferData(cmd, messageHandle);
				this.socketClient?.SendPacket(pack.ToBytes());
			}
			catch
			{
				this.SendError(cmd);
			}
		}

		/// <summary>
		/// 响应消息包
		/// </summary>
		/// <param name="cmd"></param>
		private void SendClient(IMessage message, short cmd)
		{
			try
			{
				var pack = message.GetBufferData(cmd);
				if (pack.cmd != protocal.msg_1004)
				{
					GameCenter.Log($"发送消息：cmd={cmd},len={pack.Size}");
				}
				//处理完成，发包
				this.socketClient?.SendPacket(pack.ToBytes());
			}
			catch (Exception ex)
			{
				this.SendError(cmd);
			}
		}

		//推送和发送消息
		private IMessage CreateMessage(int cmd)
		{
			short intCmd = (short)(cmd / 100);
			if (!this.receivePb.ContainsKey(intCmd))
			{
				GameCenter.Log(LogType.Error, this, $"请配置数据类{intCmd}", null);
				return null;
			}

			if (this.messageDic.ContainsKey(intCmd)) return this.messageDic[intCmd];
			
			string typeName = $"{nameof(Game)}.{nameof(Game.Http)}." + this.receivePb[intCmd];
			var type = Type.GetType(typeName);
			object obj = Activator.CreateInstance(type);
			this.messageDic[intCmd] = obj as IMessage;
			return this.messageDic[intCmd];
		}

		public void Close()
		{
			GameCenter.Log($"关闭客户端：ip={this.socketClient.ipAddress}");
			this.isEnable = false;
			msgbuff = null;
			receivePb.Clear();
			messageDic = null;
		}
	}
}
