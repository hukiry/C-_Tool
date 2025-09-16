using Server;
using System;
using System.Reflection;

namespace Game.Http
{
    /// <summary>
    /// 推送消息句柄
    /// </summary>
    /// <param name="proto">协议参数</param>
    public delegate void PushMessageHandle(IProto proto);
    /// <summary>
    /// 后期，三个接口改成抽象方法
    /// </summary>
    public interface IMessage
    {
        /// <summary>
        /// 用户id
        /// </summary>
        uint RoleID { get; set; }
        /// <summary>
        /// 客户端版本
        /// </summary>
        byte version { get; set; }
        /// <summary>
        /// 接收客户端数据
        /// </summary>
        bool Receive(Packet buffer, string ipAddress);

        Packet GetBufferData(short cmd);
        /// <summary>
        /// 推送数据：回调方法中赋值
        /// </summary>
        Packet GetBufferData(short cmd, PushMessageHandle methodCall);
    }

    public abstract class MessageProto : IMessage
    {
        public uint RoleID { get; set; }
        public byte version { get; set; }
        Packet IMessage.GetBufferData(short cmd)
        {
            try
            {
                var _this = this.GetMessagePb();
                MethodInfo method = _this.GetType().GetMethod($"send_{cmd}", BindingFlags.Instance | BindingFlags.NonPublic);
                IProto msg = method?.Invoke(_this, null) as IProto;
                using (ByteBlock byteBlock = new ByteBlock())
                {
                    msg.Write(byteBlock);
                    byte[] buffer = new byte[byteBlock.Len];
                    Array.Copy(byteBlock.Buffer, 0, buffer, 0, buffer.Length);
                    Packet packet = new Packet(cmd, buffer);
                    return packet;
                }
            }
            catch (Exception ex)
            {
                GameCenter.Log(LogType.Error, "读取消息异常" + cmd, ex);
            }
            return Packet.Empty;
        }

        Packet IMessage.GetBufferData(short cmd, PushMessageHandle methodCall)
        {
            try
            {
                var type = Type.GetType($"{nameof(Game)}.{nameof(Game.Http)}.msg_{cmd}");
                IProto msg = Activator.CreateInstance(type) as IProto;
                if (methodCall == null) return new Packet(cmd, new byte[0]);
                methodCall?.Invoke(msg);
                using (ByteBlock byteBlock = new ByteBlock())
                {
                    msg.Write(byteBlock);
                    byte[] buffer = new byte[byteBlock.Len];
                    Array.Copy(byteBlock.Buffer, 0, buffer, 0, buffer.Length);
                    Packet packet = new Packet(cmd, buffer);
                    return packet;
                }
            }
            catch (Exception ex)
            {
                GameCenter.Log(LogType.Error, "读取消息异常" + cmd, ex);
            }
            return new Packet(cmd, new byte[0]);
        }

        bool IMessage.Receive(Packet buffer, string ipAddress)
        {
            bool isSucc = false;
            try
            {
                this.version = buffer.version;
                using (ByteBlock byteBlock = new ByteBlock(Packet.MSG_MAX_SIZE))
                {
                    buffer.WirteByteBlock(byteBlock);
                    int cmd = buffer.cmd;
                    var type = Type.GetType($"{nameof(Game)}.{nameof(Game.Http)}.msg_{cmd}");
                    IProto msg = Activator.CreateInstance(type) as IProto;
                    msg.Read(byteBlock);
                    //保存接收的数据
                    var _this = this.GetMessagePb();
                    MethodInfo method = _this.GetType().GetMethod($"receive_{cmd}", BindingFlags.Instance | BindingFlags.NonPublic);
                    isSucc = (bool)method?.Invoke(_this, new object[] { msg, ipAddress });
                }
            }
            catch (Exception ex)
            {
                GameCenter.Log(LogType.Error, $"receive_{buffer?.cmd},读取消息异常" + ipAddress, ex);
            }
            return isSucc;
        }

        public abstract MessageProto GetMessagePb();

        protected string GetMD5(byte[] buffer)
        {
            var mMD5Provider = System.Security.Cryptography.MD5.Create();
            byte[] dmd5b = mMD5Provider.ComputeHash(buffer);
            System.Text.StringBuilder dmd5t = new System.Text.StringBuilder();
            for (int i = 0; i < dmd5b.Length; i++)
                dmd5t.Append(dmd5b[i].ToString("x2"));
            return dmd5t.ToString();
        }
    }
}
