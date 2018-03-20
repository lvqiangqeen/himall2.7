using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.SmallProgAPI.Model
{
    public class OrderRefundGetReturnDetailModel : BaseResultModel
    {
        public OrderRefundGetReturnDetailModel(bool status) : base(status)
        {
        }
        public ReturnDetail Data { get; set; }
    }

    public class ReturnDetail
    {
        public string SkuId { get; set; }
        public string Cellphone { get; set; }
        public string AdminRemark { get; set; }
        public string ShipAddress { get; set; }
        public string ShipTo { get; set; }
        public string ApplyForTime { get; set; }
        public string Remark { get; set; }
        public int Status { get; set; }
        public string StatusText { get; set; }
        public string DealTime { get; set; }
        public string FinishTime { get; set; }
        public string UserSendGoodsTime { get; set; }
        public string ConfirmGoodsTime { get; set; }
        public string Operator { get; set; }
        public string Reason { get; set; }
        public long ReturnId { get; set; }
        public string ShipOrderNumber { get; set; }
        public string OrderId { get; set; }
        public long Quantity { get; set; }
        public string OrderTotal { get; set; }
        public bool IsOnlyRefund { get; set; }
        public string RefundMoney { get; set; }
        public string RefundType { get; set; }
        public List<string> UserCredentials { get; set; }
        public string BankAccountName { get; set; }
        public string BankAccountNo { get; set; }
        public string BankName { get; set; }
        public string ShopName { get; set; }
        public bool CanResetActive { get; set; }
        public bool IsOrderRefundTimeOut { get; set; }
        public List<RefundDetailSKU> ProductInfo { get; set; }
    }
}
