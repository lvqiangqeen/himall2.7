using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.Web.Framework;
using Himall.CommonModel;
using Himall.Application;
using Himall.Web.Models;
using Himall.DTO;
using Himall.Web.Common;
using Himall.Core.Helper;
using Himall.IServices.QueryModel;
using Himall.IServices;

namespace Himall.Web.Areas.Mobile.Controllers
{
    public class ShopBranchController : BaseMobileTemplatesController
    {
        private ILimitTimeBuyService _iLimitTimeBuyService;

        public ShopBranchController(ILimitTimeBuyService iLimitTimeBuyService)
        {
            _iLimitTimeBuyService = iLimitTimeBuyService;
        }
        /// <summary>
        /// 门店首页
        /// </summary>
        /// <param name="id">门店ID</param>
        /// <returns></returns>
        public ActionResult Index(long id = 0)
        {
            bool isOpenStore = SiteSettingApplication.GetSiteSettings() != null && SiteSettingApplication.GetSiteSettings().IsOpenStore;
            if(!isOpenStore)
                throw new Core.HimallException("门店未授权！");
            var shopBranch = ShopBranchApplication.GetShopBranchById(id);
            if (shopBranch == null)
                return RedirectToAction("Error404", "Error", new { area = "Web" });
            shopBranch.AddressDetail = ShopBranchApplication.RenderAddress(shopBranch.AddressPath,shopBranch.AddressDetail,2);
            ViewBag.ShopBranch = shopBranch;
            ViewBag.ShopCategory = ShopCategoryApplication.GetCategoryByParentId(0, shopBranch.ShopId);

            return View();
        }

        /// <summary>
        /// 周边门店列表
        /// </summary>
        /// <returns></returns>
        public ActionResult StoreList()
        {
            bool isOpenStore = SiteSettingApplication.GetSiteSettings() != null && SiteSettingApplication.GetSiteSettings().IsOpenStore;
            if (!isOpenStore)
                throw new Core.HimallException("门店未授权！");
            ViewBag.QQMapKey = "SYJBZ-DSLR3-IWX3Q-3XNTM-ELURH-23FTP";
            return View();
        }

