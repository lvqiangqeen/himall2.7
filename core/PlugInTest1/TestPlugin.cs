using System;

namespace Himall.Plugin.PlugInTest1
{
    public class TestPlugin:Himall.CoreTest.Plugins.PluginSample
    {

        public string Name
        {
            get { return "这是：Himall.Plugin.PlugInTest1"; }
        }

        public string WorkDirectory
        {
            set { throw new NotImplementedException(); }
        }


        public void CheckCanEnable()
        {
            throw new NotImplementedException();
        }
    }
}
