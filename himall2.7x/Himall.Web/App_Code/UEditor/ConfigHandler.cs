using System;
using System.Web;



namespace Himall.Web.App_Code.UEditor
{

    /// <summary>
    /// Config 的摘要说明
    /// </summary>
    public class ConfigHandler : IUEditorHandle
    {
        public ConfigHandler() : base() { }

        public Object Process()
        {
            return (Config.Items);
        }
    }

}