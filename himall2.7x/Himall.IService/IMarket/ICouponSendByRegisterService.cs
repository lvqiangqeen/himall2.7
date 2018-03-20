using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.IServices
{
    /// <summary>
    /// 注册有礼优惠券设置
    /// </summary>
    public interface ICouponSendByRegisterService : IService
    {
        /// <summary>
        /// 添加设置
        /// </summary>
        /// <param name="mCouponSendByRegister">数据实体</param>
        void AddCouponSendByRegister(CouponSendByRegisterInfo mCouponSendByRegister);

        /// <summary>
        /// 修改设置
        /// </summary>
        /// <param name="mCouponSendByRegister">数据实体</param>
        void UpdateCouponSendByRegister(CouponSendByRegisterInfo mCouponSendByRegister);

        /// <summary>
        /// 获取第一条设置数据
        /// </summary>
        /// <returns></returns>
        CouponSendByRegisterInfo GetCouponSendByRegister();
    }
}
