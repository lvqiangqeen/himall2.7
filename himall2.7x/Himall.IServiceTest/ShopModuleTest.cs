using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.ServiceProvider;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Himall.IServiceTest
{
    [TestClass]
    public class ShopModuleTest
    {
        const long SHOPID = 33;

        [TestMethod]
        public void GetShopsTest()
        {
            ShopQuery queryModel = new ShopQuery
           { 
               ShopName = "店铺"
           };
            var service = Instance<IShopService>.Create;
            var shops = service.GetShops( queryModel );
            string s = "";
        }

        [TestMethod]
        public void AddShopProductModuleTest()
        {

            var service = Instance<IShopHomeModuleService>.Create;
            var shopHomeModule = new ShopHomeModuleInfo()
            {
                Name = DateTime.Now.ToString( "name_yyyyMMddHHmmss" ) ,
                ShopId = SHOPID
            };
            service.AddShopProductModule( shopHomeModule );

        }

        [TestMethod]
        public void UpdateShopProductModuleNameTest()
        {

            var service = Instance<IShopHomeModuleService>.Create;
            var shopHomeModule = new ShopHomeModuleInfo()
            {
                Name = DateTime.Now.ToString( "name_yyyyMMddHHmmss" ) ,
                ShopId = SHOPID
            };
            service.AddShopProductModule( shopHomeModule );

            string newName = DateTime.Now.ToString( "name2_yyyyMMddHHmmss" );
            service.UpdateShopProductModuleName( SHOPID , shopHomeModule.Id , newName );
            var updatedShopHomeModule = service.GetShopHomeModuleInfo( SHOPID , shopHomeModule.Id );
            Assert.AreEqual( newName , updatedShopHomeModule.Name );

        }


        [TestMethod]
        public void GetShopHomeModuleInfoTest()
        {

            var service = Instance<IShopHomeModuleService>.Create;
            var shopHomeModule = new ShopHomeModuleInfo()
            {
                Name = DateTime.Now.ToString( "name_yyyyMMddHHmmss" ) ,
                ShopId = SHOPID
            };
            service.AddShopProductModule( shopHomeModule );

            var updatedShopHomeModule = service.GetShopHomeModuleInfo( SHOPID , shopHomeModule.Id );
            Assert.AreEqual( shopHomeModule.Name , updatedShopHomeModule.Name );
        }


        [TestMethod]
        public void UpdateShopProductModuleProductsTest()
        {

            var service = Instance<IShopHomeModuleService>.Create;
            var shopHomeModule = new ShopHomeModuleInfo()
            {
                Name = DateTime.Now.ToString( "name_yyyyMMddHHmmss" ) ,
                ShopId = SHOPID
            };
            service.AddShopProductModule( shopHomeModule );

            var productIds = new long[] { 9 , 10 , 11 , 12 };
            service.UpdateShopProductModuleProducts( SHOPID , shopHomeModule.Id , productIds );

            var updatedShopHomeModule = service.GetShopHomeModuleInfo( SHOPID , shopHomeModule.Id );
            Assert.AreEqual( productIds.Length , updatedShopHomeModule.ShopHomeModuleProductInfo.Count );

        }


        [TestMethod]
        public void DeleteTest()
        {

            var service = Instance<IShopHomeModuleService>.Create;
            var shopHomeModule = new ShopHomeModuleInfo()
            {
                Name = DateTime.Now.ToString( "name_yyyyMMddHHmmss" ) ,
                ShopId = SHOPID
            };
            service.AddShopProductModule( shopHomeModule );
            var updatedShopHomeModule = service.GetShopHomeModuleInfo( SHOPID , shopHomeModule.Id );
            Assert.IsNotNull( updatedShopHomeModule );

            service.Delete( SHOPID , shopHomeModule.Id );
            updatedShopHomeModule = service.GetShopHomeModuleInfo( SHOPID , shopHomeModule.Id );
            Assert.IsNull( updatedShopHomeModule );
        }
    }
}
