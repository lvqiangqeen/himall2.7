using Himall.Core.Plugins.Express;
using System.Collections.Generic;

namespace Himall.Core.Plugins
{
    public interface IExpress : IPlugin
    {
        /// <summary>
        /// 快递名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 显示名称
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// 淘宝Code
        /// </summary>
        string TaobaoCode { get; }

        /// <summary>
        /// 快递100Code
        /// </summary>
        string Kuaidi100Code { get; }

        /// <summary>
        /// 快递鸟Code
        /// </summary>
        string KuaidiNiaoCode { get; }

        /// <summary>
        /// 快递单宽
        /// </summary>
        int Width { get; }

        /// <summary>
        /// 快递单高
        /// </summary>
        int Height { get; }

        /// <summary>
        /// 快递单图片
        /// </summary>
        string BackGroundImage { get; set; }

        /// <summary>
        /// Logo
        /// </summary>
        string Logo { get; set; }

        /// <summary>
        /// 获取下一个快递单号
        /// </summary>
        /// <param name="currentExpressCode">当前快递单号</param>
        /// <returns></returns>
        string NextExpressCode(string currentExpressCode);

        /// <summary>
        /// 所包含的打印元素
        /// </summary>
        IEnumerable<ExpressPrintElement> Elements { get; }

        /// <summary>
        /// 检查快递单号是否有效
        /// </summary>
        /// <param name="expressCode">快递单号</param>
        /// <returns></returns>
        bool CheckExpressCodeIsValid(string expressCode);

        /// <summary>
        /// 更新打印项
        /// </summary>
        /// <param name="printElements"></param>
        void UpdatePrintElement(IEnumerable<ExpressPrintElement> printElements);

    }
}
