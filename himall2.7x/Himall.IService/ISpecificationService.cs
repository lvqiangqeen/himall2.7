using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.IServices
{
	/// <summary>
	/// 规格服务
	/// </summary>
	public interface ISpecificationService : IService
	{
		/// <summary>
		/// 获取分类对应的规格，如果商家有自定义规格，将返回修改后的规格
		/// </summary>
		/// <param name="categoryId"></param>
		/// <param name="shopId"></param>
		/// <returns></returns>
		List<Model.SpecificationValueInfo> GetSpecification(long categoryId, long shopId);
	}
}
