using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Model;

namespace Himall.API.Model.ParamsModel
{
    /// <summary>
    /// 提交退款/售后申请
    /// </summary>
    public class OrderRefundApplyModel
    {
        public long? refundId { get; set; }
        public long OrderId { get; set; }
        public long? OrderItemId { get; set; }
        public string Reason { get; set; }
        public string ContactPerson { get; set; }
        public string ContactCellPhone { get; set; }
        public string RefundAccount { get; set; }
        public System.DateTime ApplyDate { get; set; }
        public OrderRefundInfo.OrderRefundPayType RefundPayType { get; set; }
        public decimal Amount { get; set; }
        public long? ReturnQuantity { get; set; }
        /// <summary>
        /// 售后类型
        /// </summary>
        public int RefundType { set; get; }
    }
}
