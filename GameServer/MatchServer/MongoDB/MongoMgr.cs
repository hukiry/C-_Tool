
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Game.Http
{
    /// <summary>
    /// 使用等待线程时，泛型需要特殊处理
    /// </summary>
    /// <typeparam name="K"></typeparam>
    public sealed class MongoData<K> where K : class, IEntity
    {
        public static MongoData<K> ins { get; } = new MongoData<K>();
        private Queue<K> m_entities = new Queue<K>();
        private bool isEnable = false;
        /// <summary>
        /// 添加或更新数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="mongo"></param>
        public Task Insert<T>(T entity, IMongoRepository mongo) where T : class, IEntity
        {
            this.m_entities.Enqueue(entity as K);
            if (!this.isEnable)
            {
                this.isEnable = this.m_entities.Count > 0;
                Task.Run(async () =>
                {
                    while (this.isEnable)
                    {
                        K t = this.m_entities.Dequeue();
                        if (t != null)
                        {
                            mongo?.Add(t);
                        }
                        await Task.Delay(20);
                        this.isEnable = this.m_entities.Count > 0;
                    }
                });
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// 增加多个数据，如果已经存在，则更新数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public Task AddMul<T>(IMongoRepository mongo, T[] entity) where T : class, IEntity => mongo?.AddMul(entity);
    }

    /// <summary>
    /// 根据唯一id 增删改查 <see cref="IEntity"/>
    /// </summary>
    public sealed class MongoMgr
    {
        
        private IMongoRepository m_mongo;
        /// <summary>
        /// 链接地址：固定
        /// </summary>
        private string connectionString = "mongodb://localhost:27017";
        /// <summary>
        /// 数据库：不同项目数据库不同
        /// </summary>
        private string databaseName = string.Empty;
        /// <summary>
        /// 是异步
        /// </summary>
        private bool isAsync = false;

        /// <summary>
        /// 初始化链接的字符串
        /// </summary>
        /// <param name="connectionString">mongodb://localhost:27017</param>
        /// <param name="databaseName">数据库</param>
        /// <param name="isAsync">是异步：默认同步</param>
        private void InitMongo(string connectionString , string databaseName , bool isAsync)
        {
            this.m_mongo = isAsync ? new MongoRepositoryAsync(connectionString, databaseName) : new MongoRepository(connectionString, databaseName);
        }

        public static MongoMgr ins { get; } = new MongoMgr();
        /// <summary>
        /// 初始化数据库
        /// </summary>
        /// <param name="databaseName">数据库名称：后缀游戏名称</param>
        public void InitMongo(string databaseName)
        {
            this.databaseName = $"{databaseName}_game";
            this.InitMongo(this.connectionString, this.databaseName, this.isAsync);
        }

        public Task<T> Find<T>(Expression<Func<T, bool>> filter) => this.m_mongo?.FindOneAsync(filter);
        /// <summary>
        /// 查询：读取唯一的Key值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public Task<T> Find<T>(uint roleId) where T : IEntity => this.m_mongo?.FindOneAsync<T>(roleId);
        /// <summary>
        /// 查询：查找指定条件的集合，无过滤表示全部数据
        /// </summary>
        public Task<List<T>> FindAll<T>(Expression<Func<T, bool>> filter) => this.m_mongo?.FindAllAsync(filter);

        /// <summary>
        ///  增加一个数据，如果已经存在，则更新数据
        /// </summary>
        /// <typeparam name="T"><see cref="IEntity">必须继承此IEntity接口</see></typeparam>
        /// <param name="entity"></param>
        public Task Insert<T>(T entity) where T : class, IEntity
        {
            if (entity == null) return null;
            return MongoData<T>.ins.Insert(entity, this.m_mongo);
        }

        /// <summary>
        /// 删除：根据唯一键，删除指定的key值这条数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyId"></param>
        /// <returns></returns>
        public Task Delete<T>(uint keyId) where T : class, IEntity
        {
            if (keyId == 0) return null;
            return this.m_mongo?.Delete<T>(keyId);
        }
        /// <summary>
        /// 删除：根据T实例删除这条数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public Task Delete<T>(T entity) where T : class, IEntity
        {
            if (entity == null) return null;
            return this.m_mongo?.Delete(entity);
        }
        /// <summary>
        /// 获取数据表的长度
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filter"></param>
        /// <returns></returns>
        public Task<long> Count<T>(Expression<Func<T, bool>> filter) => this.m_mongo?.Count(filter);
        /// <summary>
        /// 删除数据表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Task DeleteTable<T>() => this.m_mongo?.DeleteTable<T>();
        /// <summary>
        /// 删除数据库
        /// </summary>
        public Task DeleteDatabase(string dataName)
        {
            if (string.IsNullOrEmpty(dataName)) return null;
            return this.m_mongo?.DeleteDatabase(dataName);
        }
    }
}
