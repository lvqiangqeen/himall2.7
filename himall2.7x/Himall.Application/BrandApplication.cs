using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.IServices;
using Himall.Application.Mappers;
using Himall.Model;
using Himall.CommonModel;

namespace Himall.Application
{
	public class BrandApplication
	{
		#region 字段
		private static IBrandService _brandService;
		#endregion

		#region 构造函数
		static BrandApplication()
		{
			_brandService = Core.ObjectContainer.Current.Resolve<IBrandService>();
		}
		#endregion

		#region 方法
		/// <summary>
		/// 添加一个品牌
		/// </summary>
		/// <param name="model"></param>
		public static void AddBrand(DTO.Brand model)
		{
			_brandService.AddBrand(model.Map<Model.BrandInfo>());
		}

		/// <summary>
		/// 申请一个品牌
		/// </summary>
		/// <param name="model"></param>
		public static void ApplyBrand(DTO.ShopBrandApply model)
		{
			_brandService.ApplyBrand(model.Map<Model.ShopBrandApplysInfo>());
		}

		/// <summary>
		/// 编辑一个品牌
		/// </summary>
		/// <param name="model"></param>
		public static void UpdateBrand(DTO.Brand model)
		{
			_brandService.UpdateBrand(model.Map<Model.BrandInfo>());
		}

		/// 商家编辑品牌
		/// </summary>
		/// <param name="model"></param>
		public static void UpdateSellerBrand(DTO.ShopBrandApply model)
		{
			_brandService.UpdateSellerBrand(model.Map<Model.ShopBrandApplysInfo>());
		}

		/// <summary>
		/// 删除一个品牌
		/// </summary>
		/// <param name="id"></param>
		public static void DeleteBrand(long id)
		{
			_brandService.DeleteBrand(id);
		}

		/// <summary>
		/// 分页获取品牌列表
		/// </summary>
		/// <param name="keyWords">查询关键字</param>
		/// <param name="pageNo">当前第几页</param>
		/// <param name="pageSize">每页显示多少条数据</param>
		/// <returns></returns>
		public static QueryPageModel<DTO.Brand> GetBrands(string keyWords, int pageNo, int pageSize)
		{
			var list = _brandService.GetBrands(keyWords, pageNo, pageSize);

			return new QueryPageModel<DTO.Brand>
			{
				Models = list.Models.Map<List<DTO.Brand>>(),
				Total = list.Total
			};
		}

		/// <summary>
		/// 审核品牌
		/// </summary>
		/// <param name="id">品牌ID</param>
		/// <param name="status">审核结果</param>
		public static void AuditBrand(long id, Himall.Model.ShopBrandApplysInfo.BrandAuditStatus status)
		{
			_brandService.AuditBrand(id, status);
		}

		/// <summary>
		/// 获取查询的品牌名称列表用于下拉显示
		/// </summary>
		/// <param name="keyWords">关键字</param>
		/// <returns></returns>
		public static List<DTO.Brand> GetBrands(string keyWords)
		{
			return _brandService.GetBrands(keyWords).Map<List<DTO.Brand>>();
		}
		
		/// <summary>
		/// 根据品牌id获取品牌
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
		public static List<DTO.Brand> GetBrandsByIds(IEnumerable<long> ids)
		{
			return _brandService.GetBrandsByIds(ids).Map<List<DTO.Brand>>();
		}

		/// <summary>
		/// 获取一个品牌信息
		/// </summary>
		/// <param name="id">品牌ID</param>
		/// <returns></returns>
		public static DTO.Brand GetBrand(long id)
		{
			return _brandService.GetBrand(id).Map<DTO.Brand>();
		}

		/// <summary>
		/// 获取指定分类下的所有商品
		/// </summary>
		/// <param name="categoryIds">分类id</param>
		/// <returns></returns>
		public static List<DTO.Brand> GetBrandsByCategoryIds(params long[] categoryIds)
		{
			return _brandService.GetBrandsByCategoryIds(categoryIds).Map<List<DTO.Brand>>();
		}

