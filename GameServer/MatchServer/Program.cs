using Game.Http;
using Hukiry.Table;
using Server;
using System;

namespace Game
{
    class Program
    {
        static string information = @"
* -----------MatchServer----------------------
* 1, 输入 q直接暂停log 打印
* 2，输入 start 启动服务器,自动开启打印
* 3，输入 stop 关闭服务器
* 4，输入 log open/close 打开或关闭日志打印
* 5, 输入 exit 退出服务器
* 6，输入 clearsql 清空数据库
";
        static bool IsLoopInput;
        //启动dll 程序
        static void Main(string[] args)
        {
           
            if (ServerConifg.ReadConifg())
            {
                TableManager.ins.InitTable();
                IsLoopInput = true;
                Console.WriteLine(information);
                while (IsLoopInput)
                {
                    Console.Write("请输入：");
                    try
                    {
                        string input = Console.ReadLine();
                        if (string.IsNullOrEmpty(input)) continue;
                        input = input.Trim();
                        string[] array = input.Split(' ');
                        int len = array.Length;

                        switch (len)
                        {
                            case 2:
                                if (array[0].ToLower().Equals("log"))
                                {
                                    if (array[1].ToLower().Equals("open"))
                                    {
                                        GameCenter.instance.GetGameControl().OpenLog();
                                    }
                                    else if (array[1].ToLower().Equals("close"))
                                    {
                                        GameCenter.instance.GetGameControl().CloseLog();
                                    }
                                }
                                break;
                            case 1:
                                if (array[0].ToLower().Equals("start"))
                                {
                                    if (GameCenter.instance.GetGameControl().IsEnable())
                                    {
                                        Console.Write("已启动");
                                    }
                                    else
                                    {
                                        GameCenter.instance.GetGameControl().EnableServer();
                                    }
                                }
                                else if (array[0].ToLower().Equals("stop"))
                                {
                                    GameCenter.instance.GetGameControl().StopServer();
                                }
                                else if (array[0].ToLower().Equals("q"))
                                {
                                    GameCenter.instance.GetGameControl().CloseLog();
                                }
                                else if (array[0].ToLower().Equals("exit"))
                                {
                                    if (GameCenter.instance.GetGameControl().IsEnable())
                                    {
                                        GameCenter.instance.GetGameControl().StopServer();
                                    }
                                    IsLoopInput = false;
                                }
                                else if (array[0].ToLower().Equals("clearsql"))
                                {
                                    if (GameCenter.instance.GetGameControl().IsEnable())
                                    {
                                        Console.Write("服务器正在运行中...请停服清空表数据\n");
                                    }
                                    else
                                    {
                                        GameCenter.instance.ClearSql();
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    catch
                    {
                        Console.WriteLine("输入异常，  继续");
                    }
                }

                Console.WriteLine("退出游戏服成功,请输入  ./MatchServer  继续");
            }
        }
    }
}
