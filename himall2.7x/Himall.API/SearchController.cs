using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.API.Model;
using Himall.Application;
using Himall.CommonModel;
using Himall.Core;

namespace Himall.API
{
    public class SearchController : BaseApiController
    {
        public object GetSearchProducts(
            string keywords = "", /* 搜索关键字 */
            long cid = 0,  /* 分类ID */
            long b_id = 0, /* 品牌ID */
            string a_id = "",  /* 属性ID, 表现形式：attrId_attrValueId */
            int orderKey = 1, /* 排序项（1：默认，2：销量，3：价格，4：评论数，5：上架时间） */
            int orderType = 1, /* 排序方式（1：升序，2：降序） */
            int pageNo = 1, /*页码*/
            int pageSize = 10,/*每页显示数据量*/
            long vshopId = 0,
            long sid = 0/*商家ID*/
            )
        {
            if (string.IsNullOrEmpty(keywords) && vshopId == 0 && cid <= 0 && b_id <= 0 && a_id == "")
                keywords = Application.SiteSettingApplication.GetSiteSettings().Keyword;
            #region 初始化查询Model
            SearchProductQuery model = new SearchProductQuery();
            model.VShopId = vshopId;
            model.ShopId = sid;
            model.BrandId = b_id;
            if (vshopId == 0 && cid != 0)
            {
                var catelist = ServiceProvider.Instance<ICategoryService>.Create.GetCategories();
                var cate = catelist.FirstOrDefault(r => r.Id == cid);
                if (cate.Depth == 1)
                    model.FirstCateId = cid;
                else if (cate.Depth == 2)
                    model.SecondCateId = cid;
                else if (cate.Depth == 3)
                    model.ThirdCateId = cid;
            }
            else if (vshopId != 0 && cid != 0)
            {
                model.ShopCategoryId = cid;
            }
            model.AttrValIds = a_id.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            model.Keyword = keywords;
            model.OrderKey = orderKey;
            model.OrderType = orderType == 1;
            model.PageNumber = pageNo;
            model.PageSize = pageSize;
            #endregion
            SearchProductResult result = ServiceProvider.Instance<ISearchProductService>.Create.SearchProduct(model);
            int total = result.Total;
            //当查询的结果少于一页时用like进行补偿（与PC端同步）
            if (result.Total < pageSize)
            {
                model.IsLikeSearch = true;
                SearchProductResult result2 = ServiceProvider.Instance<ISearchProductService>.Create.SearchProduct(model);
                var idList1 = result.Data.Select(a => a.ProductId).ToList();
                var nresult = result2.Data.Where(a => !idList1.Contains(a.ProductId)).ToList();
                if (nresult.Count > 0)
                {
                    result.Total += nresult.Count;
                    result.Data.AddRange(nresult);
                }
            }
            total = result.Total;

            #region 价格更新
            //会员折扣
            decimal discount = 1M;
            long SelfShopId = 0;
            if (CurrentUser != null)
            {
                discount = CurrentUser.MemberDiscount;
                var shopInfo = ShopApplication.GetSelfShop();
                SelfShopId = shopInfo.Id;
            }

            var limit = LimitTimeApplication.GetLimitProducts();
            var fight = FightGroupApplication.GetFightGroupPrice();

            foreach (var item in result.Data)
            {
                item.ImagePath = Core.HimallIO.GetRomoteProductSizeImage(Core.HimallIO.GetImagePath(item.ImagePath), 1, (int)Himall.CommonModel.ImageSize.Size_350);
                if (item.ShopId == SelfShopId)
                    item.SalePrice = item.SalePrice * discount;
                var isLimit = limit.Where(r => r.ProductId == item.ProductId).FirstOrDefault();
                var isFight = fight.Where(r => r.ProductId == item.ProductId).FirstOrDefault();
                if (isLimit != null)
                    item.SalePrice = isLimit.MinPrice;
                if (isFight != null)
                    item.SalePrice = isFight.ActivePrice;
            }
            #endregion

            return Json(new
            {
                Success = "true",
                Product = result.Data,
                keywords = model.Keyword,
                Total = total,
                cid = cid,
                b_id = b_id,
                a_id = a_id,
                orderKey = orderKey,
                orderType = orderType
            });
        }

        public object GetSearchFilter(string keyword = "", long cid = 0, long b_id = 0, string a_id = "")
        {
            if (string.IsNullOrEmpty(keyword) && cid <= 0 && b_id <= 0 && a_id == "")
                keyword = Application.SiteSettingApplication.GetSiteSettings().Keyword;

            SearchProductQuery query = new SearchProductQuery()
            {
                Keyword = keyword,
                AttrValIds = a_id.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList(),
                BrandId = b_id,
            };

            if (cid != 0)
            {
                var catelist = ServiceProvider.Instance<ICategoryService>.Create.GetCategories();
                var cate = catelist.FirstOrDefault(r => r.Id == cid);
                if (cate.Depth == 1)
                    query.FirstCateId = cid;
                else if (cate.Depth == 2)
                    query.SecondCateId = cid;
                else if (cate.Depth == 3)
                    query.ThirdCateId = cid;
            }

            var result = ServiceProvider.Instance<ISearchProductService>.Create.SearchProductFilter(query);
            foreach (BrandView brand in result.Brand)
                brand.Logo = Himall.Core.HimallIO.GetImagePath(brand.Logo);

            return Json(new { Success = "true", Attrs = result.Attribute, Brand = result.Brand, Category = result.Category });
        }
    }
}