		/// <summary>
		/// 获取指定分类下的所有商品
		/// </summary>
		/// <param name="shopId">店铺id</param>
		/// <param name="categoryIds">分类id</param>
		/// <returns></returns>
		public static List<DTO.Brand> GetBrandsByCategoryIds(long shopId, params long[] categoryIds)
		{
			return _brandService.GetBrandsByCategoryIds(shopId, categoryIds).Map<List<DTO.Brand>>();
		}

		/// <summary>
		/// 分页获取商家品牌列表
		/// </summary>
		/// <param name="shopId">商家Id</param>
		/// <param name="pageNo">当前第几页</param>
		/// <param name="pageSize">每页显示多少条数据</param>
		/// <returns></returns>
		public static QueryPageModel<DTO.Brand> GetShopBrands(long shopId, int pageNo, int pageSize)
		{
			var list = _brandService.GetShopBrands(shopId, pageNo, pageSize);
			return new QueryPageModel<DTO.Brand>
			{
				Models = list.Models.Map<List<DTO.Brand>>(),
				Total = list.Total
			};
		}

		/// <summary>
		/// 获取查询的商家品牌名称列表用于下拉显示
		/// </summary>
		/// <param name="shopId">商家Id</param>
		/// <returns></returns>
		public static List<DTO.Brand> GetShopBrands(long shopId)
		{
			return _brandService.GetShopBrands(shopId).Map<List<DTO.Brand>>();
		}

		/// <summary>
		/// 分页获取商家品牌申请列表
		/// </summary>
		/// <param name="shopId">商家Id</param>
		/// <param name="auditStatus">审核状态</param>
		/// <param name="pageNo">当前第几页</param>
		/// <param name="pageSize">每页显示多少条数据</param>
		/// <param name="keyWords">店铺名称</param>
		/// <returns></returns>
		public static QueryPageModel<DTO.ShopBrandApply> GetShopBrandApplys(long? shopId, int? auditStatus, int pageNo, int pageSize, string keyWords)
		{
			var list = _brandService.GetShopBrandApplys(shopId, auditStatus, pageNo, pageSize, keyWords);

			return new QueryPageModel<DTO.ShopBrandApply>
			{
				Models = list.Models.Map<List<DTO.ShopBrandApply>>(),
				Total = list.Total
			};
		}

		/// <summary>
		/// 获取查询的商家申请品牌名称列表用于下拉显示
		/// </summary>
		/// <param name="shopId">商家Id</param>
		/// <returns></returns>
		public static List<DTO.ShopBrandApply> GetShopBrandApplys(long shopId)
		{
			return _brandService.GetShopBrandApplys(shopId).Map<List<DTO.ShopBrandApply>>();
		}

		/// <summary>
		/// 获取一个品牌申请信息
		/// </summary>
		/// <param name="id">品牌ID</param>
		/// <returns></returns>
		public static DTO.ShopBrandApply GetBrandApply(long id)
		{
			return _brandService.GetBrandApply(id).Map<DTO.ShopBrandApply>();
		}

		/// <summary>
		/// 删除商家品牌申请
		/// </summary>
		/// <param name="id"></param>
		public static void DeleteApply(int id)
		{
			_brandService.DeleteApply(id);
		}

		/// <summary>
		/// 是否已申请(审核中和审核通过)
		/// </summary>
		/// <param name="shopId">店铺Id</param>
		/// <param name="brandName">要申请的品牌名称</param>
		/// <returns></returns>
		public static bool IsExistApply(long shopId, string brandName)
		{
			return _brandService.IsExistApply(shopId, brandName);
		}

		/// <summary>
		/// 是否已存在指定品牌
		/// </summary>
		/// <param name="brandName">要添加的品牌名称</param>
		/// <returns></returns>
		public static bool IsExistBrand(string brandName)
		{
			return _brandService.IsExistBrand(brandName);
		}

		/// <summary>
		/// 判断品牌是否使用中
		/// </summary>
		/// <param name="id">品牌Id</param>
		/// <returns></returns>
		public static bool BrandInUse(long id)
		{
			return _brandService.BrandInUse(id);
		}
		#endregion
	}
}
