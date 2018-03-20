using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Model;

namespace Himall.IServices
{
    public interface IVirtualOrderService : IService
    {
        /// <summary>
        /// 根据虚拟订单实体创建虚拟订单
        /// </summary>
        /// <param name="model">虚拟订单实体</param>
        /// <returns></returns>
        bool CreateVirtualOrder(VirtualOrderInfo model);
        /// <summary>
        /// 根据虚拟订单号更新虚拟订单支付状态
        /// </summary>
        /// <param name="payNum">虚拟订单号</param>
        /// <returns></returns>
        bool UpdateMoneyFlagByPayNum(string payNum);
        /// <summary>
        /// 根据虚拟订单号更新商家结算
        /// </summary>
        /// <param name="payNum">虚拟订单号</param>
        /// <returns></returns>
        bool UpdateShopAccountByPayNum(string payNum);
        /// <summary>
        /// 根据虚拟订单号更新平台结算
        /// </summary>
        /// <param name="payNum">虚拟订单号</param>
        /// <returns></returns>
        bool UpdatePlatAccountByPayNum(string payNum);
    }
}
