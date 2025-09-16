using Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace HttpClient
{
    class Program
    {
        static string line = string.Empty;
        static void Main(string[] args)
        {
            Console.WriteLine("输入1退出");
            List<Client> all = new List<Client>();
           
            while (line != "1")
            {
                line = Console.ReadLine();
                if (line.ToLower().StartsWith("post"))
                {
                    ProcessRequest();
                }

                if (line.ToLower().StartsWith("start"))
                {
                    for (int i = 0; i < 10000; i++)
                    {
                        Client client = new Client();
                        client.Connect("192.168.1.4", 1001);
                        all.Add(client);
                    }
                   
                }


                if (line.ToLower().StartsWith("send"))
                {
                    Console.WriteLine("输入要发送的内容");
                    var msg = Console.ReadLine();
                    foreach (var item in all)
                    {
                        if (item.IsConnected())
                        {
                            item.SendPacket(msg);
                        }
                    }
                }
            }
        }

        static void ProcessRequest()
        {
            try
            {
                WebRequest request = WebRequest.Create("http://192.168.1.4:1234/");
                request.Method = WebRequestMethods.Http.Post;
                request.ContentType = "application/x-www-form-urlencoded";
                request.Timeout = 3 * 1000;
                var argsBuffer = Encoding.UTF8.GetBytes("{key:6}");
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(argsBuffer, 0, argsBuffer.Length);
                }

                //返回数据
                WebResponse response = request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                StreamReader myreader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string retString = myreader.ReadToEnd();

                Console.WriteLine("v:" + retString);
            }
            catch (WebException ex)
            {
                Console.WriteLine(ex);
            }

        }
    }
}
