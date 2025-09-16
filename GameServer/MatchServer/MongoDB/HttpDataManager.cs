using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Linq.Expressions;

namespace Game.Http
{
    /// <summary>
    /// 后台数据管理
    /// </summary>
    public class HttpDataManager : StageSystemManager
    {
        public static HttpDataManager ins { get; } = new HttpDataManager();
        private HttpDataManager() { }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableType"></param>
        /// <returns></returns>
        private List<T> getData<T>(int tableType, condition_data condition) where T : EntityBase<T>
        {
            int type = condition.type, state = condition.state;
            (int startIndex, int endIndex, uint startTime, uint endTime) = condition.GetConditionSql();
            Expression<Func<T, bool>> expressionFunc = condition.GetExpression<T>(tableType, type, state, startIndex, endIndex, startTime, endTime);
            List<T> temp = MongoLibrary.ins.FindAll(expressionFunc);

            if (condition.state == 6 && temp.Count > condition.limit)
            {
                temp.Sort((n, m) => (m as UserRole).GetLevel() - (m as UserRole).GetLevel());
                temp = temp.GetRange(0, condition.limit);
            }
            return temp;
        }

        private string GetStageData<T>(int systemType, condition_data condition) where T : EntityBase<T>
        {
            var items = this.getData<T>(systemType, condition);
            List<T> rangeItems = items;
            if (items.Count > 0)
            {
                items.Sort((n, m) => (int)(m.id - n.id));//降序排序
            }
            string jsonItems = JsonConvert.SerializeObject(rangeItems);
            return ConvertJson(jsonItems, items.Count, 0);
        }

        /// <summary>
        /// 处理后台数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_this"></param>
        /// <param name="sqlType">系统界面类型</param>
        /// <param name="operation">4=获取数据</param>
        /// <param name="condition">获取筛选范围条件</param>
        /// <param name="funcBool">排序条件</param>
        /// <returns></returns>
        public string DealStageData<T>(T _this, int sqlType, HttpOprateState operation, condition_data condition = null) where T : EntityBase<T>
        {
            switch (operation)
            {
                case HttpOprateState.Add:
                    MongoLibrary.ins.AddData(_this);
                    break;
                case HttpOprateState.Update:
                    MongoLibrary.ins.AddData(_this);
                    break;
                case HttpOprateState.Delete:
                    if (sqlType == MongoStageTable.UserData)
                    {
                        MongoLibrary.ins.Delete(_this);
                    }
                    else
                    {
                        MongoLibrary.ins.Delete(_this);
                    }
                    break;
                case HttpOprateState.Get:
                    return this.GetStageData<T>(sqlType, condition);
                case HttpOprateState.Push:
                    //存入数据库
                    MongoLibrary.ins.AddData(_this);
                    break;
                default:
                    break;
            }
            return SuccessJson();
        }


        /// <summary>
        /// 20封邮件
        /// </summary>
        public List<MailBillboardStage> GetMailList() => this.getData<MailBillboardStage>(MongoStageTable.mailbillBoard, new condition_data()
        {
            page=1, limit = 20, type=1
        });

        /// <summary>
        /// 20 种语言公告
        /// </summary>
        public List<MailBillboardStage> GetBillboard() => this.getData<MailBillboardStage>(MongoStageTable.mailbillBoard, new condition_data()
        {
            page = 1,
            limit = 20,
            type = 2
        });

        /// <summary>
        /// 10条活动循环列表 type>2
        /// </summary>
        /// <param name="type">type>2</param>
        /// <returns></returns>
        public List<ActivityStage> GetActivity(SystemFunctionType systemType) => this.getData<ActivityStage>(MongoStageTable.activity, new condition_data()
        {
            page = 1,
            limit = 15,
            type = (int)systemType
        });

        public List<Rank_Data> GetRank(int type)
        {
           var temp = this.getData<UserRole>(MongoStageTable.UserData, new condition_data()
            {
                limit = 30,
                type = type,
                state = 6
            });

            List<Rank_Data> lst = new List<Rank_Data>();
            temp.ForEach(p => {
                lst.Add(p);
            });
            return lst;
        }
    }
}
