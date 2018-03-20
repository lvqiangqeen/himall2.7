using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    public enum SaleDimension : int
    {
        /// <summary>
        /// 订单量
        /// </summary>
        [Description("销售量")]
        SaleCount = 1,

        /// <summary>
        /// 销售额
        /// </summary>
        [Description("销售额")]
        Sales,
    }
}
