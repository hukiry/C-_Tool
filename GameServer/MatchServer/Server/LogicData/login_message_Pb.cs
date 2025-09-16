using Server;

namespace Game.Http
{

    //用于模板生成
    public class login_message_Pb : MessageProto
	{
		private const string SIGN_KEY = "hctam5emoc4yekngis7";
		public override MessageProto GetMessagePb() => this;

		private UserRole userdata;
		bool receive_1001(IProto proto, string ipAddress)//接收客户端数据
		{
			msg_1001 msg = proto as msg_1001;
			string key = msg.deviceId + msg.signTime + SIGN_KEY;
			string resultSign = this.GetMD5(System.Text.ASCIIEncoding.UTF8.GetBytes(key));
			if (msg.sign.Equals(resultSign))
			{
				this.userdata = new UserRole()
				{
					deviceId = msg.deviceId,
					ipAddress = ipAddress,
					lanCode = msg.lang,
					expirateTime = GameCenter.GetTimeSecond()
				};

				this.userdata.ReadUserData(0);
				this.RoleID = this.userdata.id;
				return true;
			}
			return false;
		}

		IProto send_1001()//发送给客户端数据
		{
			IProto msg = new msg_1001()
			{
			};
			return msg;
		}

		bool receive_1002(IProto proto, string ipAddress)//接收客户端上传的数据
		{
			msg_1002 msg = proto as msg_1002;

			if (this.userdata == null)
			{
				this.userdata = new UserRole();
				this.userdata.ReadUserData(msg.roleId);
			}

			if (this.userdata.IsCanAccess())
			{
				if (this.userdata.id <= 0)//没有读取到数据
				{
					this.userdata.id = msg.roleId;
				}

				this.userdata.curLifeTime = msg.curLifeTime;
				this.userdata.timeStamp = msg.timeStamp;
				this.userdata.state = msg.state;
				this.userdata.items = msg.items;
				this.userdata.headID = msg.headID;
				this.userdata.metaMapIds = msg.metaMapIds;

				this.userdata.roleNick = msg.roleNick;
				this.userdata.lanCode = msg.lanCode;

				//无token上传
				this.userdata.UpdateSqlData();
			}
			this.RoleID = msg.roleId;
			return false;
		}

		IProto send_1002()//登陆成功
		{
			msg_1002 msg = new msg_1002();
			msg.timeStamp = GameCenter.GetTimeSecond();
			msg.token = this.userdata.token;
			msg.headID = this.userdata.headID;
			msg.items = this.userdata.items;
			msg.roleId = this.userdata.id;
			msg.curLifeTime = this.userdata.curLifeTime;
			msg.roleNick = this.userdata.roleNick;
			msg.state = this.userdata.state;
			msg.lanCode = this.userdata.lanCode;
			msg.items = this.userdata.items;
			msg.gassId = this.userdata.gassId;
			//msg.deviceId = this.userdata.deviceId;
			msg.metaMapIds = this.userdata.metaMapIds;
			this.RoleID = msg.roleId;
			return msg;
		}

		private byte stateMsg;

		bool receive_1003(IProto proto, string ipAddress)//接收客户端数据
		{
			msg_1003 msg = proto as msg_1003;
			try
			{
				if (this.userdata == null)
				{
					this.userdata = new UserRole();
					this.userdata.ReadUserData((uint)msg.roleId);
				}

				this.stateMsg = msg.state;
				LoginState state = (LoginState)msg.state;

				if (state == LoginState.Logout)
				{
					//删除数据库
					this.userdata.ClearData();
				}
				else if (state == LoginState.Bind)
				{
					bool isChange = this.userdata.CheckBind(msg.token, this.userdata.deviceId, msg.roleId);
					this.stateMsg = isChange ? (byte)LoginState.ChangeCloudData : (byte)LoginState.Bind;
					this.userdata.bindAccount = msg.bindAccount;
					this.userdata.UpdateSqlData();
				}
				else if (state == LoginState.ChangeDevice)
				{
					int deviceId = GameCenter.GetOfflineDeviceId(msg.deviceId, true);
					if (deviceId == 0)
					{
						//不需要切换
						return false;
					}
				}
				else if (state == LoginState.FixNick)
				{
					this.userdata.roleNick = msg.roleNick;
					this.userdata.UpdateSqlData();
				}
			}
			catch (System.Exception ex)
			{
				GameCenter.Log(ex.ToString());
				return false;
			}
			return true;
		}


		IProto send_1003()//发送给客户端数据
		{
			IProto msg = new msg_1003()
			{
				state = this.stateMsg,
			};

			if (this.stateMsg == (byte)LoginState.ChangeCloudData)
			{
				if (this.userdata.id > 0)
				{
					this.userdata.state = 1;
					(msg as msg_1003).state = 1;
				}
			}
			return msg;
		}

		bool receive_1004(IProto proto, string ipAddress)//接收客户端数据
		{
			return true;
		}
		IProto send_1004()//接收客户端数据
		{
			IProto msg = new msg_1004()
			{
				timeStamp = GameCenter.GetTimeSecond(),
			};
			return msg;
		}

		bool receive_1005(IProto proto, string ipAddress)//接收客户端数据
		{
			msg_1005 msg = proto as msg_1005;
			if (msg.roleId > 0)
			{
				if (this.userdata == null)
				{
					this.userdata = new UserRole();
					this.userdata.ReadUserData(msg.roleId);
				}

				var item = this.userdata.items.Find(p => p.type == msg.type);
				if (item != null)
				{
					item.number = msg.number;
				}
				this.userdata.UpdateSqlData();
			}
			return false;
		}
		IProto send_1005()//接收客户端数据
		{
			IProto msg = new msg_1005();

			return msg;
		}

		bool receive_1006(IProto proto, string ipAddress)//接收客户端数据
		{
			msg_1006 msg = proto as msg_1006;
			if (this.userdata == null)
			{
				this.userdata = new UserRole();
				this.userdata.ReadUserData(msg.roleId);
			}
			this.userdata.headID = msg.headID;
			this.userdata.UpdateSqlData();
			return false;
		}

		IProto send_1006()=> new msg_1006();
	}
}