using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Search.Service.Data
{
    /// <summary>
    /// 商品名索引存储对象
    /// </summary>
    public class NameIndexs
    {
        /// <summary>
        /// 标识
        /// </summary>
        public ObjectId _id { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// ID集合
        /// </summary>
        public List<int> Ids { get; set; }
    }
}
