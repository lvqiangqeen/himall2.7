using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel
{
    public class ProductStatisticQuery : BaseQuery
    {
        public long? ShopId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        /// <summary>
        /// 排序字段
        /// </summary>
        public string Sort { get; set; }

        /// <summary>
        /// 是否倒序
        /// </summary>
        public bool IsAsc { get; set; }
    }
}
