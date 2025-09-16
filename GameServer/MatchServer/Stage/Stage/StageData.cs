using Newtonsoft.Json;
using System.Collections.Generic;
/// <summary>
/// 配置数据
/// </summary>
namespace Game.Http
{
    /**********************************************后台数据表*************************************************************/
    /// <summary>
    /// 邮件和公告
    /// </summary>
    public class MailBillboardStage : EntityBase<MailBillboardStage>
    {
        public int roleId;
        public int pushState;//推送状态
        public string title = string.Empty;
        public string content = string.Empty;
        public string rewards = string.Empty;//邮件奖励
        public string lanCode = string.Empty;//公告语言代码

        public bool IsExpirate(long regeditTime) => GameCenter.GetTimeSecond() > this.expirateTime || regeditTime > this.createTime;
        public bool IsSendPush() => this.pushState == 1;
        public bool IsSelf(int roleId) => this.roleId == 0 || this.roleId == roleId;

        public override void ChildReadData(MailBillboardStage data)
        {
            this.roleId = data.roleId;
            this.pushState = data.pushState;
            this.title = data.title;
            this.content = data.content;
            this.rewards = data.rewards;
            this.lanCode = data.lanCode;
        }

        public override void ChildCreateData()
        {
            this.UpdateSqlData();
        }
    }

    /// <summary>
    /// 所有活动数据
    /// </summary>
    public class ActivityStage : EntityBase<ActivityStage>
    {
        public int roleId;//指定用户推送

        public int pushState;//推送状态
        public string paramsValue;

        public bool IsExpirate() => GameCenter.GetTimeSecond() > this.expirateTime;
        public bool IsSendPush() => this.pushState == 1;

        public bool IsSelf(int roleId) => this.roleId == 0 || this.roleId == roleId;
        /// <summary>
        /// 是否在活动时间内
        /// </summary>
        public bool IsActivityTime()
        {
            long curTime = GameCenter.GetTimeSecond();
            return curTime >= this.createTime && curTime < this.expirateTime;
        }

        public override void ChildReadData(ActivityStage data)
        {
            this.roleId = data.roleId;
            this.pushState = data.pushState;
            this.paramsValue = data.paramsValue;
        }
        public override void ChildCreateData()
        {
            this.UpdateSqlData();
        }
    }

    /***********************************************用户数据表************************************************************/
    public class MailBillboard : EntityBase<MailBillboard>
    {
        public List<mail_lifeData> mailData = new List<mail_lifeData>();
        public override void ChildReadData(MailBillboard data){
            this.mailData = data.mailData;
        }
    }

    public class Activity : EntityBase<Activity>
    {
        public List<activity_data> actData =new List<activity_data>();
        public override void ChildReadData(Activity data){
            this.actData = data.actData;
        }
    }

    //生命社团
    public class LifeMass : EntityBase<LifeMass>
    {
        public List<GMemeberInfo> memberInfos = new List<GMemeberInfo>();
        public List<GLifeInfo> lifeInfos = new List<GLifeInfo>();
        public override void ChildReadData(LifeMass data)
        {
            this.lifeInfos = data.lifeInfos;
        }
    }

    //聊天，id为聊天房间
    public class ChatMessage : EntityBase<ChatMessage>
    {
        public uint roId;
        public uint friendId;
        public List<GChatInfo> chatInfos = new List<GChatInfo>();
        public override void ChildReadData(ChatMessage data)
        {
            this.chatInfos = data.chatInfos;
            this.roId = data.roId;
            this.friendId = data.friendId;
        }
    }

    //每个用户的好友
    public class Friend : EntityBase<Friend>
    {
        public int count = 0;//成员数量
        public string lanCode = string.Empty;//国家语言
        public string jsaonMeta = string.Empty;

        public FriendInfo friendInfo = new FriendInfo ();
        public FriendMeta friendMeta = new FriendMeta();
        public override void ChildReadData(Friend data)
        {
            this.count = data.count;
            this.lanCode = data.lanCode;
            this.jsaonMeta = data.jsaonMeta;

            this.friendInfo = data.friendInfo;
            this.friendMeta = data.friendMeta;
        }
    }

    /// <summary>
    /// 反馈
    /// </summary>
    public class Feedback : EntityBase<Feedback>
    {
        public List<Feedback_Data> list = new List<Feedback_Data>();
        public override void ChildReadData(Feedback data)
        {
            this.list = data.list;
        }
    }

    /// <summary>
    /// 用户查询
    /// </summary>
    public class UserRole : EntityBase<UserRole>
    {
        public string deviceId = string.Empty;//设备唯一标示符
        public string token = string.Empty;//用户
        public string lanCode = string.Empty;//语言代码
        public string roleNick = "Match";
        public string ipAddress = string.Empty;
        /// <summary>
        /// 绑定的账号类型
        /// </summary>
        public byte bindAccount = 0;

        /// <summary>
        /// 当前的生命时间
        /// </summary>
        public uint curLifeTime = 0;
        /// <summary>
        /// 上传数据的时间戳
        /// </summary>
        public uint timeStamp = 0;
        /// <summary>
        /// 用于切换设备
        /// </summary>
        public byte state = 0;
        /// <summary>
        /// 货币数据集合
        /// </summary>
        public List<itemsResource> items = new List<itemsResource>();
        public short headID = 0;
        public List<itemsMeta> metaMapIds = new List<itemsMeta>();
        public uint gassId;
        public void ReadUserData(uint id) {
            if (id > 0)
            {
                var data = MongoLibrary.ins.Find(p => p.id == id, this);
                if (data != null)
                {
                    base.ReadSqlData(id);
                }
            }
            else
            {
                base.ReadSqlData(p => p.deviceId == this.deviceId);
            }
        }

