using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Core;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;

namespace Himall.Application
{
    public class ShopBonusApplication
    {
        private static IShopBonusService _iShopBonusService = ObjectContainer.Current.Resolve<IShopBonusService>();
        /// <summary>
        ///  获取红包列表
        /// </summary>
        public static ObsoletePageModel<ShopBonusInfo> Get(long shopid, string name, int state, int pageIndex, int pageSize)
        {
            return _iShopBonusService.Get(shopid, name, state, pageIndex, pageSize);
        }

        public static List<ShopBonusReceiveInfo> GetCanUseDetailByUserId(long userid)
        {
            return _iShopBonusService.GetCanUseDetailByUserId(userid);
        }
        public static List<ShopBonusReceiveInfo> GetDetailByUserId(long userid)
        {
            return _iShopBonusService.GetDetailByUserId(userid);
        }
        public static ShopBonusReceiveInfo GetDetailById(long userid, long id)
        {
            return _iShopBonusService.GetDetailById(userid, id);
        }
        public static List<ShopBonusReceiveInfo> GetDetailToUse(long shopid, long userid, decimal sumprice)
        {
            return _iShopBonusService.GetDetailToUse(shopid, userid, sumprice);
        }
        public static ObsoletePageModel<ShopBonusReceiveInfo> GetDetailByQuery(CouponRecordQuery query)
        {
            return _iShopBonusService.GetDetailByQuery(query);
        }
        public static ShopBonusGrantInfo GetByOrderId(long orderid)
        {
            return _iShopBonusService.GetByOrderId(orderid);
        }
        public static decimal GetUsedPrice(long orderid, long userid)
        {
            return _iShopBonusService.GetUsedPrice(orderid, userid);
        }

        public static ShopBonusGrantInfo GetGrantByUserOrder(long orderid, long userid)
        {
            return _iShopBonusService.GetGrantByUserOrder(orderid, userid);
        }

        /// <summary>
        /// 获取红包
        /// </summary>
        public static ShopBonusInfo Get(long id)
        {
            return _iShopBonusService.Get(id);
        }

        /// <summary>
        /// 根据grantid获取
        /// </summary>
        public static ShopBonusInfo GetByGrantId(long grantid)
        {
            return _iShopBonusService.GetByGrantId(grantid);
        }

        public static long GetGrantIdByOrderId(long orderid)
        {
           return  _iShopBonusService.GetGrantIdByOrderId(orderid);
        }

        public static void SetBonusToUsed(long userid, List<OrderInfo> orders, long rid)
        {
            _iShopBonusService.SetBonusToUsed(userid, orders, rid);
        }

        /// <summary>
        /// 检查是否能添加
        /// </summary>
        public static bool IsAdd(long shopid)
        {
            return _iShopBonusService.IsAdd(shopid);
        }

        /// <summary>
        ///  获取红包详情
        /// </summary> 
        public static ObsoletePageModel<ShopBonusReceiveInfo> GetDetail(long bonusid, int pageIndex, int pageSize)
        {
            return _iShopBonusService.GetDetail(bonusid, pageIndex, pageSize);
        }

        public static List<ShopBonusReceiveInfo> GetDetailByGrantId(long grantid)
        {
           return  _iShopBonusService.GetDetailByGrantId(grantid);
        }

        /// <summary>
        ///  添加红包 
        /// </summary>
        public static void Add(ShopBonusInfo model, long shopid)
        {
            _iShopBonusService.Add(model, shopid);
        }

        /// <summary>
        ///  修改红包
        /// </summary>
        public static void Update(ShopBonusInfo model)
        {
            _iShopBonusService.Update(model);
        }

        /// <summary>
        ///  失效
        /// </summary>
        public static void Invalid(long id)
        {
            _iShopBonusService.Invalid(id);
        }

        /// <summary>
        /// 领取
        /// </summary>
        public static object Receive(long grantid, string openId, string wxhead, string wxname)
        {
           return  _iShopBonusService.Receive(grantid, openId, wxhead, wxname);
        }

        /// <summary>
        /// 新增红包时判断是否超出服务费用结束日期
        /// </summary>
        /// <returns></returns>
        public static bool IsOverDate(DateTime bonusDateEnd, DateTime dateEnd, long shopid)
        {
            return _iShopBonusService.IsOverDate(bonusDateEnd, dateEnd, shopid);
        }

        public static ActiveMarketServiceInfo GetShopBonusService(long shopId)
        {
            return _iShopBonusService.GetShopBonusService(shopId);
        }

        /// <summary>
        /// 订单支付完成时，生成红包详情
        /// </summary>
        public static long GenerateBonusDetail(ShopBonusInfo model, long orderid, string receiveurl)
        {
            return _iShopBonusService.GenerateBonusDetail(model, orderid, receiveurl);
        }

        public static ShopBonusInfo GetByShopId(long shopid)
        {
            return _iShopBonusService.GetByShopId(shopid);
        }
    }
}
