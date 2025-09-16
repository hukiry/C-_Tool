using Server;
using System;
using System.Collections.Generic;

namespace Game.Http
{
    /// <summary>
    /// 服务器广播，后台数据处理，http数据处理，客户端踢人+查询
    /// </summary>
    public class GameCenter
    {
        public static GameCenter instance { get; } = new GameCenter();
        private readonly GameControl gameControl;
        private readonly MatchServer httpServer;
        private readonly GameTcpServer gameServer;

        private static Dictionary<string, int> userBindDic = new Dictionary<string, int>();
      
        private GameCenter() {
            //1，启动控制台命令行
            gameControl = new GameControl();
            //2，启动http服务
            httpServer = new MatchServer();
            //3，启动游戏服
            gameServer = new GameTcpServer();
        }

        /// <summary>
        /// 清除数据表
        /// </summary>
        public void ClearSql()
        {
            MongoLibrary.ins.InitLibrary();
            MongoLibrary.ins.DeleteAllTable();
        }

        public GameControl GetGameControl()=> this.gameControl;
        public MatchServer GetHttpServer() => this.httpServer;

        public GameTcpServer GetGameServer() => this.gameServer;


        public static void Log(LogType logType, string message, Exception exception=null)
        {
            if (instance.httpServer.GetHttpService() != null)
            {
                instance.httpServer.GetHttpService().Logger.Log(logType, null, message, exception);
            }
        }

        public static void Log(LogType logType, object source, string message, Exception exception = null)
        {
            if (instance.httpServer.GetHttpService() != null)
            {
                instance.httpServer.GetHttpService().Logger.Log(logType, source, message, exception);
            }
        }

        public static void Log(string message)
        {
            if (instance.httpServer.GetHttpService() != null)
            {
                instance.httpServer.GetHttpService().Logger.Info(message);
            }
        }

        public static void AddOffLine(string deviceId)
        {
            userBindDic[deviceId] = 1;
        }

        public static int GetOfflineDeviceId(string deviceId, bool isRemove)
        {
            int id = 0;
            if (userBindDic.ContainsKey(deviceId))
            {
                id = userBindDic[deviceId];
                if (isRemove)
                {
                    userBindDic.Remove(deviceId);
                }
            }
            return id;
        }

        /// <summary>
        /// 获取时间戳 秒
        /// </summary>
        public static uint GetTimeSecond()
        {
            TimeSpan ts = new TimeSpan(DateTime.Now.ToUniversalTime().Ticks - new DateTime(1970, 1, 1).Ticks);
            long t = ts.Ticks / TimeSpan.TicksPerSecond;
            return (uint)t;
        }

        /// <summary>
        /// 通过日期获取时间戳
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static int GetTimestamp(DateTime date)
        {
            TimeSpan ts = new TimeSpan(date.Ticks - new DateTime(1970, 1, 1).Ticks);
            long t = ts.Ticks / TimeSpan.TicksPerSecond;
            return (int)t;
        }

        /// <summary>
        /// 获取时间戳 毫秒
        /// </summary>
        public static long GetTimeMilliseconds()
        {
            TimeSpan ts = new TimeSpan(DateTime.Now.ToUniversalTime().Ticks - new DateTime(1970, 1, 1).Ticks);
            return (long)ts.TotalMilliseconds;
        }

        public static DateTime GetDateTime(long timetamp)
        {
            DateTime dateTime = new DateTime(1970, 1, 1);
            dateTime = dateTime.AddSeconds(timetamp);
            return dateTime;
        }

        /// <summary>
        /// 是当天时间
        /// </summary>
        /// <param name="timetamp">时间戳</param>
        /// <returns></returns>
        public static bool IsCurrentDay(long timetamp)
        {
            var todyData = DateTime.Now.ToUniversalTime();
            var lastData = GetDateTime(timetamp);
            return todyData.Year== lastData.Year&&todyData.Month==lastData.Month&&todyData.Day==todyData.Day;
        }

        public static bool IsCurrentMonth(long timetamp)
        {
            var todyData = DateTime.Now.ToUniversalTime();
            var lastData = GetDateTime(timetamp);
            return todyData.Year == lastData.Year && todyData.Month == lastData.Month;
        }

       
    }
}
