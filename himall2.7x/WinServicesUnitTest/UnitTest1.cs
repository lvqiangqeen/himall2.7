using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WinServicesUnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            WinOrderProductStatisticsService.StatisticOrderProduct service = new WinOrderProductStatisticsService.StatisticOrderProduct();
            service.Execute();
        }
    }
}
