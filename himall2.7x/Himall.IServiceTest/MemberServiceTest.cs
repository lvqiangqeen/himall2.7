using Himall.IServices;
using Himall.ServiceProvider;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Himall.IServiceTest
{
    [TestClass]
    public class MemberServiceTest
    {
        [TestMethod]
        public void RegisterTest()
        {
            var service = Instance<IMemberService>.Create;
            string username = "test" + DateTime.Now.ToString("yyyyMMddHHmmss");
            string password = "123456";
            service.Register(username, password);
        }
    }
}
