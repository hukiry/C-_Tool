using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Game.Http
{
    /// <summary>
    /// 数据库方法接口
    /// </summary>
    public interface IMongoRepository
    {
        Task<T> FindOneAsync<T>(Expression<Func<T, bool>> filter);
        /// <summary>
        /// 查找指定条件的集合，无过滤表示全部数据
        /// </summary>
        Task<List<T>> FindAllAsync<T>(Expression<Func<T, bool>> filter);
        /// <summary>
        /// 读取唯一的Key值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="roleId"></param>
        /// <returns></returns>
        Task<T> FindOneAsync<T>(uint roleId) where T : IEntity;

        /// <summary>
        /// 增加一个数据，如果已经存在，则更新数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task Add<T>(T entity) where T : class, IEntity;

        Task AddMul<T>(params T[] entity) where T : class, IEntity;
        /// <summary>
        /// 根据唯一键，删除指定的key值这条数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyId"></param>
        /// <returns></returns>
        Task Delete<T>(uint keyId) where T : class, IEntity;
        /// <summary>
        /// 根据T实例删除这条数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task Delete<T>(T entity) where T : class, IEntity;
        /// <summary>
        /// 获取数据表的长度
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<long> Count<T>(Expression<Func<T, bool>> filter);
        /// <summary>
        /// 删除数据表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task DeleteTable<T>();
        /// <summary>
        /// 删除数据库
        /// </summary>
        Task DeleteDatabase(string dataName);
    }
    /// <summary>
    /// 属性索引接口
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// 查询角色id, 唯一id
        /// </summary>
        uint id { get; set; }
        /// <summary>
        /// 注册时间和创建时间
        /// </summary>
        uint createTime { get; set; }
        /// <summary>
        /// 查询时间搓（更新时间，登录时间，过期时间）
        /// </summary>
        uint expirateTime { get; set; }
        /// <summary>
        /// 功能的类型
        /// </summary>
        byte type { get; set; }
        /// <summary>
        /// 活动配置id
        /// </summary>
        ushort configId { get; set; }

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="obj">json</param>
        void UpdateSqlData();
    }
    /// <summary>
    /// 数据库抽象类
    /// </summary>
    /// <typeparam name="K">继承类</typeparam>
    public abstract class EntityBase<K> : IEntity where K : class, IEntity
    {
        public uint id { get; set; }
        public uint createTime { get; set; }
        public uint expirateTime { get; set; }
        public byte type { get; set; }
        public ushort configId { get; set; }

        private string jsonItem = string.Empty;

        /// <summary>
        /// 读到数据后，继续读剩下的数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        public abstract void ChildReadData(K data);
        /// <summary>
        /// 没有读到数据，可以创建新数据，对应ReadData
        /// </summary>
        public virtual void ChildCreateData() { }
        /// <summary>
        /// 唯一键id读取数据
        /// </summary>
        public void ReadSqlData(uint id)
        {
            this.id = id;
            K data = MongoLibrary.ins.Find<K>(p => p.id == this.id);
            this.CheckData(data);
        }
        /// <summary>
        /// 条件查找读取数据
        /// </summary>
        /// <param name="filter">λ表达式</param>
        public void ReadSqlData(Expression<Func<K, bool>> filter)
        {
            K data = MongoLibrary.ins.Find(filter);
            this.CheckData(data);
        }
        /// <summary>
        /// 删除指定的数据
        /// </summary>
        /// <param name="id"></param>
        public void Delete(uint id)=> MongoLibrary.ins.Delete<K>(id);
        /// <summary>
        /// 删除所有数据
        /// </summary>
        public void DeleteAll()=> MongoLibrary.ins.DeleteTable<K>();

        private void CheckData(K data)
        {
            bool isHasData = data != null;
            if (isHasData)//无数据
            {
                this.createTime = data.createTime;
                this.expirateTime = data.expirateTime;
                this.type = data.type;
                this.configId = data.configId;
                this.ChildReadData(data);
            }
            else
            {
                this.ChildCreateData();
            }
        }
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="obj">json对象</param>
        public void UpdateSqlData()
        {
            this.expirateTime = this.GetTimeSecond();
            MongoLibrary.ins.AddData(this as K);
        }

        private uint GetTimeSecond()
        {
            TimeSpan ts = new TimeSpan(DateTime.Now.ToUniversalTime().Ticks - new DateTime(1970, 1, 1).Ticks);
            long t = ts.Ticks / TimeSpan.TicksPerSecond;
            return (uint)t;
        }

        /// <summary>
        /// 获取json数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected T GetJsonData<T>() where T : class, new()
        {
            if (string.IsNullOrEmpty(this.jsonItem)) return new T();
            return JsonConvert.DeserializeObject<T>(this.jsonItem);
        }

        protected void UpdateSqlData(object obj)
        {
            if (obj != null)
            {
                this.jsonItem = JsonConvert.SerializeObject(obj);
            }
            MongoLibrary.ins.AddData(this as K);
        }
    }
}
