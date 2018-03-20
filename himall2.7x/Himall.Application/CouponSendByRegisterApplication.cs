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
    public class CouponSendByRegisterApplication
    {
        private static ICouponSendByRegisterService _ICouponSendByRegisterService = ObjectContainer.Current.Resolve<ICouponSendByRegisterService>();

        /// <summary>
        /// 添加设置
        /// </summary>
        /// <param name="mCouponSendByRegister">数据实体</param>
        public static void AddCouponSendByRegister(CouponSendByRegisterInfo mCouponSendByRegister)
        {
            _ICouponSendByRegisterService.AddCouponSendByRegister(mCouponSendByRegister);
        }

        /// <summary>
        /// 修改设置
        /// </summary>
        /// <param name="mCouponSendByRegister">数据实体</param>
        public static void UpdateCouponSendByRegister(CouponSendByRegisterInfo mCouponSendByRegister)
        {
            _ICouponSendByRegisterService.UpdateCouponSendByRegister(mCouponSendByRegister);
        }

        /// <summary>
        /// 获取第一条设置数据
        /// </summary>
        /// <returns></returns>
        public static CouponSendByRegisterInfo GetCouponSendByRegister()
        {
            return _ICouponSendByRegisterService.GetCouponSendByRegister();
        }
    }
}
