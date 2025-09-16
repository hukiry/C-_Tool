using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Server
{
    public class HttpServer
    {
        private HttpListener httpobj;
        private Queue<HttpListenerContext> httpListeners;
        private SocketTimer timer;
        public static HttpServer ins { get; } = new HttpServer();
        public Logger Logger { get; } = new Logger();
        private Action<HttpListenerRequest, HttpListenerResponse> handleGet;
        private Action<HttpListenerRequest, HttpListenerResponse> handlePost;
        public HttpServer StartListener(string prefixes)
        {
            if (!HttpListener.IsSupported)
            {
                Console.WriteLine("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                return null;
            }
            httpListeners = new Queue<HttpListenerContext>();


            //提供一个简单的、可通过编程方式控制的 HTTP 协议侦听器。此类不能被继承。
            httpobj = new HttpListener();
            httpobj.UnsafeConnectionNtlmAuthentication = true;
            //定义url及端口号，通常设置为配置文件
            httpobj.Prefixes.Add(prefixes);
            //启动监听器
            httpobj.Start();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Start Listener...：{DateTime.Now.ToString()}");
            //异步监听客户端请求，当客户端的网络请求到来时会自动执行Result委托
            //该委托没有返回值，有一个IAsyncResult接口的参数，可通过该参数获取context对象
            httpobj.BeginGetContext(AsyncCallResult, null);

            timer = new SocketTimer(20, OnTimeUpdate);
            timer.Start();
            return this;
        }
        public HttpServer Awake(Action<HttpListenerRequest, HttpListenerResponse> handleGet, Action<HttpListenerRequest, HttpListenerResponse> handlePost)
        {
            this.handleGet = handleGet;
            this.handlePost = handlePost;
            return this;
        }

        public void Close()
        {
            httpobj.Close();
            timer.Stop();
        }

        private void OnTimeUpdate(DateTime sender)
        {
            lock (httpListeners)
            {
                if (httpListeners.Count > 0)
                {
                    var context = httpListeners.Dequeue();
                    if (context == null) return;
                    var request = context.Request;
                    var response = context.Response;
                    if (request.HttpMethod == "POST" && request.InputStream != null)
                    {
                        this.handleGet(request, response);
                    }
                    else
                    {
                        if (request.UrlEqual("favicon.ico"))
                        {
                            return;
                        }
                        this.handlePost(request, response);
                    }
                }
            }
        }

        private void AsyncCallResult(IAsyncResult ar)
        {
            //继续异步监听
            httpobj.BeginGetContext(AsyncCallResult, null);
            var guid = Guid.NewGuid().ToString();
            //获得context对象
            var context = httpobj.EndGetContext(ar);
            var request = context.Request;
            var response = context.Response;
            httpListeners.Enqueue(context);
        }

        private void ExampleIcon(HttpListenerResponse response)
        {
            try
            {
                var returnByteArr = File.ReadAllBytes("favicon.ico");//可以是任意图片格式
                using (var stream = response.OutputStream)
                {
                    //把处理信息返回到客户端
                    stream.Write(returnByteArr, 0, returnByteArr.Length);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"网络蹦了：{ex.ToString()}");
            }
        }

        private void ExampleGet(HttpListenerRequest request, HttpListenerResponse response)
        {
            response.AppendHeader("Access-Control-Allow-Origin", "*");//后台跨域请求，通常设置为配置文件
            response.AppendHeader("Access-Control-Allow-Headers", "*");//后台跨域参数设置，通常设置为配置文件
            response.AppendHeader("Access-Control-Allow-Method", "*");//后台跨域请求设置，通常设置为配置文件
            string returnObj = "Null";
            var reslutKey = request.QueryString.Get("key");
            var reslutAge = request.QueryString.Get("age");
            Console.WriteLine(reslutKey + "," + reslutAge);
            if (request.UrlEqual("json"))
            {
                returnObj = $"{{网络蹦了key:1,age:2}}";
                response.FromJson().Send(returnObj);
            }
            else if (request.UrlEqual("text"))
            {
                returnObj = $"在接收数据时发生错误";
                response.FromText().Send(returnObj);//添加响应头信息

            }
            else if (request.UrlEqual("html"))
            {

                var sendStr = "html数据";
                returnObj = $"<html>" +
                   $"<head>" +
                   $"<meta charset=\"utf-8\">" +
                   $"<meta name=\"color-scheme\" content=\"light dark\">" +
                   $"</head>" +
                   $"<body>" +
                   $"{sendStr}" +
                   $"</body>" +
                   $"</html>";
                response.FromHtml().Send(sendStr);//添加响应头信息
            }
            else if (request.UrlEqual("xml"))
            {
                response.FromXml().Send("" +
                    "<message>" +
                    "   SDFSDF" +
                    "</message>");//添加响应头信息  
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{returnObj},请求处理完成：{Guid.NewGuid()},时间：{ DateTime.Now.ToString()}\r\n");
        }

        private void ExampleRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            //response.AppendHeader("Access-Control-Allow-Origin", "*");//后台跨域请求，通常设置为配置文件
            //response.AppendHeader("Access-Control-Allow-Headers", "*");//后台跨域参数设置，通常设置为配置文件
            //response.AppendHeader("Access-Control-Allow-Method", "*");//后台跨域请求设置，通常设置为配置文件
            string data = null;
            //Console.WriteLine($"ContentType:{request.ContentType},{request.Url}");
            try
            {
                var byteList = new List<byte>();
                var byteArr = new byte[2048];
                int readLen = 0;
                int len = 0;
                //接收客户端传过来的数据并转成字符串类型
                do
                {
                    readLen = request.InputStream.Read(byteArr, 0, byteArr.Length);
                    len += readLen;
                    byteList.AddRange(byteArr);
                } while (readLen != 0);
                data = Encoding.UTF8.GetString(byteList.ToArray(), 0, len);

                //获取得到数据data可以进行其他操作
            }
            catch (Exception ex)
            {
                response.StatusDescription = "404";
                response.StatusCode = 404;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"在接收数据时发生错误:{ex.ToString()}");
            }
            //response.StatusDescription = "200";//获取或设置返回给客户端的 HTTP 状态代码的文本说明。
            //response.StatusCode = 200;// 获取或设置返回给客户端的 HTTP 状态代码。
            //Console.ForegroundColor = ConsoleColor.Green;
            //Console.WriteLine($"接收数据完成:{data.Trim()},时间：{DateTime.Now.ToString()}");

            //try
            //{
            //    byte[] returnByteArr = Encoding.UTF8.GetBytes("设置客户端返回信息的编码");//设置客户端返回信息的编码
            //    using (var stream = response.OutputStream)
            //    {
            //        //把处理信息返回到客户端
            //        stream.Write(returnByteArr, 0, returnByteArr.Length);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Console.ForegroundColor = ConsoleColor.Red;
            //    Console.WriteLine($"网络蹦了：{ex.ToString()}");
            //}
        }
    }
}
