using Himall.Core;
using Himall.IServices;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.DTO;

namespace Himall.Application
{
    public class CustomerServiceApplication
    {
		#region 字段
		private static ICustomerService _customerService = ObjectContainer.Current.Resolve<ICustomerService>();
		#endregion

		#region 方法
		/// <summary>
		/// 获取平台的客服信息
		/// </summary>
		/// <param name="isOpen">是否开启</param>
		/// <param name="isMobile">是否适用于移动端</param>
		public static List<CustomerService> GetPlatformCustomerService(bool isOpen=false, bool isMobile=false)
		{
			return _customerService.GetPlatformCustomerService(isOpen, isMobile).ToList().Map<List<CustomerService>>();
		}

		/// <summary>
		/// 获取门店可用售前客服
		/// </summary>
		/// <param name="shopId"></param>
		/// <returns></returns>
		public static List<CustomerService> GetPreSaleByShopId(long shopId)
		{
			return _customerService.GetPreSaleByShopId(shopId).Map<List<CustomerService>>();
		}

		/// <summary>
		/// 获取门店可用售后客服
		/// </summary>
		/// <param name="shopId"></param>
		/// <returns></returns>
		public static List<CustomerService> GetAfterSaleByShopId(long shopId)
		{
			return _customerService.GetAfterSaleByShopId(shopId).Map<List<CustomerService>>();
		}

		/// <summary>
		/// 获取移动端客服
		/// </summary>
		/// <param name="shopId"></param>
		/// <returns></returns>
		public static List<CustomerService> GetMobileCustomerService(long shopId)
		{
			return _customerService.GetMobileCustomerService(shopId).ToList().Map<List<CustomerService>>();
		}

		/// <summary>
		/// 更新平台客服信息
		/// </summary>
		/// <param name="models"></param>
		public static void UpdatePlatformService(IEnumerable<CustomerService> models)
		{
			var css = models.Map<List<CustomerServiceInfo>>();
			_customerService.UpdatePlatformService(css);
		}

		/// <summary>
		/// 添加客服
		/// </summary>
		/// <param name="customerService">客服信息</param>
		public static long AddCustomerService(CustomerService model)
		{
			var cs = model.Map<CustomerServiceInfo>();
			_customerService.AddCustomerService(cs);
			model.Id = cs.Id;
			return cs.Id;
		}

		/// <summary>
		/// 添加客服
		/// </summary>
		/// <param name="model">客服信息</param>
		public static long AddPlateCustomerService(CustomerService model)
		{
			var cs = model.Map<CustomerServiceInfo>();
			_customerService.AddPlateCustomerService(cs);
			model.Id = cs.Id;
			return cs.Id;
		}
		#endregion
    }
}
