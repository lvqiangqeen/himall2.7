using Himall.Model;
using System.Linq;
using System.Collections.Generic;

namespace Himall.IServices
{
    public interface ICustomerService : IService
    {
        /// <summary>
        /// 添加客服
        /// </summary>
        /// <param name="customerService">客服信息</param>
        void AddCustomerService(CustomerServiceInfo customerService);

		/// <summary>
		/// 添加平台客服
		/// </summary>
		/// <param name="customerService">客服信息</param>
		void AddPlateCustomerService(CustomerServiceInfo customerService);

        /// <summary>
        /// 获取指定店铺的所有PC客服
        /// <param name="shopId">店铺id</param>
        /// </summary>
        IQueryable<CustomerServiceInfo> GetCustomerService(long shopId);

        /// <summary>
        /// 获取指定店铺指定id的客服信息
        /// </summary>
        /// <param name="shopId">店铺id</param>
        /// <param name="id">客服id</param>
        /// <returns></returns>
        CustomerServiceInfo GetCustomerService(long shopId, long id);

        /// <summary>
        /// 获取指定店铺的移动端客服信息
        /// </summary>
        /// <param name="shopId">店铺ID</param>
        /// <returns></returns>
        CustomerServiceInfo GetCustomerServiceForMobile(long shopId);


        IQueryable<CustomerServiceInfo> GetMobileCustomerService(long shopId);

        /// <summary>
        /// 更新客服信息
        /// </summary>
        /// <param name="customerService"></param>
        void UpdateCustomerService(CustomerServiceInfo customerService);

        /// <summary>
        /// 删除客服信息
        /// </summary>
        /// <param name="ids">待删除的客服id</param>
        /// <param name="shopId">店铺id</param>
        void DeleteCustomerService(long shopId,params long[] ids);

        /// <summary>
        /// 删除移动端客服信息
        /// </summary>
        /// <param name="shopId"></param>
        void DeleteCustomerServiceForMobile(long shopId);

        
        /// <summary>
        /// 获取平台的客服信息
        /// </summary>
        /// <param name="isOpen">是否开启</param>
        /// <param name="isMobile">是否适用于移动端</param>
        /// <returns></returns>
        IQueryable<CustomerServiceInfo> GetPlatformCustomerService(bool isOpen = false, bool isMobile = false);

        /// <summary>
        /// 更新平台客服信息
        /// </summary>
		/// <param name="models"></param>
		void UpdatePlatformService(IEnumerable<CustomerServiceInfo> models);

		/// <summary>
		/// 获取门店可用售前客服
		/// </summary>
		/// <param name="shopId"></param>
		/// <returns></returns>
		List<CustomerServiceInfo> GetPreSaleByShopId(long shopId);

		/// <summary>
		/// 获取门店可用售后客服
		/// </summary>
		/// <param name="shopId"></param>
		/// <returns></returns>
		List<CustomerServiceInfo> GetAfterSaleByShopId(long shopId);
    }
}