        /// <summary>
        /// 周边门店列表
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageNo"></param>
        /// <param name="fromLatLng"></param>
        /// <param name="shopId"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult List(int pageSize, int pageNo, string fromLatLng, string shopId)
        {
            bool isOpenStore = SiteSettingApplication.GetSiteSettings() != null && SiteSettingApplication.GetSiteSettings().IsOpenStore;
            if (!isOpenStore)
                throw new Core.HimallException("门店未授权！");
            ShopBranchQuery query = new ShopBranchQuery();
            query.PageNo = pageNo;
            query.PageSize = pageSize;
            query.Status = ShopBranchStatus.Normal; 
            query.CityId = -1;
            query.FromLatLng = fromLatLng;
            query.OrderKey = 2;
            query.OrderType = true;
            if (query.FromLatLng.Split(',').Length!=2)
                return Json(new { Success = false, Message = "无法获取您的当前位置，请确认是否开启定位服务！" }, JsonRequestBehavior.AllowGet);

            if (!string.IsNullOrWhiteSpace(shopId))//如果传入了商家ID，则只取商家下门店
            {
                query.ShopId = TypeHelper.ObjectToInt(shopId, 0);
                if (query.ShopId <= 0)
                    return Json(new { Success = false, Message = "无法定位到商家！" }, JsonRequestBehavior.AllowGet);
            }
            else//否则取用户同城门店
            {
                string address = "", province = "", city = "", district = "", street = "";
                ShopbranchHelper.GetAddressByLatLng(query.FromLatLng, ref address, ref province, ref city, ref district, ref street);
                if (string.IsNullOrWhiteSpace(city))
                    return Json(new { Success = false, Message = "无法定位到城市！" }, JsonRequestBehavior.AllowGet);

                Region cityInfo = RegionApplication.GetRegionByName(city, Region.RegionLevel.City);
                if (cityInfo != null)
                {
                    query.CityId = cityInfo.Id;
                }
            }
            var shopBranchs = ShopBranchApplication.GetNearShopBranchs(query);
            return Json(new { Success = true, Models = shopBranchs.Models, Total = shopBranchs.Total }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 门店首页商品列表
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageNo"></param>
        /// <param name="shopCategoryId"></param>
        /// <param name="shopId"></param>
        /// <param name="shopBranchId"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult ProductList(int pageSize, int pageNo, string shopCategoryId, string shopId, string shopBranchId)
        {
            bool isOpenStore = SiteSettingApplication.GetSiteSettings() != null && SiteSettingApplication.GetSiteSettings().IsOpenStore;
            if (!isOpenStore)
                throw new Core.HimallException("门店未授权！");
            ShopBranchProductQuery query = new ShopBranchProductQuery();
            query.PageSize = pageSize;
            query.PageNo = pageNo;
            query.ShopId = TypeHelper.ObjectToInt(shopId, 0);
            query.shopBranchId = TypeHelper.ObjectToInt(shopBranchId, 0);
            query.ShopBranchProductStatus = ShopBranchSkuStatus.Normal;

            if (query.ShopId <= 0)
                return Json(new { Success = false, Message = "无法定位到商家！" }, JsonRequestBehavior.AllowGet);

            if (TypeHelper.ObjectToInt(shopCategoryId, 0) > 0)
            {
                query.ShopCategoryId = TypeHelper.ObjectToInt(shopCategoryId);
            }

            if (query.shopBranchId <= 0)
                return Json(new { Success = false, Message = "无法定位到门店！" }, JsonRequestBehavior.AllowGet);

            var pageModel = ShopBranchApplication.GetShopBranchProducts(query);
            if (pageModel.Models != null && pageModel.Models.Count > 0)
            {
                #region 处理商品 官方自营店会员折扣价，各活动价等。
                var flashSalePriceList = _iLimitTimeBuyService.GetPriceByProducrIds(pageModel.Models.Select(p => p.Id).ToList());
                var fightGroupSalePriceList = FightGroupApplication.GetActiveByProductIds(pageModel.Models.Select(p => p.Id).ToArray());

                if (CurrentUser != null)
                {
                    var shopInfo = ShopApplication.GetShop(query.ShopId.Value);
                    if (shopInfo != null && shopInfo.IsSelf)//当前商家是否是官方自营店
                    {
                        decimal discount = 1M;
                        discount = CurrentUser.MemberDiscount;
                        foreach (var item in pageModel.Models)
                        {
                            item.MinSalePrice = Math.Round(item.MinSalePrice * discount,2);
                        }
                    }
                }

                foreach (var item in pageModel.Models)
                {
                    var flashSale = flashSalePriceList.Any(p => p.ProductId == item.Id);
                    var fightGroupSale = fightGroupSalePriceList.Any(p => p.ProductId == item.Id);

                    if (flashSale && !fightGroupSale)
                    {
                        item.MinSalePrice = TypeHelper.ObjectToDecimal(flashSalePriceList.FirstOrDefault(p => p.ProductId == item.Id).MinPrice.ToString("f2"));
                    }
                    else if (!flashSale && fightGroupSale)
                    {
                        item.MinSalePrice = TypeHelper.ObjectToDecimal(fightGroupSalePriceList.FirstOrDefault(p => p.ProductId == item.Id).MiniGroupPrice.ToString("f2"));
                    }
                }
                #endregion
            }
            var product = pageModel.Models.ToList().Select(item =>
            {
                return new
                {
                    Id = item.Id,
                    ProductName = item.ProductName,
                    MeasureUnit = item.MeasureUnit,
                    MinSalePrice = item.MinSalePrice.ToString("f2"),
                    SaleCounts = item.SaleCounts,//销量统计没有考虑订单支付完成。
                    RelativePath = Core.HimallIO.GetRomoteProductSizeImage(item.RelativePath, 1, (int)Himall.CommonModel.ImageSize.Size_350),
                };
            });
            return Json(new { Success = true, Models = product, Total = pageModel.Total }, JsonRequestBehavior.AllowGet);
        }
    }
}