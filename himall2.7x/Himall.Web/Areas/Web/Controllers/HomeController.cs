using Himall.Core.Plugins.OAuth;
using Himall.IServices;
using Himall.Model;
using Himall.Web.Areas.Web.Models;
using Himall.Web.Framework;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using System;
using Himall.CommonModel;

namespace Himall.Web.Areas.Web.Controllers
{
	public class HomeController : BaseWebController
	{
		private IMemberService _iMemberService;
		private ISlideAdsService _iSlideAdsService;
		private IFloorService _iFloorService;
		private IArticleCategoryService _iArticleCategoryService;
		private IArticleService _iArticleService;
		private IBrandService _iBrandService;
		private ILimitTimeBuyService _iLimitTimeBuyService;
		private ISiteSettingService _iSiteSettingService;

		public HomeController(
			IMemberService iMemberService,
			ISlideAdsService iSlideAdsService,
			IFloorService iFloorService,
			IArticleCategoryService iArticleCategoryService,
			IArticleService iArticleService,
			IBrandService iBrandService,
			ILimitTimeBuyService iLimitTimeBuyService,
			ISiteSettingService iSiteSettingService
			)
		{
			_iMemberService = iMemberService;
			_iSlideAdsService = iSlideAdsService;
			_iFloorService = iFloorService;
			_iArticleCategoryService = iArticleCategoryService;
			_iArticleService = iArticleService;
			_iBrandService = iBrandService;
			_iLimitTimeBuyService = iLimitTimeBuyService;
			_iSiteSettingService = iSiteSettingService;
		}
		private bool IsInstalled()
		{
			var t = ConfigurationManager.AppSettings["IsInstalled"];
			return null == t || bool.Parse(t);
		}

