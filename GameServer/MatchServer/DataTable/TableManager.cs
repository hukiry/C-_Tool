using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Hukiry.Table
{
    public interface ITable
    {
        int Id { get; }
        void ParseData(string line);
    }

    public class TableManager
    {
        public static TableManager ins { get; } = new TableManager();
        private Dictionary<string, Dictionary<int, ITable>> tabDic = new Dictionary<string, Dictionary<int, ITable>>();

        public void InitTable()
        {
            //this.Load<TableShop>();
            //this.Load<TableLevel>();
            //this.Load<TableTask>();
        }

        /// <summary>
        /// 获取表中的一行数据
        /// </summary>
        /// <typeparam name="T"><see cref="T"/></typeparam>
        public T GetData<T>(int key) where T :class, ITable
        {
            string keyName = typeof(T).Name;
            this.Load<T>();
            if (tabDic[keyName].ContainsKey(key))
            {
                return tabDic[keyName][key] as T;
            }
            return default(T);
        }

        /// <summary>
        /// 获取表的所有数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public List<T> GetTable<T>() where T : class, ITable
        {
            string keyName = typeof(T).Name;
            this.Load<T>();
            return tabDic[keyName].Values.ToList().ConvertAll(p=>(T)p);
        }

        private void Load<T>() where T : ITable
        {
            string keyName = typeof(T).Name;
            if (!tabDic.ContainsKey(keyName))
            {
                tabDic[keyName] = new Dictionary<int, ITable>();
                this.ReadTableData<T>(keyName);
            }
        }

        /// <summary>
        /// 读取表数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName"></param>
        private void ReadTableData<T>(string tableName) where T : ITable
        {
            var dataList = tabDic[tableName];
            string configFile = $"TableData/{tableName}.txt";
            if (File.Exists(configFile))
            {
                string[] lines = File.ReadAllLines(configFile);
                foreach (var line in lines)
                {
                    if (line.StartsWith("#"))
                    {
                        continue;
                    }
                    T t = Activator.CreateInstance<T>();
                    t.ParseData(line);
                    dataList[t.Id] = t;
                }
            }
            else
            {
                Game.Http.GameCenter.Log($"此文件不存在：{configFile}");
            }
        }

    }
}
