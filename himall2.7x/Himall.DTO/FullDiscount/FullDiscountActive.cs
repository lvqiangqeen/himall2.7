using Himall.CommonModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
    /// <summary>
    /// 满减-活动
    /// </summary>
    public class FullDiscountActive:FullDiscountActiveBase
    {
        /// <summary>
        /// 优惠阶梯
        /// </summary>
        public List<FullDiscountRules> Rules { get; set; }
        public List<FullDiscountActiveProduct> Products { get; set; }

        public FullDiscountActive()
        {
            Rules = new List<FullDiscountRules>();
            Products = new List<FullDiscountActiveProduct>();
        }
    }
}
