using Server;
using System.Linq;

namespace Game.Http
{
    //用于模板生成
    public class mail_message_Pb: MessageProto
    {
        public override MessageProto GetMessagePb() => this;
        msg_1101 msg101;
		bool receive_1101(IProto proto, string ipAddress)//接收客户端数据
		{
			msg101 = proto as msg_1101;
            try
            {
                if (msg101.type == 1)//邮件
                {
                    MailBillboard data = new MailBillboard();
                    data.ReadSqlData((uint)msg101.roleId);
                    if (msg101.state == (byte)MailState.Reded)
                    {
                        //邮件id
                        int index = data.mailData.FindIndex(p => p.id == msg101.id);
                        if (index >= 0)
                        {
                            data.mailData[index].id = msg101.id + 1000000;
                        }
                    }
                    else
                    {

                        var regeditTime = data.createTime;
                        var mails = HttpDataManager.ins.GetMailList();
                        //读取后台数据表，根据邮件结束时间，用户注册时间获取邮件
                        var selectMails = mails.Where(p =>
                        {
                            bool isOk = !p.IsExpirate(regeditTime) && p.IsSendPush() && p.IsSelf(msg101.roleId);
                            return isOk;
                        })?.Select(p => new mailData()
                        {
                            id = (short)p.id,
                            title = p.title,
                            content = p.content,
                            rewards = p.rewards,
                        })?.ToList();

                        msg101.mails = selectMails.Where(p => data.mailData.Count == 0 || data.mailData.FindIndex(p1 => p1.id == p.id + 1000000) < 0).ToList();
                        var ids = msg101.mails.Select(p => (int)p.id).ToList();
                        ids.ForEach(p =>
                        {
                            if (data.mailData.FindIndex(p1=>p1.id ==p + 1000000)<0)
                            {
                                data.mailData.Add(new mail_lifeData() { id=p});
                            }
                        });
                    }
                    data.UpdateSqlData();
                }
                else if (msg101.type == 2)//公告
                {
                    UserRole userdata = new UserRole();
                    userdata.ReadUserData((uint) msg101.roleId);
                    var regeditTime = userdata.createTime;

                    var mails = HttpDataManager.ins.GetBillboard();
                    //读取后台数据表，根据邮件结束时间，用户注册时间获取邮件
                    msg101.mails = mails.Where(p => !p.IsExpirate(regeditTime) && p.IsSendPush() && p.lanCode.Equals(msg101.lanCode))?.Select(p => new mailData()
                    {
                        id = (short)p.id,
                        title = p.title,
                        content = p.content,
                        rewards = p.rewards,
                    }).ToList();
                }
                else
                {
                    return false;
                }
            }
            catch (System.Exception ex)
            {
                GameCenter.Log(ex.ToString());
                return false;
            }
			return true;
		}

		IProto send_1101()//发送给客户端数据
		{
			IProto msg = new msg_1101()
			{
				type = msg101.type,
				state = msg101.state,
				mails = msg101.mails
			};
			return msg;
		}
    }
}