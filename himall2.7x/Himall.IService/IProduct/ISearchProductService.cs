using Himall.IServices.QueryModel;
using Himall.Model;
using System.Collections.Generic;
using System.Linq;
using System;
using Himall.CommonModel;

namespace Himall.IServices
{
	/// <summary>
	/// 商品搜索服务接口
	/// </summary>
	public interface ISearchProductService : IService
	{
        /// <summary>
        /// 添加冗余搜索商品数据
        /// </summary>
        /// <param name="productId"></param>
        void AddSearchProduct(long productId);
        /// <summary>
        /// 修改冗余搜索商品数据
        /// </summary>
        /// <param name="productId"></param>
        void UpdateSearchProduct(long productId);
        SearchProductResult SearchProduct(SearchProductQuery query);
        SearchProductFilterResult SearchProductFilter(SearchProductQuery query);
        void UpdateSearchStatusByProduct(long productId);
        void UpdateSearchStatusByProducts(List<long> productIds);
        /// <summary>
        /// 修改品牌名称、LOGO
        /// </summary>
        /// <param name="brand"></param>
        void UpdateBrand(BrandInfo brand);
        /// <summary>
        /// 修改分类名称
        /// </summary>
        /// <param name="category"></param>
        void UpdateCategory(CategoryInfo category);
        void UpdateShop(long shopId, string shopName);
        void UpdateSearchStatusByShop(long shopId);

        /// <summary>
        /// 小程序查询
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        SearchProductResult SearchAppletProduct(SearchProductQuery query);

    }
}
