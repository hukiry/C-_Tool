using Server;

namespace Game.Http
{
    //用于模板生成
    public class error_message_Pb: MessageProto
    {
        public override MessageProto GetMessagePb() => this;

        private readonly string DATA_TABLE = string.Empty;//SQLTable
		bool receive_1901(IProto proto, string ipAddress)//接收客户端数据
		{
			msg_1901 msg = proto as msg_1901;
			return true;
		}

		IProto send_1901()=>new msg_1901();
    }
}