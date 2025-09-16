using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;

namespace Server
{
    public static class ClassExtend
    {
        private const string CONTENT_TYPE = "Content-type";
        private const string CHARSET_UTF8 = "charset=UTF-8";
        public static bool UrlEqual(this HttpListenerRequest Request, string param)
        {
            if (Request.Url.AbsolutePath == null || param == null) return false;
            var kopf = Request.Url.AbsolutePath.Trim('/');
            return kopf.StartsWith(param);
        }

        public static HttpListenerResponse FromJson(this HttpListenerResponse response)
        {
            response.AddHeader(CONTENT_TYPE, $"text/json;{CHARSET_UTF8}");
            return response;
        }

        public static HttpListenerResponse FromText(this HttpListenerResponse response)
        {
            response.AddHeader(CONTENT_TYPE, $"text/plain;{CHARSET_UTF8}");
            return response;
        }

        public static HttpListenerResponse FromHtml(this HttpListenerResponse response)
        {
            response.AddHeader(CONTENT_TYPE, $"text/html;{CHARSET_UTF8}");
            return response;
        }

        public static HttpListenerResponse FromXml(this HttpListenerResponse response)
        {
            response.AddHeader(CONTENT_TYPE, $"text/xml;{CHARSET_UTF8}");
            return response;
        }

        public static HttpListenerResponse FromGif(this HttpListenerResponse response)
        {
            response.AddHeader(CONTENT_TYPE, $"image/gif;{CHARSET_UTF8}");
            return response;
        }

        public static HttpListenerResponse FromJpeg(this HttpListenerResponse response)
        {
            response.AddHeader(CONTENT_TYPE, $"image/jpeg;{CHARSET_UTF8}");
            return response;
        }
        public static HttpListenerResponse FromPng(this HttpListenerResponse response)
        {
            response.AddHeader(CONTENT_TYPE, $"image/png;{CHARSET_UTF8}");
            return response;
        }
        //application/xhtml+xml ：XHTML格式
        public static HttpListenerResponse ApplicationXhtml(this HttpListenerResponse response)
        {
            response.AddHeader(CONTENT_TYPE, $"application/xhtml+xml;{CHARSET_UTF8}");
            return response;
        }
        // application/xml： XML数据格式
        public static HttpListenerResponse ApplicationXml(this HttpListenerResponse response)
        {
            response.AddHeader(CONTENT_TYPE, $"application/xml;{CHARSET_UTF8}");
            return response;
        }
        //application/json： JSON数据格式
        public static HttpListenerResponse ApplicationJson(this HttpListenerResponse response)
        {
            //跨域设置
            response.AddHeader("Access-Control-Allow-Origin", "*");
            response.AddHeader("Access-Control-Allow-Credentials", "true");
            response.AddHeader("Access-Control-Allow-Motheds", "*");
            response.AddHeader("Access-Control-Allow-Headers", "*");
            response.AddHeader(CONTENT_TYPE, $"application/json;{CHARSET_UTF8}");
            return response;
        }
        //application/atom+xml ：Atom XML聚合格式
        public static HttpListenerResponse ApplicationAtom(this HttpListenerResponse response)
        {
            response.AddHeader(CONTENT_TYPE, $"application/atom+xml;{CHARSET_UTF8}");
            return response;
        }
        //  application/pdf：pdf格式
        public static HttpListenerResponse ApplicationPdf(this HttpListenerResponse response)
        {
            response.AddHeader(CONTENT_TYPE, $"application/pdf;{CHARSET_UTF8}");
            return response;
        }
        //application/msword ： Word文档格式
        public static HttpListenerResponse ApplicationWord(this HttpListenerResponse response)
        {
            response.AddHeader(CONTENT_TYPE, $"application/msword;{CHARSET_UTF8}");
            return response;
        }
        //application/octet-stream ： 二进制流数据（如常见的文件下载）
        public static HttpListenerResponse ApplicationStream(this HttpListenerResponse response)
        {
            response.AddHeader(CONTENT_TYPE, $"application/octet-stream;{CHARSET_UTF8}");
            return response;
        }
        //application/x-www-form-urlencoded ： <form encType=””>中默认的encType，form表单数据被编码为key/value格式发送到服务器（表单默认的提交数据的格式）
        public static HttpListenerResponse ApplicationWWW(this HttpListenerResponse response)
        {
            response.AddHeader(CONTENT_TYPE, $"application/x-www-form-urlencoded;{CHARSET_UTF8}");
            return response;
        }
        //multipart/form-data ： 需要在表单中进行文件上传时，就需要使用该格式
        public static HttpListenerResponse FormData(this HttpListenerResponse response)
        {
            response.AddHeader(CONTENT_TYPE, $"multipart/form-data;{CHARSET_UTF8}");
            return response;
        }

        public static string GetBody(this HttpListenerRequest request, out bool isException)
        {
            string data = null;
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
                isException = false;
                //获取得到数据data可以进行其他操作
            }
            catch (Exception ex)
            {
                isException = true;
                data = ex.ToString();
            }
            return data;
        }

        public static HttpListenerResponse SetStatus(this HttpListenerResponse response)
        {
            response.StatusCode = 200;
            response.StatusDescription = "Success";
            response.AddHeader("Server", $"Hukiry.Http v1.0");
            response.AddHeader("Date", DateTime.Now.ToString("r", CultureInfo.InvariantCulture));
            return response;
        }

        public static void Send(this HttpListenerResponse response, string text)
        {
            byte[] returnByteArr = Encoding.UTF8.GetBytes(text);//设置客户端返回信息的编码
            response.Send(returnByteArr);
        }

        public static void Send(this HttpListenerResponse response, byte[] buffer)
        {
            try
            {
                using (var stream = response.OutputStream)
                {
                    //把处理信息返回到客户端
                    stream.Write(buffer, 0, buffer.Length);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"网络蹦了：{ex.ToString()}");
            }

        }
    }
}
/*常见的媒体格式类型如下：
    text / html ： HTML格式
    text/plain ：纯文本格式
    text/xml ： XML格式
    image/gif ：gif图片格式
    image/jpeg ：jpg图片格式
    image/png：png图片格式
以application开头的媒体格式类型：
    application/xhtml+xml ：XHTML格式
    application/xml： XML数据格式
    application/atom+xml ：Atom XML聚合格式
    application/json： JSON数据格式
    application/pdf：pdf格式
    application/msword ： Word文档格式
    application/octet-stream ： 二进制流数据（如常见的文件下载）
    application/x-www-form-urlencoded ： <form encType=””>中默认的encType，form表单数据被编码为key/value格式发送到服务器（表单默认的提交数据的格式）
另外一种常见的媒体格式是上传文件之时使用的：
    multipart/form-data ： 需要在表单中进行文件上传时，就需要使用该格式
*/