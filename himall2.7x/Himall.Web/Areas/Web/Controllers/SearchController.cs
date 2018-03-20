using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Areas.SellerAdmin.Models;
using Himall.Web.Areas.Web.Models;
using Himall.Web.Framework;
using Himall.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Helpers;
using System.Web.Mvc;
using Himall.Application;
using Himall.CommonModel;
using Himall.Core.Helper;

namespace Himall.Web.Areas.Web.Controllers
{

    public class SearchController : System.Web.Mvc.Controller
    {
        private IBrandService _iBrandService;
        private ICategoryService _iCategoryService;
        private IProductService _iProductService;
        private ILimitTimeBuyService _iLimitTimeBuyService;
        private ISearchProductService _iSearchProductService;
        public SearchController(
            IBrandService iBrandService,
            ICategoryService iCategoryService,
            IProductService iProductService,
            ILimitTimeBuyService iLimitTimeBuyService,
            ISearchProductService iSearchProductService)
        {
            _iBrandService = iBrandService;
            _iCategoryService = iCategoryService;
            _iProductService = iProductService;
            _iLimitTimeBuyService = iLimitTimeBuyService;
            _iSearchProductService = iSearchProductService;
        }
        public UserMemberInfo CurrentUser
        {
            get
            {
                var cookieValue = WebHelper.GetCookie(CookieKeysCollection.HIMALL_USER);
                var userId = UserCookieEncryptHelper.Decrypt(cookieValue, CookieKeysCollection.USERROLE_USER);
                if (userId != 0)
                    return Application.MemberApplication.GetUserByCache(userId);

                return null;
            }
        }

        // GET: Web/Search/SearchAd
        /// <summary>
        ///  商品搜索页面
        /// </summary>
        /// <param name="keywords">搜索关键字</param>
        /// <param name="cid">分类ID</param>
        /// <param name="b_id">品牌ID</param>
        /// <param name="a_id">属性ID, 表现形式：attrId_attrValueId</param>
        /// <param name="orderKey">序项（1：默认，2：销量，3：价格，4：评论数，5：上架时间）</param>
        /// <param name="orderType">排序方式（1：升序，2：降序）</param>
        /// <param name="pageNo">页码</param>
        /// <param name="pageSize">每页显示数据量</param>
        /// <returns></returns>
        public ActionResult SearchAd(
            string keywords = "", /* 搜索关键字 */
            long cid = 0,  /* 分类ID */
            long b_id = 0, /* 品牌ID */
            string a_id = "",  /* 属性值ID, 表现形式：valueid,valueid */
            int orderKey = 1, /* 排序项（1：默认，2：销量，3：价格，4：评论数，5：上架时间） */
            int orderType = 1, /* 排序方式（1：升序，2：降序） */
            int pageNo = 1, /*页码*/
            int pageSize = 60 /*每页显示数据量*/
            )
        {
            try
            {
                if (string.IsNullOrEmpty(keywords) && cid <= 0 && b_id <= 0 && a_id == "")
                    keywords = Application.SiteSettingApplication.GetSiteSettings().Keyword;

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
                #endregion
                SearchProductResult result = _iSearchProductService.SearchProduct(model);
                int total = result.Total;

                //当查询的结果少于一页时用like进行补偿
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
                if (result.Total == 0)
                {
                    ViewBag.BrowsedHistory = BrowseHistrory.GetBrowsingProducts(13, CurrentUser == null ? 0 : CurrentUser.Id);
                    var category = _iCategoryService.GetCategory(model.ThirdCateId);
                    string categoryName = category == null ? string.Empty : category.Name;
                    var brand = _iBrandService.GetBrand(b_id) ?? new BrandInfo();
                    string bname = brand == null ? "" : brand.Name;
                    ViewBag.categoryName = categoryName;
                    ViewBag.bName = bname;
                }
                total = result.Total;

                if (Core.HimallIO.GetHimallIO().GetType().FullName.Equals("Himall.Strategy.OSS"))
                    ViewBag.IsOss = true;
                else
                    ViewBag.IsOss = false;

                ViewBag.keywords = model.Keyword;
                ViewBag.cid = cid;
                ViewBag.b_id = b_id;
                ViewBag.a_id = a_id;
                ViewBag.orderKey = orderKey;
                ViewBag.orderType = orderType;

                #region 分页控制
                PagingInfo info = new PagingInfo
                {
                    CurrentPage = model.PageNumber,
                    ItemsPerPage = pageSize,
                    TotalItems = total
                };
                ViewBag.pageInfo = info;
                #endregion


                return View(result.Data);
            }
            catch (Exception e)
            {
                throw e;
            }
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

            var list = LimitTimeApplication.GetLimitProducts();

            return Json(new { success = true, SelfShopId = shopId, Discount = discount, LimitProducts = list }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSearchFilter(string keyword, long cid, long b_id, string a_id)
        {
            string cacheKey = CacheKeyCollection.CACHE_SEARCHFILTER(keyword, cid, b_id, a_id);
            if (Core.Cache.Exists(cacheKey))
                return Core.Cache.Get(cacheKey) as JsonResult;

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