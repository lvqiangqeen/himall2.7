
using System;
namespace Himall.Core.Plugins
{
    public interface IPlugin
    {
        /// <summary>
        /// 插件工作目录
        /// </summary>
        string WorkDirectory { set; }

        /// <summary>
        /// 检查是否可以开启插件
        /// </summary>
        /// <returns></returns>
        /// <exception cref="PluginConfigException"></exception>
        void CheckCanEnable();
    }
}
