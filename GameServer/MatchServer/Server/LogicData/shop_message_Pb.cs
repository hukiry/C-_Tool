using Server;
using System.Linq;
namespace Game.Http
{
    //用于模板生成
    public class shop_message_Pb: MessageProto
    {
        public override MessageProto GetMessagePb() => this;
		private int shopId;
		private byte state;
		bool receive_1401(IProto proto, string ipAddress)//接收客户端数据
		{
			msg_1401 msg = proto as msg_1401;
			this.shopId = msg.shopId;
			this.state = msg.state;

            Recharge data = new Recharge();
            data.ReadSqlData((uint)msg.roleId);
            data.createTime = GameCenter.GetTimeSecond();
            string showPrice = string.Format("${0:f2}", msg.price / 100.0);
            var time = GameCenter.GetDateTime(data.createTime);
			string t = time.ToShortDateString() + " " + time.ToLongTimeString();
			var dic = data.chargeData.ToDictionary(p => p.shopId);
			if (dic.ContainsKey(msg.shopId))
			{
				dic[msg.shopId].total += msg.price;
				dic[msg.shopId].price = showPrice;
				dic[msg.shopId].t = t;
			}
			else
			{
				dic[msg.shopId] = new recharge_data() {
					id = msg.roleId,
					shopId = msg.shopId,
					total = msg.price,
					price = showPrice,
					t = t };
			}

			data.chargeData = dic.Values.ToList();
			data.UpdateSqlData();
			return true;
		}

		IProto send_1401()//发送给客户端数据
		{
			IProto msg = new msg_1401()
			{
				shopId = this.shopId,
				state = this.state
			};
			return msg;
		}
    }
}