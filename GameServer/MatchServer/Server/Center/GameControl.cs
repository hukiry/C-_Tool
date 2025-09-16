using System;

namespace Game.Http
{
    /// <summary>
    /// 服务器命令行，控制开发者的输入命令
    /// </summary>
    public class GameControl
    {
        private bool isEnableServer = false;
        public GameControl()
        {
            /*
             * 1, 输入 q直接暂停log 打印
             * 2，输入 start 启动服务器,自动开启打印
             * 3，输入 stop 关闭服务器
             * 4，输入 send all ...广播消息
             * 5，输入 log .. 查询日志消息
             * 6，输入 sql ..查看数据库
             */
        }

        //启动服务器
        public void EnableServer()
        {
            isEnableServer = true;
            Console.WriteLine("--启动MongoDB");
            MongoLibrary.ins.InitLibrary();
           
            //后台服务器
            Console.WriteLine("--启动后台服务器");
            GameCenter.instance.GetHttpServer().EnableSerer();
           
            //游戏服务器
            Console.WriteLine("--启动游戏服务器");
            GameCenter.instance.GetGameServer().EnableSerer();

            //读取全局
            GlobalGameData.ins.ReadSQL();
        }

        public void StopServer()
        {
            isEnableServer = false;
            GameCenter.instance.GetHttpServer().CloseServer();
            GameCenter.instance.GetGameServer().CloseServer();
            Console.WriteLine("关闭服务器");
        }

        public void CloseLog()
        {
            Console.WriteLine("关闭 Log");
            GameCenter.instance.GetHttpServer().SetEnable(false);
            GameCenter.instance.GetGameServer().SetEnable(false);
        }

        public void OpenLog()
        {
            Console.WriteLine("启动 Log");
            GameCenter.instance.GetHttpServer().SetEnable(true);
            GameCenter.instance.GetGameServer().SetEnable(true);
        }

        public bool IsEnable() => isEnableServer;
    }
}
