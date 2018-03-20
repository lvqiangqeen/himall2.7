using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
    /// <summary>
    /// 满减-优惠规则
    /// </summary>
    public class FullDiscountRules
    {
        public long Id { get; set; }
        public long ActiveId { get; set; }
        /// <summary>
        /// 优惠门槛
        /// </summary>
        public decimal Quota { get; set; }
        /// <summary>
        /// 优惠方式
        /// </summary>
        public decimal Discount { get; set; }
    }
}
