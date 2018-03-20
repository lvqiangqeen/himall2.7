using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Model
{
    public class SellerConsoleModel
    {      
        
        /// <summary>
        /// 今日订单数
        /// </summary>
        public int TodayOrders { set; get; }

        /// <summary>
        /// 今日销售总额
        /// </summary>
        public decimal? TodaySaleAmount{set;get;}

        /// <summary>
        /// 历史销售总额
        /// </summary>
        public  decimal? SaleAmount{set;get;}
        /// <summary>
        /// 店铺名称
        /// </summary>
        public string ShopName { set; get; }
        /// <summary>
        /// 店铺套餐
        /// </summary>
        public string ShopGrade { set; get; }
      /// <summary>
      /// 本期结算
      /// </summary>
        public decimal? SettlementPeriod { set; get; }
     
        /// <summary>
        /// 店铺到期时间
        /// </summary>
        public DateTime ShopEndDate { set; get; }

        /// <summary>
        /// 店铺运费
        /// </summary>
        public decimal ShopFreight { set; get; }

        /// <summary>
        /// 出售商品数
        /// </summary>
        public int OnSaleProducts { set; get; }

        /// <summary>
        /// 待审核商品数
        /// </summary>
        public int WaitForAuditingProducts { set; get; }

        /// <summary>
        /// 审核失败商品数
        /// </summary>
        public int AuditFailureProducts { set; get; }
        /// <summary>
        /// 违规下架商品数
        /// </summary>
        public int InfractionSaleOffProducts { set; get; }
        /// <summary>
        /// 仓库中商品数
        /// </summary>
        public int InStockProducts { set; get; }

        /// <summary>
        /// 品牌申请数
        /// </summary>
        public int BrandApply { set; get; }

        /// <summary>
        /// 商品评论数
        /// </summary>
        public int ProductComments { set; get; }

        /// <summary>
        /// 商品咨询数
        /// </summary>
        public int ProductConsultations { set; get; }

        /// <summary>
        /// 所有发布的商品
        /// </summary>
        public int ProductsCount { set; get; }
        /// <summary>
        /// 发布上限
        /// </summary>
        public int ProductLimit { set; get; }

        /// <summary>
        /// 已传图片
        /// </summary>
        public long ProductImages { set; get; }

        /// <summary>
        /// 图片上限
        /// </summary>
        public long ImageLimit { set; get; }

        /// <summary>
        /// 待付款交易
        /// </summary>
        public int WaitPayTrades { set; get; }

        /// <summary>
        /// 待发货交易数
        /// </summary>
        public int WaitDeliveryTrades { set; get; }

        /// <summary>
        /// 退款记录数(包括订单退款)
        /// </summary>
        public int RefundTrades { set; get; }
        /// <summary>
        /// 退货退款数
        /// </summary>
        public int RefundAndRGoodsTrades { set; get; }

        /// <summary>
        /// 待处理投诉数
        /// </summary>
        public int Complaints { set; get; }
        /// <summary>
        /// 订单总数
        /// </summary>
        public int OrderCounts { set; get; }

        /// <summary>
        /// 订单总金额
        /// </summary>
        public decimal? OrderAmount { set; get; }

        /// <summary>
        /// 退款金额
        /// </summary>
        public decimal? RefundAmount { set; get; }
    }
}