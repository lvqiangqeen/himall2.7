using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.Model;
using System.IO;
using Senparc.Weixin.MP.Helpers;
using Himall.Core;
using Senparc.Weixin.MP.CommonAPIs;
using Himall.Web.App_Code.Common;
using Himall.Core.Plugins.Message;
using Himall.Web.Areas.Mobile.Models;
using Himall.CommonModel;

namespace Himall.Web.Areas.Mobile.Controllers
{
    public class DistributionShopController : BaseMobileTemplatesController
    {
        private SiteSettingsInfo _siteSetting = null;
        private ISiteSettingService _iSiteSettingService;
        private IMemberService _iMemberService;
        private IRegionService _iRegionService;
        private IMessageService _iMessageService;
        private IDistributionService _iDistributionService;
        private IShopService _iShopService;
        private const string SMSPLUGIN = "Himall.Plugin.Message.SMS";
        private long curUserId;
        public DistributionShopController(IDistributionService iDistributionService, IMemberService iMemberService, IMessageService iMessageService
            , IRegionService iRegionService, ISiteSettingService iSiteSettingService, IShopService iShopService)
        {
            _iDistributionService = iDistributionService;
            _iMemberService = iMemberService;
            _iMessageService = iMessageService;
            _iRegionService = iRegionService;
            _iSiteSettingService = iSiteSettingService;
            this._siteSetting = _iSiteSettingService.GetSiteSettings();
            _iShopService = iShopService;
        }
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (CurrentUser != null)
            {
                curUserId = CurrentUser.Id;
            }
        }

        public ActionResult Promoter(long id, string skey)
        {
            var user = _iMemberService.GetMember(id);
            if (user == null)
            {
               // throw new HimallException("错误的用户编号");
                return RedirectToAction("index", "home");
            }
            var promoter = user.Himall_Promoter.FirstOrDefault();
            if (promoter == null)
            {
                return RedirectToAction("index", "home");
               // throw new HimallException("错误的销售员");
            }
            if (promoter.Status != PromoterInfo.PromoterStatus.Audited)
            {
                return RedirectToAction("index", "home");
              //  throw new HimallException("销售员未通过审核");

            }
            DistributionShopShowModel model = new DistributionShopShowModel();
            model.ShopName = promoter.ShopName;
            model.SearchKey = skey;
            model.UserId = id;
            return View(model);
        }
        /// <summary>
        /// 获取商品列表
        /// </summary>
        /// <param name="skey"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpPost]
        [UnAuthorize]
        public JsonResult ProductList(string skey, int page, long id = 0, long categoryId = 0, long shopId = 0, int sort = 0)
        {
            //查询条件
            ProductBrokerageQuery query = new ProductBrokerageQuery();
            query.skey = skey;
            query.PageSize = 6;
            query.PageNo = page;
            query.OnlyShowNormal = true;
            query.ProductBrokerageState = ProductBrokerageInfo.ProductBrokerageStatus.Normal;
            if (id != 0)
            {
                query.AgentUserId = id;
            }
            if (categoryId != 0)
            {
                query.CategoryId = categoryId;
            }
            if (shopId != 0)
            {
                query.ShopId = shopId;
            }

            query.Sort = ProductBrokerageQuery.EnumProductSort.Default;
            switch (sort)
            {
                case 1:
                    query.Sort = ProductBrokerageQuery.EnumProductSort.SalesNumber;
                    break;
                case 3:
                    query.Sort = ProductBrokerageQuery.EnumProductSort.Brokerage;
                    break;
                case 4:
                    query.Sort = ProductBrokerageQuery.EnumProductSort.PriceAsc;
                    break;
                case 5:
                    query.Sort = ProductBrokerageQuery.EnumProductSort.PriceDesc;
                    break;
                case 6:
                    query.Sort = ProductBrokerageQuery.EnumProductSort.AgentNum;
                    break;
            }

            ObsoletePageModel<ProductBrokerageInfo> datasql = _iDistributionService.GetDistributionProducts(query);

            List<ProductBrokerageInfo> datalist = new List<ProductBrokerageInfo>();
            List<DistributionProductListModel> result = new List<DistributionProductListModel>();
            if (datasql.Models != null)
            {
                datalist = datasql.Models.ToList();
                List<long> proids = datalist.Select(d => d.ProductId.Value).ToList();
                List<long> canAgentIds = _iDistributionService.GetCanAgentProductId(proids, curUserId).ToList();
                result = datalist.Select(d => new DistributionProductListModel
                {
                    ShopId = d.ShopId,
                    ProductId = d.ProductId,
                    ProductName = d.Product.ProductName,
                    ShortDescription = d.Product.ShortDescription,
                    Image = d.Product.GetImage(ImageSize.Size_350),
					ShareImageUrl=Himall.Core.HimallIO.GetRomoteImagePath( d.Product.GetImage(ImageSize.Size_350)),
                    CategoryId = d.Product.CategoryId,
                    CategoryName = d.CategoryName,
                    DistributorRate = d.rate,
                    ProductBrokerageState = d.Status,
                    ProductSaleState = d.Product.SaleStatus,
                    SellPrice = d.Product.MinSalePrice,
                    ShowProductBrokerageState = d.Status.ToDescription(),
                    ShowProductSaleState = d.Product.SaleStatus.ToDescription(),
                    SaleNum = d.SaleNum,
                    AgentNum = d.AgentNum,
                    ForwardNum = d.ForwardNum,
                    isHasAgent = (!canAgentIds.Contains(d.ProductId.Value))
                }).ToList();
            }
            return Json(result);
        }

        /// <summary>
        /// 店铺查看
        /// </summary>
        /// <param name="id"></param>
        /// <param name="partnerid">代理用户编号</param>
        /// <returns></returns>
        public ActionResult Shop(long id, long partnerid = 0)
        {
            var shop = _iShopService.GetShop(id);
            if (shop == null)
            {
                throw new HimallException("错误的店铺编号");
            }

            #region 销售员
            if (partnerid > 0)
            {
                long curuserid = 0;
                if (CurrentUser != null)
                {
                    curuserid = CurrentUser.Id;
                }
                SaveDistributionUserLinkId(partnerid, id, curuserid);
            }
            #endregion

            DistributionShopShowModel model = new DistributionShopShowModel();
            model.ShopName = shop.ShopName;
            model.ShopId = id;
            model.PartnerId = partnerid;

            if (shop.Himall_VShop.Any())
            {
                model.ShopLogo = shop.Himall_VShop.FirstOrDefault().BackgroundImage;
            }
            if (CurrentUser != null)
            {
                model.isFavorite = _iShopService.GetFavoriteShopInfos(CurrentUser.Id).Any(d => d.ShopId == id);
            }

            return View(model);
        }

    }
}