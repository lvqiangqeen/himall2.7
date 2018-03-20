using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.SellerAdmin.Models
{
    public class HomeModel
    {
        /// <summary>
        /// 老版本的model对象
        /// </summary>
        public SellerConsoleModel SellerConsoleModel { get; set; }
        /// <summary>
        /// 店铺logo
        /// </summary>
        public string ShopLogo { get; set; }


        //店铺ID
        public long ShopId { set; get; }

        /// <summary>
        /// 店铺名称
        /// </summary>
        public string ShopName { get; set; }

        /// <summary>
        /// 店铺截至日期
        /// </summary>
        public string ShopEndDate { get; set; }

        /// <summary>
        /// 店铺等级
        /// </summary>
        public string ShopGradeName { get; set; }

        /// <summary>
        /// 宝贝与描述相符
        /// </summary>
        public string ProductAndDescription { get; set; }

        /// <summary>
        /// 宝贝与描述相符百分比
        /// </summary>
        public string ProductAndDescriptionPercentage { get {
            return ((int)( Convert.ToDouble( this.ProductAndDescription) / 5.0 * 100)).ToString()+"%";
        } }

        /// <summary>
        /// 卖家服务态度
        /// </summary>
        public string SellerServiceAttitude { get; set; }

        /// <summary>
        /// 卖家服务态度百分比
        /// </summary>
        public string SellerServiceAttitudePercentage
        {
            get
            {
                return ((int)(Convert.ToDouble(this.SellerServiceAttitude) / 5.0 * 100)).ToString() + "%";
            }
        }

        /// <summary>
        /// 卖家发货速度
        /// </summary>
        public string SellerDeliverySpeed { get; set; }

        /// <summary>
        /// 卖家发货速度百分比
        /// </summary>
        public string SellerDeliverySpeedPercentage
        {
            get
            {
                return ((int)(Convert.ToDouble(this.SellerDeliverySpeed) / 5.0 * 100)).ToString() + "%";
            }
        }

        /// <summary>
        /// 发布商品数
        /// </summary>
        public string ProductsNumber { get; set; }

        /// <summary>
        /// 所有商品数
        /// </summary>
        public string ProductsNumberIng { get; set; }

        /// <summary>
        /// 使用空间
        /// </summary>
        public string UseSpace { get; set; }

        /// <summary>
        /// 正使用的空间
        /// </summary>
        public string UseSpaceing { get; set; }

        /// <summary>
        /// 订单 商品咨询
        /// </summary>
        public string OrderProductConsultation { get; set; }

        /// <summary>
        /// 订单总数
        /// </summary>
        public string OrderCounts { get; set; }

        /// <summary>
        /// 待买家付款
        /// </summary>
        public string OrderWaitPay { get; set; }

        /// <summary>
        /// 待发货
        /// </summary>
        public string OrderWaitDelivery { get; set; }


        /// <summary>
        /// 待回复评价
        /// </summary>
        public string OrderReplyComments { get; set; }

        /// <summary>
        /// 待处理投诉
        /// </summary>
        public string OrderHandlingComplaints{get;set;}

        /// <summary>
        /// 待处理退款
        /// </summary>
        public string OrderWithRefund { get; set; }

        /// <summary>
        /// 待处理退款
        /// </summary>
        public string OrderWithRefundAndRGoods { get; set; }

        /// <summary>
        /// 商品 销售中
        /// </summary>
        public string ProductsOnSale { get; set; }
        /// <summary>
        /// 商品 草稿箱
        /// </summary>
        public string ProductsInDraft { get; set; }

        /// <summary>
        /// 商品 待审核
        /// </summary>
        public string ProductsWaitForAuditing { get; set; }

        /// <summary>
        /// 商品 审核未通过
        /// </summary>
        public string ProductsAuditFailed { get; set; }

        /// <summary>
        /// 商品 违规下架
        /// </summary>
        public string ProductsInfractionSaleOff { get; set; }

        /// <summary>
        /// 商品 仓库中
        /// </summary>
        public string ProductsInStock { get; set; }
        /// <summary>
        /// 超出警戒库存的商品数
        /// </summary>
        public string OverSafeStockProducts { get; set; }

        /// <summary>
        /// 商品评价
        /// </summary>
        public string ProductsEvaluation { get; set; }

        /// <summary>
        /// 授权品牌
        /// </summary>
        public string ProductsBrands { get; set; }

        /// <summary>
        /// 公告
        /// </summary>
        public IQueryable<ArticleInfo> Articles { get; set; }
    }
}