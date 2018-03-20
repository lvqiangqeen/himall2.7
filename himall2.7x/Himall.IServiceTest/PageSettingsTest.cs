using Himall.IServices;
using Himall.ServiceProvider;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Himall.IServiceTest
{
    [TestClass]
    public class PageSettingsTest
    {

        #region 首页分类设置 测试

        [TestMethod]
        public void GetHomeCategorySetsTest()
        {
            var service = Instance<IHomeCategoryService>.Create;
            var homeCategorySets = service.GetHomeCategorySets();
            Assert.AreEqual(service.TotalRowsCount, homeCategorySets.Count());
        }


        [TestMethod]
        public void GetHomeCategorySet()
        {
            int rowNumber = 10;
            var service = Instance<IHomeCategoryService>.Create;
            var homeCategorySet = service.GetHomeCategorySet(rowNumber);
            Assert.AreEqual(rowNumber, homeCategorySet.RowNumber);
        }


        [TestMethod]
        public void AddHomeFloorBasicInfoest()
        {
            string name = "楼层" + DateTime.Now.ToString("yyyyMMddHHmmss");
            long [] categoryIds = new long[] { 2, 3, 4, 5 };

            var service = Instance<IFloorService>.Create;
            var homeFloor = service.AddHomeFloorBasicInfo(name, categoryIds);
            homeFloor = service.GetHomeFloor(homeFloor.Id);

            Assert.AreEqual(name, homeFloor.FloorName);
            Assert.AreEqual(categoryIds.Length, homeFloor.FloorCategoryInfo.Count());
        }


        [TestMethod]
        public void UpdateFloorBasicInfoTest()
        {

            long homeFloorId = 1;
            string name = "楼层" + DateTime.Now.ToString("yyyyMMddHHmmss");
            long[] categoryIds = new long[] { 2, 3, 4, 5 };

            var service = Instance<IFloorService>.Create;
            service.UpdateFloorBasicInfo(homeFloorId, name, categoryIds);

            var homeFloor = service.GetHomeFloor(homeFloorId);
            Assert.AreEqual(name, homeFloor.FloorName);
            Assert.AreEqual(categoryIds.Length, homeFloor.FloorCategoryInfo.Count());
        }


        [TestMethod]
        public void UpdateHomeFloorSequenceTest()
        {
            string name = "楼层" + DateTime.Now.ToString("yyyyMMddHHmmss");
            long[] categoryIds = new long[] { 2, 3, 4, 5 };

            var service = Instance<IFloorService>.Create;

            var floor1 = service.AddHomeFloorBasicInfo(name, categoryIds);
            var floor2 = service.AddHomeFloorBasicInfo(name, categoryIds);

            long sourceDisplaySequence = floor1.DisplaySequence, destinationDisplaySequence = floor2.DisplaySequence;
            service.UpdateHomeFloorSequence(sourceDisplaySequence, destinationDisplaySequence);

            var newFloor1 = service.GetHomeFloor(floor1.Id);
            var newFloor2 = service.GetHomeFloor(floor2.Id);

            Assert.AreEqual(newFloor1.DisplaySequence, destinationDisplaySequence);
            Assert.AreEqual(newFloor2.DisplaySequence, sourceDisplaySequence);
        }


        [TestMethod]
        public void EnableHomeFloorTest()
        {
            string name = "楼层" + DateTime.Now.ToString("yyyyMMddHHmmss");
            long[] categoryIds = new long[] { 2, 3, 4, 5 };
            bool enable = true;

            var service = Instance<IFloorService>.Create;

            var floor = service.AddHomeFloorBasicInfo(name, categoryIds);
            service.EnableHomeFloor(floor.Id, !enable);

            floor = service.GetHomeFloor(floor.Id);

            Assert.AreEqual(!enable, floor.IsShow);
        }




        #endregion
    }
}
