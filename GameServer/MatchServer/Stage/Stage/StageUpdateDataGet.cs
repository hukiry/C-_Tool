using Newtonsoft.Json;
using System;
using System.Linq.Expressions;

namespace Game.Http
{
    public class base_data
    {
        /// <summary>
        /// 系统表类型
        /// <see cref="MongoStageTable">查看 SQLTable</see>
        /// </summary>
        public int systemType;
        /// <summary>
        /// 操作状态
        /// </summary>
        public int state;
    }

    //条件查找
    /// <summary>
    /// select 选择字段
    /// from 表名
    /// where 查询条件
    /// group by 分组条件
    /// order by 降序查询
    /// </summary>
    public class condition_data {
        /// <summary>
        /// 第几页
        /// </summary>
        public int page;
        /// <summary>
        /// 每页多少条数据
        /// </summary>
        public int limit;
        /// <summary>
        /// 查询时间戳:用户和充值查询
        /// </summary>
        public int timestamp;
        /// <summary>
        /// 查询的状态:1=查询日登陆，2=查询日注册, 3=查询月登陆，4=查询月注册， 5=语言查询, 6=降序查询 7=反馈数据:当天查询 8=90天未登陆
        /// </summary>
        public int state;
        /// <summary>
        /// 查询系统功能类型
        /// <see cref="SystemFunctionType">系统功能类型 SystemFunctionType</see>
        /// </summary>
        public int type;

        public DateTime GetDateTime() => GameCenter.GetDateTime(this.timestamp);

        public (int startIndex, int endIndex, uint startTime, uint endTime) GetConditionSql()
        {
            int startIndex = (this.page - 1) * this.limit;
            int endIndex = this.page * this.limit;
            uint startTime = 0;
            uint endTime = 0;
            if (this.state > 0)
            {
                if (this.state == 8)
                {
                    endTime = GameCenter.GetTimeSecond() - 90 * 24 * 3600;
                }
                else if (this.state == 6)//排行榜
                {
                    if (this.type == 1)
                    {
                        var date = DateTime.Now.ToUniversalTime();
                        int endDay = GetMaxDay(date.Year, date.Month);
                        startTime = (uint)GameCenter.GetTimestamp(new DateTime(date.Year, date.Month, 1, 0, 0, 0));
                        endTime = (uint)GameCenter.GetTimestamp(new DateTime(date.Year, date.Month, endDay, 23, 59, 59));
                    }
                    else if (this.type == 2)
                    {
                        var date = DateTime.Now.ToUniversalTime();
                        int endDay = GetMaxDay(date.Year, date.Month);
                        startTime = (uint)GameCenter.GetTimestamp(new DateTime(date.Year, 1, 1, 0, 0, 0));
                        endTime = (uint)GameCenter.GetTimestamp(new DateTime(date.Year, 12, endDay, 23, 59, 59));
                    }
                }
                else//用户等查询
                {
                    var date = this.GetDateTime();
                    int day = this.state == 1 || this.state == 2 ? date.Day : 1;
                    int endDay = this.state == 1 || this.state == 2 ? date.Day : GetMaxDay(date.Year, date.Month);
                    startTime = (uint)GameCenter.GetTimestamp(new DateTime(date.Year, date.Month, day, 0, 0, 0));
                    endTime = (uint)GameCenter.GetTimestamp(new DateTime(date.Year, date.Month, endDay, 23, 59, 59));
                }

            }
            return (startIndex, endIndex, startTime, endTime);
        }


        /// <summary>
        /// 获取lamba 表达式
        /// </summary>
        /// <returns></returns>
        public Expression<Func<T, bool>> GetExpression<T>(int tableType, int type, int state, int startIndex, int endIndex, uint startTime, uint endTime) where T : EntityBase<T>
        {
            Expression<Func<T, bool>> expressionFunc = p => p.id >= startIndex && p.id <= endIndex;
            if (type > 0)
                expressionFunc = p => p.id >= startIndex && p.id <= endIndex && p.type == type;

            if (tableType == MongoStageTable.mailbillBoard || tableType == MongoStageTable.activity)
            {
                expressionFunc = p => p.configId>= startIndex && p.configId <= endIndex;
                if (type > 0)
                    expressionFunc = p => p.configId >= startIndex && p.configId <= endIndex && p.type == type;
            }

            if (state > 0)
            {
                if (state == 8)
                    expressionFunc = p => p.expirateTime < endTime;
                else if (state == 6)//排行榜
                {
                    expressionFunc = p => true;
                    if (type == 1)
                        expressionFunc = p => p.expirateTime >= startTime && p.expirateTime <= endTime;
                    else if (type == 2)
                        expressionFunc = p => p.expirateTime >= startTime && p.expirateTime <= endTime;
                }
                else//用户等查询
                {
                    expressionFunc = p => p.createTime >= startTime && p.createTime <= endTime;
                    if (state == 7 || tableType == 2 || state == 3 || state == 1)//如果表类型=2（充值查询）
                    {
                        expressionFunc = p => p.expirateTime >= startTime && p.expirateTime <= endTime;
                        if (type > 0)
                        {
                            expressionFunc = p => p.expirateTime >= startTime && p.expirateTime <= endTime && p.type == type;
                        }
                    }
                    else
                    {
                        if (type > 0)
                        {
                            expressionFunc = p => p.createTime >= startTime && p.createTime <= endTime && p.type == type;
                        }
                    }
                }
            }
            return expressionFunc;
        }

