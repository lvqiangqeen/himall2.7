using Himall.CommonModel;
using Himall.Core;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Areas.Mobile.Models;
using Himall.Web.Areas.SellerAdmin.Models;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Himall.Application;

namespace Himall.Web.Areas.Mobile.Controllers
{
    public class SearchController : BaseMobileTemplatesController
    {
        private IVShopService _iVShopService;
        private IProductService _iProductService;
        private ILimitTimeBuyService _iLimitTimeBuyService;
        private ISearchProductService _iSearchProductService;
        private ICategoryService _iCategoryService;
        public SearchController(
            IVShopService iVShopService,
            IProductService iProductService,
            ILimitTimeBuyService iLimitTimeBuyService,
            ISearchProductService iSearchProductService,
            ICategoryService iCategoryService
            )
        {
            _iVShopService = iVShopService;
            _iProductService = iProductService;
            _iLimitTimeBuyService = iLimitTimeBuyService;
            _iSearchProductService = iSearchProductService;
            _iCategoryService = iCategoryService;
        }

        public ActionResult Index(
             string keywords = "", /* 搜索关键字 */
             long cid = 0,  /* 分类ID */
             long b_id = 0, /* 品牌ID */
             string a_id = "",  /* 属性ID, 表现形式：attrId_attrValueId */
             int orderKey = 1, /* 排序项（1：默认，2：销量，3：价格，4：评论数，5：上架时间） */
             int orderType = 1, /* 排序方式（1：升序，2：降序） */
             int pageNo = 1, /*页码*/
             int pageSize = 10, /*每页显示数据量*/
             long vshopId = 0//店铺ID
         )
        {
            if (string.IsNullOrEmpty(keywords) && cid <= 0 && b_id <= 0 && a_id == "")
                keywords = Application.SiteSettingApplication.GetSiteSettings().Keyword;

            var result = DoSearch(keywords, cid, b_id, a_id, orderKey, orderType, pageNo, pageSize, vshopId);
            return View(result.Data);
        }

        private SearchProductResult DoSearch(
            string keywords = "", /* 搜索关键字 */
            long cid = 0,  /* 分类ID */
            long b_id = 0, /* 品牌ID */
            string a_id = "",  /* 属性ID, 表现形式：attrId_attrValueId */
            int orderKey = 1, /* 排序项（1：默认，2：销量，3：价格，4：评论数，5：上架时间） */
            int orderType = 1, /* 排序方式（1：升序，2：降序） */
            int pageNo = 1, /*页码*/
            int pageSize = 10, /*每页显示数据量*/
            long vshopId = 0//店铺ID
            )
        {
            #region 初始化查询Model
            SearchProductQuery model = new SearchProductQuery();
            model.ShopId = 0;
            model.BrandId = b_id;
            if (cid != 0)
            {
                var catelist = _iCategoryService.GetCategories();
                var cate = catelist.FirstOrDefault(r => r.Id == cid);
                if (cate.Depth == 1)
                    model.FirstCateId = cid;
                else if (cate.Depth == 2)
                    model.SecondCateId = cid;
                else if (cate.Depth == 3)
                    model.ThirdCateId = cid;
            }
            model.AttrValIds = a_id.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            model.Keyword = keywords;
            model.OrderKey = orderKey;
            model.OrderType = orderType == 1;
            model.PageNumber = pageNo;
            model.PageSize = pageSize;
            model.VShopId = vshopId;
            #endregion
            SearchProductResult result = _iSearchProductService.SearchProduct(model);
            int total = result.Total;

            //当查询的结果少于一页时用like进行补偿（与PC端同步）
            if (result.Total < pageSize)
            {
                model.IsLikeSearch = true;
                SearchProductResult result2 = _iSearchProductService.SearchProduct(model);
                var idList1 = result.Data.Select(a => a.ProductId).ToList();
                var nresult = result2.Data.Where(a => !idList1.Contains(a.ProductId)).ToList();
                if (nresult.Count > 0)
                {
                    result.Total += nresult.Count;
                    result.Data.AddRange(nresult);
                }
            }
            total = result.Total;
            if (Core.HimallIO.GetHimallIO().GetType().FullName.Equals("Himall.Strategy.OSS"))
                ViewBag.IsOss = true;
            else
                ViewBag.IsOss = false;

            ViewBag.keywords = model.Keyword;
            ViewBag.Total = total;
            ViewBag.cid = cid;
            ViewBag.b_id = b_id;
            ViewBag.a_id = a_id;
            ViewBag.orderKey = orderKey;
            ViewBag.orderType = orderType;

            return result;

        }
        [HttpPost]
        public JsonResult Index(
            string keywords = "", /* 搜索关键字 */
            long cid = 0,  /* 分类ID */
            long b_id = 0, /* 品牌ID */
            string a_id = "",  /* 属性ID, 表现形式：attrId_attrValueId */
            int orderKey = 1, /* 排序项（1：默认，2：销量，3：价格，4：评论数，5：上架时间） */
            int orderType = 1, /* 排序方式（1：升序，2：降序） */
            int pageNo = 1, /*页码*/
            int pageSize = 10, /*每页显示数据量*/
            long vshopId = 0,//店铺ID
            bool t = false
            )
        {
            var result = DoSearch(keywords, cid, b_id, a_id, orderKey, orderType, pageNo, pageSize, vshopId);
            if (Core.HimallIO.GetHimallIO().GetType().FullName.Equals("Himall.Strategy.OSS"))
            {
                foreach (var item in result.Data)
                {
                    item.ImagePath = HimallIO.GetProductSizeImage(item.ImagePath, 1, (int)Himall.CommonModel.ImageSize.Size_220);
                }
            }
            return Json(result.Data);
        }

