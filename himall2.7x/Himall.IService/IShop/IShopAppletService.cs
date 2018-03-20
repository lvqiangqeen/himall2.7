using Himall.IServices.QueryModel;
using System.Linq;
using System;
using System.Collections.Generic;
using Himall.CommonModel;

namespace Himall.IServices
{
    /// <summary>
    /// 用户小程序店铺模块
    /// </summary>
    public interface IShopAppletService : IService
    {
        /// <summary>
        /// 获取附近商家列表
        /// </summary>
        /// <param name="Lng"></param>
        /// <param name="Lat"></param>
        /// <param name="userId"></param>
        /// <param name="query"></param>
        /// <param name="totalCount"></param>
        /// <param name="rangeSize"></param>
        /// <returns></returns>
        List<AppletShopInfo> GetRoundShops(string Lng, string Lat, long userId, AppletShopQuery query,out int totalCount,int rangeSize);

        AppletShopInfo GetShopInfoDetail(long shopId, long userId);

        AppletShopInfo GetShopInfo(long shopId);

        long GetLastFavoriteShop(long userId);

        decimal GetSkuSalePrice(string skuId,long productId=0);

    }
}
