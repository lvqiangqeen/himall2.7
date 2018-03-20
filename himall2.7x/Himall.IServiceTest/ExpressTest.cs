using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Himall.IServices;

namespace Himall.IServiceTest
{
    [TestClass]
    public class ExpressTest
    {
        [TestMethod]
        public void GetExpressDataTest()
        {
            var expressService = ServiceProvider.Instance<IExpressService>.Create;
            string companyName = "申通快递";
            string shipOrderNO = "868905358323";
            var expressData = expressService.GetExpressData(companyName, shipOrderNO);
            Assert.IsTrue(expressData.Success);
        }
    }
}
