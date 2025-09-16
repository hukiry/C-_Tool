namespace Game.Http
{
    public enum LoginState : byte
    {
        None = 0,
        Logout = 1,
        Bind = 2,
        ChangeDevice = 3,
        FixNick = 4,
        ChangeCloudData = 5,

    }

    public enum MailState : byte
    {
        Request = 1,
        Reded = 2
    }

    public enum HttpOprateState : int
    {
        None = 0,
        Add = 1,
        Update = 2,
        Delete = 3,
        Get = 4,
        Push = 5
    }
}