		//#if !DEBUG
		//               [OutputCache(Duration = ConstValues.PAGE_CACHE_DURATION)]
		//#endif
        [OutputCache(Duration = ConstValues.PAGE_CACHE_DURATION)]
		[HttpGet]
		public ActionResult Index()
		{
			if (!IsInstalled())
			{
				return RedirectToAction("Agreement", "Installer");
			}
			var ser_user = _iMemberService;

			#region 初始化首页数据
			var homePageModel = new HomePageModel();

			if (CurrentSiteSetting.AdvertisementState)
			{
				homePageModel.AdvertisementUrl = CurrentSiteSetting.AdvertisementUrl;
				homePageModel.AdvertisementImagePath = CurrentSiteSetting.AdvertisementImagePath;
			}

			//获取信任登录插件需要在首页head中填充的验证内容
			ViewBag.OAuthValidateContents = GetOAuthValidateContents();
			homePageModel.SiteName = CurrentSiteSetting.SiteName;
			homePageModel.Title = string.IsNullOrWhiteSpace(CurrentSiteSetting.Site_SEOTitle) ? "商城首页" : CurrentSiteSetting.Site_SEOTitle;
			var view = ViewEngines.Engines.FindView(ControllerContext, "Index", null);
			List<HomeFloorModel> floorModels = new List<HomeFloorModel>();
			homePageModel.handImage = _iSlideAdsService.GetHandSlidAds().ToList();
			var silder = _iSlideAdsService.GetSlidAds(0, SlideAdInfo.SlideAdType.PlatformHome).ToList();
			homePageModel.slideImage = silder;
			var imageAds = _iSlideAdsService.GetImageAds(0).ToList();
			//人气单品
			homePageModel.imageAds = imageAds.Where(p => p.TypeId == Himall.CommonModel.ImageAdsType.Single).ToList();
			//banner右侧广告
			homePageModel.imageAdsTop = imageAds.Where(p => p.TypeId == Himall.CommonModel.ImageAdsType.BannerAds).ToList();

			homePageModel.CenterAds = imageAds.Where(p => p.TypeId == Himall.CommonModel.ImageAdsType.Customize).ToList();
			homePageModel.ShopAds = imageAds.Where(p => p.TypeId == Himall.CommonModel.ImageAdsType.BrandsAds).ToList();

			if (homePageModel.imageAds.Count == 0)
			{
				homePageModel.imageAds = imageAds.Take(8).ToList();
			}
			if (homePageModel.imageAdsTop.Count == 0)
			{
				homePageModel.imageAdsTop = imageAds.Take(2).ToList();
			}
			if (homePageModel.CenterAds.Count == 0)
			{
				homePageModel.CenterAds = imageAds.Take(3).ToList();
			}
			if (homePageModel.ShopAds.Count == 0)
			{
				homePageModel.ShopAds = imageAds.Take(2).ToList();
			}
			/*没地方用，先去掉
			var articleService = ServiceHelper.Create<IArticleService>();
			ViewBag.ArticleTabs = new List<IQueryable<ArticleInfo>>()
			{   articleService.GetTopNArticle<ArticleInfo>(8, 4),
				articleService.GetTopNArticle<ArticleInfo>(8, 5),
				articleService.GetTopNArticle<ArticleInfo>(8, 6),
				articleService.GetTopNArticle<ArticleInfo>(8, 7)
			};
			*/

			//楼层数据
			var floors = _iFloorService.GetHomeFloors().ToList();
			foreach (var f in floors)
			{
				var model = new HomeFloorModel();
				var texts = f.FloorTopicInfo.Where(a => a.TopicType == Position.Top).ToList();
				var products = f.FloorTopicInfo.Where(a => a.TopicType != Position.Top).ToList();
				var productModules = f.FloorProductInfo.ToList();
				var brands = f.FloorBrandInfo.Take(10).ToList();
				model.Name = f.FloorName;
				model.SubName = f.SubName;
				model.StyleLevel = f.StyleLevel;
				model.DefaultTabName = f.DefaultTabName;

				//文本设置
				foreach (var s in texts)
				{
					model.TextLinks.Add(new HomeFloorModel.WebFloorTextLink()
					{
						Id = s.Id,
						Name = s.TopicName,
						Url = s.Url
					});
				}

				//广告设置
				foreach (var s in products)
				{
					model.Products.Add(new HomeFloorModel.WebFloorProductLinks
					{
						Id = s.Id,
						ImageUrl = s.TopicImage,
						Url = s.Url,
						Type = s.TopicType
					});
				}

				//推荐品牌
				foreach (var s in brands)
				{
					model.Brands.Add(new WebFloorBrand
					{
						Id = s.BrandInfo.Id,
						Img = s.BrandInfo.Logo,
						Url = "",
						Name = s.BrandInfo.Name
					});
				}

				//推荐商品
				foreach (var s in productModules)
				{
					model.ProductModules.Add(new HomeFloorModel.ProductModule
					{
						Id = s.Id,
						ProductId = s.ProductId,
						MarketPrice = s.ProductInfo.MarketPrice,
						price = s.ProductInfo.MinSalePrice,
						productImg = Himall.Core.HimallIO.GetProductSizeImage(s.ProductInfo.ImagePath, 1, (int)ImageSize.Size_350),
						productName = s.ProductInfo.ProductName,
						Tab = s.Tab
					});
				}

                if (model.StyleLevel == 1 || model.StyleLevel == 4 || model.StyleLevel == 5 || model.StyleLevel == 6 || model.StyleLevel == 7)
				{
					model.Tabs = f.Himall_FloorTabls.OrderBy(p => p.Id).Select(p => new Himall.Web.Areas.Web.Models.HomeFloorModel.Tab()
					 {
						 Name = p.Name,
						 Detail = p.Himall_FloorTablDetails.ToList()
						 .Where(d => d.Himall_Products.AuditStatus == ProductInfo.ProductAuditStatus.Audited
						 && d.Himall_Products.SaleStatus == ProductInfo.ProductSaleStatus.OnSale)
						 .Select(d => new Himall.Web.Areas.Web.Models.HomeFloorModel.ProductDetail()
						 {
							 ProductId = d.Himall_Products.Id,
							 ImagePath = Himall.Core.HimallIO.GetProductSizeImage(d.Himall_Products.ImagePath, 1, (int)ImageSize.Size_350),
							 Price = d.Himall_Products.MinSalePrice,
							 Name = d.Himall_Products.ProductName
						 }).ToList()
					 }).ToList();

					model.Scrolls = model.Products.Where(p => p.Type == Position.ScrollOne || p.Type == Position.ScrollTwo
																				|| p.Type == Position.ScrollThree || p.Type == Position.ScrollFour).ToList();

					model.RightTops = model.Products.Where(p => p.Type == Position.ROne || p.Type == Position.RTwo
																				|| p.Type == Position.RThree || p.Type == Position.RFour).ToList();

					model.RightBottons = model.Products.Where(p => p.Type == Position.RFive || p.Type == Position.RSix
																				|| p.Type == Position.RSeven || p.Type == Position.REight).ToList();
				}
				floorModels.Add(model);
			}
			homePageModel.floorModels = floorModels;

			//全部品牌
			HomeBrands homeBrands = new HomeBrands();
			var listBrands = Application.BrandApplication.GetBrands(null);
			foreach (var item in listBrands)
			{
				homeBrands.listBrands.Add(new WebFloorBrand
				{
					Id = item.Id,
					Img = item.Logo,
					Url = "",
					Name = item.Name
				});
			}
			homePageModel.brands = homeBrands;

			//限时购
			var setting = _iSiteSettingService.GetSiteSettings();
			if (setting.Limittime)
				homePageModel.FlashSaleModel = _iLimitTimeBuyService.GetRecentFlashSale();
			else
			{
				homePageModel.FlashSaleModel = new List<FlashSaleModel>();
			}

			return View(homePageModel);
			#endregion
		}

