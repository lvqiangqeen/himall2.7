using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
    /// <summary>
    /// 商品类别
    /// </summary>
    public class Category
    {
        /// <summary>
        /// 分类ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 分类名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 分类图片
        /// </summary>
        public string Image { get; set; }

        /// <summary>
        /// 分类的深度
        /// </summary>
        public int Depth { get; set; }

        /// <summary>
        /// 培训序列
        /// </summary>
        public long DisplaySequence { get; set; }

        /// <summary>
        /// 子类
        /// </summary>
        public List<Category> SubCategories { get; set; }
    }
}
