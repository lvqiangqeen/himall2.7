using Himall.Search.Service.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Search.Service.Interface
{
    public interface ISearch
    {

        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="model">索引对象</param>
        /// <returns>创建索引是否成功</returns>
        void CreateIndex(long productId, string productName);

        /// <summary>
        /// 根据商品标识删除索引
        /// </summary>
        /// <param name="productId">商品标识</param>
        /// <returns>删除索引是否成功</returns>
        void DeleteIndex(long productId);
        
        /// <summary>
        /// 查询商品
        /// </summary>
        /// <param name="search">查询条件</param>
        /// <returns>商品数据结果</returns>
        ProductViewResult DoSearch(SearchCondition condition);

        /// <summary>
        /// 查询品牌
        /// </summary>
        /// <param name="search">查询条件</param>
        /// <returns>品牌数据集合</returns>
        List<BrandView> QueryBrand(SearchCondition search);

        /// <summary>
        /// 查询属性集合
        /// </summary>
        /// <param name="search">查询条件</param>
        /// <returns>属性数据集合</returns>
        List<AttributeView> QueryAttrId(SearchCondition search);

        /// <summary>
        /// 查询类别
        /// </summary>
        /// <param name="search">查询条件</param>
        /// <returns>类别数据集合</returns>
        List<CategoryView> QueryCategory(SearchCondition search);

        /// <summary>
        /// 获取联想词
        /// </summary>
        /// <param name="word">查询文本</param>
        /// <returns>可以查询到产品数据的联想词集合</returns>
        List<string> getAssociationalWord(string word);
    }
}
