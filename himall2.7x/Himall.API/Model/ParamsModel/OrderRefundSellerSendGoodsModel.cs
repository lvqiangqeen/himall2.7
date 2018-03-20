using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.API.Model.ParamsModel
{
    /// <summary>
    /// 售后买家寄货
    /// </summary>
    public class OrderRefundSellerSendGoodsModel
    {
        public long Id { get; set; }
        public string ExpressCompanyName { get; set; }
        public string ShipOrderNumber { get; set; }
    }
}
