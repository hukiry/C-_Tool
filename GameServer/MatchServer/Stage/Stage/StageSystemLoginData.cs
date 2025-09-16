using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Http
{
    public class StageSystemLoginData
    {
        public string username;
        public string password;
        public bool Equals(StageSystemLoginData obj)
        {
            return this.username == obj.username.Trim()
                && this.password == obj.password.Trim();
        }
    }

    public class adminInfo {
        public List<string> roles=new List<string> ();
        public string introduction;
        public string avatar;
        public string name;
    }

    public class UserInfo
    {
        public int code;
        public adminInfo data;
      
        public void AddRole(string role)
        {
            this.data.roles.Add(role);
        }
    }

    public class GameSettingData {
        public int flag;
        public string roleId;
        public string ipAdress = string.Empty;
        public string deleteIp = string.Empty;
        public bool isFullServer = false;//全服
        public bool isPersonServer = false;//个服
        public bool isNormal = false;//正常
        public bool isMaintain = false;//维护
      
    }

    public class UserLoginMgr {
        public static UserLoginMgr instance { get; } = new UserLoginMgr();
        private List<StageSystemLoginData> userList = null;
        private Dictionary<string, UserInfo> dicToken;
        private UserLoginMgr()
        {
            userList = new List<StageSystemLoginData>();
            userList.Add(new StageSystemLoginData { username = "sjk", password = "123456" });
            userList.Add(new StageSystemLoginData { username = "cyl", password = "cyl123" });

            dicToken = new Dictionary<string, UserInfo>();
            dicToken["cumstom-token"] = new UserInfo() { 
                code = 200,
                data =new adminInfo() { 
                name= "wenye",
                avatar = "https://wpimg.wallstcn.com/f778738c-e4f8-4870-b634-56703b4acafe.gif",
                introduction = "I am a super administrator",
                }
            };
            dicToken["cumstom-token"].AddRole("admin");
        }

        public bool CheckLogin(string jsonStr)
        {
            StageSystemLoginData userData = JsonConvert.DeserializeObject<StageSystemLoginData>(jsonStr);
            if (userData == null) return false;
            foreach (var item in userList)
            {
                if(item.Equals(userData))
                {
                    return true;
                }
            }
            return false;
        }


        public string GetUserInfo(string token)
        {
            if (dicToken.ContainsKey(token))
            {
                return JsonConvert.SerializeObject( dicToken[token]);
            }
            return string.Empty;
        }

        public string OparationSuccess()
        {
            return "{\"code\":200, \"message\": \"success\"}";
        }


        public void SetGameData(string jsonStr)
        {
            GameSettingData data = JsonConvert.DeserializeObject<GameSettingData>(jsonStr);
            if (data == null) return;
            switch (data.flag)
            {
                case 1://白名单

                    break;
                case 2://踢人下线

                    break;
                case 3://正常维护

                    break;
                default:
                    break;
            }
            Console.WriteLine(jsonStr);
        }
    }
}
