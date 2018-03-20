using Himall.Core;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Areas.Admin.Models.Product;
using Himall.Web.Framework;
using Himall.Web.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Himall.Application;
using Himall.CommonModel;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class DistributionShopController : BaseAdminController
    {
        private IMobileHomeTopicService _iMobileHomeTopicService;
        private ISlideAdsService _iSlideAdsService;
        private IDistributionService _iDistributionService;

     
        public DistributionShopController(
            IMobileHomeTopicService iMobileHomeTopicService,
            ISlideAdsService iSlideAdsService,
            IDistributionService iDistributionService)
        {
            _iMobileHomeTopicService = iMobileHomeTopicService;
            _iSlideAdsService = iSlideAdsService;
            _iDistributionService = iDistributionService;
        }

        /// <summary>
        /// 页面初始
        /// </summary>
        /// <returns></returns>
        public ActionResult HomePageSetting()
        {
            return View();
        }

        #region 轮播图片设置
        /// <summary>
        /// 添加轮播图
        /// </summary>
        /// <param name="id"></param>
        /// <param name="description"></param>
        /// <param name="imageUrl"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public JsonResult AddSlideImage(string id, string description, string imageUrl, string url)
        {
            if (imageUrl.Contains("/temp/"))
            {
                string source = imageUrl.Substring(imageUrl.LastIndexOf("/temp/"));
                string dest = @"/Storage/Plat/APP/SlidAd/";
                imageUrl = dest + Path.GetFileName(imageUrl);
                Core.HimallIO.CopyFile(source, imageUrl, true);
            }
            else if (imageUrl.Contains("/Storage/"))
            {
                imageUrl = imageUrl.Substring(imageUrl.LastIndexOf("/Storage/"));
            }

            Result result = new Result();
            var slideAdInfo = new SlideAdInfo();
            slideAdInfo.Id = Convert.ToInt64(id);
            slideAdInfo.ImageUrl = imageUrl;
            slideAdInfo.TypeId = SlideAdInfo.SlideAdType.DistributionHome;
            slideAdInfo.Url = url.ToLower().Replace("/m-wap", "/m-ios").Replace("/m-weixin", "/m-ios");
            slideAdInfo.Description = description;
            slideAdInfo.ShopId = 0;
            if (slideAdInfo.Id > 0)
                _iSlideAdsService.UpdateSlidAd(slideAdInfo);
            else
                _iSlideAdsService.AddSlidAd(slideAdInfo);
            result.success = true;
            return Json(result);
        }

        /// <summary>
        /// 删除图片
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DeleteSlideImage(string id)
        {
            Result result = new Result();
            _iSlideAdsService.DeleteSlidAd(0, Convert.ToInt64(id));
            result.success = true;
            return Json(result);
        }

        /// <summary>
        /// 轮播图片排序修改
        /// </summary>
        /// <param name="oriRowNumber"></param>
        /// <param name="newRowNumber"></param>
        /// <returns></returns>
        [HttpPost]
        [UnAuthorize]
        public JsonResult SlideImageChangeSequence(int oriRowNumber, int newRowNumber)
        {
            _iSlideAdsService.UpdateWeixinSlideSequence(0, oriRowNumber, newRowNumber, SlideAdInfo.SlideAdType.DistributionHome);
            return Json(new { success = true });
        }

        /// <summary>
        /// 获取所有分销轮播图片
        /// </summary>
        /// <returns></returns>
        public JsonResult GetSlideImages()
        {
            //轮播图
            var slideImageSettings = _iSlideAdsService.GetSlidAds(0, SlideAdInfo.SlideAdType.DistributionHome).ToArray();
            var slideImageService = _iSlideAdsService;
            var slideModel = slideImageSettings.Select(item =>
            {
                var slideImage = slideImageService.GetSlidAd(0, item.Id);
                return new
                {
                    id = item.Id,
                    imgUrl = Core.HimallIO.GetImagePath(item.ImageUrl),
                    displaySequence = item.DisplaySequence,
                    url = item.Url,
                    description = item.Description
                };
            });
            return Json(new { rows = slideModel, total = 100 }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 根据ID获取图轮播图片信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult GetImageAd(long id)
        {
            var models = _iSlideAdsService.GetImageAd(0, id);
            return Json(new { success = true, imageUrl = models.ImageUrl, url = models.Url }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 更新图片信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pic"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public JsonResult UpdateImageAd(long id, string pic, string url)
        {
            var image = _iSlideAdsService.GetImageAd(0, id);
            if (!string.IsNullOrWhiteSpace(pic) && (!image.ImageUrl.Equals(pic)))
            {
                //转移图片
                if (pic.Contains("/temp/"))
                {
                    string source = pic.Substring(pic.LastIndexOf("/temp"));
                    string dest = @"/Storage/Plat/ImageAd/";
                    pic = Path.Combine(dest, Path.GetFileName(source));
                    Core.HimallIO.CopyFile(source, pic, true);
                }
                else if (pic.Contains("/Storage/"))
                {
                    pic = pic.Substring(pic.LastIndexOf("/Storage"));
                }
            }
            var imageAd = new ImageAdInfo { ShopId = 0, Url = url, ImageUrl = pic, Id = id };
            _iSlideAdsService.UpdateImageAd(imageAd);
            return Json(new { success = true });
        }
        #endregion

        #region 推荐分销商品设置

        /// <summary>
        /// 获取所有分销商品信息
        /// </summary>
        /// <param name="categoryId">3级分类ID</param>
        /// <param name="brandName"></param>
        /// <param name="productCode"></param>
        /// <param name="auditStatus"></param>
        /// <param name="ids">只查询指定商品ID</param>
        /// <param name="page">分页页码</param>
        /// <param name="rows">每页行数</param>
        /// <param name="keyWords">搜索关键词</param>
        /// <param name="shopName"></param>
        /// <param name="saleStatus"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ProductLists(long? categoryId, string brandName, string productCode, int? auditStatus, string ids, int page, int rows, string keyWords, string shopName, int? saleStatus)
        {
            ProductBrokerageQuery query = new ProductBrokerageQuery()
            {
                CategoryId = categoryId,
                PageNo = page,
                PageSize = rows,
                skey = keyWords,
                ProductIds = ids,
                ProductBrokerageState = ProductBrokerageInfo.ProductBrokerageStatus.Normal,
                OnlyShowNormal = true
            };
            Himall.Model.ObsoletePageModel<Himall.Model.ProductBrokerageInfo> datasql = _iDistributionService.GetDistributionProducts(query);

            IEnumerable<ProductModel> products = datasql.Models.ToArray().Select(item => new ProductModel()
            {
                name = item.Product.ProductName,
                brandName = item.Product.BrandName,
                categoryName = item.Product.CategoryNames,
                id = item.Product.Id,
                imgUrl = item.Product.GetImage(ImageSize.Size_50),
                price = item.Product.MinSalePrice,
                state = item.Product.ShowProductState,
                auditStatus = (int)item.Product.AuditStatus,
                url = "",
                auditReason = item.Product.ProductDescriptionInfo != null ? item.Product.ProductDescriptionInfo.AuditReason : "",
                shopName = "",
                saleStatus = (int)item.Product.SaleStatus,
                productCode = item.Product.ProductCode
            });
            DataGridModel<ProductModel> dataGrid = new DataGridModel<ProductModel>() { rows = products, total = datasql.Total };
            return Json(dataGrid);
        }

        /// <summary>
        /// 获取所有已选ID
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetAllHomeProductIds()
        {
            var ProductIds = DistributionApplication.GetAllHomeProductIds();
            return Json(ProductIds);
        }

        /// <summary>
        /// 设置首页商品
        /// </summary>
        /// <param name="productIds">商品ID，用','号隔开</param>
        /// <param name="platformType">平台类型</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult AddHomeProducts(string productIds, PlatformType platformType)
        {
            DistributionApplication.SetDistributionProducts(productIds);
            return Json(new { success = true });
        }


        /// <summary>
        /// 查询已绑定的分销首页商品信息
        /// </summary>
        /// <param name="page">分页页码</param>
        /// <param name="rows">每页行数</param>
        /// <param name="keyWords">搜索关键字</param>
        /// <param name="categoryId">3级分类</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetDistributionProducts(int page, int rows, string keyWords, long? categoryId = null)
        {
            var allProducts = DistributionApplication.GetDistributionProducts(page, rows, keyWords, categoryId);

            return Json(new { rows = allProducts.Models, total = allProducts.Total });
        }

        /// <summary>
        /// 删除首页图片设置
        /// </summary>
        /// <param name="Id">设置ID</param>
        /// <returns></returns>
        public JsonResult Delete(long Id)
        {
            DistributionApplication.DelDistributionProducts(Id);
            return Json(new { success = true });
        }

        /// <summary>
        /// 修改首页商品排序
        /// </summary>
        /// <param name="Id">商品ID</param>
        /// <param name="sequence">排序ID</param>
        /// <returns></returns>
        public JsonResult UpdateSequence(long Id, short sequence)
        {
            DistributionApplication.UpdateSequence(Id, sequence);
            return Json(new { success = true });
        }
        #endregion
    }
}