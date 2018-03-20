
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
namespace Himall.Model
{
    public partial class SKUInfo
    {
        /// <summary>
        /// 颜色别名
        /// </summary>
        public string ColorAlias { get; set; }
        /// <summary>
        /// 尺码别名
        /// </summary>
        public string SizeAlias { get; set; }
        /// <summary>
        /// 规格别名
        /// </summary>
        public string VersionAlias { set; get; }


    }
}
