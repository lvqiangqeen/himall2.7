using Himall.IServices;
using Himall.Model;
using Himall.ServiceProvider;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Himall.IServiceTest
{
    [TestClass]
    public class CustomerService
    {

        const int SHOPID = 33;

        [TestMethod]
        public void AddCustomerServiceTest()
        {
            var customerService = new CustomerServiceInfo()
            {
                ShopId = SHOPID,
                Name = "小黑",
                Tool = CustomerServiceInfo.ServiceTool.Wangwang,
                Type = CustomerServiceInfo.ServiceType.AfterSale,
                AccountCode = "564654343"
            };
            var service = Instance<ICustomerService>.Create;
            service.AddCustomerService(customerService);
        }

      



        [TestMethod]
        public void GetCustomerServiceTest()
        {
            var service = Instance<ICustomerService>.Create;
            var customerServices = service.GetCustomerService(SHOPID);
            Assert.IsTrue(customerServices.Count() > 0);
        }


        [TestMethod]
        public void GetCustomerServiceByIdTest()
        {
            var customerService = new CustomerServiceInfo()
            {
                ShopId = SHOPID,
                Name = "小黑",
                Tool = CustomerServiceInfo.ServiceTool.Wangwang,
                Type = CustomerServiceInfo.ServiceType.AfterSale,
                AccountCode = "564654343"
            };
            var service = Instance<ICustomerService>.Create;
            service.AddCustomerService(customerService);

            var customerService2 = service.GetCustomerService(SHOPID,customerService.Id);
            Assert.AreEqual(customerService.Name, customerService2.Name);
            Assert.AreEqual(customerService.Tool, customerService2.Tool);
            Assert.AreEqual(customerService.Type, customerService2.Type);
            Assert.AreEqual(customerService.AccountCode, customerService2.AccountCode);
        }

        [TestMethod]
        public void UpdateCustomerServicTest()
        {
            var customerService = new CustomerServiceInfo()
            {
                ShopId = SHOPID,
                Name = "小黑",
                Tool = CustomerServiceInfo.ServiceTool.Wangwang,
                Type = CustomerServiceInfo.ServiceType.AfterSale,
                AccountCode = "564654343"
            };
            var service = Instance<ICustomerService>.Create;
            service.AddCustomerService(customerService);

            var customerService2 = new CustomerServiceInfo()
            {
                Id = customerService.Id,
                ShopId = SHOPID,
                Name = "小黑2",
                Tool = CustomerServiceInfo.ServiceTool.Wangwang,
                Type = CustomerServiceInfo.ServiceType.AfterSale,
                AccountCode = Guid.NewGuid().ToString("N").Substring(0, 12)
            };
            service.UpdateCustomerService(customerService2);

            var actualCustomerService2 = service.GetCustomerService(SHOPID, customerService.Id);

            Assert.AreEqual(customerService2.Name, actualCustomerService2.Name);
            Assert.AreEqual(customerService2.Tool, actualCustomerService2.Tool);
            Assert.AreEqual(customerService2.Type, actualCustomerService2.Type);
            Assert.AreEqual(customerService2.AccountCode, actualCustomerService2.AccountCode);
        }



        [TestMethod]
        public void DeleteCustomerServiceTest()
        {
            var customerService = new CustomerServiceInfo()
            {
                ShopId = SHOPID,
                Name = "小黑",
                Tool = CustomerServiceInfo.ServiceTool.Wangwang,
                Type = CustomerServiceInfo.ServiceType.AfterSale,
                AccountCode = "564654343"
            };
            var service = Instance<ICustomerService>.Create;
            service.AddCustomerService(customerService);
            var customerService2 = service.GetCustomerService(SHOPID, customerService.Id);
            Assert.IsNotNull(customerService2);

            service.DeleteCustomerService(SHOPID, customerService.Id);
            customerService2 = service.GetCustomerService(SHOPID, customerService.Id);
            Assert.IsNull(customerService2);

        }


        [TestMethod]
        public void TestNew()
        {
            var s = Instance<IBonusService>.Create;
            
        }

    }
}
