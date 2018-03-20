using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
    /// <summary>
    /// 满减-商品
    /// </summary>
    public class FullDiscountActiveProduct
    {
        public long Id { get; set; }
        public long ActiveId { get; set; }
        /// <summary>
        /// 商品编号
        /// <para>-1表示所有商品</para>
        /// </summary>
        public long ProductId { get; set; }
    }
}