        private int GetMaxDay(int year,int month)
        {
            if (month == 2)
            {
                if (year % 400 == 0 || (year % 100 > 0 && year % 4 == 0))
                    return 29;
                else
                    return 28;
            }
            else if (month == 2 || month == 4 || month == 6 || month == 9 || month == 11)
            {
                return 30;
            }
            else
            {
                return 31;
            }
        }
    }

    public abstract class StageSystemManager
    {
        protected string ConvertJson(string items, int total=0, int id=1)
        {
            string jsonItems = $"\"{nameof(items)}\":{items},\"{nameof(total)}\":{total},\"{nameof(id)}\":{id}";
            return "{\"code\":200," + jsonItems + "}";
        }

        protected string ErrorJson(string message)
        {
            return "{" + $"\"code\":-1,\"message\":\"{message}\"" + "}";
        }

        protected string SuccessJson(string message="success")
        {
            return "{" + $"\"code\":200,\"message\":\"{message}\"" + "}";
        }
    }

    /// <summary>
    /// 后台获取数据库 ////////////////////////////////////1=公告，邮件，2=活动，3=用户查询，4=充值查询
    /// </summary>
    class StageUpdateDataGet : StageSystemManager
    {
        public static StageUpdateDataGet instance { get; } = new StageUpdateDataGet();
        private StageUpdateDataGet() { }

        public string GetDataList(string conditionJson, int type)
        {
            condition_data condition = JsonConvert.DeserializeObject<condition_data>(conditionJson);
            var result = type switch
            {
                //在线数据
                MongoStageTable.UserData => this.DealStageData<UserRole>(type, condition),
                //每日数据
                MongoStageTable.recharge => this.DealStageData<Recharge>(type, condition),
                //用户数据
                MongoStageTable.mailbillBoard => this.DealStageData<MailBillboardStage>(type, condition),
                //日志打点
                MongoStageTable.activity => this.DealStageData<ActivityStage>(type, condition),
                //玩家反馈
                MongoStageTable.feedback => this.DealStageData<Feedback>(type, condition),
                _ => ErrorJson("参数异常")
            };

            return result;
        }

        private string DealStageData<T>(int tableType, condition_data condition) where T:EntityBase<T>
        {
           return HttpDataManager.ins.DealStageData<T>(default, tableType, HttpOprateState.Get, condition);
        }
    }

    /// <summary>
    /// 后台上传到数据库
    /// </summary>
    public class StageUpdateDataPost : StageSystemManager
    {
        public static StageUpdateDataPost ins { get; } = new StageUpdateDataPost();
        private StageUpdateDataPost() { }

        public string UpdateData(string baseJson)
        {
            try
            {
                base_data data = JsonConvert.DeserializeObject<base_data>(baseJson);
                if (data == null) return ErrorJson("参数为空");

                var result = data.systemType switch
                {
                    MongoStageTable.mailbillBoard => this.UdateMail(baseJson, data.systemType, data.state),
                    MongoStageTable.activity => this.UdateActivity(baseJson, data.systemType, data.state),
                    MongoStageTable.UserData => this.UdateUser(baseJson, data.systemType, data.state),
                    _ => default
                };

                if (!string.IsNullOrEmpty(result))
                    return result;

            }
            catch (Exception ex)
            {

                return ErrorJson("json 字符串转换失败：" + ex.ToString());
            }

            return ErrorJson("参数为空");
        }

        private string UdateMail(string postJson, int sqlType, int operation)
        {
            MailBillboardStage data = JsonConvert.DeserializeObject<MailBillboardStage>(postJson);
            return HttpDataManager.ins.DealStageData(data, sqlType, (HttpOprateState)operation);
        }

        private string UdateActivity(string postJson, int sqlType, int operation)
        {
            ActivityStage data = JsonConvert.DeserializeObject<ActivityStage>(postJson);
            return HttpDataManager.ins.DealStageData(data, sqlType, (HttpOprateState)operation);
        }

        private string UdateUser(string postJson, int sqlType, int operation)
        {
            UserRole data = JsonConvert.DeserializeObject<UserRole>(postJson);
            return HttpDataManager.ins.DealStageData(data, sqlType, (HttpOprateState)operation);
        }
    }
}
