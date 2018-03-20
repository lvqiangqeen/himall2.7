using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.IServices;

namespace Himall.Service
{
	public class SpecificationService : ServiceBase, ISpecificationService
	{
		#region ISpecificationService 成员

		public List<Model.SpecificationValueInfo> GetSpecification(long categoryId, long shopId)
		{
			var category = this.Context.CategoryInfo.FirstOrDefault(model => model.Id == categoryId);

			var specifications = this.Context.SpecificationValueInfo.Where(model => model.TypeId == category.TypeId);

			//排除当前类型不支持的规格
			if (!category.ProductTypeInfo.IsSupportColor)
				specifications = specifications.Where(item => item.Specification != Model.SpecificationType.Color);
			if (!category.ProductTypeInfo.IsSupportSize)
				specifications = specifications.Where(item => item.Specification != Model.SpecificationType.Size);
			if (!category.ProductTypeInfo.IsSupportVersion)
				specifications = specifications.Where(item => item.Specification != Model.SpecificationType.Version);

			var result = specifications.ToList();

			//获取商家自定义的规格
			var shopSpecifications = this.Context.SellerSpecificationValueInfo.Where(p => p.ShopId == shopId && p.TypeId == category.TypeId).ToList();
			//覆盖平台默认规格
			foreach (var item in shopSpecifications)
			{
				var temp = result.FirstOrDefault(model => model.Id == item.ValueId);
				if (temp != null)
					temp.Value = item.Value;
			}

			return result;
		}

		#endregion
	}
}
