using AutoMapper;
using Himall.Core.Helper;
using Himall.IServices;
using Himall.Model;
using Himall.Web.Framework;
using Himall.Web.Models;
using System;
using System.IO;
using System.Web.Mvc;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Linq;
using System.Configuration;
using System.Web.Configuration;
using Himall.CommonModel;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class SiteSettingController : BaseAdminController
    { /// <summary>
        /// 上传文件的扩展名集合
        /// </summary>
        string[] AllowApkFileExtensions = new string[] { ".apk" };
        ISiteSettingService _iSiteSettingService;
        public SiteSettingController(ISiteSettingService iSiteSettingService)
        {
            _iSiteSettingService = iSiteSettingService;
        }
        // GET: Admin/SiteSetting
        public ActionResult Edit()
        {
            var siteSetting = _iSiteSettingService.GetSiteSettings();
            Mapper.CreateMap<SiteSettingsInfo, SiteSettingModel>().ForMember(a => a.SiteIsOpen, b => b.MapFrom(s => s.SiteIsClose));
            var siteSettingModel = Mapper.Map<SiteSettingsInfo, SiteSettingModel>(siteSetting);
            siteSettingModel.Logo = Core.HimallIO.GetImagePath(siteSettingModel.Logo);
            siteSettingModel.MemberLogo = Core.HimallIO.GetImagePath(siteSettingModel.MemberLogo);
            siteSettingModel.PCLoginPic = Core.HimallIO.GetImagePath(siteSettingModel.PCLoginPic);
            siteSettingModel.QRCode = Core.HimallIO.GetImagePath(siteSettingModel.QRCode);
            siteSettingModel.WXLogo = Core.HimallIO.GetImagePath(siteSettingModel.WXLogo);
            return View(siteSettingModel);
        }

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult Edit(SiteSettingModel siteSettingModel)
        {
            if (string.IsNullOrWhiteSpace(siteSettingModel.WXLogo))
            {
                return Json(new Result() { success = false, msg = "请上传微信Logo", status = -1 });
            }
            if (string.IsNullOrWhiteSpace(siteSettingModel.Logo))
            {
                return Json(new Result() { success = false, msg = "请上传Logo", status = -2 });
            }

            if (StringHelper.GetStringLength(siteSettingModel.SiteName) > CommonModel.CommonConst.SITENAME_LENGTH)
            {
                var unicodeChar = CommonModel.CommonConst.SITENAME_LENGTH / 2;
                return Json(new Result() { success = false, msg = "网站名字最长" + CommonModel.CommonConst.SITENAME_LENGTH + "位," + unicodeChar + "个中文字符", status = -2 });
            }

            if (string.IsNullOrEmpty(siteSettingModel.SitePhone))
            {
                return Json(new Result() { success = false, msg = "请填写客服电话", status = -2 });
            }

            
            //var reg =new Regex(  @"([0-9]{1}.){2,}[0-9]{1}$" );
            //var aaa = reg.IsMatch(siteSettingModel.AppVersion);
            //  var logoN=  siteSettingModel.Logo.Substring(siteSettingModel.Logo.LastIndexOf("/"));

            //string logo = IOHelper.GetMapPath(siteSettingModel.Logo);
            //string memberLogo = IOHelper.GetMapPath(siteSettingModel.MemberLogo);
            //string qrCode = IOHelper.GetMapPath(siteSettingModel.QRCode);
            //string PCLoginPic = IOHelper.GetMapPath(siteSettingModel.PCLoginPic);
            //改成文件策略模式

        
        

            string logoName = "logo.png";
            string memberLogoName = "memberLogo.png";
            string qrCodeName = "qrCode.png";
            string PCLoginPicName = "pcloginpic.png";

            string relativeDir = "/Storage/Plat/Site/";
            string imageDir = relativeDir;

            //if (!Directory.Exists(imageDir))
            //{
            //    Directory.CreateDirectory(imageDir);
            //}

            if (!string.IsNullOrWhiteSpace(siteSettingModel.Logo))
            {

                if (siteSettingModel.Logo.Contains("/temp/"))
                {
                    string Logo = siteSettingModel.Logo.Substring(siteSettingModel.Logo.LastIndexOf("/temp"));
                    Core.HimallIO.CopyFile(Logo, imageDir + logoName, true);
                }
            }
            if (!string.IsNullOrWhiteSpace(siteSettingModel.MemberLogo))
            {
                if (siteSettingModel.MemberLogo.Contains("/temp/"))
                {
                    string memberLogo = siteSettingModel.MemberLogo.Substring(siteSettingModel.MemberLogo.LastIndexOf("/temp"));

                    Core.HimallIO.CopyFile(memberLogo, imageDir + memberLogoName, true);
                    //  Core.Helper.IOHelper.CopyFile(memberLogo, imageDir, false, memberLogoName);
                }
            }
            if (!string.IsNullOrWhiteSpace(siteSettingModel.QRCode))
            {
                if (siteSettingModel.QRCode.Contains("/temp/"))
                {
                    string qrCode = siteSettingModel.QRCode.Substring(siteSettingModel.QRCode.LastIndexOf("/temp"));
                    Core.HimallIO.CopyFile(qrCode, imageDir + qrCodeName, true);
                 //   Core.Helper.IOHelper.CopyFile(qrCode, imageDir, false, qrCodeName);
                }
            }

            if (!string.IsNullOrWhiteSpace(siteSettingModel.PCLoginPic))
            {
                if (siteSettingModel.PCLoginPic.Contains("/temp/"))
                {
                    string PCLoginPic = siteSettingModel.PCLoginPic.Substring(siteSettingModel.PCLoginPic.LastIndexOf("/temp"));
                //    Core.Helper.IOHelper.CopyFile(PCLoginPic, imageDir, true, PCLoginPicName);
                    Core.HimallIO.CopyFile(PCLoginPic, imageDir + PCLoginPicName, true);

                 
                }
            }

            if (!string.IsNullOrWhiteSpace(siteSettingModel.WXLogo))
            {
                if (siteSettingModel.WXLogo.Contains("/temp/"))
                {
                    string newFile = relativeDir + "wxlogo.png";
                   // string oriFullPath = Core.Helper.IOHelper.GetMapPath(siteSettingModel.WXLogo);
                   // string newFullPath = Core.Helper.IOHelper.GetMapPath(newFile);
                    string wxlogoPic = siteSettingModel.WXLogo.Substring(siteSettingModel.WXLogo.LastIndexOf("/temp"));
                    Core.HimallIO.CopyFile(wxlogoPic, newFile, true);
                    Core.HimallIO.CreateThumbnail(wxlogoPic, newFile, (int)ImageSize.Size_100, (int)ImageSize.Size_100);
                    //using (Image image = Image.FromFile(oriFullPath))
                    //{
                    //    image.Save(oriFullPath + ".png", System.Drawing.Imaging.ImageFormat.Png);
                    //    if (System.IO.File.Exists(newFullPath))
                    //    {
                    //        System.IO.File.Delete(newFullPath);
                    //    }
                    //    ImageHelper.CreateThumbnail(oriFullPath + ".png", newFullPath, 100, 100);
                    //}
                    siteSettingModel.WXLogo = newFile;
                }
            }


            Result result = new Result();
            var siteSetting = _iSiteSettingService.GetSiteSettings();
            siteSetting.SiteName = siteSettingModel.SiteName;
            siteSetting.SitePhone = siteSettingModel.SitePhone;
            siteSetting.SiteIsClose = siteSettingModel.SiteIsOpen;
            siteSetting.Logo = relativeDir + logoName;
            siteSetting.MemberLogo = relativeDir + memberLogoName;
            siteSettingModel.PCLoginPic =relativeDir+PCLoginPicName;
            siteSetting.QRCode = relativeDir + qrCodeName;
            siteSetting.FlowScript = siteSettingModel.FlowScript;
            siteSetting.Site_SEOTitle = siteSettingModel.Site_SEOTitle;
            siteSetting.Site_SEOKeywords = siteSettingModel.Site_SEOKeywords;
            siteSetting.Site_SEODescription = siteSettingModel.Site_SEODescription;

            siteSetting.MobileVerifOpen = siteSettingModel.MobileVerifOpen;

            siteSetting.RegisterType = (int)siteSettingModel.RegisterType;
            siteSetting.MobileVerifOpen = false;
            siteSetting.EmailVerifOpen = false;
            siteSetting.RegisterEmailRequired = false;
            switch (siteSettingModel.RegisterType)
            {
                case SiteSettingsInfo.RegisterTypes.Mobile:
                    siteSetting.MobileVerifOpen = true;
                    break;
                case SiteSettingsInfo.RegisterTypes.Normal:
                    if (siteSettingModel.EmailVerifOpen == true)
                    {
                        siteSetting.EmailVerifOpen = true;
                        siteSetting.RegisterEmailRequired = true;
                    }
                    break;
            }

            siteSetting.WXLogo = siteSettingModel.WXLogo;
            siteSetting.PCLoginPic = siteSettingModel.PCLoginPic;

            Version ver = null;
            try
            {
                ver = new Version(siteSettingModel.AppVersion);
            }
            catch (Exception)
            {
                throw new Himall.Core.HimallException("错误的版本号");
            }
            siteSetting.AppUpdateDescription = siteSettingModel.AppUpdateDescription;
            siteSetting.AppVersion = ver.ToString();//siteSettingModel.AppVersion;
            siteSetting.AndriodDownLoad = siteSettingModel.AndriodDownLoad;
            siteSetting.IOSDownLoad = siteSettingModel.IOSDownLoad;
            siteSetting.CanDownload = siteSettingModel.CanDownload;
            _iSiteSettingService.SetSiteSettings(siteSetting);
            result.success = true;
            return Json(result);
        }
        [HttpPost]
        public ActionResult UploadApkFile()
        {
            string strResult = "NoFile";
            if (Request.Files.Count > 0)
            {
                var file = Request.Files[0];
                if (file.ContentLength == 0)
                {
                    strResult = "文件长度为0,格式异常。";
                }
                else
                {
                    Configuration config = WebConfigurationManager.OpenWebConfiguration(Request.ApplicationPath);
                    var appName = config.AppSettings.Settings["AppName"].Value;
                    string filename = appName + Path.GetExtension(file.FileName);
                    if (!CheckApkFileType(filename))
                    {
                        return Content("上传的文件格式不正确", "text/html");
                    }
                    string DirUrl = Server.MapPath("~/app/");
                    if (!System.IO.Directory.Exists(DirUrl))      //检测文件夹是否存在，不存在则创建
                    {
                        System.IO.Directory.CreateDirectory(DirUrl);
                    }
                    var curhttp = System.Web.HttpContext.Current;
                    string url = curhttp.Request.Url.Scheme + "://" + curhttp.Request.Url.Authority;
                    string strfile = url + "/app/" + filename;
                    try
                    {
                        object opcount = Core.Cache.Get(CacheKeyCollection.UserImportOpCount);
                        if (opcount == null)
                        {
                            Core.Cache.Insert(CacheKeyCollection.UserImportOpCount, 1);
                        }
                        else
                        {
                            Core.Cache.Insert(CacheKeyCollection.UserImportOpCount, int.Parse(opcount.ToString()) + 1);
                        }
                        file.SaveAs(Path.Combine(DirUrl, filename));
                    }
                    catch (Exception e)
                    {
                        object opcount = Core.Cache.Get(CacheKeyCollection.UserImportOpCount);
                        if (opcount != null)
                        {
                            Core.Cache.Insert(CacheKeyCollection.UserImportOpCount, int.Parse(opcount.ToString()) - 1);
                        }
                        strfile = "Error";
                    }
                    strResult = strfile;
                }
            }
            return Content(strResult, "text/html");
        }
        private bool CheckApkFileType(string filename)
        {
            var fileExtension = Path.GetExtension(filename).ToLower();
            return AllowApkFileExtensions.Select(x => x.ToLower()).Contains(fileExtension);
        }
    }
}