using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web;
using Himall.Model;

namespace Himall.Web.Areas.Web.Models
{
    public class RefundApplyModel
    {
        public RefundApplyModel()
        {
            OrderItems = new List<OrderItemInfo>();
        }
        /// <summary>
        /// 是否整笔退
        /// </summary>
        public bool IsOrderAllRefund { get; set; }
        /// <summary>
        /// 订单编号
        /// </summary>
        public long OrderId { get; set; }
        /// <summary>
        /// 订单
        /// </summary>
        public OrderInfo OrderInfo { get; set; }
        /// <summary>
        /// 订单项
        /// </summary>
        public List<OrderItemInfo> OrderItems { get; set; }
        /// <summary>
        /// 订单项编号
        /// </summary>
        public long? OrderItemId { get; set; }
        /// <summary>
        /// 售后编号
        /// </summary>
        public long? RefundId { get; set; }
        /// <summary>
        /// 售后类型
        /// </summary>
        public OrderRefundInfo.OrderRefundMode? RefundMode { get; set; }
        /// <summary>
        /// 最大可退金额
        /// </summary>
        public decimal? MaxRefundAmount { get; set; }
        /// <summary>
        /// 最大可退数量
        /// </summary>
        public long MaxRefundGoodsNumber { get; set; }
        /// <summary>
        /// 单价
        /// </summary>
        public decimal RefundGoodsPrice { get; set; }
        /// <summary>
        /// 联系人
        /// </summary>
        public string ContactPerson { get; set; }
        /// <summary>
        /// 联系电话
        /// </summary>
        public string ContactCellPhone { get; set; }
        /// <summary>
        /// 退款方式(选框)
        /// </summary>
        public List<SelectListItem> RefundWay { get; set; }
        /// <summary>
        /// 退款方式
        /// </summary>
        public OrderRefundInfo.OrderRefundPayType? RefundWayValue { get; set; }
        /// <summary>
        /// 退款原因
        /// </summary>
        public string RefundReasonValue { get; set; }
        /// <summary>
        /// 退款原因(选框)
        /// </summary>
        public List<SelectListItem> RefundReasons { get; set; }

		/// <summary>
		/// 退货地址
		/// </summary>
		public string ReturnGoodsAddress { get; set; }

        /// <summary>
        /// 退款原因详情
        /// </summary>
        public string RefundReasonDetail { set; get; }


        public int? BackOut { get; set; }
        /// <summary>
        /// 售后凭证
        /// </summary>
        public string CertPic1 { get; set; }
        /// <summary>
        /// 售后凭证
        /// </summary>
        public string CertPic2 { get; set; }
        /// <summary>
        /// 售后凭证
        /// </summary>
        public string CertPic3 { get; set; }
    }
}