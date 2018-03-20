using System.Collections.Generic;

namespace Himall.Model
{
    /// <summary>
    /// 首页分类集
    /// </summary>
    public class HomeCategorySet
    {

        public class HomeCategoryTopic
        {

            public string Url{get;set;}

            public string ImageUrl{get;set;}
        }


        /// <summary>
        /// 首页分类集所包含的分类
        /// </summary>
        public IEnumerable<HomeCategoryInfo> HomeCategories { get; set; }

        /// <summary>
        /// 首页分类集所包含的专题
        /// </summary>
        public IEnumerable<HomeCategoryTopic> HomeCategoryTopics { get; set; }
       

        /// <summary>
        /// 分类集所在行号
        /// </summary>
        public int RowNumber { get; set; }

        /// <summary>
        /// 产品对拼品牌
        /// </summary>
        public IEnumerable<BrandInfo> HomeBrand { get; set; }
    }
}
