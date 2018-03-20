using Himall.IServices;
using Himall.Model;
using Himall.Web.Areas.Admin.Models;
using Himall.Web.Framework;

using Himall.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using Himall.IServices.QueryModel;
using Himall.Core;
using System.IO;
using Himall.Application;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class WeiXinController : BaseAdminController
    {
        private ISlideAdsService _iSlideAdsService;
        private ISiteSettingService _iSiteSettingService;
        private IWeixinMenuService _iWeixinMenuService;
        private IMobileHomeTopicService _iMobileHomeTopicService;
        private ITopicService _iTopicService;
        private ITemplateSettingsService _iTemplateSettingsService;
        private IWXMsgTemplateService _iWXMsgTemplateService;
        public WeiXinController(ISlideAdsService iSlideAdsService,
            ISiteSettingService iSiteSettingService,
            IWeixinMenuService iWeixinMenuService,
            IMobileHomeTopicService iMobileHomeTopicService,
            ITopicService iTopicService,
            ITemplateSettingsService iTemplateSettingsService,
            IWXMsgTemplateService iWXMsgTemplateService
            )
        {
            _iSiteSettingService = iSiteSettingService;
            _iWeixinMenuService = iWeixinMenuService;
            _iMobileHomeTopicService = iMobileHomeTopicService;
            _iTopicService = iTopicService;
            _iSlideAdsService = iSlideAdsService;
            _iTemplateSettingsService = iTemplateSettingsService;
            _iWXMsgTemplateService = iWXMsgTemplateService;
        }

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult AutoReplay()
        {
            return View();
        }
        [HttpPost]
        public JsonResult PostAutoReplyList(int page, int rows)
        {
            var data = WeixinAutoReplyApplication.GetPage(page, rows);
            return Json(new { rows = data.Models.ToList(), total = data.Total });
        }
        [HttpPost]
        public JsonResult GetAutoReplayById(int Id)
        {
            var result = new Result();
            var data = WeixinAutoReplyApplication.GetAutoReplyById(Id);
            result.success = true;
            result.Data = data;
            return Json(result);
        }
        public JsonResult ModAutoReplay(AutoReplyInfo item) {
            var result = new Result();
            WeixinAutoReplyApplication.ModAutoReply(item);
            result.success = true;
            result.msg = "规则保存成功！";
            return Json(result);
        }
        public JsonResult DelAutoReplay(AutoReplyInfo item) {
            var result = new Result();
            WeixinAutoReplyApplication.DeleteAutoReply(item);
            result.success = true;
            result.msg = "规则删除成功！";
            return Json(result);
        }
        // GET: Admin/WeiXin

        public ActionResult FocusReplay()
        {
            var item = WeixinAutoReplyApplication.GetAutoReplyByKey(CommonModel.ReplyType.Follow);
            if (item == null) {
                item = new AutoReplyInfo();
            }
            return View(item);
        }
        public ActionResult NewsReplay()
        {
            var item = WeixinAutoReplyApplication.GetAutoReplyByKey(CommonModel.ReplyType.Msg);
            if (item == null)
            {
                item = new AutoReplyInfo();
            }
            return View(item);
        }
        public ActionResult BasicSettings()
        {
            var siteSetting = _iSiteSettingService.GetSiteSettings();
            if (string.IsNullOrEmpty(siteSetting.WeixinToken))
            {
                siteSetting.WeixinToken = CreateKey(8);
                _iSiteSettingService.SetSiteSettings(siteSetting);

            }
            var siteSettingMode = new SiteSettingModel()
            {
                WeixinAppId = string.IsNullOrEmpty(siteSetting.WeixinAppId) ? string.Empty : siteSetting.WeixinAppId.Trim(),
                WeixinAppSecret = string.IsNullOrEmpty(siteSetting.WeixinAppSecret) ? string.Empty : siteSetting.WeixinAppSecret.Trim(),
                WeixinToKen = siteSetting.WeixinToken.Trim()
            };
            ViewBag.Url = String.Format("http://{0}/m-Weixin/WXApi", Request.Url.Host);
            //TODO:演示站处理
            //如果是演示站，支付方式参数做特别处理
            if (DemoAuthorityHelper.IsDemo())
            {
                siteSettingMode.WeixinAppId = "*".PadRight(siteSettingMode.WeixinAppId.Length, '*');
                siteSettingMode.WeixinAppSecret = "*".PadRight(siteSettingMode.WeixinAppSecret.Length, '*');
                ViewBag.isDemo = true;
            }
            return View(siteSettingMode);
        }

        private string CreateKey(int len)
        {
            byte[] bytes = new byte[len];
            new RNGCryptoServiceProvider().GetBytes(bytes);
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(string.Format("{0:X2}", bytes[i]));
            }
            return sb.ToString();
        }

        [UnAuthorize]
        [HttpPost]
        public JsonResult SaveWeiXinSettings(string weixinAppId, string WeixinAppSecret)
        {
            //TODO:演示站处理
            if (DemoAuthorityHelper.IsDemo())
            {
                return Json(new { success = false, msg = "演示站点自动隐藏此参数，且不能保存！" });
            }
            Result result = new Result();
            var siteSetting = _iSiteSettingService.GetSiteSettings();
            siteSetting.WeixinAppId = weixinAppId.Trim();
            siteSetting.WeixinAppSecret = WeixinAppSecret.Trim();
            _iSiteSettingService.SetSiteSettings(siteSetting);
            result.success = true;
            return Json(result);
        }

        public ActionResult MenuManage()
        {
            List<MenuManageModel> listMenuManage = new List<MenuManageModel>();
            var menuManage = _iWeixinMenuService.GetMainMenu(CurrentManager.ShopId);
            foreach (MenuInfo item in menuManage)
            {
                MenuManageModel model = new MenuManageModel()
                {
                    ID = item.Id,
                    TopMenuName = item.Title,
                    SubMenu = _iWeixinMenuService.GetMenuByParentId(item.Id),
                    URL = item.Url,
                    LinkType = item.UrlType
                };
                listMenuManage.Add(model);
            }
            return View(listMenuManage);
        }

        [HttpPost]
        public JsonResult DeleteMenu(int menuId)
        {
            Result result = new Result();
            _iWeixinMenuService.DeleteMenu(menuId);
            result.success = true;
            return Json(result);
        }

        [HttpPost]
        public JsonResult RequestToWeixin()
        {
            Result result = new Result();
            _iWeixinMenuService.ConsistentToWeixin(CurrentManager.ShopId);
            result.success = true;
            return Json(result);
        }

        [HttpPost]
        public JsonResult AddMenu(string title, string url, string parentId, int urlType)
        {
            short depth;
            if (parentId == "0")
                depth = 1;
            else
                depth = 2;

            switch (urlType)
            {
                case 1:
                    url = "http://" + HttpContext.Request.Url.Host + "/m-" + PlatformType.WeiXin.ToString() + "/";
                    break;
                case 2:
                    url = "http://" + HttpContext.Request.Url.Host + "/m-" + PlatformType.WeiXin.ToString() + "/vshop";
                    break;
                case 3:
                    url = "http://" + HttpContext.Request.Url.Host + "/m-" + PlatformType.WeiXin.ToString() + "/category/Index";
                    break;
                case 4:
                    url = "http://" + HttpContext.Request.Url.Host + "/m-" + PlatformType.WeiXin.ToString() + "/member/center";
                    break;
                case 5:
                    url = "http://" + HttpContext.Request.Url.Host + "/m-" + PlatformType.WeiXin.ToString() + "/cart/cart";
                    break;
                default:
                    break;
            }
            if ((!string.IsNullOrEmpty(url)) && (!url.Contains("http://")))
                throw new Himall.Core.HimallException("链接必须以http://开头");
            Result result = new Result();
            MenuInfo menu = new MenuInfo()
            {
                Title = title,
                Url = url,
                ParentId = Convert.ToInt64(parentId),
                Platform = PlatformType.WeiXin,
                Depth = depth,
                ShopId = CurrentManager.ShopId,
                FullIdPath = "1",
                Sequence = 1,
                UrlType = (MenuInfo.UrlTypes)urlType
            };
            _iWeixinMenuService.AddMenu(menu);
            result.success = true;
            return Json(result);
        }

        public ActionResult EditMenu(long menuId)
        {

            var menu = _iWeixinMenuService.GetMenu(menuId);
            var menuMode = new MenuManageModel()
            {
                ID = menu.Id,
                TopMenuName = menu.Title,
                URL = menu.Url,
                LinkType = menu.UrlType
            };
            if (menu.ParentId != 0)
            {
                ViewBag.parentName = _iWeixinMenuService.GetMenu(menu.ParentId).Title;
                ViewBag.parentId = menu.ParentId;
            }
            else
                ViewBag.parentId = 0;
            return View(menuMode);
        }

        [HttpPost]


        public ActionResult WeiXinReplay1()
        {
            return View();
        }
        public JsonResult UpdateMenu(string menuId, string menuName, int urlType, string url, string parentId)
        {
            switch (urlType)
            {
                case 1:
                    url = "http://" + HttpContext.Request.Url.Host + "/m-" + PlatformType.WeiXin.ToString() + "/";
                    break;
                case 2:
                    url = "http://" + HttpContext.Request.Url.Host + "/m-" + PlatformType.WeiXin.ToString() + "/vshop";
                    break;
                case 3:
                    url = "http://" + HttpContext.Request.Url.Host + "/m-" + PlatformType.WeiXin.ToString() + "/category/Index";
                    break;
                case 4:
                    url = "http://" + HttpContext.Request.Url.Host + "/m-" + PlatformType.WeiXin.ToString() + "/member/center";
                    break;
                case 5:
                    url = "http://" + HttpContext.Request.Url.Host + "/m-" + PlatformType.WeiXin.ToString() + "/cart/cart";
                    break;
                default:
                    break;
            }
            if ((!string.IsNullOrEmpty(url)) && (!url.Contains("http://")))
                throw new Himall.Core.HimallException("链接必须以http://开头");

            Result result = new Result();
            var menuInfo = new MenuInfo();
            menuInfo.Id = Convert.ToInt64(menuId);
            menuInfo.Title = menuName;
            menuInfo.UrlType = (MenuInfo.UrlTypes)urlType;
            menuInfo.Url = url;
            menuInfo.ParentId = Convert.ToInt64(parentId);
            _iWeixinMenuService.UpdateMenu(menuInfo);
            result.success = true;
            return Json(result);
        }

        public ActionResult TopicSettings()
        {
            var homeTopicInfos = _iMobileHomeTopicService.GetMobileHomeTopicInfos(PlatformType.WeiXin).ToArray();
            var topicService = _iTopicService;
            var models = homeTopicInfos.Select(item =>
            {
                var topic = topicService.GetTopicInfo(item.TopicId);
                return new TopicModel()
                {
                    FrontCoverImage = topic.FrontCoverImage,
                    Id = item.Id,
                    Name = topic.Name,
                    Tags = topic.Tags,
                    Sequence = item.Sequence
                };
            });
            return View(models);
        }

        [HttpPost]
        public JsonResult ChooseTopic(string frontCoverImage, long topicId)
        {
            var topicService = _iTopicService;
            var topic = topicService.GetTopicInfo(topicId);
            topic.FrontCoverImage = frontCoverImage;
            topicService.UpdateTopic(topic);
            _iMobileHomeTopicService.AddMobileHomeTopic(topicId, 0, PlatformType.WeiXin, frontCoverImage);
            return Json(new { success = true });
        }


        [HttpPost]
        public JsonResult RemoveChoseTopic(long id)
        {
            _iMobileHomeTopicService.Delete(id);
            return Json(new { success = true });
        }


        [HttpPost]
        public JsonResult UpdateSequence(long id, int sequence)
        {
            _iMobileHomeTopicService.SetSequence(id, sequence);
            return Json(new { success = true });
        }


        public ActionResult SlideImageSettings()
        {
            var slideImageSettings = _iSlideAdsService.GetSlidAds(0, SlideAdInfo.SlideAdType.WeixinHome).ToArray();
            var slideImageService = _iSlideAdsService;
            var models = slideImageSettings.Select(item =>
            {
                var slideImage = slideImageService.GetSlidAd(0, item.Id);
                return new SlideAdModel()
                {
                    ID = item.Id,
                    imgUrl = item.ImageUrl,
                    DisplaySequence = item.DisplaySequence,
                    Url = item.Url,
                    Description = item.Description
                };
            });
            return View(models);
        }

        public JsonResult AddSlideImage(string id, string description, string imageUrl, string url)
        {
            Result result = new Result();
            var slideAdInfo = new SlideAdInfo();
            slideAdInfo.Id = Convert.ToInt64(id);
            slideAdInfo.ImageUrl = imageUrl;
            slideAdInfo.TypeId = SlideAdInfo.SlideAdType.WeixinHome;
            slideAdInfo.Url = url;
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

        public ActionResult SaveSlideImage(long id = 0)
        {
            SlideAdInfo slideImageIfo;
            if (id > 0)
                slideImageIfo = _iSlideAdsService.GetSlidAd(0, id);
            else
                slideImageIfo = new SlideAdInfo();
            SlideAdModel model = new SlideAdModel()
            {
                Description = slideImageIfo.Description,
                imgUrl = Core.HimallIO.GetImagePath(slideImageIfo.ImageUrl),
                Url = slideImageIfo.Url,
                ID = id
            };
            return View(model);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult SlideImageChangeSequence(int oriRowNumber, int newRowNumber)
        {
            _iSlideAdsService.UpdateWeixinSlideSequence(0, oriRowNumber, newRowNumber, SlideAdInfo.SlideAdType.WeixinHome);
            return Json(new { success = true });
        }
        public ActionResult ProductSettings()
        {
            return View();
        }

        public JsonResult GetSlideImages()
        {
            //轮播图
            var slideImageSettings = _iSlideAdsService.GetSlidAds(0, SlideAdInfo.SlideAdType.WeixinHome).ToArray();
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

        public ActionResult WXMsgTemplateManage(string mediaid)
        {
            IEnumerable<WXMaterialInfo> result = new List<WXMaterialInfo>() { 
                new WXMaterialInfo{}
            };
            if (!string.IsNullOrWhiteSpace(mediaid))
            {
                result = _iWXMsgTemplateService.GetMedia(mediaid, this.CurrentSiteSetting.WeixinAppId, CurrentSiteSetting.WeixinAppSecret);
            }
            return View(result);
        }
        [ValidateInput(false)]
        public JsonResult AddWXMsgTemplate(string mediaid, string data)
        {
            IEnumerable<WXMaterialInfo> template = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<WXMaterialInfo>>(data);
            if (string.IsNullOrWhiteSpace(mediaid))
            {
                var result = _iWXMsgTemplateService.Add(template, CurrentSiteSetting.WeixinAppId, CurrentSiteSetting.WeixinAppSecret);
                if (string.IsNullOrEmpty(result.media_id))
                {
                    return Json(new { success = false, msg = result.errmsg });
                }
            }
            else
            {
                var updateResult = _iWXMsgTemplateService.UpdateMedia(mediaid, template, CurrentSiteSetting.WeixinAppId, CurrentSiteSetting.WeixinAppSecret);
                var result = updateResult.Where(e => !string.IsNullOrWhiteSpace(e.errmsg)).FirstOrDefault();
                if (result != null)
                {
                    if (result.errmsg == "ok")
                    {
                        return Json(new { success = true, msg = result.errmsg });
                    }
                    else
                    {
                        return Json(new { success = false, msg = result.errmsg });
                    }
                }
            }

            return Json(new { success = true });
        }
        public JsonResult AddWXImageMsg(string name)
        {
            var filename = Server.MapPath(name);
            var mediaid = _iWXMsgTemplateService.AddImage(filename, this.CurrentSiteSetting.WeixinAppId, this.CurrentSiteSetting.WeixinAppSecret);
            if (string.IsNullOrWhiteSpace(mediaid))
            {
                return Json(new { success = false, msg = "上传图片失败！" });
            }
            else
            {
                return Json(new { success = true, media = mediaid });
            }
        }
        public JsonResult GetWXMaterialList(int pageIdx, int pageSize)
        {
            var siteSetting = _iSiteSettingService.GetSiteSettings();
            if (string.IsNullOrWhiteSpace(siteSetting.WeixinAppId))
            {
                throw new HimallException("未配置微信公众号");
            }
            var offset = (pageIdx - 1) * pageSize;
            var list = _iWXMsgTemplateService.GetMediaMsgTemplateList(siteSetting.WeixinAppId, siteSetting.WeixinAppSecret, offset, pageSize);
            return Json(list);
        }
        public ActionResult GetMedia(string mediaid)
        {
            var siteSetting = _iSiteSettingService.GetSiteSettings();
            if (string.IsNullOrWhiteSpace(siteSetting.WeixinAppId))
            {
                throw new HimallException("未配置微信公众号");
            }
            MemoryStream stream = new MemoryStream();
            _iWXMsgTemplateService.GetMedia(mediaid, siteSetting.WeixinAppId, siteSetting.WeixinAppSecret, stream);
            return File(stream.ToArray(), "Image/png");
        }
        public ActionResult WXMsgTemplate()
        {
            return View();
        }
        public JsonResult GetMediaInfo(string mediaid)
        {
            var siteSetting = _iSiteSettingService.GetSiteSettings();
            if (string.IsNullOrWhiteSpace(siteSetting.WeixinAppId))
            {
                throw new HimallException("未配置微信公众号");
            }
            if (string.IsNullOrEmpty(mediaid))
            {
                throw new HimallException("素材ID不能为空");
            }
            var result = _iWXMsgTemplateService.GetMedia(mediaid, this.CurrentSiteSetting.WeixinAppId, this.CurrentSiteSetting.WeixinAppSecret);
            return Json(new { success = true, data = result });
        }
        public JsonResult DeleteMedia(string mediaid)
        {
            var siteSetting = _iSiteSettingService.GetSiteSettings();
            if (string.IsNullOrWhiteSpace(siteSetting.WeixinAppId))
            {
                throw new HimallException("未配置微信公众号");
            }
            if (string.IsNullOrEmpty(mediaid))
            {
                throw new HimallException("素材ID不能为空");
            }
            var result = _iWXMsgTemplateService.DeleteMedia(mediaid, this.CurrentSiteSetting.WeixinAppId, this.CurrentSiteSetting.WeixinAppSecret);
            if (string.IsNullOrEmpty(result.errmsg))
            {
                return Json(new { success = false, msg = result.errmsg });
            }
            else
            {
                return Json(new { success = true });
            }
        }

    }
}