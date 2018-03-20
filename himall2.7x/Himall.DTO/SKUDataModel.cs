using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
    /// <summary>
    /// 规格前端用数据
    /// </summary>
    public class SKUDataModel
    {
        public string SkuId { get; set; }

        public string Color { get; set; }

        public string Size { get; set; }

        public string Version { get; set; }

        public int Stock { get; set; }

        public decimal CostPrice { get; set; }
        /// <summary>
        /// 市场价
        /// </summary>
        public decimal SalePrice { get; set; }
        /// <summary>
        /// 售价
        /// </summary>
        public decimal Price { get; set; }

    }
}
