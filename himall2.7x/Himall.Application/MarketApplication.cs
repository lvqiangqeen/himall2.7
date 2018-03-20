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
    public class MarketApplication
    {
        private static IMarketService _iMarketService = ObjectContainer.Current.Resolve<IMarketService>();
        /// <summary>
        /// 取店铺激活的营销活动
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ActiveMarketServiceInfo GetMarketService(long shopId, MarketType type)
        {
            return _iMarketService.GetMarketService(shopId, type);
        }
        /// <summary>
        /// 获取服务设置
        /// </summary>
        /// <returns></returns>
        public static MarketSettingInfo GetServiceSetting(MarketType type)
        {
            return _iMarketService.GetServiceSetting(type);
        }

        /// <summary>
        /// 获取指定营销类型服务的已购买商家列表
        /// </summary>
        /// <param name="MarketBoughtQuery">营销查询对象</param>
        /// <returns></returns>
        public static ObsoletePageModel<MarketServiceRecordInfo> GetBoughtShopList(MarketBoughtQuery query)
        {
            return _iMarketService.GetBoughtShopList(query);
        }

        /// <summary>
        /// 添加或者更新服务设置
        /// </summary>
        public static void AddOrUpdateServiceSetting(MarketSettingInfo info)
        {
            _iMarketService.AddOrUpdateServiceSetting(info);
        }

        /// <summary>
        /// 商家订购服务
        /// </summary>
        /// <param name="monthCount"></param>
        /// <param name="shopId"></param>
        /// <param name="type"></param>
        public static void OrderMarketService(int monthCount, long shopId, MarketType type)
        {
            _iMarketService.OrderMarketService(monthCount, shopId, type);
        }

        /// <summary>
        /// 根据购买记录ID获取服务购买记录
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static MarketServiceRecordInfo GetShopMarketServiceRecordInfo(long Id)
        {
            return _iMarketService.GetShopMarketServiceRecordInfo(Id);
        }
    }
}
