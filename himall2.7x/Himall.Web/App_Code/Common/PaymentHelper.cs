using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Himall.IServices;
using Himall.Model;
using Himall.Web.Framework;

namespace Himall.Web.App_Code.Common
{
    public class PaymentHelper
    {
        /// <summary>
        /// 支付完生成红包
        /// </summary>
        public static Dictionary<long , ShopBonusInfo> GenerateBonus( IEnumerable<long> orderIds , string urlHost )
        {
            Dictionary<long , ShopBonusInfo> bonusGrantIds = new Dictionary<long , ShopBonusInfo>();
            string url = "http://" + urlHost + "/m-weixin/shopbonus/index/";
            var bonusService = ServiceHelper.Create<IShopBonusService>(); 
            var buyOrders = ServiceHelper.Create<IOrderService>().GetOrders( orderIds );
            foreach( var o in buyOrders )
            {
                var shopBonus = bonusService.GetByShopId( o.ShopId );
                if( shopBonus == null )
                { 
                    continue;
                }
                if( shopBonus.GrantPrice <= o.OrderTotalAmount )
                {
                    long grantid = bonusService.GenerateBonusDetail( shopBonus  , o.Id , url );
                    bonusGrantIds.Add( grantid , shopBonus );
                }
            }

            return bonusGrantIds;
        }

        /// <summary>
        /// 更改限时购销售量
        /// </summary>
        public static void  IncreaseSaleCount( List<long> orderid )
        {
            if( orderid.Count == 1 )
            {
                var service = ServiceHelper.Create<ILimitTimeBuyService>();
                service.IncreaseSaleCount( orderid );
            }
        }
    }
}