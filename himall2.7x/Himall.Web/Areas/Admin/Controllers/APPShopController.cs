using Himall.Application;
using Himall.Core;
using Himall.Core.Helper;
using Himall.IServices;
using Himall.Model;
using Himall.Web.Areas.Admin.Models;
using Himall.Web.Framework;
using Himall.Web.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class APPShopController : BaseAdminController
    {
        IMobileHomeTopicService _iMobileHomeTopicService;
        ISlideAdsService _iSlideAdsService;
        public APPShopController(IMobileHomeTopicService iMobileHomeTopicService, ISlideAdsService iSlideAdsService)
        {
            _iMobileHomeTopicService = iMobileHomeTopicService;
            _iSlideAdsService = iSlideAdsService;
        }
        // GET: Admin/APPShop
        //APP首页配置共用于安卓和IOS，这里的平台类型写的为IOS，安卓调用首页接口数据时平台类型也选IOS
        public ActionResult HomePageSetting()
        {
            var homeTopicInfos = _iMobileHomeTopicService.GetMobileHomeTopicInfos(PlatformType.IOS).ToArray();

            //专题
            ViewBag.imageAds = _iSlideAdsService.GetImageAds(0).Where(p => p.TypeId == Himall.CommonModel.ImageAdsType.APPSpecial).ToList();
            //门店授权
            ViewBag.IsOpenStore = SiteSettingApplication.GetSiteSettings() != null && SiteSettingApplication.GetSiteSettings().IsOpenStore;

            var models = homeTopicInfos.Select(item =>
            {
                return new TopicModel()
                {
                    FrontCoverImage = item.Himall_Topics.frontCoverImage,
                    Id = item.Id,
                    Name = item.Himall_Topics.Name,
                    Tags = item.Himall_Topics.Tags,
                    Sequence = item.Sequence
                };
            });
            return View(models);
        }


        #region 轮播图

        public JsonResult AddSlideImage(string id, string description, string imageUrl, string url)
        {
            Result result = new Result();
            var slideAdInfo = new SlideAdInfo();
            slideAdInfo.Id = Convert.ToInt64(id);
            slideAdInfo.ImageUrl = imageUrl;
            slideAdInfo.TypeId = SlideAdInfo.SlideAdType.IOSShopHome;
            slideAdInfo.Url = url.ToLower();
            slideAdInfo.Description = description;
            slideAdInfo.ShopId = 0;
            if (slideAdInfo.Id > 0)
                _iSlideAdsService.UpdateSlidAd(slideAdInfo);
            else
                _iSlideAdsService.AddSlidAd(slideAdInfo);
            result.success = true;
            return Json(result);
        }

        [HttpPost]
        public JsonResult DeleteSlideImage(string id)
        {
            Result result = new Result();
            _iSlideAdsService.DeleteSlidAd(0, Convert.ToInt64(id));
            result.success = true;
            return Json(result);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult SlideImageChangeSequence(int oriRowNumber, int newRowNumber)
        {
            _iSlideAdsService.UpdateWeixinSlideSequence(0, oriRowNumber, newRowNumber, SlideAdInfo.SlideAdType.IOSShopHome);
            return Json(new { success = true });
        }
        public JsonResult GetSlideImages()
        {
            //轮播图
            var slideImageSettings = _iSlideAdsService.GetSlidAds(0, SlideAdInfo.SlideAdType.IOSShopHome).ToArray();
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

        public JsonResult GetImageAd(long id)
        {
            var models = _iSlideAdsService.GetImageAd(0, id);
            return Json(new { success = true, imageUrl = Core.HimallIO.GetImagePath(models.ImageUrl), url = models.Url }, JsonRequestBehavior.AllowGet);
        }
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


        public ActionResult APPGuidePages()
        {
            var slideImage = _iSlideAdsService.GetSlidAds(0, SlideAdInfo.SlideAdType.AppGuide).OrderBy(a => a.DisplaySequence).ToList();
            var model = slideImage.Select(a => new Himall.DTO.SlideAdModel() { Id = a.Id, DisplaySequence = a.DisplaySequence, ImageUrl = a.ImageUrl }).ToList();
            return View(model);
        }

        [HttpPost]
        public JsonResult APPGuidePages(List<string> pics)
        {
            List<Himall.DTO.SlideAdModel> list = new List<DTO.SlideAdModel>();
            foreach (var p in pics)
            {
                if (!string.IsNullOrWhiteSpace(p))
                {
                    list.Add(new DTO.SlideAdModel() { ImageUrl = p });
                }
            }
            if (list.Count == 0)
            {
                throw new HimallException("至少上传一张引导页图");
            }
            SlideApplication.AddGuidePages(list);
            return Json(new { success = true });
        }





        #region App图标
        public JsonResult AddAppIcon(string id, string description, string imageUrl, string url)
        {
            Result result = new Result();
            var slideAdInfo = new SlideAdInfo();
            slideAdInfo.Id = Convert.ToInt64(id);
            slideAdInfo.ImageUrl = imageUrl;
            slideAdInfo.TypeId = SlideAdInfo.SlideAdType.APPIcon;
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

        [HttpPost]
        [UnAuthorize]
        public JsonResult AppIconChangeSequence(int oriRowNumber, int newRowNumber)
        {
            _iSlideAdsService.UpdateWeixinSlideSequence(0, oriRowNumber, newRowNumber, SlideAdInfo.SlideAdType.APPIcon);
            return Json(new { success = true });
        }

        public JsonResult GetAPPIcons()
        {
            //轮播图
            var slideImageSettings = _iSlideAdsService.GetSlidAds(0, SlideAdInfo.SlideAdType.APPIcon).ToArray();
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

        [HttpPost]
        public JsonResult DeleteAppIcon(string id)
        {
            Result result = new Result();
            _iSlideAdsService.DeleteSlidAd(0, Convert.ToInt64(id));
            result.success = true;
            return Json(result);
        }

        #endregion

        #region
        public ActionResult Management()
        {
            var AgreementTypes = Himall.Model.AgreementInfo.AgreementTypes.APP;
            return View(GetManagementModel(AgreementTypes));
        }

        [HttpPost]
        public JsonResult GetManagement(int agreementType)
        {
            return Json(GetManagementModel((Himall.Model.AgreementInfo.AgreementTypes)agreementType));
        }

        public AgreementModel GetManagementModel(Himall.Model.AgreementInfo.AgreementTypes type)
        {
            AgreementModel model = new AgreementModel();
            model.AgreementType = (int)type;
            var agreement = SystemAgreementApplication.GetAgreement(type);
            if (agreement != null)
            {
                model.AgreementContent = agreement.AgreementContent;
            }
            return model;
        }

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult UpdateAgreement(int agreementType, string agreementContent)
        {
            AgreementInfo model = SystemAgreementApplication.GetAgreement((Himall.Model.AgreementInfo.AgreementTypes)agreementType);
            bool isSuccess = false;
            if (model == null)
            {//第一次修改，则新增
                model = new AgreementInfo();
                model.AgreementType = agreementType;
                model.AgreementContent = ProcessHtml(agreementContent);
                SystemAgreementApplication.AddAgreement(model);
                isSuccess = true;
            }
            else
            {
                model.AgreementType = agreementType;
                model.AgreementContent = ProcessHtml(agreementContent);
                isSuccess = SystemAgreementApplication.UpdateAgreement(model);
            }
            if (isSuccess)
                return Json(new Result() { success = true, msg = "更新协议成功！" });
            else
                return Json(new Result() { success = false, msg = "更新协议失败！" });
        }
        /// <summary>
        /// 处理图片地址
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private string ProcessHtml(string html)
        {
            if (string.IsNullOrEmpty(html))
                return html;

            var descriptionImagePath = "/Storage/Plat/APP/About";
            html = Core.Helper.HtmlContentHelper.TransferToLocalImage(html, "/", descriptionImagePath, Core.HimallIO.GetImagePath(descriptionImagePath) + "/");
            html = Core.Helper.HtmlContentHelper.RemoveScriptsAndStyles(html);
            return html;
        }
        #endregion
    }
}