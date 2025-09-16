namespace Game.Http
{
    //操作状态
    public enum EFriendState : byte
    {
        none = 0,
        //1,我的好友 30个
        Friend = 1,
        //2,好友消息 30个
        Message = 2,
        //3, 陌生人 10个
        Stranger = 3,
        //4, 查找好友
        Search = 4,
        //5, 刷新陌生
        Refresh = 5,
        //6, 生命列表
        Life = 6,
    }

    //1 = 好友，2 = 挑战，3 = 生命
    public enum EFriendHandleType : byte
    {
        /// <summary>
        /// 限制 30人
        /// </summary>
        Friend = 1,
        /// <summary>
        /// 每天限制1人
        /// </summary>
        Challenge = 2
    }

    //0=帮助生命，1=申请好友，2=接受好友，3=邀请好友挑战，4=接受挑战
    public enum EFriendHandleState : byte
    {
        /// <summary>
        /// 陌生人状态
        /// </summary>
        None = 0,
        /// <summary>
        /// 发起好友申请|请求帮助|发起挑战|发送帮助
        /// </summary>
        SendAccept = 1,
        /// <summary>
        /// 取消好友申请|取消发送挑战|取消帮助
        /// </summary>
        CancelAccept = 2,
        /// <summary>
        /// 接受好友申请|获得帮助|接收好友挑战
        /// </summary>
        Accept = 3,
        /// <summary>
        /// 拒绝好友申请|拒绝挑战
        /// </summary>
        RefuseAccept = 4,
        /// <summary>
        /// 删除好友|删除挑战
        /// </summary>
        Delete = 5,
        /// <summary>
        /// 被邀请|收到帮助|收到挑战
        /// </summary>
        ReceiveAccepted = 6,
        /// <summary>
        /// 好友
        /// </summary>
        Friend = 7,
        /// <summary>
        /// 完成挑战|完成帮助
        /// </summary>
        Finished = 8,

        //超出20个限制
        OverLimit = 9,
    }


    public enum EChatState : byte
    {
        //获取聊天
        Chat = 0,
        //发送聊天
        SendChat = 1
    }


    public enum ELifeState : byte
    {
        //社团生命列表
        Life = 0,
        // 询问帮助
        AskHelp = 1,
        // 获取生命
        GetHelp = 2,
        // 帮助好友
        Help = 3,
        //社团列表
        Gass = 4,
        //加入社团
        AddGass = 5,
        //退出社团
        QuitGass = 6,
        //删除社团
        DeleteGass = 7,
        //成员超出上限
        OverLimit=8,
    }
}
