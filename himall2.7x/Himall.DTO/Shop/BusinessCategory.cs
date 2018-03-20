using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
    /// <summary>
    /// 店铺经营项目
    /// </summary>
    public class BusinessCategory
    {
        long _id;
        /// <summary>
        /// 主键
        /// </summary>
        public long Id { get { return _id; } set { _id = value; } }
        /// <summary>
        /// 店铺ID
        /// </summary>
        public long ShopId { get; set; }
        /// <summary>
        /// 项目ID
        /// </summary>
        public long CategoryId { get; set; }
        /// <summary>
        /// 项目名称
        /// </summary>
        public string CategoryName { get; set; }

    }
}
