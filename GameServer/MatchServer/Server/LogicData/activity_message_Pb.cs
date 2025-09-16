using Server;
using System.Collections.Generic;
using System.Linq;

namespace Game.Http
{

    public class activity_data
	{
		public int configId = 0;
		public int type = 0;
		public bool isFinshed;
		/// <summary>
		/// 进度值：签到天数，进度值
		/// </summary>
		public string value = string.Empty;
	}
	//用于模板生成
	public class activity_message_Pb : MessageProto
	{
		public override MessageProto GetMessagePb() => this;
		private msg_1201 msg;
		bool receive_1201(IProto proto, string ipAddress)//接收客户端数据
		{
			msg = proto as msg_1201;
			Activity mySqlData = new Activity();
			mySqlData.ReadSqlData((uint) msg.roleId);

			Dictionary<int, activity_data> msgDic = new Dictionary<int, activity_data>();
			if (mySqlData.actData.Count > 0)
			{
				msgDic = mySqlData.actData.ToDictionary(p => p.type);
			}

			if (msg.state == 1)//活动请求数据
			{
				int finishId = 0;
				string strValue = string.Empty;
				bool isFinshed = false;
				if (msgDic.ContainsKey(msg.type))
				{
					if (msgDic[msg.type].isFinshed)
					{
						finishId = msgDic[msg.type].configId;
					}
					isFinshed = msgDic[msg.type].isFinshed;
					strValue = msgDic[msg.type].value;
				}

				var eType = (SystemFunctionType)msg.type;
				List<ActivityStage> dataList = HttpDataManager.ins.GetActivity(eType);
				if (dataList.Count > 0)
					dataList = dataList.Where(p => !p.IsExpirate() && p.IsSendPush()&&p.IsSelf(msg.roleId)).Where(p => p.configId != finishId).ToList();
				if (dataList.Count > 0)
				{
					msg.configId = (short)dataList[0].configId;
					msg.creatTime = (uint)dataList[0].createTime;
					msg.expirateTime = (uint)dataList[0].expirateTime;
					msg.paramsValue = dataList[0].paramsValue;
				}
				else
				{
					msg.configId = (short)finishId;
				}

				if (msg.configId != finishId || finishId == 0)//没有完成，或者上次完成id和这次不一致时
				{
					isFinshed = false;
				}

				msgDic[msg.type] = new activity_data() { configId = msg.configId, isFinshed = isFinshed, type = msg.type };
				if (finishId != msg.configId)
				{
					msgDic[msg.type].value = strValue;
				}
				else
				{
					strValue = string.Empty;//进度值，完成
				}

				msg.strValue = strValue;
				msg.isFinshed = isFinshed;

				mySqlData.actData = msgDic.Values.ToList();
				mySqlData.UpdateSqlData();
			}
			else if (msg.state == 2)//完成活动
			{
				msgDic[msg.type].isFinshed = true;
				mySqlData.actData = msgDic.Values.ToList();
				mySqlData.UpdateSqlData();
			}
			else if (msg.state == 3)//活动进度情况
			{
				msgDic[msg.type].value = msg.strValue;
				mySqlData.actData = msgDic.Values.ToList();
				mySqlData.UpdateSqlData();
			}
			else
			{
				return false;
			}

			return true;
		}

		IProto send_1201()//发送给客户端数据
		{
			return msg;
		}
	}
}