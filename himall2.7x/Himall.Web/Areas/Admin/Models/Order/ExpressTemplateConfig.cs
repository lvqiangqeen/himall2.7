using System.Collections.Generic;

namespace Himall.Web.Areas.Admin.Models
{
    public class ExpressTemplateConfig
    {
        /// <summary>
        /// 模板宽度
        /// </summary>
        public int width { get; set; }

        /// <summary>
        /// 模板高度
        /// </summary>
        public int height { get; set; }

        /// <summary>
        /// 模板所包含的元素数据
        /// </summary>
        public Element [] data { get; set; }

        public int selectedCount { get; set; }
    }


    public class Element
    {
        /// <summary>
        /// 元素键
        /// </summary>
        public string key { get; set; }

        /// <summary>
        /// 元素的值
        /// </summary>
        public string value { get; set; }

        /// <summary>
        /// 是否为选中状态
        /// </summary>
        public bool selected { get; set; }

        /// <summary>
        /// A点（左上角顶点）坐标
        /// </summary>
        public int[] a { get; set; }

        /// <summary>
        /// B点（右下解顶点）坐标
        /// </summary>
        public int[] b { get; set; }
    }

}