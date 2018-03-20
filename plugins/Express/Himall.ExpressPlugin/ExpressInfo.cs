using Himall.Core.Plugins.Express;
using System.Collections.Generic;

namespace Himall.ExpressPlugin
{
    public class ExpressInfo
    {

        public string Name { get; set; }

        public string DisplayName { get; set; }

        /// <summary>
        /// 淘宝Code
        /// </summary>
        public string TaobaoCode { get; set; }

        /// <summary>
        /// 快递100Code
        /// </summary>
        public string Kuaidi100Code { get; set; }

        /// <summary>
        /// 快递鸟Code
        /// </summary>
        public string KuaidiNiaoCode { get; set; }

        /// <summary>
        /// 快递单宽
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// 快递单高
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// 快递单图片
        /// </summary>
        public string BackGroundImage { get; set; }

        /// <summary>
        /// Logo
        /// </summary>
        public string Logo { get; set; }

        /// <summary>
        /// 所包含的打印元素
        /// </summary>
        public ExpressPrintElement [] Elements { get; set; }
    }
}
