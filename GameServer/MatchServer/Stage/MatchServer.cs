using System.Net;
using System.Threading.Tasks;
using Server;

namespace Game.Http
{
    /// <summary>
    /// 后台服务
    /// </summary>
    public class MatchServer
    {
        HttpServer service = null;
        public MatchServer() { }
        public HttpServer GetHttpService() => this.service;

        public void EnableSerer()
        {
            //"http://192.168.1.4:9302/"
            string address = ServerConifg.GetAddressServer().GetBackServerAdress();
            this.service = HttpServer.ins.StartListener(address)
                .Awake(this.OnGet, this.OnPost);
        }

        public void CloseServer()
        {
            if (this.service != null)
            {
                this.service.Close();
            }
        }

        public void SetEnable(bool isEnable)
        {
            if (this.service != null)
            {
                this.service.Logger.IsEnable = isEnable;
            }
        }

        protected void OnGet(HttpListenerRequest request, HttpListenerResponse response)
        {
            if (request.UrlEqual("game"))//后台数据列表
            {
                string token = request.QueryString.Get("token");
                if (!string.IsNullOrEmpty(token))//后台登陆
                {
                    string info = UserLoginMgr.instance.GetUserInfo(token);
                    if (!string.IsNullOrEmpty(info))
                    {
                        response.ApplicationJson().Send(info);
                    }
                    else
                    {
                        response.ApplicationJson().Send("{\"code\":1,\"message\":\"验证失败\"}");
                    }
                }
                else
                {
                    try
                    {
                        string condition = request.QueryString.Get("condition");
                        int.TryParse(request.QueryString.Get("type"), out int type);
                        string info = StageUpdateDataGet.instance.GetDataList(condition, type);
                        response.ApplicationJson().Send(info);
                    }
                    catch (System.Exception ex)
                    {
                        GameCenter.Log(LogType.Error, "后台查询失败", ex);
                    }
                }
            }
        }

        protected void OnPost(HttpListenerRequest request, HttpListenerResponse response)
        {
            if (request.UrlEqual("login"))//后台登陆
            {
                string token = request.QueryString.Get("state");
                if (!string.IsNullOrEmpty(token)&& token.Equals("1"))//后台登出
                {
                    response.SetStatus().ApplicationJson().Send(UserLoginMgr.instance.OparationSuccess());
                }
                else
                {
                    string jsonStr = request.GetBody(out bool isException);
                    if (isException == false && UserLoginMgr.instance.CheckLogin(jsonStr))
                    {
                        response.SetStatus().ApplicationJson().Send("{\"code\":200,\"data\":{\"access_token\":\"cumstom-token\"}}");
                    }
                    else
                    {
                        response.SetStatus().ApplicationJson().Send("{\"code\":1,\"message\":\"登陆密码或账号错误\"}");
                    }
                }
            }
            else if(request.UrlEqual("game"))//后台数据列表
            {
                try
                {
                    string jsonStr = request.GetBody(out bool isException);
                    if (isException)
                    {
                        GameCenter.Log(LogType.Error, "读取数据异常"+ jsonStr);
                    }
                    else
                    {
                        string resultAnswer = StageUpdateDataPost.ins.UpdateData(jsonStr);
                        response.SetStatus().ApplicationJson().Send(resultAnswer);
                    }
                }
                catch (System.Exception ex)
                {
                    GameCenter.Log(LogType.Error, "后台写入失败", ex);
                }
            }
        }
    }
}


