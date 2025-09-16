using System;
using System.IO;
using System.Reflection;

namespace Game.Http
{
    public struct AddressClient {
        public string ip;
        public int port;
        public int back_port;
        public string GetGameServerAdress() => $"{this.ip}:{this.port}";
        public string GetBackServerAdress() => $"http://{this.ip}:{this.back_port}/";
    }

    public class ServerConifg
    {
        private static AddressClient addressClient ;
        private const string configFile = "config/server.ini";
        private static string server_ip;
        private static int server_port, back_port;

        //本地配置表读取
        public static bool ReadConifg()
        {
            string OS = Environment.GetEnvironmentVariable("OS");
            string SystemDrie = Environment.GetEnvironmentVariable("SystemDrive");
            Console.WriteLine($"OS:{OS},SystemDrie:{SystemDrie}");
            if (File.Exists(configFile))
            {
                string[] lines = File.ReadAllLines(configFile);
                foreach (var item in lines)
                {
                    if (item.StartsWith("#"))
                    {
                        continue;
                    }
                    string[] array = item.Split('=');
                    if (array.Length == 2)
                    {
                        FieldInfo fieldInfo = typeof(ServerConifg).GetField(array[0].Trim(), BindingFlags.NonPublic | BindingFlags.Static);
                        if (fieldInfo != null)
                        {
                            if (fieldInfo.FieldType == typeof(string))
                                fieldInfo.SetValue(typeof(ServerConifg), array[1].Trim());
                            else
                            {
                                int.TryParse(array[1].Trim(), out int result);
                                fieldInfo.SetValue(typeof(ServerConifg), result);
                            }
                        }
                    }
                }

                addressClient = new AddressClient()
                {
                    ip = server_ip,
                    port = server_port,
                    back_port = back_port
                };
                return true;
            }
            else
            {
                Console.WriteLine($"文件不存在：{configFile}");
                return false;
            }
        }

        public static AddressClient GetAddressServer() => addressClient;
    }
}
