using Himall.IServices;
using Himall.Model;
using Himall.Web.Framework;
using Himall.Web.Models;
using System;
using System.Web.Mvc;
using Himall.Application;
using System.Drawing;
using System.IO;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class AgreementController : BaseAdminController
    {
        ISystemAgreementService _iSystemAgreementService;
        public AgreementController(ISystemAgreementService iSystemAgreementService)
        {
            _iSystemAgreementService = iSystemAgreementService;
        }
        // GET: Admin/Agreement
        public ActionResult Management()
        {
            var AgreementTypes = Himall.Model.AgreementInfo.AgreementTypes.Buyers;
            if (!string.IsNullOrEmpty(Request.QueryString["type"]) && Request.QueryString["type"].Equals("Seller"))
            {
                AgreementTypes = Himall.Model.AgreementInfo.AgreementTypes.Seller;
            }
            //初始化默认返回买家注册协议
            return View(GetManagementModel(AgreementTypes));
        }
        /// <summary>
        /// 入驻链接
        /// </summary>
        /// <returns></returns>
        public ActionResult SettledLink()
        {
            #region 商家入驻链接和二维码
            string LinkUrl = String.Format("http://{0}/m-weixin/shopregister/step1", Request.Url.Authority);
            ViewBag.LinkUrl = LinkUrl;
            string qrCodeImagePath = string.Empty;
            if (!string.IsNullOrWhiteSpace(LinkUrl))
            {
                Bitmap map;
                map = Core.Helper.QRCodeHelper.Create(LinkUrl);
                MemoryStream ms = new MemoryStream();
                map.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                qrCodeImagePath = "data:image/gif;base64," + Convert.ToBase64String(ms.ToArray()); // 将图片内存流转成base64,图片以DataURI形式显示  
                ms.Dispose();
            }
            ViewBag.Imgsrc = qrCodeImagePath;
            #endregion
            return View();
        }

        public ActionResult EnterSet()
        {
            //入驻参数设置
            return View();
        }


        [HttpPost]
        public JsonResult GetManagement(int agreementType)
        {
            return Json(GetManagementModel((Himall.Model.AgreementInfo.AgreementTypes)agreementType));
        }

        public AgreementModel GetManagementModel(Himall.Model.AgreementInfo.AgreementTypes type)
        {
            AgreementModel model = new AgreementModel();
            var iAgreement = _iSystemAgreementService.GetAgreement(type);
            model.AgreementType = iAgreement.AgreementType;
            model.AgreementContent = iAgreement.AgreementContent;
            return model;

        }

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult UpdateAgreement(int agreementType, string agreementContent)
        {
            var iAgreement = _iSystemAgreementService;
            AgreementInfo model = iAgreement.GetAgreement((Himall.Model.AgreementInfo.AgreementTypes)agreementType);
            model.AgreementType = agreementType;
            model.AgreementContent = agreementContent;
            if (iAgreement.UpdateAgreement(model))
                return Json(new Result() { success = true, msg = "更新协议成功！" });
            else
                return Json(new Result() { success = false, msg = "更新协议失败！" });
        }

        #region 入驻设置
        public ActionResult Settled()
        {
            var model = ShopApplication.GetSettled();
            return View(model);
        }


        /// <summary>
        /// 商家入驻设置
        /// </summary>
        /// <param name="mSettled"></param>
        /// <returns></returns>
        public JsonResult setSettled(Himall.DTO.Settled mSettled)
        {
            ShopApplication.Settled(mSettled);
            return Json(new Result() { success = true, msg = "设置成功！" });
        }

        #endregion
    }
}