        public override void ChildReadData(UserRole data)
        {
            if (this.id == 0)
            {
                this.id = data.id;
            }
            this.deviceId = data.deviceId;//设备唯一标示符
            this.token = data.token;//用户
            this.lanCode = data.lanCode;//语言代码
            this.roleNick = data.roleNick;
            this.ipAddress = data.ipAddress;
            this.bindAccount = data.bindAccount;

            this.curLifeTime = data.curLifeTime;
            this.timeStamp = data.timeStamp;
            this.state = data.state;
            this.items = data.items;
            this.headID = data.headID;
            this.metaMapIds = data.metaMapIds;
            this.gassId = data.gassId;
        }

        public override void ChildCreateData()
        {
            this.CreateData();
        }

        public void ClearData()=> MongoLibrary.ins.DeleteUserData(this.id);

        /// <summary>
        /// 绑定token
        /// </summary>
        /// <param name="token"></param>
        /// <returns>true 切换设备，否则绑定</returns>
        public bool CheckBind(string token, string newDeviceId, uint curRoleId)
        {
            this.token = token;
            this.deviceId = newDeviceId;
            bool isHasData = false;
            UserRole data = MongoLibrary.ins.Find<UserRole>(p => p.token == this.token);
            string deviceRemoteID = string.Empty;
            if (data!=null)
            {
                this.id = data.id;
                this.createTime = data.createTime;
                this.expirateTime = data.expirateTime;
                this.ipAddress = data.ipAddress;
                deviceRemoteID = data.deviceId;
                //处理上一次数据的逻辑
                isHasData = true;
            }


            if (isHasData && !string.IsNullOrEmpty(deviceRemoteID) && this.id != curRoleId)
            {
                MongoLibrary.ins.DeleteUserData((uint)curRoleId);
                //通知给远程的设备id 下线
                GameCenter.AddOffLine(deviceRemoteID);
                return true;
            }

            return false;
        }
        public bool IsCanAccess()
        {
            int device = GameCenter.GetOfflineDeviceId(this.deviceId, false);
            return device == 0;
        }
        private void CreateData()
        {
            GameCenter.Log("this : " + this.id);
            this.createTime = GameCenter.GetTimeSecond();//创建时间
            this.id = GlobalGameData.ins.CreateUser();//创建角色id
            this.curLifeTime = 1 * 5 * 3600;
            this.token = string.Empty;
            this.headID = 1;
            this.roleNick = "Match Default";

            this.timeStamp = GameCenter.GetTimeSecond();
            for (EMoneyType i = EMoneyType.gun; i <= EMoneyType.integral; i++)
            {
                this.items.Add(new itemsResource()
                {
                    type = (byte)i,
                    number = i switch
                    {
                        EMoneyType.life => 5,
                        EMoneyType.lifehour => 1,
                        EMoneyType.lifeMax => 5,
                        EMoneyType.level => 1,
                        EMoneyType.metaExpendNum => 1,
                        _ => 0
                    },
                });
            }

            this.metaMapIds.Add(new itemsMeta() { numberId=0, state=0 });
            this.UpdateSqlData();
        }

        public int GetLevel() {
            var item = this.items.Find(p => p.type == (int)EMoneyType.level);
            return item != null ? item.number : 1;
        }

        public static implicit operator Rank_Data(UserRole user)
        {
            Rank_Data sys = new Rank_Data();
            sys.roleId = (int)user.id;
            sys.level = user.GetLevel();
            sys.lanCode = user.lanCode;
            sys.roleNick = user.roleNick;
            sys.headId = user.headID;
            return sys;
        }
    }

    /// <summary>
    /// 充值查询
    /// </summary>
    public class Recharge : EntityBase<Recharge>
    {
        public List<recharge_data> chargeData = new List<recharge_data>();
        public override void ChildReadData(Recharge data)
        {
            this.chargeData = data.chargeData;
        }
    }

    public class MetaDb : EntityBase<MetaDb>
    {
        public override void ChildReadData(MetaDb data){}
    }

    /***********************************************客户数据************************************************************/
    public class mail_lifeData
    {
        public int id;
        public string nick = string.Empty;
    }

    public class recharge_data
    {
        public int id;//最新充值的商品id

        public int shopId;
        public string t = string.Empty;//最新充值时间
        public string price = string.Empty;//最新充值价格
        /// <summary>
        /// 价格总数
        /// </summary>
        public int total;
    }

    public class Rank_Data
    {
        public int roleId = 0;
        public string roleNick = "——";
        public int level;//通过最新关卡
        public string lanCode = string.Empty;
        public short headId = 0;
    }

    public class Feedback_Data
    {
        /// <summary>
		/// 角色id
		/// </summary>
		public int roleId = 0;
        /// <summary>
        /// 反馈问题类型：1=游戏问题，2=崩溃问题，3=充值问题，4=其他问题
        /// </summary>
        public byte type = 0;
        /// <summary>
        /// 反馈描述：30个字
        /// </summary>
        public string content = string.Empty;
        /// <summary>
        /// 电子邮件
        /// </summary>
        public string e_mail = string.Empty;

        public uint expirateTime = 0;
    }
}
