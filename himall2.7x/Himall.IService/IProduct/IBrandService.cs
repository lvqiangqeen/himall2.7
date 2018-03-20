using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.CommonModel;

namespace Himall.IServices
{
    public interface IBrandService : IService
    {
        /// <summary>
        /// 添加一个品牌
        /// </summary>
        /// <param name="model"></param>
        void AddBrand(BrandInfo model);

        /// <summary>
        /// 申请一个品牌
        /// </summary>
        /// <param name="model"></param>
        void ApplyBrand(ShopBrandApplysInfo model);

        /// <summary>
        /// 编辑一个品牌
        /// </summary>
        /// <param name="model"></param>
        void UpdateBrand(BrandInfo model);

        /// 商家编辑品牌
        /// </summary>
        /// <param name="model"></param>
        void UpdateSellerBrand(ShopBrandApplysInfo model);

        /// <summary>
        /// 删除一个品牌
        /// </summary>
        /// <param name="id"></param>
        void DeleteBrand(long id);

        /// <summary>
        /// 分页获取品牌列表
        /// </summary>
        /// <param name="keyWords">查询关键字</param>
        /// <param name="pageNo">当前第几页</param>
        /// <param name="pageSize">每页显示多少条数据</param>
        /// <returns></returns>
        QueryPageModel<BrandInfo> GetBrands(string keyWords, int pageNo, int pageSize);

        /// <summary>
        /// 审核品牌
        /// </summary>
        /// <param name="id">品牌ID</param>
        /// <param name="status">审核结果</param>
        void AuditBrand(long id, Himall.Model.ShopBrandApplysInfo.BrandAuditStatus status);

        /// <summary>
        /// 获取查询的品牌名称列表用于下拉显示
        /// </summary>
        /// <param name="keyWords">关键字</param>
        /// <returns></returns>
        List<BrandInfo> GetBrands(string keyWords);

		/// <summary>
		/// 根据品牌id获取品牌
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
		List<BrandInfo> GetBrandsByIds(IEnumerable<long> ids);

        /// <summary>
        /// 获取一个品牌信息
        /// </summary>
        /// <param name="id">品牌ID</param>
        /// <returns></returns>
        BrandInfo GetBrand(long id);

        /// <summary>
        /// 获取指定分类下的所有商品
        /// </summary>
        /// <param name="categoryIds">分类id</param>
        /// <returns></returns>
        IEnumerable<BrandInfo> GetBrandsByCategoryIds(params long[] categoryIds);

        /// <summary>
        /// 获取指定分类下的所有商品
        /// </summary>
        /// <param name="shopId">店铺id</param>
        /// <param name="categoryIds">分类id</param>
        /// <returns></returns>
        IEnumerable<BrandInfo> GetBrandsByCategoryIds(long shopId, params long[] categoryIds);

        /// <summary>
        /// 分页获取商家品牌列表
        /// </summary>
        /// <param name="shopId">商家Id</param>
        /// <param name="pageNo">当前第几页</param>
        /// <param name="pageSize">每页显示多少条数据</param>
        /// <returns></returns>
        QueryPageModel<BrandInfo> GetShopBrands(long shopId, int pageNo, int pageSize);

        /// <summary>
        /// 获取查询的商家品牌名称列表用于下拉显示
        /// </summary>
        /// <param name="shopId">商家Id</param>
        /// <returns></returns>
        IQueryable<BrandInfo> GetShopBrands(long shopId);


        /// <summary>
        /// 分页获取商家品牌申请列表
        /// </summary>
        /// <param name="shopId">商家Id</param>
        /// <param name="auditStatus">审核状态</param>
        /// <param name="pageNo">当前第几页</param>
        /// <param name="pageSize">每页显示多少条数据</param>
        /// <param name="keyWords">店铺名称</param>
        /// <returns></returns>
        ObsoletePageModel<ShopBrandApplysInfo> GetShopBrandApplys(long? shopId, int? auditStatus, int pageNo, int pageSize, string keyWords);

        /// <summary>
        /// 获取查询的商家申请品牌名称列表用于下拉显示
        /// </summary>
        /// <param name="shopId">商家Id</param>
        /// <returns></returns>
        IQueryable<ShopBrandApplysInfo> GetShopBrandApplys(long shopId);

        /// <summary>
        /// 获取一个品牌申请信息
        /// </summary>
        /// <param name="id">品牌ID</param>
        /// <returns></returns>
        ShopBrandApplysInfo GetBrandApply(long id);

        /// <summary>
        /// 删除商家品牌申请
        /// </summary>
        /// <param name="id"></param>
        void DeleteApply(int id);

        /// <summary>
        /// 是否已申请(审核中和审核通过)
        /// </summary>
        /// <param name="shopId">店铺Id</param>
        /// <param name="brandName">要申请的品牌名称</param>
        /// <returns></returns>
        bool IsExistApply(long shopId, string brandName);

        /// <summary>
        /// 是否已存在指定品牌
        /// </summary>
        /// <param name="brandName">要添加的品牌名称</param>
        /// <returns></returns>
        bool IsExistBrand(string brandName);

        /// <summary>
        /// 判断品牌是否使用中
        /// </summary>
        /// <param name="id">品牌Id</param>
        /// <returns></returns>
        bool BrandInUse(long id);
    }
}
