using Himall.Core.Helper;
using Himall.Core.Plugins;
using Himall.CoreTest.Plugins;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Himall.CoreTest
{
    class PluginTest
    {
        [TestClass]
        public class PluginTestBase
        {
            [TestMethod]
            public void Core_Plugin_InStallTest()
            {
                string pluginDirectory = IOHelper.GetMapPath("plugin\\TestPlugin");
                Core.PluginsManagement.InstallPlugin(pluginDirectory);
                PluginInfo pluginInfo = Core.PluginsManagement.GetInstalledPluginInfos(PluginType.PayPlugin).ElementAt(0);
                PluginSample plugin = Core.Instance.Get<PluginSample>(pluginInfo.ClassFullName);
                string expectedName = "这是：Himall.Plugin.PlugInTest1";
                Assert.AreEqual(expectedName, plugin.Name);
            }
        }
    }
}
