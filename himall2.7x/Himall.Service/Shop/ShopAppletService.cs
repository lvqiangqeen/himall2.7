using Himall.Core;
using Himall.Core.Plugins.Message;
using Himall.Entity;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.Entity.Infrastructure;
using System.Text;
using Himall.CommonModel;

namespace Himall.Service
{
    /// <summary>
    /// 会员小程序店铺模块服务
    /// </summary>
    public class ShopAppletService : ServiceBase, IShopAppletService
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
        public List<AppletShopInfo> GetRoundShops(string Lng,string Lat, long userId, AppletShopQuery query,out int totalCount, int rangeSize = 1000000)
        {
            totalCount = 0;
            int pageIndex = query.PageNo;
            int pageSize = query.PageSize;
            int start = (pageIndex - 1) * pageSize;
            int end = pageSize * pageIndex;
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("SELECT * from (SELECT  IFNULL(f.id,0) IsSubscribe,(select count(1) from Himall_FavoriteShops where ShopId=himall_shops.Id) FavoriteShopCount, himall_shops.id, ShopName, Logo, ContactsPhone, WelcomeTitle, Lng, Lat, CompanyRegionId, CompanyAddress,OpeningTime,ROUND(6378.138 * 2 * ASIN(SQRT( POW(SIN(( {0} * PI()/180 - lat * PI() / 180)/2  ), 2 ) + COS({0} * PI() / 180) * COS(lat * PI() / 180) * POW(  SIN(({1} * PI()/180 - lng * PI()/180)/2 ), 2 ))) * 1000 ) AS DistanceMi FROM  himall_shops  LEFT JOIN Himall_FavoriteShops f on f.ShopId = himall_shops.Id and f.UserId = {2} where Lng != 0  and Lat != 0 and ShopStatus = 7 ORDER BY    DistanceMi ASC) t where t.DistanceMi<{3}", Lat,Lng, userId, rangeSize);

            var shops=  Context.Database.SqlQuery<AppletShopInfo>( sb.ToString()+" limit "+ start+","+end);
            totalCount = Context.Database.SqlQuery<AppletShopInfo>(sb.ToString()).Count();
            return shops.ToList();
        }
        public AppletShopInfo GetShopInfoDetail(long shopId, long userId)
        { 
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("SELECT  IFNULL(f.id,0) IsSubscribe,(select count(1) from Himall_FavoriteShops where ShopId=himall_shops.Id) FavoriteShopCount, himall_shops.id, ShopName, Logo, ContactsPhone, WelcomeTitle, Lng, Lat, CompanyRegionId, CompanyAddress,OpeningTime FROM  himall_shops  LEFT JOIN Himall_FavoriteShops f on f.ShopId = himall_shops.Id and f.UserId = {0} WHERE   ShopStatus = 7  and himall_shops.id ={1}", userId,shopId);
            var shops = Context.Database.SqlQuery<AppletShopInfo>(sb.ToString()); 
            return shops.ToList().FirstOrDefault();
        }

        public AppletShopInfo GetShopInfo(long shopId)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("SELECT  IFNULL(f.id,0) IsSubscribe,(select count(1) from Himall_FavoriteShops where ShopId=himall_shops.Id) FavoriteShopCount, himall_shops.id, ShopName, Logo, ContactsPhone, WelcomeTitle, Lng, Lat, CompanyRegionId, CompanyAddress,OpeningTime FROM  himall_shops  LEFT JOIN Himall_FavoriteShops f on f.ShopId = himall_shops.Id  WHERE   ShopStatus = 7  and himall_shops.id ={0}", shopId);
            var shops = Context.Database.SqlQuery<AppletShopInfo>(sb.ToString());
            return shops.ToList().FirstOrDefault();
        }

        /// <summary>
        /// 获取某个用户最后关注的店铺，没有关注的店铺则返回0
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public long GetLastFavoriteShop(long userId)
        {
           var info= Context.FavoriteShopInfo.Where(t => t.UserId == userId).OrderByDescending(t => t.Date).FirstOrDefault();
            if (null != info)
            {
                return info.ShopId;
            }
            else
            {
                return 0;
            }
        }
        public decimal GetSkuSalePrice(string skuId, long productId = 0)
        {
            var info = Context.SKUInfo.Where(d => skuId == d.Id);
            if (productId > 0)
                info.Where(d => d.ProductId == productId);
            if (null!=info.FirstOrDefault())
            {
                return info.FirstOrDefault().SalePrice;
            }
            else
            {
                return 0;
            }
        }
      

    }
}
