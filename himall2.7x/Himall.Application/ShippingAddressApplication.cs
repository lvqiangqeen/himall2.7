using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Core;
using Himall.IServices;
using Himall.Model;

namespace Himall.Application
{
    public  class ShippingAddressApplication
    {
        private static IShippingAddressService _iShippingAddressService = ObjectContainer.Current.Resolve<IShippingAddressService>();

        /// <summary>
        /// 添加收货地址
        /// </summary>
       public static  void AddShippingAddress(ShippingAddressInfo shipinfo)
        {
            _iShippingAddressService.AddShippingAddress(shipinfo);
        }

        /// <summary>
        /// 更新收货地址信息
        /// </summary>
        /// <param name="shipinfo"></param>
        public static void UpdateShippingAddress(ShippingAddressInfo shipinfo)
        {
            _iShippingAddressService.UpdateShippingAddress(shipinfo);
        }

        /// <summary>
        /// 获取用户的收货地址列表
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static IQueryable<ShippingAddressInfo> GetUserShippingAddressByUserId(long userId)
        {
           return  _iShippingAddressService.GetUserShippingAddressByUserId(userId);
        }

        /// <summary>
        /// 获取会员默认收货地址
        /// </summary>
        /// <param name="userId">会员编号</param>
        /// <returns></returns>
        public static ShippingAddressInfo GetDefaultUserShippingAddressByUserId(long userId)
        {
            var addr = _iShippingAddressService.GetDefaultUserShippingAddressByUserId(userId);
            if (addr != null)
            {
                var region = RegionApplication.GetRegion(addr.RegionId);
                if (region==null)
                {//收货地址被删除后，设置默认地址
                    addr.RegionId = RegionApplication.GetDefaultRegionId();
                    addr.RegionFullName = RegionApplication.GetFullName(addr.RegionId);
                }
            }
            return addr;
        }

        /// <summary>
        /// 获取用户的收货地址列表
        /// </summary>
        /// <param name="shippingAddressId">收货地址Id</param>
        /// <returns></returns>
        public static ShippingAddressInfo GetUserShippingAddress(long shippingAddressId)
        {
            return _iShippingAddressService.GetUserShippingAddress(shippingAddressId);
        }

        /// <summary>
        /// 设置用户的默认收获地址
        /// </summary>
        /// <param name="id"></param>
        public static void SetDefaultShippingAddress(long id, long userId)
        {
            _iShippingAddressService.SetDefaultShippingAddress(id, userId);
        }

        /// <summary>
        /// 设置用户的收货地址为轻松购
        /// </summary>
        /// <param name="id"></param>
        public static void SetQuickShippingAddress(long id, long userId)
        {
            _iShippingAddressService.SetQuickShippingAddress(id, userId);
        }

        /// <summary>
        /// 删除用户的收货地址
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        public static void DeleteShippingAddress(long id, long userId)
        {
            _iShippingAddressService.DeleteShippingAddress(id, userId);
        }
    }
}
