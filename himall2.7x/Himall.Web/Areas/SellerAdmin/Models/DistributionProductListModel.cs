using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Himall.Model;

namespace Himall.Web.Areas.SellerAdmin.Models
{
    public class DistributionProductListModel
    {
        /// <summary>
        /// 分销商品记录编号
        /// </summary>
        public long BrokerageId { get; set; }
        /// <summary>
        /// 分销商品编号
        /// </summary>
        public long? ProductId { get; set; }
        /// <summary>
        /// 分销商品名称
        /// </summary>
        public string ProductName { get; set; }
        /// <summary>
        /// 分销商品佣金比
        /// </summary>
        public decimal? DistributorRate { get; set; }
        /// <summary>
        /// 所属分类
        /// </summary>
        public long CategoryId { get; set; }
        /// <summary>
        /// 所属分类名称
        /// </summary>
        public string CategoryName { get; set; }
        /// <summary>
        /// 售价
        /// </summary>
        public decimal SellPrice { get; set; }
        /// <summary>
        /// 销售状态
        /// </summary>
        public ProductInfo.ProductSaleStatus ProductSaleState { get; set; }
        /// <summary>
        /// 销售状态显示文字
        /// </summary>
        public string ShowProductSaleState { get; set; }
        /// <summary>
        /// 分销状态
        /// </summary>
        public ProductBrokerageInfo.ProductBrokerageStatus ProductBrokerageState { get; set; }
        /// <summary>
        /// 分销状态显示文字
        /// </summary>
        public string ShowProductBrokerageState { get; set; }
        /// <summary>
        /// 商品图片
        /// </summary>
        public string Image { get; set; }
    }
}