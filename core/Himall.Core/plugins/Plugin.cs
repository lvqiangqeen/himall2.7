
namespace Himall.Core.Plugins
{
    /// <summary>
    /// 插件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Plugin<T> : PluginBase where T : IPlugin
    {
        /// <summary>
        /// 相关业务
        /// </summary>
        public T Biz { get; set; }
    }
}
