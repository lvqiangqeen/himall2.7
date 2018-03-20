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
   public class BonusApplication
    {
        private static IBonusService _iBonusService = ObjectContainer.Current.Resolve<IBonusService>();

        /// <summary>
        ///  获取红包列表
        /// </summary>
        public static ObsoletePageModel<BonusInfo> Get(int type, int state, string name, int pageIndex, int pageSize)
        {
            return _iBonusService.Get(type, state, name, pageIndex, pageSize);
        }

        /// <summary>
        /// 获取红包
        /// </summary>
        public static BonusInfo Get(long id)
        {
            return _iBonusService.Get(id);
        }

        /// <summary>
        ///  获取红包详情
        /// </summary>
        public static ObsoletePageModel<BonusReceiveInfo> GetDetail(long bonusId, int pageIndex, int pageSize)
        {
            return _iBonusService.GetDetail(bonusId, pageIndex, pageSize);
        }

        /// <summary>
        ///  添加红包
        /// </summary>
        public static void Add(BonusInfo model, string baseAddress)
        {
            _iBonusService.Add(model, baseAddress);
        }

        /// <summary>
        ///  修改红包
        /// </summary>
        public static void Update(BonusInfo model)
        {
            _iBonusService.Update(model);
        }

        /// <summary>
        ///  红包失效
        /// </summary>
        public static void Invalid(long id)
        {
            _iBonusService.Invalid(id);
        }

        /// <summary>
        /// 领取活动红包
        /// </summary>
        public static object Receive(long id, string openId)
        {
           return  _iBonusService.Receive(id, openId);
        }


        /// <summary>
        /// 关注送红包
        /// </summary>
        public static string Receive(string openId)
        {
            return _iBonusService.Receive(openId);
        }

        /// <summary>
        /// 获取某用户领取的金额
        /// </summary>
        public static decimal GetReceivePriceByOpendId(long id, string openId)
        {
            return _iBonusService.GetReceivePriceByOpendId(id, openId);
        }

        /// <summary>
        /// 能否添加红包
        /// </summary>
        public static bool CanAddBonus()
        {
           return  _iBonusService.CanAddBonus();
        }


        public static void SetShare(long id, string openId)
        {
            _iBonusService.SetShare(id, openId);
        }
        /// <summary>
        /// 刮刮卡新增红包
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isShare"></param>
        /// <param name="userId"></param>
        /// <param name="openId"></param>
        public static void SetShareByUserId(long id, bool isShare, long userId)
        {
            _iBonusService.SetShareByUserId(id, isShare, userId);
        }
        public static void DepositToRegister(long userid)
        {
            _iBonusService.DepositToRegister(userid);
        }
        /// <summary>
        /// 获取红包集合 
        /// </summary>
        public static IEnumerable<BonusInfo> GetBonusByType(BonusInfo.BonusType bonusType)
        {
           return  _iBonusService.GetBonusByType(bonusType);
        }

        /// <summary>
        /// 获取红包剩余数量
        /// </summary>
        public static string GetBonusSurplus(long bonusId)
        {
            return _iBonusService.GetBonusSurplus(bonusId);
        }

        public static decimal GetReceivePriceByUserId(long id, long userId)
        {
           return  _iBonusService.GetReceivePriceByUserId(id, userId);
        }

        /// <summary>
        /// 增加红包增加个数
        /// </summary>
        /// <param name="id">红包Id</param>
        public static void IncrReceiveCount(long id)
        {
            _iBonusService.IncrReceiveCount(id);
        }
    }
}
