using Server;

namespace Game.Http
{
    //用于模板生成
    public class meta_message_Pb : MessageProto
    {
        public override MessageProto GetMessagePb() => this;
        msg_1601 msg;
        bool receive_1601(IProto proto, string ipAddress)//接收客户端数据
        {
            msg = proto as msg_1601;
            EMetaMessageState state = (EMetaMessageState)msg.state;
            MetaDb metaDb = new MetaDb();
            //metaDb.ReadSqlData()
            //地图数据：所属地图和id
            //消息数据：所属id
            switch (state)
            {
                case EMetaMessageState.battle:
                    
                    break;
                case EMetaMessageState.sendChallenge:
                    break;
                case EMetaMessageState.like:
                    break;
                case EMetaMessageState.comment:
                    break;
                case EMetaMessageState.finished:
                    break;
                default:
                    break;
            }
            return true;
        }

        IProto send_1601()//发送给客户端数据
        {
            return msg;
        }
    }


    public enum EMetaMessageState
    {
        //0,对战消息
        battle = 0,
        //1,发起挑战
        sendChallenge = 1,
        //2,点赞
        like = 2,
        //3,评论
        comment = 3,
        //4，完成挑战
        finished = 4,

    }

}