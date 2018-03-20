using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.OpenApi
{
    public enum ProductStatus
    {
        /// <summary>
        /// 上架
        /// </summary>
        [Description("上架")]
        On_Sale = 1,
        /// <summary>
        /// 下架
        /// </summary>
        [Description("下架")]
        Un_Sale = 2,
        /// <summary>
        /// 入库
        /// </summary>
        [Description("入库")]
        In_Stock = 3
    }
}
