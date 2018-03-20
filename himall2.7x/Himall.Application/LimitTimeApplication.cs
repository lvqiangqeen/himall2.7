using Himall.Core;
using Himall.IServices;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Application
{
    /// <summary>
    /// 限时购
    /// </summary>
    public class LimitTimeApplication
    {
        /// <summary>
        /// 营销活动类型
        /// </summary>
        private static MarketType CurMarketType = MarketType.LimitTimeBuy;
        /// <summary>
        /// 活动服务
        /// </summary>
        private static ILimitTimeBuyService _iLimitTimeBuyService = ObjectContainer.Current.Resolve<ILimitTimeBuyService>();
        /// <summary>
        /// 是否正在限时购
        /// </summary>
        /// <param name="pid">商品ID</param>
        /// <returns></returns>
        public static bool IsLimitTimeMarketItem(long pid)
        {
            return _iLimitTimeBuyService.IsLimitTimeMarketItem(pid);
        }
        /// <summary>
        /// 取限时购价格
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static List<FlashSalePrice> GetPriceByProducrIds(List<long> ids)
        {
            return _iLimitTimeBuyService.GetPriceByProducrIds(ids);
        }

        public static List<FlashSalePrice> GetLimitProducts(List<long> ids=null)
        {
            if (Cache.Exists(CacheKeyCollection.CACHE_LIMITPRODUCTS))
                return Cache.Get<List<FlashSalePrice>>(CacheKeyCollection.CACHE_LIMITPRODUCTS);
            var result = _iLimitTimeBuyService.GetPriceByProducrIds(ids);
            Cache.Insert(CacheKeyCollection.CACHE_LIMITPRODUCTS, result, 120);
            return result;
        }
        /// <summary>
        /// 商家删除限时购
        /// </summary>
        /// <param name="id"></param>
        public void DeleteLimitBuy(long id, long shopId)
        {
            _iLimitTimeBuyService.Delete(id, shopId);
        }
    }
}
