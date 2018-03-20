
namespace Himall.Web.Areas.Admin.Models
{
    public class ExpressElement
    {
        /// <summary>
        /// A点（左上角顶点）坐标
        /// </summary>
        public int[] a { get; set; }

        /// <summary>
        /// B点（右下解顶点）坐标
        /// </summary>
        public int[] b { get; set; }

        /// <summary>
        /// 元素名称
        /// </summary>
        public int name { get; set; }
    }
}