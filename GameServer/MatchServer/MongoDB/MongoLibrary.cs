using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Game.Http
{
    /// <summary>
    /// 后台数据库表控制
    /// </summary>
    public class MongoStageTable
    {
        /// <summary>
        /// 用户数据
        /// </summary>
        public const int UserData = 1;
        /// <summary>
        /// 充值活动
        /// </summary>
        public const int recharge = 2;
        /// <summary>
        /// 邮件和公告
        /// </summary>
        public const int mailbillBoard = 3;
        /// <summary>
        /// 活动
        /// </summary>
        public const int activity = 4;
        /// <summary>
        /// 反馈
        /// </summary>
        public const int feedback = 5;
    }

    /// <summary>
    /// 数据库缓存，数据结构
    /// </summary>
    public class MongoLibrary
    {

        public static MongoLibrary ins { get; } = new MongoLibrary();
        //数据库名称
        private string dataLibraryName = "match";

        public void InitLibrary()
        {
            MongoMgr.ins.InitMongo(this.dataLibraryName);
        }

        public void DeleteAllTable()
        {
            //后台表
            this.DeleteTable<MailBillboardStage>();
            this.DeleteTable<ActivityStage>();
            //全局表
            this.DeleteTable<GlobalGameData>();
            //用户表
            this.DeleteTable<MailBillboard>();
            this.DeleteTable<Activity>();
            this.DeleteTable<UserRole>();
            this.DeleteTable<LifeMass>();
            this.DeleteTable<Recharge>();
            this.DeleteTable<Feedback>();
            this.DeleteTable<MetaDb>();
            this.DeleteTable<Friend>();
            this.DeleteTable<ChatMessage>();
        }

        public void DeleteUserData(uint id)
        {
            this.Delete<MailBillboard>(id);
            this.Delete<Activity>(id);
            this.Delete<UserRole>(id);
            this.Delete<LifeMass>(id);
            this.Delete<Recharge>(id);
            this.Delete<Feedback>(id);
            this.Delete<MetaDb>(id);
            this.Delete<ChatMessage>(id);
        }

        public T Find<T>(Expression<Func<T, bool>> filter) where T : class, IEntity
        {
            return MongoMgr.ins.Find<T>(filter).Result;
        }

        public T Find<T>(Expression<Func<T, bool>> filter, T t) where T : class, IEntity
        {
            return MongoMgr.ins.Find<T>(filter).Result;
        }

        /// <summary>
        /// 读取一个数据
        /// </summary>
        public T Find<T>(uint id) where T : class, IEntity
        {
            return MongoMgr.ins.Find<T>(id).Result;
        }

        /// <summary>
        /// 读取大量数据
        /// </summary>
        public List<T> FindAll<T>(Expression<Func<T, bool>> filter) where T : class, IEntity
        {
            return MongoMgr.ins.FindAll(filter).Result;
        }
        /// <summary>
        /// 添加和更新数据
        /// </summary>
        public void AddData<T>(T t) where T : class, IEntity
        {
            MongoMgr.ins.Insert(t);
        }

        /// <summary>
        /// 删除指定id数据 <see cref="IEntity.id"/>
        /// </summary>
        public void Delete<T>(uint id) where T : class, IEntity
        {
            MongoMgr.ins.Delete<T>(id);
        }

        /// <summary>
        /// 删除数据 
        /// </summary>
        /// <param name="t">t.id = <see cref="IEntity.id"/></param>
        public void Delete<T>(T t) where T : class, IEntity
        {
            MongoMgr.ins.Delete<T>(t);
        }

        /// <summary>
        /// 删除表
        /// </summary>
        public void DeleteTable<T>() where T : class, IEntity
        {
            MongoMgr.ins.DeleteTable<T>();
        }
    }
}
