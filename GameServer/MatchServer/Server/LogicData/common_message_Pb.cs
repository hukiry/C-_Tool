using Newtonsoft.Json;
using Server;
using System;
using System.Collections.Generic;

namespace Game.Http
{
    //用于模板生成
    /// select 选择字段
    /// from 表名
    /// where 查询条件
    /// group by 分组条件
    /// order by 降序查询
    public class common_message_Pb: MessageProto
    {
        public override MessageProto GetMessagePb() => this;
		msg_1301 msg;
		bool receive_1301(IProto proto, string ipAddress)//排行榜
		{
			msg = proto as msg_1301;
            try
            {
                //1=月排行，2=年排行, 3，总排行
                List<Rank_Data> dataList = HttpDataManager.ins.GetRank(msg.type);
                msg.jsonDatas = JsonConvert.SerializeObject(dataList);
            }
            catch (Exception ex)
            {
				GameCenter.Log(ex.ToString());
				return false;
            }
			return true;
		}

		IProto send_1301()//发送给客户端数据
		{
			return msg;
		}

		bool receive_1302(IProto proto, string ipAddress)//客户反馈
		{
			msg_1302 msg = proto as msg_1302;
			try
			{
				Feedback data = new Feedback();
				data.ReadSqlData((uint)msg.roleId);
				if (data.list.Count > 30)
				{
					data.list.RemoveAt(0);
				}
				data.list.Add(new Feedback_Data()
				{
					roleId = msg.roleId,
					expirateTime = GameCenter.GetTimeSecond(),
					e_mail = msg.e_mail,
					content = msg.content,
					type = msg.type
				});
				data.UpdateSqlData();
			}
			catch (Exception ex)
			{
				GameCenter.Log(ex.ToString());
				return false;
			}
			return true;
		}

		IProto send_1302()//发送给客户端数据
		{
			return new msg_1301();
		}
	}
}