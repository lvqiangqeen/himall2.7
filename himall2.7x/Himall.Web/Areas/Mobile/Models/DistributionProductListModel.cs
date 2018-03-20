using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Himall.Model;

namespace Himall.Web.Areas.Mobile.Models
{
    public class DistributionProductListModel
    {
        /// <summary>
        /// 店铺编号
        /// </summary>
        public long? ShopId { get; set; }
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
        /// <summary>
        /// 销量
        /// </summary>
        public Nullable<int> SaleNum { get; set; }
        /// <summary>
        /// 代理次数
        /// </summary>
        public Nullable<int> AgentNum { get; set; }
        /// <summary>
        /// 转发数
        /// </summary>
        public Nullable<int> ForwardNum { get; set; }
        /// <summary>
        /// 是否已代理
        /// </summary>
        public bool isHasAgent { get; set; }
        /// <summary>
        /// 佣金
        /// </summary>
        public decimal Commission
        {
            get
            {
                decimal result = 0;
                decimal rate = (decimal)DistributorRate.Value;
                if (rate > 0)
                {
                    result = (SellPrice * rate / 100);
                    int _tmp = (int)(result * 100);
                    //保留两位小数，但不四舍五入
                    result = (decimal)(((decimal)_tmp) / (decimal)100);
                }
                return result;
            }
        }

		public string ShareImageUrl { get; set; }
        /// <summary>
        /// 广告语
        /// </summary>
        public string ShortDescription { get; set; }
    }
}