		/// <summary>n
		/// 用于响应SLB，直接返回
		/// </summary>
		/// <returns></returns>
		[HttpHead]
		public ContentResult Index(string s)
		{
			return Content("");
		}


		IEnumerable<string> GetOAuthValidateContents()
		{
			var oauthPlugins = Core.PluginsManagement.GetPlugins<IOAuthPlugin>(true);
			return oauthPlugins.Select(item => item.Biz.GetValidateContent());
		}


		// GET: Web/Home
		public ActionResult Index2()
		{
            BranchShopDayFeatsQuery query = new BranchShopDayFeatsQuery();
            query.StartDate = DateTime.Now.Date.AddDays(-10);
            query.EndDate = DateTime.Now.Date;
            query.ShopId = 288;
            query.BranchShopId = 21;
             var model=  Himall.Application.OrderAndSaleStatisticsApplication.GetDayAmountSale(query);
			return View();
		}

		[HttpGet]
		public JsonResult GetFoot()
		{
			var articleCategoryService = _iArticleCategoryService;
			var articleService = _iArticleService;
			//服务文章
			var pageFootServiceCategory = articleCategoryService.GetSpecialArticleCategory(SpecialCategory.PageFootService);
			if (pageFootServiceCategory == null)
			{
				return Json(new List<PageFootServiceModel>(), JsonRequestBehavior.AllowGet);
			}
			var pageFootServiceSubCategies = articleCategoryService.GetArticleCategoriesByParentId(pageFootServiceCategory.Id);
			var pageFootService = pageFootServiceSubCategies.ToArray().Select(item =>
				 new PageFootServiceModel()
				 {
					 CateogryName = item.Name,
					 Articles = articleService.GetArticleByArticleCategoryId(item.Id).Where(t => t.IsRelease)
				 }
				);
			var PageFootService = pageFootService;
			return Json(PageFootService, JsonRequestBehavior.AllowGet);
		}

		public ActionResult TestLogin()
		{
			return View();
		}

	}
}