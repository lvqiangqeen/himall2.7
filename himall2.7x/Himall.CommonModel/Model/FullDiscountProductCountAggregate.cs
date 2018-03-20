using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel
{
    /// <summary>
    /// 满减数量聚合
    /// </summary>
    public class FullDiscountProductCountAggregate
    {
        public long ActiveId { get; set; }
        public int ProductCount { get; set; }
    }
}
