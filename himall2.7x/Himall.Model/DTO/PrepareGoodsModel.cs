using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    /// <summary>
    /// 商品配货表
    /// </summary>
    public class PrepareGoodsModel
    {
        /// <summary>
        /// SKU ID
        /// </summary>
        public string SkuId { get; set; }
        /// <summary>
        /// 商品名称
        /// </summary>
        public string ProductName { get; set; }
        /// <summary>
        /// 货号
        /// </summary>
        public string ProductCode { get; set; }
        /// <summary>
        /// 规格
        /// </summary>
        public string Standard { get; set; }
        /// <summary>
        /// 拣货数量
        /// </summary>
        public long Quantity { get; set; }
        /// <summary>
        /// 库存
        /// </summary>
        public long Stock { get; set; }
    }
}
