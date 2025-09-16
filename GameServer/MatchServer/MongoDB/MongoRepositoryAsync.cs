using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Game.Http
{
    /// <summary>
    /// 异步处理
    /// </summary>
    internal class MongoRepositoryAsync : IMongoRepository
    {
        private IMongoDatabase database;
        private MongoClient mongoClient;
        /// <summary>
        /// 链接数据库
        /// </summary>
        /// <param name="connectionString">"mongodb://localhost:27017"</param>
        /// <param name="databaseName">数据库名称</param>
        public MongoRepositoryAsync(string connectionString, string databaseName)
        {
            mongoClient = new MongoClient(connectionString);
            database = mongoClient.GetDatabase(databaseName);
        }


        /// <summary>
        /// 如果表不存在，会自动创建
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private async Task<IMongoCollection<T>> GetCollection<T>()
        {
            var tbName = typeof(T).Name;
            var _collection = database.GetCollection<T>(tbName);
            //创建索引，必须是在首次创建表时
            if (await _collection.CountDocumentsAsync(P => P != null) <= 0)
            {
                var indexKeysDefinitionid = Builders<T>.IndexKeys.Ascending("id");
                var indexKeysDefinitiont = Builders<T>.IndexKeys.Descending("createTime");
                var indexKeysDefinitiontype = Builders<T>.IndexKeys.Ascending("type");
                var keys = Builders<T>.IndexKeys.Combine(indexKeysDefinitionid, indexKeysDefinitiont, indexKeysDefinitiontype);
                await _collection.Indexes.CreateOneAsync(new CreateIndexModel<T>(keys));
            }
            return _collection;
        }

        //创建索引，必须是在首次创建表时

        async Task<T> IMongoRepository.FindOneAsync<T>(Expression<Func<T, bool>> filter)
        {
            var _collection = await GetCollection<T>();
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        /// <summary>
        /// 查找指定条件的集合，无过滤表示全部数据
        /// </summary>
        async Task<List<T>> IMongoRepository.FindAllAsync<T>(Expression<Func<T, bool>> filter)
        {
            var _collection = await GetCollection<T>();
            var result = filter == null
                ? await _collection.FindAsync(Builders<T>.Filter.Empty)
                : await _collection.FindAsync(filter);
            if (result == null)
            {
                return await Task.FromResult(new List<T>());
            }
            return await result.ToListAsync();
        }


        /// <summary>
        /// 读取唯一的Key值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="roleId"></param>
        /// <returns></returns>
        async Task<T> IMongoRepository.FindOneAsync<T>(uint roleId)
        {

            var _collection = await GetCollection<T>();

            var filter = Builders<T>.Filter.Eq(p => p.id, roleId);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        /// <summary>
        /// 增加一个数据，如果已经存在，则更新数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        async Task IMongoRepository.Add<T>(T entity)
        {
            var filter = Builders<T>.Filter.Eq(p => p.id, entity.id);
            var _collection = await GetCollection<T>();
            if (_collection.Find(filter).FirstOrDefault() == null)
            {
                await _collection.InsertOneAsync(entity);
            }
            else
            {
                await this.Update(entity);
            }
        }

        async Task IMongoRepository.AddMul<T>(params T[] entity)
        {
            var _collection = await GetCollection<T>();
            await _collection.InsertManyAsync(entity);
        }


        /// <summary>
        /// 更新数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        async Task Update<T>(T entity) where T : class, IEntity
        {
            var _collection = await GetCollection<T>();
            var filter = Builders<T>.Filter.Eq(p => p.id, entity.id);
            await _collection.ReplaceOneAsync(filter, entity);
        }


        /// <summary>
        /// 根据唯一键，删除指定的key值这条数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyId"></param>
        /// <returns></returns>
        async Task IMongoRepository.Delete<T>(uint keyId)
        {
            var _collection = await GetCollection<T>();
            var filter = Builders<T>.Filter.Eq(p => p.id, keyId);
            await _collection.DeleteOneAsync(filter);
        }

        /// <summary>
        /// 根据T实例删除这条数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        async Task IMongoRepository.Delete<T>(T entity)
        {
            var _collection = await GetCollection<T>();
            var filter = Builders<T>.Filter.Eq(p => p.id, entity.id);
            await _collection.DeleteOneAsync(filter);
        }


        /// <summary>
        /// 获取数据表的长度
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filter"></param>
        /// <returns></returns>

        async Task<long> IMongoRepository.Count<T>(Expression<Func<T, bool>> filter)
        {
            var _collection = await GetCollection<T>();
            return await _collection.CountDocumentsAsync(filter);
        }

        /// <summary>
        /// 删除数据表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        async Task IMongoRepository.DeleteTable<T>()
        {
            var tbName = typeof(T).Name;
            await database?.DropCollectionAsync(tbName);
        }

        /// <summary>
        /// 删除数据库
        /// </summary>
        async Task IMongoRepository.DeleteDatabase(string dataName)
        {
            await mongoClient?.DropDatabaseAsync(dataName);
        }
    }
}
