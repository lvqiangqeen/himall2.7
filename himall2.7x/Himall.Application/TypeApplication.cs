using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.IServices;
using Himall.Model;
using Himall.Application.Mappers;
using Himall.CommonModel;

namespace Himall.Application
{
	public class TypeApplication
	{
		#region 字段
		private static ITypeService _typeService;
		#endregion

		#region 构造函数
		static TypeApplication()
		{
			_typeService = Core.ObjectContainer.Current.Resolve<ITypeService>();
		}
		#endregion

		#region 方法
		/// <summary>
		/// 获取所有的商品类型列表，包括分页信息
		/// search是搜索条件，如果search为空即显示全部
		/// </summary>
		/// <param name="search">搜索条件</param>
		/// <param name="page">页码</param>
		/// <param name="rows">每页行数</param>
		/// <param name="count">总行数</param>
		/// <returns></returns>
		public static QueryPageModel<DTO.ProductType> GetTypes(string search, int pageNo, int pageSize)
		{
			var result= _typeService.GetTypes(search, pageNo, pageSize);

			return new QueryPageModel<DTO.ProductType>()
			{
				Models = result.Models.Map<List<DTO.ProductType>>(),
				Total = result.Total
			};
		}

		/// <summary>
		/// 获取所有的商品类型列表
		/// </summary>
		/// <returns></returns>
		public static List<ProductTypeInfo> GetTypes()
		{
			return _typeService.GetTypes().ToList();
		}

		/// <summary>
		/// 根据Id获取商品类型实体
		/// </summary>
		/// <param name="id">类型Id</param>
		/// <returns></returns>
		public static DTO.ProductType GetType(long id)
		{
			return _typeService.GetType(id).Map<DTO.ProductType>();
		}

		/// <summary>
		/// 更新商品类型
		/// </summary>
		/// <param name="model"></param>
		public static void UpdateType(DTO.ProductType model)
		{
			_typeService.UpdateType(model.Map<ProductTypeInfo>());
		}

		/// <summary>
		/// 删除商品类型
		/// </summary>
		/// <param name="id"></param>
		public static void DeleteType(long id)
		{
			_typeService.DeleteType(id);
		}

		/// <summary>
		/// 创建商品类型
		/// </summary>
		/// <param name="model"></param>
		public static void AddType(DTO.ProductType model)
		{
			var type = model.Map<ProductTypeInfo>();
			_typeService.AddType(type);
			model.Id = type.Id;
		}
		#endregion
	}
}