        public JsonResult GetSalePrice()
        {
            //会员折扣
            decimal discount = 1M;
            long shopId = 0;
            if (CurrentUser != null)
            {
                discount = CurrentUser.MemberDiscount;
                var shopInfo = ShopApplication.GetSelfShop();
                shopId = shopInfo.Id;
            }

            var limit = LimitTimeApplication.GetLimitProducts();
            var fight = FightGroupApplication.GetFightGroupPrice();

            return Json(new { success = true, SelfShopId = shopId, Discount = discount, LimitProducts = limit, FightProducts = fight }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSearchFilter(string keyword, long cid = 0, long b_id = 0, string a_id = "")
        {
            if (string.IsNullOrEmpty(keyword) && cid <= 0 && b_id <= 0 && a_id == "")
                keyword = Application.SiteSettingApplication.GetSiteSettings().Keyword;

            string cacheKey = CacheKeyCollection.CACHE_SEARCHFILTER(keyword, cid, b_id, a_id);
            //if (Core.Cache.Exists(cacheKey))
            //    return Core.Cache.Get(cacheKey) as JsonResult;

            SearchProductQuery query = new SearchProductQuery()
            {
                Keyword = keyword,
                AttrValIds = a_id.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList(),
                BrandId = b_id,
            };
            if (cid != 0)
            {
                var catelist = _iCategoryService.GetCategories();
                var cate = catelist.FirstOrDefault(r => r.Id == cid);
                if (cate.Depth == 1)
                    query.FirstCateId = cid;
                else if (cate.Depth == 2)
                    query.SecondCateId = cid;
                else if (cate.Depth == 3)
                    query.ThirdCateId = cid;
            }

            var result = _iSearchProductService.SearchProductFilter(query);
            foreach (BrandView brand in result.Brand)
                brand.Logo = Himall.Core.HimallIO.GetImagePath(brand.Logo);

            JsonResult json = Json(new { success = true, Attr = result.Attribute, Brand = result.Brand, Category = result.Category }, JsonRequestBehavior.AllowGet);
            Core.Cache.Insert(cacheKey, json, 300);
            return json;
        }
    }
}