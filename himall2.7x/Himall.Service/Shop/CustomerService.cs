using Himall.Core;
using Himall.Entity;
using Himall.IServices;
using Himall.Model;
using System.Linq;
using System.Collections.Generic;

namespace Himall.Service
{
    public class CustomerService : ServiceBase, ICustomerService
    {
        public void AddCustomerService(CustomerServiceInfo customerService)
        {
            CheckPropertyWhenAdd(customerService);
            customerService.Name = customerService.Name.Trim();//去除首尾空白
            Context.CustomerServiceInfo.Add(customerService);
            Context.SaveChanges();
        }

		/// <summary>
		/// 添加客服
		/// </summary>
		/// <param name="customerService">客服信息</param>
		public void AddPlateCustomerService(CustomerServiceInfo customerService)
		{
			CheckPlatformCustomerServiceWhenAdd(customerService);
			Context.CustomerServiceInfo.Add(customerService);
			Context.SaveChanges();
		}

        public IQueryable<CustomerServiceInfo> GetCustomerService(long shopId)
        {
            return Context.CustomerServiceInfo.Where(item => item.ShopId == shopId && item.TerminalType == CustomerServiceInfo.ServiceTerminalType.PC);
        }

        public void UpdateCustomerService(CustomerServiceInfo customerService)
        {
            //检查
            var ori = CheckPropertyWhenUpdate(customerService);

            //更新信息
            ori.Name = customerService.Name;
            ori.Type = customerService.Type;
            ori.Tool = customerService.Tool;
            ori.AccountCode = customerService.AccountCode;
            ori.TerminalType = customerService.TerminalType;
            ori.ServerStatus = customerService.ServerStatus;

            //保存更改
            Context.SaveChanges();
        }

        public void DeleteCustomerService(long shopId, params long[] ids)
        {
            //删除
            Context.CustomerServiceInfo.Remove(item => item.ShopId == shopId && ids.Contains(item.Id));
            Context.SaveChanges();
        }

        public void DeleteCustomerServiceForMobile(long shopId)
        {
            Context.CustomerServiceInfo.Remove(item => item.ShopId == shopId && item.TerminalType == CustomerServiceInfo.ServiceTerminalType.Mobile);
            Context.SaveChanges();
        }


        /// <summary>
        /// 添加时检查属性
        /// </summary>
        /// <param name="customerService"></param>
        void CheckPropertyWhenAdd(CustomerServiceInfo customerService)
        {
			CheckPlatformCustomerServiceWhenAdd(customerService);

            if (customerService.ShopId == 0)
                throw new InvalidPropertyException("店铺id必须大于0");
        }

        /// <summary>
        /// 添加时检查属性
        /// </summary>
        /// <param name="customerService"></param>
		void CheckPlatformCustomerServiceWhenAdd(CustomerServiceInfo customerService)
        {
            if (string.IsNullOrWhiteSpace(customerService.Name))
                throw new InvalidPropertyException("客服名称不能为空");
            if (string.IsNullOrWhiteSpace(customerService.AccountCode))
                throw new InvalidPropertyException("沟通工具账号不能为空");
        }

        /// <summary>
        /// 更新时检查属性
        /// </summary>
        /// <param name="customerService"></param>
        /// <returns>返回原始客服信息</returns>
        CustomerServiceInfo CheckPropertyWhenUpdate(CustomerServiceInfo customerService)
        {
			if (customerService.ShopId == 0)
				throw new InvalidPropertyException("店铺id必须大于0");

			return CheckPlatformCustomerServiceWhenUpdate(customerService);
        }

		/// <summary>
		/// 更新时检查属性
		/// </summary>
		/// <param name="customerService"></param>
		/// <returns>返回原始客服信息</returns>
		CustomerServiceInfo CheckPlatformCustomerServiceWhenUpdate(CustomerServiceInfo customerService)
		{
			if (string.IsNullOrWhiteSpace(customerService.Name))
				throw new InvalidPropertyException("客服名称不能为空");
			if (customerService.Id == 0)
				throw new InvalidPropertyException("客服id必须大于0");
			if (string.IsNullOrWhiteSpace(customerService.AccountCode))
				throw new InvalidPropertyException("沟通工具账号不能为空");

			var ori = Context.CustomerServiceInfo.FirstOrDefault(item => item.Id == customerService.Id && item.ShopId == customerService.ShopId);//查找指定店铺下指定id的客服
			if (ori == null)//查询不到，说明店铺id与客服id不匹配或至少有一个不存在
				throw new InvalidPropertyException("不存在id为" + customerService.Id + "的客服信息");
			return ori;
		}

        public CustomerServiceInfo GetCustomerService(long shopId, long id)
        {
            return Context.CustomerServiceInfo.FirstOrDefault(item => item.Id == id && item.ShopId == shopId);
        }

        public CustomerServiceInfo GetCustomerServiceForMobile(long shopId)
        {
            return Context.CustomerServiceInfo.FirstOrDefault(r => r.TerminalType == CustomerServiceInfo.ServiceTerminalType.Mobile && r.ShopId == shopId);
        }

        public IQueryable<CustomerServiceInfo> GetMobileCustomerService(long shopId)
        {
            return Context.CustomerServiceInfo.Where(r => r.TerminalType == CustomerServiceInfo.ServiceTerminalType.Mobile && r.ShopId == shopId).AsQueryable();
        }

		/// <summary>
		/// 获取门店可用售前客服
		/// </summary>
		/// <param name="shopId"></param>
		/// <returns></returns>
		public List<CustomerServiceInfo> GetPreSaleByShopId(long shopId)
		{
			return Context.CustomerServiceInfo.Where(p => p.ServerStatus == CustomerServiceInfo.ServiceStatusType.Open && p.Type == CustomerServiceInfo.ServiceType.PreSale&&p.ShopId==shopId).ToList();
		}

		/// <summary>
		/// 获取门店可用售后客服
		/// </summary>
		/// <param name="shopId"></param>
		/// <returns></returns>
		public List<CustomerServiceInfo> GetAfterSaleByShopId(long shopId)
		{
			return Context.CustomerServiceInfo.Where(p => p.ServerStatus == CustomerServiceInfo.ServiceStatusType.Open && p.Type == CustomerServiceInfo.ServiceType.AfterSale && p.ShopId == shopId).ToList();
		}

        public IQueryable<CustomerServiceInfo> GetPlatformCustomerService(bool isOpen = false, bool isMobile = false)
        {
            var result = Context.CustomerServiceInfo.Where(r => r.ShopId == 0);
            if (isOpen)
                result = result.Where(r => r.ServerStatus == CustomerServiceInfo.ServiceStatusType.Open);
            if (isMobile)
                result = result.Where(r => r.TerminalType == CustomerServiceInfo.ServiceTerminalType.All);
            return result.AsQueryable();
        }

		public void UpdatePlatformService(IEnumerable<CustomerServiceInfo> models)
		{
			foreach (var item in models)
			{
				CheckPlatformCustomerServiceWhenUpdate(item);

				base.UpdateData(item);
			}

			this.Context.SaveChanges();
		}
    }
}
