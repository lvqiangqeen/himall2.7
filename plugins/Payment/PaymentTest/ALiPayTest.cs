using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Himall.Plugin.Payment.Alipay_App;
using Himall.Core;
using Himall.Core.Plugins.Payment;

namespace PaymentTest
{
    [TestClass]
    public class ALiPayTest
    {

        [TestMethod]
        public void TestConfigRead()
        {
            Service ser = new Service();
            ser.WorkDirectory = Himall.Core.Helper.IOHelper.GetMapPath("/");
            string refund_batch_no = "2015102289470119";
            string out_trade_no = "2015102221001004990001944835";
            decimal refund_fee = 0.01M;
            decimal total_fee = 0;
            string notify_url = "http://lussnail.wicp.net/Pay/RefundNotify/Himall-Plugin-Payment-Alipay";
            var refundResult = ser.GetRequestUrl(notify_url,notify_url,refund_batch_no,total_fee,"测试");
            //string result = ser.GetRequestUrl("returnurl", "notifyurl", "2001545orderid", 55, "productinfo");
            Assert.IsTrue(!string.IsNullOrWhiteSpace(refundResult));
            //Assert.IsTrue((ser != null));
        }
    }
}
