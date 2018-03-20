using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Himall.Web.Areas.Web.Controllers
{
    public class ShopSearchController : BaseWebController
    {
        private IShopService _iShopService;
        private IBrandService _iBrandService;
        public ShopSearchController(IShopService iShopService, IBrandService iBrandService)
        {
            _iShopService = iShopService;
            _iBrandService = iBrandService;
        }
        public ActionResult Index(string keywords = "", long categoryId = 0, long brandId = 0, int orderBy = 0, int pageNo = 1, int pageSize = 40)
        {
            ShopQuery queryModel = new ShopQuery
            {
                ShopName = keywords,
                CategoryId = categoryId,
                BrandId = brandId,
                PageNo = pageNo,
                PageSize = pageSize,
                OrderBy = orderBy,
                Status = ShopInfo.ShopAuditStatus.Open
            };

            var shopService = _iShopService;
            var model = shopService.GetShops(queryModel);
            var shops = model.Models.ToList();

            var defaultValue = "5.00";

            Dictionary<long, string> brands = new Dictionary<long, string>();
            Dictionary<long, string> categorys = new Dictionary<long, string>();
            foreach (var m in shops)
            {
                //销量
                m.Sales = shopService.GetSales(m.Id);

                var shopStatisticOrderComments = _iShopService.GetShopStatisticOrderComments(m.Id);
                var productAndDescription = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.ProductAndDescription).FirstOrDefault();
                var sellerServiceAttitude = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.SellerServiceAttitude).FirstOrDefault();
                var sellerDeliverySpeed = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.SellerDeliverySpeed).FirstOrDefault();
                //宝贝与描述
                m.ProductAndDescription = productAndDescription != null ? string.Format("{00:F}", productAndDescription.CommentValue) : defaultValue;
                //卖家服务态度
                m.SellerServiceAttitude = sellerServiceAttitude != null ? string.Format("{00:F}", sellerServiceAttitude.CommentValue) : defaultValue;
                //卖家发货速度
                m.SellerDeliverySpeed = sellerDeliverySpeed != null ? string.Format("{00:F}", sellerDeliverySpeed.CommentValue) : defaultValue;

                foreach (var b in m.Himall_ShopBrands)  //获取品牌
                {
                    if (!brands.ContainsKey(b.BrandId))
                    {
                        brands.Add(b.BrandId, b.Himall_Brands.Name);
                    }
                }

                foreach (var p in m.Himall_Products.Where(d => d.SaleStatus == ProductInfo.ProductSaleStatus.OnSale && d.AuditStatus == ProductInfo.ProductAuditStatus.Audited))  //获取类目
                {
                    if (!categorys.ContainsKey(p.CategoryId))
                    {
                        categorys.Add(p.CategoryId, p.Himall_Categories.Name);
                    }
                    if (m.IsSelf == true)
                    {
                        if (!brands.ContainsKey(p.BrandId))
                        {
                            var branddata = _iBrandService.GetBrand(p.BrandId);
                            if (branddata != null)
                            {
                                brands.Add(branddata.Id, branddata.Name);
                            }
                        }
                        m.SellerDeliverySpeed = "5.00";
                        m.SellerServiceAttitude = "5.00";
                        m.ProductAndDescription = "5.00";
                    }
                }
            }

            PagingInfo info = new PagingInfo
            {
                CurrentPage = pageNo,
                ItemsPerPage = pageSize,
                TotalItems = model.Total
            };
            ViewBag.pageInfo = info;
            ViewBag.QueryModel = queryModel;
            ViewBag.Brands = brands;
            ViewBag.Categorys = categorys;

            return View(shops);
        }
    }
}