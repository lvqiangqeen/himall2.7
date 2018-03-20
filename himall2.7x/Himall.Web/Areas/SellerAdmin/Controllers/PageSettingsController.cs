using Himall.Core.Helper;
using Himall.IServices;
using Himall.Model;
using Himall.Web.Framework;

using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace Himall.Web.Areas.SellerAdmin.Controllers
{
    public class PageSettingsController : BaseSellerController
    {
        private ISlideAdsService _iSlideAdsService;
        private IShopService _iShopService;
        public PageSettingsController(ISlideAdsService iSlideAdsService, IShopService iShopService)
        {
            _iSlideAdsService = iSlideAdsService;
            _iShopService = iShopService;
        }
        // GET: SellerAdmin/PageSettings
        public ActionResult Management()
        {
            var images = _iSlideAdsService.GetImageAds(CurrentSellerManager.ShopId).OrderBy(item => item.Id);
            ViewBag.Logo = _iShopService.GetShop(CurrentSellerManager.ShopId).Logo;
            ViewBag.Logo = ViewBag.Logo == null ? "" : ViewBag.Logo;
            ViewBag.TransverseAD = images.FirstOrDefault(p => p.IsTransverseAD);
            return View(images.Where(p => !p.IsTransverseAD));
        }

        #region 普通广告图片

        [HttpPost]
        [UnAuthorize]
        public ActionResult UpdateImageAd(long id, string pic, string url)
        {
            var image = _iSlideAdsService.GetImageAd(CurrentSellerManager.ShopId, id);
            if(url.IndexOf("javascript:")>-1)
            {
                return Json(new { success =false,msg="错误的跳转地址"});
            }

            if (!string.IsNullOrWhiteSpace(pic))
            {
                string dest = string.Format(@"/Storage/Shop/{0}/ImageAd/", CurrentSellerManager.ShopId);

                if (pic.Contains("/temp/"))
                {
                    var d = pic.Substring(pic.LastIndexOf("/temp/"));

                    var destimg = Path.Combine(dest, Path.GetFileName(pic));
                    Core.HimallIO.CopyFile(d, destimg, true);
                    pic = destimg;
                }
                else if (pic.Contains("/Storage/"))
                {
                    pic = pic.Substring(pic.LastIndexOf("/Storage/"));
                }
                else
                {
                    pic = "";
                }

            }
            var imageAd = new ImageAdInfo { ShopId = CurrentSellerManager.ShopId, Url = url, ImageUrl = pic, Id = id };
            _iSlideAdsService.UpdateImageAd(imageAd);
            pic = Himall.Core.HimallIO.GetImagePath(pic);
            return Json(new { success = true, imageUrl = pic });
        }

        #endregion

        public ActionResult SlideAds()
        {
            var slides = _iSlideAdsService.GetSlidAds(CurrentSellerManager.ShopId, SlideAdInfo.SlideAdType.ShopHome);
            return View(slides);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult SaveSlideAd(long? id, string pic, string url)
        {
            var slide = new SlideAdInfo()
            {
                ImageUrl = pic,
                Url = url,
                ShopId = CurrentSellerManager.ShopId,
                DisplaySequence = 0,
                Id = id.GetValueOrDefault(),
                TypeId = SlideAdInfo.SlideAdType.ShopHome
            };
            if (!string.IsNullOrWhiteSpace(pic) && pic.Contains("/temp/"))
            {
                string dest = string.Format(@"/Storage/Shop/{0}/ImageAd/", CurrentSellerManager.ShopId);
                var destimg = slide.ImageUrl = Path.Combine(dest, Path.GetFileName(pic));
                var source = pic.Substring(pic.LastIndexOf("/temp/"));
                Core.HimallIO.CopyFile(source, destimg, true);
                slide.ImageUrl = destimg;
            }
            else if (pic.Contains("/Storage/"))
            {
                slide.ImageUrl = pic.Substring(pic.LastIndexOf("/Storage/"));
            }
            else
            {
                slide.ImageUrl = "";
            }
            if (id.HasValue)
                _iSlideAdsService.UpdateSlidAd(slide);
            else
                _iSlideAdsService.AddSlidAd(slide);
            return Json(new { success = true, imageUrl = slide.ImageUrl });
        }


        [HttpPost]
        [UnAuthorize]
        public ActionResult AdjustSlideIndex(long id, int direction)
        {
            _iSlideAdsService.AdjustSlidAdIndex(CurrentSellerManager.ShopId, id, direction == 1, SlideAdInfo.SlideAdType.ShopHome);
            return Json(new { success = true });
        }

        [UnAuthorize]
        [HttpPost]
        public JsonResult DeleteSlide(long Id)
        {
            _iSlideAdsService.DeleteSlidAd(CurrentSellerManager.ShopId, Id);
            return Json(new { success = true });
        }


        #region LOGO图片设置

        [HttpPost]
        [UnAuthorize]
        public JsonResult SetLogo(string logo)
        {
            var img = MoveImages(logo);
            _iShopService.UpdateLogo(CurrentSellerManager.ShopId, img);
            return Json(new { success = true, logo = Himall.Core.HimallIO.GetImagePath(img) });
        }


        /// <summary>
        /// 转移LOGO图片
        /// </summary>
        /// <param name="id"></param>
        /// <param name="image"></param>
        /// <returns></returns>
        private string MoveImages(string image)
        {
            if (string.IsNullOrWhiteSpace(image))
            {
                return "";
            }
            string ImageDir = string.Empty;
            var dir = string.Format("/Storage/Shop/{0}/ImageAd/", CurrentSellerManager.ShopId);

            if (image.Replace("\\", "/").Contains("/temp/"))//只有在临时目录中的图片才需要复制
            {
                var source = image.Substring(image.LastIndexOf("/temp/"));
                Core.HimallIO.CopyFile(source, dir + "logo.png", true);
            }  //目标地址
            return dir + "logo.png";
        }
        #endregion

    }
}