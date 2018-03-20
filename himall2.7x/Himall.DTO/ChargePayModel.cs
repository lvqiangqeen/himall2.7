using Himall.Core.Plugins.Payment;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.ViewModel
{
    public class ChargePayModel
    {
        public Model.ChargeDetailInfo Orders { get; set; }
        public string OrderIds { get; set; }

        public decimal TotalAmount { get; set; }

        public int Step { get; set; }

        public int UnpaidTimeout { get; set; }
        /// <summary>
        /// 支付方式
        /// </summary>
        public List<PaymentModel> models { get; set; }

    }

    public class PaymentViewModel
    {
        public IEnumerable<PaymentModel> Models { get; set; }


        public List<OrderInfo> Orders { get; set; }
        public bool IsSuccess { get; set; }

        public bool HaveNoSalePro { get; set;}

        public decimal Capital { get; set; }

        public decimal TotalAmount { get; set; }
    }

    public class PaymentModel
    {
        public bool IsSuccess { get; set; }
        public string RequestUrl { get; set; }

        public string Logo { get; set; }

        public string Id { get; set; }

        public UrlType UrlType { get; set; }
    }

}
