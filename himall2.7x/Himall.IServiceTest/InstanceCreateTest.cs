using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Himall.IServices;
using Himall.ServiceProvider;

namespace Himall.IServiceTest
{
    [TestClass]
    public class InstanceCreateTest
    {
        [TestMethod]
        public void IService_CreateInstance()
        {
            IProductService productService =Instance<IProductService>.Create;
            Assert.IsNotNull(productService);
        }
    }
}
