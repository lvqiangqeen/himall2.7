using Himall.CommonModel;
using Himall.IServices;
using Himall.Web.Areas.Admin.Models;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Himall.Web.Areas.Mobile.Controllers
{
    public class TemplateVisualizationProcessController : BaseMobileTemplatesController
    {
        private IProductService _iProductService;
        private IShopService _iShopService;
        public TemplateVisualizationProcessController(IProductService iProductService, IShopService iShopService)
        {
            _iProductService = iProductService;
            _iShopService = iShopService;
        }
        // GET: Admin/TemplateVisualizationProcess
        public ActionResult GoodsListAction()
        {
            var data = this.ControllerContext.RouteData.Values;
            var layout = data["Layout"];
            string ids = "";
            bool showIco = false, showPrice = false, showName = false;
            bool showWarp = true;
            string warpId = "";
            if (layout != null)
            {
                ids = data["IDs"].ToString();
                showIco = bool.Parse(data["ShowIco"].ToString());
                showPrice = bool.Parse(data["showPrice"].ToString());
                showName = data["showName"].ToString() == "1";
                warpId = data["ID"].ToString();
                showWarp = true;
            }
            else
            {
                layout = Request["Layout"];
                ids = Request["IDs"];
                showIco = bool.Parse(Request["ShowIco"]);
                showPrice = bool.Parse(Request["showPrice"]);
                showName = Request["showName"] == "1";
                if (!string.IsNullOrWhiteSpace(Request["showWarp"]))
                {
                    showWarp = bool.Parse(Request["showWarp"]);
                }
            }
            var name = "~/Views/Shared/GoodGroup" + layout + ".cshtml";
            ProductAjaxModel model = new ProductAjaxModel() { list = new List<ProductContent>() };
            model.showIco = showIco;
            model.showPrice = showPrice;
            model.showName = showName;
            model.showWarp = showWarp;
            model.warpId = warpId;

            List<long> idArray = new List<long>();
            idArray = ids.Split(',').Where(d => !string.IsNullOrWhiteSpace(d)).Select(d => long.Parse(d)).ToList();
            if (idArray != null && idArray.Count > 0)
            {
                var products = _iProductService.GetProductByIds(idArray).ToList();
                model.list = new List<ProductContent>();
                var selfshop = _iShopService.GetSelfShop();
                decimal discount = 1m;
                if (CurrentUser != null)
                {
                    discount = CurrentUser.MemberDiscount;
                }
                foreach (var d in products)
                {
                    decimal minprice = d.MinSalePrice;
                    if(d.ShopId == selfshop.Id)
                    {
                        minprice = d.MinSalePrice * discount;
                    }
                    var _tmp = new ProductContent
                    {
                        product_id = d.Id,
                        link = "/m-wap/Product/Detail/" + d.Id.ToString(),
                        price = minprice.ToString("f2"),
                        original_price = d.MarketPrice.ToString("f2"),
                        pic = Core.HimallIO.GetProductSizeImage(d.RelativePath, 1, (int)ImageSize.Size_350),
                        title = d.ProductName,
                        is_limitbuy = _iProductService.IsLimitBuy(d.Id),
                        SaleCounts=d.SaleCounts
                    };
                    model.list.Add(_tmp);
                }
            }
            return PartialView(name, model);
        }
    }
}