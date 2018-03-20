using MongoDB.Driver;
using System.Configuration;

namespace WinServiceBase
{
    /// <summary>
    /// MongoDB客户端
    /// </summary>
    public static class MongodbClient
    {

        /// <summary>
        /// 获取集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IMongoCollection<T> Collection<T>()
        {
            return Database.GetCollection<T>(typeof(T).Name);
        }

        #region 私有属性

        /// <summary>
        /// 客户端锁
        /// </summary>
        static object clientLocker = new object();

        /// <summary>
        /// 用于保存客户端，全局只实例化一次
        /// </summary>
        static IMongoClient _client = null;

        /// <summary>
        /// Mongodb的连接串
        /// </summary>
        static MongoUrl mongoUrl = new MongoUrl(ConfigurationManager.ConnectionStrings["MongoDB"].ConnectionString);

        /// <summary>
        /// 客户端
        /// </summary>
        static IMongoClient Client
        {
            get
            {
                if (_client == null)
                {
                    lock (clientLocker)
                    {
                        if (_client == null)
                            _client = new MongoClient(mongoUrl);
                    }
                }
                return _client;
            }
        }

        /// <summary>
        /// MongoDB数据库实例
        /// </summary>
        static IMongoDatabase Database
        {
            get
            {
                return Client.GetDatabase(mongoUrl.DatabaseName);
            }
        }

        #endregion

    }
}
