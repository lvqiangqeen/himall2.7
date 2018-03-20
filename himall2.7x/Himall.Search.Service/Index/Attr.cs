using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Search.Service.Data
{
    /// <summary>
    /// 商品属性索引存储对象
    /// </summary>
    public class AttrIndexs
    {
        /// <summary>
        /// 标识
        /// </summary>
        public ObjectId _id { get; set; }
        /// <summary>
        /// 属性标识
        /// </summary>
        public string AttrId { get; set; }
        /// <summary>
        /// 标识集合
        /// </summary>
        public List<int> Ids { get; set; }
    }
}
