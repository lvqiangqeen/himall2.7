using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.SmallProgAPI.Model
{
    public class OrderRefundGetListModel : BaseResultModel
    {
        public OrderRefundGetListModel(bool status) : base(status)
        {
        }

        public int RecordCount { get; set; }
        public List<OrderRefundItem> Data { get; set; }
    }

    public class OrderRefundItem
    {
        public string OrderId { get; set; }
        public int Status { get; set; }
        public string StatusText { get; set; }
        public string AdminRemark { get; set; }
        public long AfterSaleId { get; set; }
        public int AfterSaleType { get; set; }
        public string ApplyForTime { get; set; }
        public string ExpressCompanyAbb { get; set; }
        public string ExpressCompanyName { get; set; }
        public string RefundAmount { get; set; }
        public int RefundType { get; set; }
        public string RefundTypeText { get; set; }
        public string ShipOrderNumber { get; set; }
        public string SkuId { get; set; }
        public string OrderTotal { get; set; }
        public int QuantityTotal { get; set; }
        public string UserExpressCompanyAbb { get; set; }
        public string UserExpressCompanyName { get; set; }
        public string UserRemark { get; set; }
        public string UserShipOrderNumber { get; set; }
        public bool IsRefund { get; set; }
        public bool IsReturn { get; set; }
        public bool IsReplace { get; set; }
        public bool IsWaitToDeal { get; set; }
        public bool IsWaitFinishReturn { get; set; }
        public bool IsShowReturnLogistics { get; set; }
        public bool IsWaitGetGoodsOfReplace { get; set; }
        public bool IsWaitConfirmReplace { get; set; }
        public string ShopName { get; set; }
        public long Vshopid { get; set; }
        public List<OrderRefundSku> LineItems { get; set; }
    }

    public class OrderRefundSku
    {
        public int Status { get; set; }
        public string StatusText { get; set; }
        public string SkuId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public decimal Amount { get; set; }
        public long Quantity { get; set; }
        public string Image { get; set; }
        public string SkuText { get; set; }
        public long ProductId { get; set; }
    }
}
