using Hukiry.Protobuf;
using Server;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Http
{
    public class FriendInfo
    {
        public List<GFriendInfo> myFriend = new List<GFriendInfo>();
        public List<GFriendInfo> message = new List<GFriendInfo>();
    }

    public class FriendMeta
    {
        /// <summary>
        /// 生命帮助
        /// </summary>
        public List<GFriendInfo> lifeList = new List<GFriendInfo>();
        /// <summary>
        /// 元宇宙挑战
        /// </summary>
        public List<GFriendInfo> metaList = new List<GFriendInfo>();
    }

    //用于模板生成
    public class friend_message_Pb : MessageProto
    {
        public override MessageProto GetMessagePb() => this;
        msg_1501 msg;
        msg_1502 msg2;
        msg_1503 msg3;
        msg_1504 msg4;
        FriendInfo friendInfo;
        FriendMeta friendMeta;

        Friend data;
        LifeMass mass;
        bool receive_1501(IProto proto, string ipAddress)//接收客户端数据
        {
            msg = proto as msg_1501;
            try
            {
                this.RoleID = msg.roleId;
                data = new Friend();
                data.ReadSqlData(msg.roleId);
                
                EFriendState state = (EFriendState)msg.state;

                friendInfo = data.friendInfo;//好友数据
                friendMeta = data.friendMeta;//元宇宙数据

                if (state == EFriendState.Friend)
                {
                    msg.friendInfos = friendInfo.myFriend;
                    msg.friendInfos.ForEach(p =>
                    {
                        p.state = (byte)EFriendHandleState.Friend;
                        UserRole user = new UserRole();
                        user.ReadSqlData(p.roleId);
                        p.headId = (byte)user.headID;
                        p.nick = user.roleNick;
                        p.level = (ushort)user.GetLevel();
                    });//设置为好友状态
                }
                else if (state == EFriendState.Message)
                {
                    msg.friendInfos = friendInfo.message;
                    msg.friendInfos.ForEach(p =>
                    {
                        UserRole user = new UserRole();
                        user.ReadSqlData(p.roleId);
                        p.headId = (byte)user.headID;
                        p.nick = user.roleNick;
                        p.level = (ushort) user.GetLevel();
                    });
                }
                else
                {
                    if (state == EFriendState.Search)
                    {
                        if (msg.friendId > 0)
                        {
                            return this.FindFriend(msg.friendId);
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return this.FindFriend(0);
                    }
                }

            }
            catch (Exception ex)
            {
                GameCenter.Log(ex.ToString());
                return false;
            }
            return true;
        }

        private bool FindFriend(uint id = 0)
        {

            if (id > 0)
            {
                var list = MongoLibrary.ins.FindAll<UserRole>(p => p != null && p.id == id);
                if (list.Count > 0)
                {

                    msg.friendInfos = list.ConvertAll(p => new GFriendInfo()
                    {
                        roleId = p.id,
                        level = (ushort)p.GetLevel(),
                        headId = (byte)p.headID,
                        nick = p.roleNick,
                        mapId = 0,
                        state = 0
                    });

                }
                else
                {
                    return false;
                }
            }
            else
            {
                //陌生人，不包含我的好友，消息列表
                var myFriend = friendInfo.myFriend.ToDictionary(p => p.roleId);
                var message = friendInfo.message.ToDictionary(p => p.roleId);
                var list = MongoLibrary.ins.FindAll<UserRole>(p => p != null).ToList();

                var temp = new List<UserRole>();
                if (list.Count <= 10)
                {
                    temp = list;
                }
                else
                {
                    Random r = new Random();
                    for (int i = 0; i < 10; i++)
                    {
                        int index = r.Next(0, list.Count);
                        temp.Add(list[index]);
                        list.RemoveAt(index);
                    }
                }

                if (temp.Count > 0)
                {
                    temp = temp.Where(p => !myFriend.ContainsKey(p.id) && !message.ContainsKey(p.id)&&p.id!=msg.roleId).ToList();
                }
                msg.friendInfos?.Clear();
                if (temp.Count > 0)
                {
                    msg.friendInfos = temp.ConvertAll(p => new GFriendInfo()
                    {
                        roleId = p.id,
                        level = (ushort)p.GetLevel(),
                        headId = (byte)p.headID,
                        nick = p.roleNick,
                        mapId = 0,
                        state = 0
                    });
                }
            }
            return true;
        }


        IProto send_1501() => msg;

        bool receive_1502(IProto proto, string ipAddress)//接收客户端数据
        {
            msg2 = proto as msg_1502;
            try
            {
                this.RoleID = msg2.roleId;
                data = new Friend();
                data.ReadSqlData(msg2.roleId);

                EFriendHandleType type = (EFriendHandleType)msg2.type;
                EFriendHandleState state = (EFriendHandleState)msg2.state;

                if (type == EFriendHandleType.Friend)//好友
                {
                    if (friendInfo == null)
                    {
                        friendInfo = data.friendInfo;
                    }

                    var myFriend = friendInfo.myFriend.ToDictionary(p => p.roleId);
                    var message = friendInfo.message.ToDictionary(p => p.roleId);
                    if (state == EFriendHandleState.SendAccept)
                    {
                        if (message.Count >= 30)
                        {
                            msg2.state = (byte)EFriendHandleState.OverLimit;
                            return true;
                        }
                    }
                    else if (myFriend.Count >= 30)
                    {
                        msg2.state = (byte)EFriendHandleState.OverLimit;
                        return true;
                    }

                    List<GFriendInfo> messageFriends = new List<GFriendInfo>();
                    switch (state)
                    {
                        case EFriendHandleState.SendAccept:
                            //双方添加消息
                            message[msg2.friendId] = this.GetGFriendInfo(msg2.friendId, EFriendHandleState.SendAccept);
                            messageFriends = this.SendToFriendMsg(msg2.friendId, msg2.roleId, true);
                            break;
                        case EFriendHandleState.CancelAccept:
                            //双方删除消息
                            messageFriends = this.SendToFriendMsg(msg2.friendId, msg2.roleId, false);
                            message.Remove(msg2.friendId);
                            break;
                        case EFriendHandleState.RefuseAccept:
                            //双方删除消息
                            messageFriends = this.SendToFriendMsg(msg2.friendId, msg2.roleId, false);
                            message.Remove(msg2.friendId);

                            break;
                        case EFriendHandleState.Accept:
                            //双方删除消息，并成为好友
                            myFriend[msg2.friendId] = this.GetGFriendInfo(msg2.friendId, EFriendHandleState.Friend);
                            this.SendToMyFriend(msg2.friendId, msg2.roleId, true, messageFriends);
                            message.Remove(msg2.friendId);
                            break;
                        case EFriendHandleState.Delete:
                            //双方删除好友
                            ChatMessage chat = MongoLibrary.ins.Find<ChatMessage>(p => p.roId == msg2.roleId && p.friendId == msg2.friendId);
                            if (chat == null) chat = MongoLibrary.ins.Find<ChatMessage>(p => p.roId == msg2.friendId && p.friendId == msg2.roleId);
                            if (chat != null)
                            {
                                MongoLibrary.ins.Delete<ChatMessage>(chat.id);
                            }
                            messageFriends = this.SendToMyFriend(msg2.friendId, msg2.roleId, false);
                            myFriend.Remove(msg2.friendId);
                            break;
                        default:
                            break;
                    }


                    GameCenter.instance.GetGameServer().PushClient(msg2.friendId, protocal.msg_1501, pr =>
                    {
                        var msgF = pr as msg_1501;
                        msgF.friendInfos = messageFriends;
                        if (state != EFriendHandleState.Delete)
                        {
                            msgF.state = (byte)EFriendState.Message;
                        }
                        else
                        {
                            msgF.state = (byte)EFriendState.Friend;
                            msgF.friendInfos.ForEach(p => p.state = 7);//设置为好友状态
                        }
                    });

                    friendInfo.myFriend = myFriend.Values.ToList();
                    friendInfo.message = message.Values.ToList();
                    data.friendInfo = friendInfo;
                    data.UpdateSqlData();
                }
                else if (type == EFriendHandleType.Challenge)//元宇宙挑战
                {
                    if (friendMeta == null)
                    {
                        friendMeta = data.friendMeta;
                    }
                    var metaList = friendMeta.metaList.ToDictionary(p => p.roleId);

                    switch (state)
                    {
                        case EFriendHandleState.SendAccept:
                            //发送邀请：改变状态
                            metaList[msg2.friendId] = this.GetGFriendInfo(msg2.friendId, EFriendHandleState.SendAccept);
                            this.SendToMeta(msg2.friendId, msg2.roleId, EFriendHandleState.ReceiveAccepted);
                            break;
                        case EFriendHandleState.CancelAccept:
                            //取消邀请：移除
                            metaList.Remove(msg2.friendId);
                            this.SendToMeta(msg2.friendId, msg2.roleId, EFriendHandleState.CancelAccept, true);
                            break;
                        case EFriendHandleState.Accept:
                            //接收邀请：改变状态
                            metaList[msg2.friendId] = this.GetGFriendInfo(msg2.friendId, EFriendHandleState.Accept);
                            this.SendToMeta(msg2.friendId, msg2.roleId, EFriendHandleState.Accept);
                            break;
                        case EFriendHandleState.RefuseAccept:
                            //拒绝邀请：移除
                            metaList.Remove(msg2.friendId);
                            this.SendToMeta(msg2.friendId, msg2.roleId, EFriendHandleState.RefuseAccept, true);
                            break;
                        case EFriendHandleState.Delete:
                            //删除：移除
                            metaList.Remove(msg2.friendId);
                            this.SendToMeta(msg2.friendId, msg2.roleId, EFriendHandleState.Delete, true);
                            break;
                        case EFriendHandleState.Finished:
                            //完成：改变状态
                            metaList[msg2.friendId] = this.GetGFriendInfo(msg2.friendId, EFriendHandleState.Finished);
                            this.SendToMeta(msg2.friendId, msg2.roleId, EFriendHandleState.Finished);
                            break;
                        default:
                            break;
                    }
                    friendMeta.metaList = metaList.Values.ToList();
                    data.UpdateSqlData();
                }
            }
            catch (Exception ex)
            {
                GameCenter.Log(ex.ToString());
                return false;
            }
            return true;
        }

        private GFriendInfo GetGFriendInfo(uint id, EFriendHandleState eState)
        {
            var p = MongoLibrary.ins.Find<UserRole>(p => p.id == id);
            if (p!=null)
            {
                return new GFriendInfo()
                {
                    roleId = p.id,
                    level = (ushort)p.GetLevel(),
                    headId = (byte)p.headID,
                    nick = p.roleNick,
                    mapId = 0,
                    state = (byte)eState
                };
            }
            else
            {
                return null;
            }
        }

        private List<GFriendInfo> SendToFriendMsg(uint roleId, uint friendID, bool isAdd)
        {
            Friend mySqlData = new Friend();
            mySqlData.ReadSqlData(roleId);

            var friend = mySqlData.friendInfo;
            var message = friend.message.ToDictionary(p => p.roleId);
            if (isAdd)
            {
                var info = this.GetGFriendInfo(friendID, EFriendHandleState.ReceiveAccepted);
                if (info != null)
                    message[info.roleId] = info;
            }
            else
            {
                message.Remove(friendID);
            }
            friend.message = message.Values.ToList();
            mySqlData.UpdateSqlData();
            return new List<GFriendInfo>(friend.message);
        }

        private List<GFriendInfo> SendToMyFriend(uint roleId, uint friendId, bool isAdd, List<GFriendInfo> messageList=null)
        {
            Friend mySqlData = new Friend();
            mySqlData.ReadSqlData(roleId);
            var friend = mySqlData.friendInfo;
            var myFriend = friend.myFriend.ToDictionary(p => p.roleId);
            var message = friend.message.ToDictionary(p => p.roleId);
            if (isAdd)
            {
                var info = this.GetGFriendInfo(friendId, EFriendHandleState.Friend);
                if (info != null)
                    myFriend[info.roleId] = info;

               
                if(message!=null)
                    message.Remove(friendId);
                if (messageList != null)
                {
                    messageList.AddRange(message.Values.ToArray());
                }
            }
            else
            {
                myFriend.Remove(friendId);
            }
            friend.myFriend = myFriend.Values.ToList();
            friend.message = message.Values.ToList();
            mySqlData.UpdateSqlData();
            return  new List<GFriendInfo>(friend.myFriend);
        }

      

        private void SendToMeta(uint roleId, uint friendId, EFriendHandleState state, bool isRemove = false)
        {
            Friend mySqlData = new Friend();
            mySqlData.ReadSqlData(roleId);
            var friend = mySqlData.friendMeta;
            var metaList = friend.metaList.ToDictionary(p => p.roleId);
            if (!isRemove)
            {
                var info = this.GetGFriendInfo(friendId, state);
                if (info != null)
                    metaList[info.roleId] = info;
            }
            else
            {
                metaList.Remove(friendId);
            }

            friend.metaList = metaList.Values.ToList();
            mySqlData.UpdateSqlData();
        }

        IProto send_1502() => msg2;

        //好友聊天
        bool receive_1503(IProto proto, string ipAddress)
        {
            msg3 = proto as msg_1503;
            EChatState chatState = (EChatState)msg3.state;
            this.RoleID = msg3.roleId;
            ChatMessage chat = MongoLibrary.ins.Find<ChatMessage>(p => p.roId == msg3.roleId && p.friendId == msg3.friendId);
            if (chat == null) chat = MongoLibrary.ins.Find<ChatMessage>(p => p.roId == msg3.friendId && p.friendId == msg3.roleId);
            if (chat == null)
            {
                chat = this.CreateRoom(msg3.roleId, msg3.friendId);
            }

            if (chatState == EChatState.Chat)
            {
                msg3.chatInfos = chat.chatInfos;
            }
            else if (chatState == EChatState.SendChat)
            {
                if (chat.chatInfos.Count >= 25)
                {
                    chat.chatInfos.RemoveAt(0);
                }

                GChatInfo info = new GChatInfo();
                info.Id = msg3.roleId;
                info.time = GameCenter.GetTimeSecond();
                info.content = msg3.message;
                chat.chatInfos.Add(info);
                chat.UpdateSqlData();
            }

            GameCenter.instance.GetGameServer().PushClient(msg3.friendId, protocal.msg_1503, pr=> {
                var msgF = pr as msg_1503;
                msgF.chatInfos = chat.chatInfos;
                msgF.friendId = msg3.roleId;
                msgF.state = (byte)EChatState.Chat;
            });
            return true;
        }

        /// <summary>
        /// 给每个用户好友设置房间id
        /// </summary>
        private ChatMessage CreateRoom(uint roleId, uint friendId)
        {
            uint roomId = GlobalGameData.ins.CreateChatRoom();
            ChatMessage chat = new ChatMessage();
            chat.ReadSqlData(roomId);
            chat.roId = roleId;
            chat.friendId = friendId;
            chat.UpdateSqlData();
            return chat;
        }

        IProto send_1503() => msg3;


        // 社团
        bool receive_1504(IProto proto, string ipAddress)
        {
            msg4 = proto as msg_1504;
            ELifeState state = (ELifeState)msg4.state;
            if (msg4.gassId == 0)
            {
                UserRole role = new UserRole();
                role.ReadSqlData(msg4.roleId);
                uint gassId = GlobalGameData.ins.CreateMassGroup();
                role.gassId = gassId;
                mass = new LifeMass();
                mass.ReadSqlData(msg4.gassId);
                mass.memberInfos.Add(new GMemeberInfo()
                {
                    roleId = role.id,
                    nick = role.roleNick,
                    level = (ushort)role.GetLevel(),
                    headId = (byte)role.headID,
                    isMasser = true
                });
                mass.UpdateSqlData();
                role.UpdateSqlData();
            }
            else
            {
                try
                {
                    if (mass == null)
                    {
                        mass = new LifeMass();
                        mass.ReadSqlData(msg4.gassId);
                    }

                    var lifeDic = mass.lifeInfos.ToDictionary(p => p.Id);
                    var memberDic = mass.memberInfos.ToDictionary(p => p.roleId);
                    if (state == ELifeState.Gass)
                    {
                        msg4.memeberInfos = mass.memberInfos;
                    }
                    else if (state == ELifeState.AddGass)
                    {
                        if (msg4.memeberInfos.Count >= 30)
                        {
                            msg4.state = (byte)ELifeState.OverLimit;
                            return true;
                        }
                        UserRole role = new UserRole();
                        role.ReadSqlData(msg4.roleId);
                        memberDic[msg4.roleId] = new GMemeberInfo()
                        {
                            roleId = role.id,
                            nick = role.roleNick,
                            level = (ushort)role.GetLevel(),
                            headId = (byte)role.headID,
                            isMasser = false
                        };
                        role.gassId = msg4.gassId;
                        role.UpdateSqlData();
                    }
                    else if (state == ELifeState.QuitGass)
                    {
                        UserRole role = new UserRole();
                        role.ReadSqlData(msg4.roleId);
                        role.gassId = 0;
                        role.UpdateSqlData();
                        memberDic.Remove(msg4.roleId);
                    }
                    else if (state == ELifeState.DeleteGass)
                    {
                        //删除社团，清空用户社团id列表
                        foreach (var item in mass.memberInfos)
                        {
                            UserRole role = new UserRole();
                            role.ReadSqlData(item.roleId);
                            role.gassId = 0;
                            role.UpdateSqlData();
                        }
                        MongoLibrary.ins.Delete<LifeMass>(msg4.gassId);
                    }
                    else if (state == ELifeState.Life)
                    {
                        msg4.lifeInfos = mass.lifeInfos;
                    }
                    else if (state == ELifeState.AskHelp)
                    {
                        UserRole role = new UserRole();
                        role.ReadSqlData(msg4.roleId);
                        lifeDic[msg4.roleId] = new GLifeInfo()
                        {
                            Id = msg4.roleId,
                            state = (byte)state,
                            count = 0,
                            nick = role.roleNick,
                            time = GameCenter.GetTimeSecond()
                        };
                    }
                    else if (state == ELifeState.Help)
                    {
                        lifeDic[msg4.friendId].count++;
                    }
                    else if (state == ELifeState.GetHelp)
                    {
                        lifeDic.Remove(msg4.roleId);
                    }
                    mass.memberInfos = memberDic.Values.ToList();
                    mass.lifeInfos = lifeDic.Values.ToList();
                    mass.UpdateSqlData();
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }

        IProto send_1504() => msg4;
    }
}