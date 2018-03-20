using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Himall.Core;

namespace Himall.Model
{
    public partial class ProductBrokerageInfo
    {
        /// <summary>
        /// 佣金
        /// </summary>
        public enum ProductBrokerageStatus
        {
            [Description("推广中")]
            Normal = 0,

            [Description("已清退")]
            Removed = 1,
        }
        /// <summary>
        /// 对应关联商品
        /// <para>非EF自动关联，需手动join</para>
        /// </summary>
        [NotMapped]
        public ProductInfo Product { get; set; }
        /// <summary>
        /// 对应分类
        /// <para>非EF自动关联，需手动join</para>
        /// </summary>
        [NotMapped]
        public string CategoryName { get; set; }

        [NotMapped]
        /// <summary>
        /// 佣金
        /// </summary>
        public decimal Commission
        {
            get
            {
                decimal result = 0;
                decimal DistributorRate = (decimal)rate;
                if (DistributorRate > 0)
                {
                    if (Product != null)
                    {
                        result = (Product.MinSalePrice * DistributorRate / 100);
                        int _tmp = (int)(result * 100);
                        //保留两位小数，但不四舍五入
                        result = (decimal)(((decimal)_tmp) / (decimal)100);
                    }
                }
                return result;
            }
        }

    }
}
