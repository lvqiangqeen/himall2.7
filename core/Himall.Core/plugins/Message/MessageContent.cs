using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Core.Plugins.Message
{
   public class MessageContent
    {
        public string Bind { set; get; }

        public string OrderCreated { set; get; }

        public string OrderPay { set; get; }

        public string OrderShipping { set; get; }

        public string OrderRefund { set; get; }

        public string FindPassWord { set; get; }

        public string ShopAudited { set; get; }

        public string SendCouponSuccess { set; get; }

        public string ShopSuccess { set; get; }
        public string RefundDeliver { set; get; }

      
    }
}
