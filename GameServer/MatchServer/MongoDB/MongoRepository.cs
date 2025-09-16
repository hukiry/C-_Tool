using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Game.Http
{
    internal class MongoRepository : IMongoRepository
    {
        private IMongoDatabase database;
        private MongoClient mongoClient;
        /// <summary>
        /// 链接数据库
        /// </summary>
        /// <param name="connectionString">"mongodb://localhost:27017"</param>
        /// <param name="databaseName">数据库名称</param>
        public MongoRepository(string connectionString, string databaseName)
        {
            mongoClient = new MongoClient(connectionString);
            database = mongoClient.GetDatabase(databaseName);
        }

        /// <summary>
        /// 如果表不存在，会自动创建
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private IMongoCollection<T> GetCollection<T>()
        {
            var tbName = typeof(T).Name;
            var _collection = database.GetCollection<T>(tbName);
            //创建索引，必须是在首次创建表时
            if (_collection.CountDocuments(P => P != null) <= 0)
            {
                var indexKeysDefinitionid = Builders<T>.IndexKeys.Ascending("id");
                var indexKeysDefinitiont = Builders<T>.IndexKeys.Descending("createTime");
                var indexKeysDefinitiontype = Builders<T>.IndexKeys.Ascending("type");
                var keys = Builders<T>.IndexKeys.Combine(indexKeysDefinitionid, indexKeysDefinitiont, indexKeysDefinitiontype);
                _collection.Indexes.CreateOne(new CreateIndexModel<T>(keys));
            }
            return _collection;
        }

        //创建索引，必须是在首次创建表时
        Task<T> IMongoRepository.FindOneAsync<T>(Expression<Func<T, bool>> filter)
        {
            var _collection = GetCollection<T>();
            if (_collection != null)
            {
                var result = _collection.Find(filter).FirstOrDefault();
                return Task.FromResult(result);
            }
            else
                return Task.FromResult(default(T));
        }

        /// <summary>
        /// 查找指定条件的集合，无过滤表示全部数据
        /// </summary>
        Task<List<T>> IMongoRepository.FindAllAsync<T>(Expression<Func<T, bool>> filter)
        {
            var _collection = this.GetCollection<T>();
            if (_collection != null)
            {
                var result = filter == null ? _collection.Find(Builders<T>.Filter.Empty) : _collection.Find(filter);
                var length = result != null ? result.CountDocuments() : 0;
                if (length <= 0)
                {
                    return Task.FromResult(new List<T>());
                }
                return Task.FromResult(result.ToList());
            }
            else
            {
                return Task.FromResult(new List<T>());
            }
        }

        /// <summary>
        /// 读取唯一的Key值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="roleId"></param>
        /// <returns></returns>
        Task<T> IMongoRepository.FindOneAsync<T>(uint roleId)
        {
            var _collection = this.GetCollection<T>();
            if (_collection != null)
            {
                var filter = Builders<T>.Filter.Eq(p => p.id, roleId);
                var result = _collection.Find(filter).FirstOrDefault();

                return Task.FromResult(result);
            }
            else
            {
                return Task.FromResult(default(T));
            }
        }

        /// <summary>
        /// 增加一个数据，如果已经存在，则更新数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task IMongoRepository.Add<T>(T entity)
        {
            var filter = Builders<T>.Filter.Eq(p => p.id, entity.id);
            var _collection = this.GetCollection<T>();
            if (_collection.Find(filter).FirstOrDefault() == null)
            {
                _collection.InsertOneAsync(entity);
            }
            else
            {
                this.Update(entity);
            }
            return Task.CompletedTask;
        }

        Task IMongoRepository.AddMul<T>(params T[] entity)
        {
            var _collection = this.GetCollection<T>();

                _collection.InsertMany(entity);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task Update<T>(T entity) where T : IEntity
        {
            var _collection = this.GetCollection<T>();
            if (_collection != null)
            {
                var filter = Builders<T>.Filter.Eq(p => p.id, entity.id);
                _collection.ReplaceOne(filter, entity);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// 根据唯一键，删除指定的key值这条数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyId"></param>
        /// <returns></returns>
        Task IMongoRepository.Delete<T>(uint keyId)
        {
            var _collection = this.GetCollection<T>();
            if (_collection != null)
            {
                var filter = Builders<T>.Filter.Eq(p => p.id, keyId);
                _collection.DeleteOne(filter);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// 根据T实例删除这条数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task IMongoRepository.Delete<T>(T entity)
        {
            var _collection = this.GetCollection<T>();
            if (_collection != null)
            {
                var filter = Builders<T>.Filter.Eq(p => p.id, entity.id);
                _collection.DeleteOne(filter);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// 获取数据表的长度
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<long> IMongoRepository.Count<T>(Expression<Func<T, bool>> filter)
        {
            var _collection = this.GetCollection<T>();
            if (_collection != null)
            {
                var result = _collection.CountDocuments(filter);
                return Task.FromResult(result);
            }
            else
                return Task.FromResult((long)0);
        }

        /// <summary>
        /// 删除数据表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task IMongoRepository.DeleteTable<T>()
        {
            var tbName = typeof(T).Name;
            database?.DropCollection(tbName);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 删除数据库
        /// </summary>
        Task IMongoRepository.DeleteDatabase(string dataName)
        {
            mongoClient?.DropDatabase(dataName);
            return Task.CompletedTask;
        }
    }
}