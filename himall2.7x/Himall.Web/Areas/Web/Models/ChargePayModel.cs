
using Himall.Model;
using System.Collections.Generic;
namespace Himall.Web.Areas.Web.Models
{
    public class ChargePayModel
    {
        public ChargeDetailInfo Orders { get; set; }
        public string OrderIds { get; set; }

        public decimal TotalAmount { get; set; }

        public int Step { get; set; }

        public int UnpaidTimeout { get; set; }
        /// <summary>
        /// 支付方式
        /// </summary>
        public List<PaymentModel> models { get; set; }

    }
}