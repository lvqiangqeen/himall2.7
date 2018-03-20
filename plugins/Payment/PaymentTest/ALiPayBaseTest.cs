using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Himall.Plugin.Payment.Alipay.Base;

namespace PaymentTest
{
    [TestClass]
    public class ALiPayBaseTest
    {

        [TestMethod]
        public void TestConfigRead()
        {
            _ServiceBase ser = new _ServiceBase();
            ser.WorkDirectory = Himall.Core.Helper.IOHelper.GetMapPath("/");
            Config con = ser.GetConfig();
            Assert.IsTrue(string.IsNullOrWhiteSpace(con.Private_key));
            //Assert.IsTrue((ser != null));
        }

        [TestMethod]
        public void TestProcessRefundFee()
        {
            _ServiceBase ser = new _ServiceBase();
            ser.WorkDirectory = Himall.Core.Helper.IOHelper.GetMapPath("/");
            var reult = ser.ProcessRefundFee(new Himall.Core.Plugins.Payment.PaymentPara() { out_refund_no = DateTime.Now.ToString("yyyyMMddHHmmss"), refund_fee = 1 });
            Assert.IsTrue(!string.IsNullOrWhiteSpace(reult.ResponseContentWhenFinished));
            //Assert.IsTrue((ser != null));
        }
    }
}
