using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Himall.Core;

namespace Himall.Model
{
    public partial class OrderRefundInfo
    {
		//由于新增门店自提功能，所以售后审核状态中待商家审核也可能是待门店审核(待商家收货,商家拒绝，商家通过审核等也类似)
		//由于枚举值已在多个地方判断时固定死，不好添加新枚举，所以需要根据订单的发货方式判断审核状态
        /// <summary>
        /// 商家审核状态
        /// </summary>
        public enum OrderRefundAuditStatus
        {
            /// <summary>
            /// 待商家/门店审核
            /// </summary>
            [Description("待商家审核")]
            WaitAudit = 1,

            /// <summary>
            /// 待买家寄货
            /// </summary>
            [Description("待买家寄货")]
            WaitDelivery = 2,

            /// <summary>
            /// 待商家/门店收货收货
            /// </summary>
            [Description("待商家收货")]
            WaitReceiving = 3,

            /// <summary>
            /// 商家/门店拒绝
            /// </summary>
            [Description("商家拒绝")]
            UnAudit = 4,

            /// <summary>
            /// 商家/门店通过审核
            /// </summary>
            [Description("商家通过审核")]
            Audited = 5
        }

        /// <summary>
        /// 平台确认状态
        /// </summary>
        public enum OrderRefundConfirmStatus
        {
            /// <summary>
            /// 待平台确认
            /// </summary>
            [Description("待平台确认")]
            UnConfirm = 6,

            /// <summary>
            /// 退款成功
            /// </summary>
            [Description("退款成功")]
            Confirmed=7
        }

        /// <summary>
        /// 退款方式
        /// </summary>
        public enum OrderRefundMode
        {
            /// <summary>
            /// 订单退款
            /// </summary>
            [Description("订单退款")]
            OrderRefund = 1,

            /// <summary>
            /// 货品退款(收到货后只退款)
            /// </summary>
            [Description("货品退款")]
            OrderItemRefund = 2,

            /// <summary>
            /// 退货退款
            /// </summary>
            [Description("退货退款")]
            ReturnGoodsRefund = 3
        }

        #region 
        //TODO: lly 2015-08-03 增加枚举 
        /// <summary>
        /// 退款支付状态
        /// </summary>
        public enum OrderRefundPayStatus
        {
            /// <summary>
            /// 支付成功
            /// </summary>
            [Description("支付成功")]
            PaySuccess = 1,
            /// <summary>
            /// 支付失败
            /// </summary>
            [Description("支付失败")]
            PayFail = -1,
            /// <summary>
            /// 已支付
            /// <para>已跳转支付平台，未收到异步通知道前</para>
            /// </summary>
            [Description("已支付")]
            Payed = 0
        }
        /// <summary>
        /// 退款支付方式
        /// </summary>
        public enum OrderRefundPayType
        {
            /// <summary>
            /// 原路返回
            /// </summary>
            [Description("原路返回")]
            BackOut = 1,
            /// <summary>
            /// 线下支付
            /// </summary>
            [Description("线下支付")]
            OffLine = 2,
            /// <summary>
            /// 退到预付款
            /// </summary>
            [Description("退到预付款")]
            BackCapital=3
        }
        #endregion

        /// <summary>
        /// 退货数量
        /// </summary>
        [NotMapped]
        public long ShowReturnQuantity {
            get
            {
                long result = 0;
                if(this!=null)
                {
                    if(this.ReturnQuantity.HasValue)
                    {
                        result = this.ReturnQuantity.Value;
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// 售后类型
        /// </summary>
        [NotMapped]
        public int RefundType { set; get; }
        /// <summary>
        /// 当前退款状态
        /// </summary>
        public string RefundStatus
        {
            get
            {
                string result = "";
                result = this.SellerAuditStatus.ToDescription();
                if (this.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.Audited)
                {
                    result = this.ManagerConfirmStatus.ToDescription();
                }
                return result;
            }
        }
        /// <summary>
        /// 当前退款状态
        /// </summary>
        public int? RefundStatusValue
        {
            get
            {
                int? result = null;
                result = (int)this.SellerAuditStatus;
                if (this.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.Audited)
                {
                    result = (int)this.ManagerConfirmStatus;
                }
                return result;
            }
        }
        /// <summary>
        /// 可退金额
        /// <para>订单退款为了(实付+运费-优惠)，单件退款(实付-优惠)</para>
        /// </summary>
        public decimal EnabledRefundAmount { get; set; }

        /// <summary>
        /// 订单是否已超过售后期
        /// </summary>
        [NotMapped]
        public bool IsOrderRefundTimeOut { get; set; }

		public object Order { get; set; }

        public string formId { get; set; }
	}
}
