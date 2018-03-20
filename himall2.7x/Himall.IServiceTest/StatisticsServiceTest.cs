using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Himall.Service;
using Himall.IServices;
using System.Linq;
using Himall.Model;
using System.Text;

namespace Himall.IServiceTest
{
    [TestClass]
    public class StatisticsServiceTest
    {
        [TestMethod]
        public void GetMemberChartTest()
        {
            var data = ServiceProvider.Instance<IStatisticsService>.Create.GetMemberChart(2014, 8, 1);
            Assert.AreEqual(0, 0);
        }

        [TestMethod]
        public void GetShopRankingChartTest()
        {
            var data = ServiceProvider.Instance<IStatisticsService>.Create.GetShopRankingChart(2014, 8);
            Assert.AreEqual(0, 0);
        }

        [TestMethod]
        public void GetShopRankingChartByWeekTest()
        {
            var data = ServiceProvider.Instance<IStatisticsService>.Create.GetShopRankingChart(2014, 8, 1, Model.ShopDimension.Sales);
            Assert.AreEqual(0, 0);
        }
    }
}
