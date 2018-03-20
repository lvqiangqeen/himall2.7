using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Core;

namespace Himall.Model
{
    public class ProductsDistributionModel 
    {
        public long Id { set; get; }

        public string ProductName { set; get; }

        public ProductBrokerageInfo.ProductBrokerageStatus Status { get; set; }

        public string ShopName { set; get; }

        public int AgentNum { set; get; }

        public int ForwardNum { set; get; }

        public int DistributionSaleNum { set; get; }

        public decimal DistributionSaleAmount { set; get; }

         /// <summary>
        /// 总交易数
         /// </summary>
        public decimal SaleNum { set; get; }

        /// <summary>
        /// 总交易额
        /// </summary>
        public decimal SaleAmount { set; get; }

        /// <summary>
        /// 分销佣金
        /// </summary>
        public decimal Brokerage { set; get; }
        /// <summary>
        /// 未结佣金
        /// </summary>
        public decimal NoSettledBrokerage { set; get; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { set; get; }


        public string ProDisStatus { get { return Status.ToDescription(); } }

    }
}
