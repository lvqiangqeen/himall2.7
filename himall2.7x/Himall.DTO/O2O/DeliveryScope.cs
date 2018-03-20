using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
    /// <summary>
    /// 门店配送范围
    /// </summary>
   public class DeliveryScope
    {
        /// ID
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 所属商家店铺ID
        /// </summary>
        public long ShopBranchId { get; set; }
        /// <summary>
        /// 区域标识
        /// </summary>
        [Required(ErrorMessage = "区域标识")]
        public int RegionId { get; set; }
        /// <summary>
        /// 区域名称
        /// </summary>
        [Required(ErrorMessage = "区域名称")]
        public string RegionName { get; set; }
        /// <summary>
        /// 全路径
        /// </summary>
        [Required(ErrorMessage = "全路径")]
        public string FullRegionPath { get; set; }
    }
}
