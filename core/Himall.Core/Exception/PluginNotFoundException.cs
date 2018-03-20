
namespace Himall.Core
{
    /// <summary>
    /// 插件未找到异常
    /// </summary>
    public class PluginNotFoundException:HimallException
    {
        /// <summary>
        /// 插件未找到异常
        /// </summary>
        /// <param name="pluginId">插件标识</param>
        public PluginNotFoundException(string pluginId) : base("未找到插件" + pluginId) { }

    }
}
