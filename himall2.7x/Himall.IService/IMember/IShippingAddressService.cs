using Himall.Model;
using System.Linq;

namespace Himall.IServices
{
    public interface IShippingAddressService : IService
    {
        /// <summary>
        /// 添加收货地址
        /// </summary>
        void AddShippingAddress(ShippingAddressInfo shipinfo);

        /// <summary>
        /// 更新收货地址信息
        /// </summary>
        /// <param name="shipinfo"></param>
        void UpdateShippingAddress(ShippingAddressInfo shipinfo);

        /// <summary>
        /// 获取用户的收货地址列表
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        IQueryable<ShippingAddressInfo> GetUserShippingAddressByUserId(long userId);

        /// <summary>
        /// 获取会员默认收货地址
        /// </summary>
        /// <param name="userId">会员编号</param>
        /// <returns></returns>
        ShippingAddressInfo GetDefaultUserShippingAddressByUserId(long userId);

        /// <summary>
        /// 获取用户的收货地址列表
        /// </summary>
        /// <param name="shippingAddressId">收货地址Id</param>
        /// <returns></returns>
        ShippingAddressInfo GetUserShippingAddress(long shippingAddressId);

        /// <summary>
        /// 设置用户的默认收获地址
        /// </summary>
        /// <param name="id"></param>
        void SetDefaultShippingAddress(long id, long userId);

        /// <summary>
        /// 设置用户的收货地址为轻松购
        /// </summary>
        /// <param name="id"></param>
        void SetQuickShippingAddress(long id, long userId);

        /// <summary>
        /// 删除用户的收货地址
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        void DeleteShippingAddress(long id, long userId);
    }
}
