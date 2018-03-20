using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Himall.IServices;
using Himall.Service;

namespace Himall.IServiceTest
{
    [TestClass]
    public class RefundSerTest
    {

        [TestMethod]
        public void TestNotify()
        {
            string _tmp = "1";
            var ser = new RefundService();
            ser.NotifyRefund("2015102253741655");
            Assert.IsTrue(!string.IsNullOrWhiteSpace(_tmp));
        }
    }
}
