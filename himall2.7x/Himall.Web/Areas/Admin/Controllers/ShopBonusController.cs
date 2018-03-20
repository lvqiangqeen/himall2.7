using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class ShopBonusController : BaseAdminController
    {
        private IMarketService _iMarketService;

        public ShopBonusController(IMarketService iMarketService)
        {
            _iMarketService = iMarketService;
        }
        //
        // GET: /Admin/ShopBonus/
        public ActionResult Management()
        {
            return View();
        }

        public ActionResult ServiceSetting()
        {
            MarketSettingInfo model = _iMarketService.GetServiceSetting( MarketType.RandomlyBonus );
            return View( model );
        }

        [HttpPost]
        [UnAuthorize]
        public ActionResult SaveServiceSetting( decimal Price )
        {
            Result result = new Result();
            var model = new MarketSettingInfo { Price = Price , TypeId = MarketType.RandomlyBonus };
            _iMarketService.AddOrUpdateServiceSetting( model );
            result.success = true;
            result.msg = "保存成功！";
            return Json( result );
        }

        [UnAuthorize]
        public ActionResult List( string shopName , int page , int rows )
        {
            var queryModel = new MarketBoughtQuery()
            {
                PageSize = rows ,
                PageNo = page ,
                ShopName = shopName ,
                MarketType = MarketType.RandomlyBonus
            };
            ObsoletePageModel<MarketServiceRecordInfo> marketEntities = _iMarketService.GetBoughtShopList( queryModel );

            var market = marketEntities.Models.OrderByDescending( m => m.MarketServiceId ).ThenByDescending( m => m.EndTime ).ToArray().Select( item => new
            {
                Id = item.Id ,
                StartDate = item.StartTime.ToString( "yyyy-MM-dd" ) ,
                EndDate = item.EndTime.ToString( "yyyy-MM-dd" ) ,
                ShopName = item.ActiveMarketServiceInfo.ShopName
            } );

            return Json( new { rows = market , total = marketEntities.Total } );
        }
	}
}