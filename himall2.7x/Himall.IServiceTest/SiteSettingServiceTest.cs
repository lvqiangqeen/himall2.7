using Himall.IServices;
using Himall.Model;
using Himall.ServiceProvider;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Himall.IServiceTest
{
    [TestClass]
    public class SiteSettingServiceTest
    {
        [TestMethod]
        public void SetSiteSettingsTest()
        {
            var service = Instance<ISiteSettingService>.Create;
            SiteSettingsInfo setting = new SiteSettingsInfo()
            {
                CustomerTel = "A123456",
                SiteName = "A这是站点名",
                ICPNubmer = "A123456789",
                SiteIsClose = true
            };
            service.SetSiteSettings(setting);

        }



        [TestMethod]
        public void GetSiteSettingsTest()
        {
            var service = Instance<ISiteSettingService>.Create;
            Assert.IsTrue(service.GetSiteSettings() != null);
        }

    }
}
