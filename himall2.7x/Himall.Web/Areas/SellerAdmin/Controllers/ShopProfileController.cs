using Himall.Core.Helper;
using Himall.IServices;
using Himall.Model;
using Himall.Web.Framework;
using Himall.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Himall.DTO;
using Senparc.Weixin.MP.CommonAPIs;
using Himall.Core;
using System.Threading.Tasks;
using Himall.Application;

namespace Himall.Web.Areas.SellerAdmin.Controllers
{
    //门店基本信息
    public class ShopProfileController : BaseMemberController
    {
        ICategoryService _iCategoryService;


        private ISiteSettingService _iSiteSettingService;
        public ShopProfileController(IManagerService iManagerService,
            ISystemAgreementService iSystemAgreementService,
            ICategoryService iCategoryService,
            ISiteSettingService iSiteSettingService
            )
        {
            _iCategoryService = iCategoryService;
            _iSiteSettingService = iSiteSettingService;

        }

        #region 页面加载时判断申请进度

        /// <summary>
        /// 页面加载时处理
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            ViewBag.Logo = CurrentSiteSetting.MemberLogo;
            string controllerName = filterContext.RouteData.Values["controller"].ToString().ToLower();
            string actionName = filterContext.RouteData.Values["action"].ToString().ToLower();
            string areaName = filterContext.RouteData.DataTokens["area"].ToString().ToLower();
            if (CurrentSellerManager == null && actionName.IndexOf("step") != 0 && filterContext.RequestContext.HttpContext.Request.HttpMethod.ToUpper() != "POST")
            {
                if (actionName != ("EditProfile0").ToLower())
                {
                    var result = RedirectToAction("EditProfile0", "ShopProfile", new { area = "SellerAdmin" });
                    filterContext.Result = result;
                    return;
                }
            }
            else if (CurrentSellerManager != null)
            {
                var shop = ShopApplication.GetShop(CurrentSellerManager.ShopId);
                int stage = (int)shop.Stage;
                if (shop.Stage == ShopInfo.ShopStage.Finish && shop.ShopStatus == ShopInfo.ShopAuditStatus.Open)
                {
                    var result = RedirectToAction("index", "home", new { area = "SellerAdmin" });
                    filterContext.Result = result;
                    return;
                }
                if ((shop.Stage != ShopInfo.ShopStage.Finish || shop.ShopStatus == ShopInfo.ShopAuditStatus.WaitConfirm) && filterContext.RequestContext.HttpContext.Request.HttpMethod.ToUpper() != "POST" && string.IsNullOrEmpty(Request.QueryString["source"]))
                {
                    if (actionName.IndexOf("step") != 0)
                    {
                        bool isJump = true;
                        if (shop.ShopStatus == ShopInfo.ShopAuditStatus.Refuse || shop.ShopStatus == ShopInfo.ShopAuditStatus.Unusable)
                        {
                            string _tmp = actionName.ToLower().Replace("EditProfile".ToLower(), "");
                            int _tmpnum = 0;
                            if (int.TryParse(_tmp, out _tmpnum))
                            {
                                if (_tmpnum >= 1 && _tmpnum <= 3)
                                {
                                    //允许在填写公司资料时返回上一步查看信息
                                    isJump = false;
                                }
                            }
                        }
                        if (isJump)
                        {
                            //如果当前action是已经是对应的值则不要跳转，否则将进入死循环
                            if (actionName != ("EditProfile" + stage).ToLower())
                            {
                                var result = RedirectToAction("EditProfile" + stage, "ShopProfile", new { area = "SellerAdmin" });
                                filterContext.Result = result;
                                return;
                            }

                        }
                    }
                }
            }
        }

        #endregion

        #region 信息保存
        /// <summary>
        /// 商家入驻第一步信息保存
        /// </summary>
        /// <param name="shopProfileStep1"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult EditProfile1(ShopProfileStep1 shopProfileStep1)
        {
            if (ShopApplication.ExistCompanyName(shopProfileStep1.CompanyName, CurrentSellerManager.ShopId))
            {
                return Json(new { success = false, msg = "该公司名已存在！" });
            }
            if (ShopApplication.ExistBusinessLicenceNumber(shopProfileStep1.BusinessLicenceNumber, CurrentSellerManager.ShopId))
            {
                return Json(new { success = false, msg = "该营业执照号已存在！" });
            }

            //公司信息
            Himall.DTO.Shop shopInfo = ShopApplication.GetShop(CurrentSellerManager.ShopId);
            shopInfo.Id = CurrentSellerManager.ShopId;
            shopInfo.CompanyName = shopProfileStep1.CompanyName;
            shopInfo.CompanyAddress = shopProfileStep1.Address;
            shopInfo.CompanyRegionId = shopProfileStep1.CityRegionId;
            shopInfo.CompanyRegionAddress = shopProfileStep1.Address;
            shopInfo.CompanyPhone = shopProfileStep1.Phone;
            shopInfo.CompanyEmployeeCount = (CompanyEmployeeCount)shopProfileStep1.EmployeeCount;
            shopInfo.CompanyRegisteredCapital = shopProfileStep1.RegisterMoney;
            shopInfo.ContactsName = shopProfileStep1.ContactName;
            shopInfo.ContactsPhone = shopProfileStep1.ContactPhone;
            shopInfo.ContactsEmail = shopProfileStep1.Email;
            shopInfo.BusinessLicenceNumber = shopProfileStep1.BusinessLicenceNumber;
            shopInfo.BusinessLicenceRegionId = shopProfileStep1.BusinessLicenceArea;
            shopInfo.BusinessLicenceStart = shopProfileStep1.BusinessLicenceValidStart;
            shopInfo.BusinessLicenceEnd = shopProfileStep1.BusinessLicenceValidEnd;
            shopInfo.BusinessSphere = shopProfileStep1.BusinessSphere;
            shopInfo.BusinessLicenceNumberPhoto = shopProfileStep1.BusinessLicenceNumberPhoto;
            shopInfo.OrganizationCode = shopProfileStep1.OrganizationCode;
            shopInfo.OrganizationCodePhoto = shopProfileStep1.OrganizationCodePhoto;
            shopInfo.GeneralTaxpayerPhot = shopProfileStep1.GeneralTaxpayerPhoto;
            shopInfo.Stage = ShopInfo.ShopStage.FinancialInfo;
            shopInfo.BusinessLicenseCert = Request.Form["BusinessLicenseCert"];
            shopInfo.ProductCert = Request.Form["ProductCert"];
            shopInfo.OtherCert = Request.Form["OtherCert"];
            shopInfo.legalPerson = shopProfileStep1.legalPerson;
            shopInfo.CompanyFoundingDate = shopProfileStep1.CompanyFoundingDate;

            ShopApplication.UpdateShop(shopInfo);

            //管理员信息
            long uid = ShopApplication.GetShopManagers(CurrentSellerManager.ShopId);
            var mTheme = ShopApplication.GetSettled();
            var model = MemberApplication.GetMemberAccountSafety(uid);
            if (!mTheme.CompanyVerificationType.Equals(Himall.CommonModel.VerificationType.VerifyEmail) && shopProfileStep1.MemberPhone.Equals(""))
            {
                return Json(new { success = false, msg = "必须认证手机！" });
            }
            if (!mTheme.CompanyVerificationType.Equals(Himall.CommonModel.VerificationType.VerifyPhone) && shopProfileStep1.MemberEmail.Equals(""))
            {
                return Json(new { success = false, msg = "必须认证邮箱！" });
            }

            //修改真实姓名
            var member = MemberApplication.GetMembers(uid);
            member.RealName = shopProfileStep1.RealName;
            MemberApplication.UpdateMember(member);

            //手机认证
            if (!shopProfileStep1.MemberPhone.Equals(model.Phone))
            {
                string pluginId = "Himall.Plugin.Message.SMS";
                int result = MemberApplication.CheckMemberCode(pluginId, shopProfileStep1.PhoneCode, shopProfileStep1.MemberPhone, uid);
                string strMsg = "";
                switch (result)
                {
                    case 0: strMsg = "手机验证码错误！"; break;
                    case -1: strMsg = "此手机号已绑定！"; break;
                }
                if (!strMsg.Equals(""))
                {
                    return Json(new { success = false, msg = strMsg });
                }
            }

            //邮箱认证
            if (!shopProfileStep1.MemberEmail.Equals(model.Email))
            {
                string pluginId = "Himall.Plugin.Message.Email";
                int result = MemberApplication.CheckMemberCode(pluginId, shopProfileStep1.EmailCode, shopProfileStep1.MemberEmail, uid);
                string strMsg = "";
                switch (result)
                {
                    case 0: strMsg = "邮箱验证码错误！"; break;
                    case -1: strMsg = "此邮箱已绑定！"; break;
                }
                if (!strMsg.Equals(""))
                {
                    return Json(new { success = false, msg = strMsg });
                }
            }


            return Json(new { success = true, msg = "成功！" });
        }

        /// <summary>
        /// 个人入驻第一步信息保存
        /// </summary>
        /// <param name="shopProfileStep1"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult EditProfiles1(ShopProfileSteps1 shopProfileStep1)
        {
            //公司信息
            Himall.DTO.Shop shopInfo = ShopApplication.GetShop(CurrentSellerManager.ShopId);
            shopInfo.Id = CurrentSellerManager.ShopId;
            shopInfo.CompanyName = shopProfileStep1.CompanyName;
            shopInfo.CompanyAddress = shopProfileStep1.Address;
            shopInfo.CompanyRegionId = shopProfileStep1.CityRegionId;
            shopInfo.CompanyRegionAddress = shopProfileStep1.Address;
            shopInfo.Stage = ShopInfo.ShopStage.FinancialInfo;
            shopInfo.BusinessLicenseCert = Request.Form["BusinessLicenseCert"];
            shopInfo.ProductCert = Request.Form["ProductCert"];
            shopInfo.OtherCert = Request.Form["OtherCert"];
            shopInfo.IDCard = shopProfileStep1.IDCard;
            shopInfo.IDCardUrl = shopProfileStep1.IDCardUrl;
			shopInfo.IDCardUrl2 = shopProfileStep1.IDCardUrl2;

            ShopApplication.UpdateShop(shopInfo);

            long uid = ShopApplication.GetShopManagers(CurrentSellerManager.ShopId);
            //管理员信息
            var mTheme = ShopApplication.GetSettled();
            var model = MemberApplication.GetMemberAccountSafety(uid);
            if (!mTheme.SelfVerificationType.Equals(Himall.CommonModel.VerificationType.VerifyEmail) && shopProfileStep1.MemberPhone.Equals(""))
            {
                return Json(new { success = false, msg = "必须认证手机！" });
            }
            if (!mTheme.SelfVerificationType.Equals(Himall.CommonModel.VerificationType.VerifyPhone) && shopProfileStep1.MemberEmail.Equals(""))
            {
                return Json(new { success = false, msg = "必须认证邮箱！" });
            }

            //修改真实姓名
            var member = MemberApplication.GetMembers(uid);
            member.RealName = shopProfileStep1.RealName;
            MemberApplication.UpdateMember(member);

            //手机认证
            if (shopProfileStep1.MemberPhone != null && !shopProfileStep1.MemberPhone.Equals(model.Phone))
            {
                string pluginId = "Himall.Plugin.Message.SMS";
                int result = MemberApplication.CheckMemberCode(pluginId, shopProfileStep1.PhoneCode, shopProfileStep1.MemberPhone, uid);
                string strMsg = "";
                switch (result)
                {
                    case 0: strMsg = "手机验证码错误！"; break;
                    case -1: strMsg = "此手机号已绑定！"; break;
                }
                if (!strMsg.Equals(""))
                {
                    return Json(new { success = false, msg = strMsg });
                }
            }

            //邮箱认证
            if (shopProfileStep1.MemberEmail != null && !shopProfileStep1.MemberEmail.ToString().Equals(model.Email))
            {
                string pluginId = "Himall.Plugin.Message.Email";
                int result = MemberApplication.CheckMemberCode(pluginId, shopProfileStep1.EmailCode, shopProfileStep1.MemberEmail, uid);
                string strMsg = "";
                switch (result)
                {
                    case 0: strMsg = "邮箱验证码错误！"; break;
                    case -1: strMsg = "此邮箱已绑定！"; break;
                }
                if (!strMsg.Equals(""))
                {
                    return Json(new { success = false, msg = strMsg });
                }
            }


            return Json(new { success = true, msg = "成功！" });
        }

        /// <summary>
        /// 商家入驻第二部
        /// </summary>
        /// <param name="shopProfileStep2"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult EditProfile2(ShopProfileStep2 shopProfileStep2)
        {
            Himall.DTO.Shop shopInfo = ShopApplication.GetShop(CurrentSellerManager.ShopId);
            shopInfo.Id = CurrentSellerManager.ShopId;
            shopInfo.BankAccountName = shopProfileStep2.BankAccountName;
            shopInfo.BankAccountNumber = shopProfileStep2.BankAccountNumber;
            shopInfo.BankCode = shopProfileStep2.BankCode;
            shopInfo.BankName = shopProfileStep2.BankName;
            shopInfo.BankPhoto = shopProfileStep2.BankPhoto;
            shopInfo.BankRegionId = shopProfileStep2.BankRegionId;
            shopInfo.TaxpayerId = shopProfileStep2.TaxpayerId;
            shopInfo.TaxRegistrationCertificate = shopProfileStep2.TaxRegistrationCertificate;
            shopInfo.TaxRegistrationCertificatePhoto = shopProfileStep2.TaxRegistrationCertificatePhoto;
            shopInfo.Stage = ShopInfo.ShopStage.ShopInfo;

            ShopApplication.UpdateShop(shopInfo);
            return Json(new { success = true });
        }


        /// <summary>
        /// 第三部店铺信息提交
        /// </summary>
        /// <param name="shopProfileStep3"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult EditProfile3(string shopProfileStep3)
        {
            ShopProfileStep3 model = Newtonsoft.Json.JsonConvert.DeserializeObject<ShopProfileStep3>(shopProfileStep3);

            int result = ShopApplication.UpdateShop(model, CurrentSellerManager.ShopId);

            if (result.Equals(-1))
            {
                var msg = string.Format("{0} 店铺名称已经存在", model.ShopName);
                return Json(new { success = false, msg = msg });
            }

            return Json(new { success = true });
        }


        [HttpPost]
        public JsonResult EditProfile4(string payOrderPhoto, string remark)
        {
            Himall.DTO.Shop shopInfo = ShopApplication.GetShop(CurrentSellerManager.ShopId);
            shopInfo.Id = CurrentSellerManager.ShopId;
            shopInfo.PayPhoto = payOrderPhoto;
            shopInfo.PayRemark = remark;
            shopInfo.Stage = ShopInfo.ShopStage.Finish;
            shopInfo.ShopStatus = ShopInfo.ShopAuditStatus.WaitConfirm;
            ShopApplication.UpdateShop(shopInfo);
            return Json(new { success = true });
        }


        #endregion

        #region 父页面初始
        /// <summary>
        /// 入驻协议
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult EditProfile0()
        {
            ViewBag.Step = 0;
            ViewBag.MenuStep = 0;
            ViewBag.Frame = "Step0";
            ViewBag.Manager = CurrentUser;
            ViewBag.SellerAdminAgreement = ShopApplication.GetSellerAgreement();
            return View("EditProfile");
        }

        /// <summary>
        /// 商家信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult EditProfile1()
        {
            var model = ShopApplication.GetShop(CurrentSellerManager.ShopId);
            ViewBag.Step = 1;
            ViewBag.MenuStep = 1;
            if (model.BusinessType.Equals(Himall.CommonModel.ShopBusinessType.Personal))
                ViewBag.Frame = "Steps1";
            else
                ViewBag.Frame = "Step1";
            ViewBag.Manager = CurrentUser;
            ViewBag.SellerAdminAgreement = ShopApplication.GetSellerAgreement();
            var step1 = new ShopProfileStep1();
            return View("EditProfile", step1);
        }

        [HttpGet]
        public ActionResult EditProfile2()
        {
            ViewBag.MenuStep = 2;
            ViewBag.Step = 1;
            ViewBag.Frame = "Step2";
            ViewBag.Manager = CurrentUser;
            ViewBag.SellerAdminAgreement = ShopApplication.GetSellerAgreement();
            var step2 = new ShopProfileStep2();
            return View("EditProfile", step2);
        }


        [HttpGet]
        public ActionResult EditProfile3()
        {
            ViewBag.Step = 2;
            ViewBag.Frame = "Step3";
            ViewBag.Manager = CurrentUser;
            ViewBag.SellerAdminAgreement = ShopApplication.GetSellerAgreement();
            return View("EditProfile");
        }


        [HttpGet]
        public ActionResult EditProfile4()
        {
            ViewBag.Step = 3;
            ViewBag.Frame = "Step4";
            ViewBag.Manager = CurrentUser;
            ViewBag.Username = CurrentSellerManager.UserName;
            ViewBag.SellerAdminAgreement = ShopApplication.GetSellerAgreement();
            return View("EditProfile");
        }

        [HttpGet]
        public ActionResult EditProfile5()
        {
            ViewBag.Step = 3;
            ViewBag.Frame = "Step5";
            ViewBag.Manager = CurrentUser;
            return View("EditProfile");
        }

        [HttpGet]
        public ActionResult EditProfile6()
        {
            ViewBag.Step = 2;
            ViewBag.Frame = "Step6";
            ViewBag.Manager = CurrentUser;
            ViewBag.SellerAdminAgreement = ShopApplication.GetSellerAgreement();
            return View("EditProfile");
        }
        #endregion

        #region 详细页初始
        /// <summary>
        /// 入驻协议，第一步
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Step0()
        {
            ViewBag.SellerAdminAgreement = ShopApplication.GetSellerAgreement();
            var model = ShopApplication.GetSettled();
            return View(model);
        }

        /// <summary>
        /// 入驻信息，第二部
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Step1()
        {
            long CompanyRegionId = 0;
            long BusinessLicenceRegionId = 0;
            string RefuseReason = "";
            var step1 = ShopApplication.GetShopProfileStep1(CurrentSellerManager.ShopId, out CompanyRegionId, out BusinessLicenceRegionId, out RefuseReason);
            ViewBag.CompanyRegionIds = RegionApplication.GetRegionPath((int)CompanyRegionId);
            ViewBag.BusinessLicenceRegionIds = RegionApplication.GetRegionPath((int)BusinessLicenceRegionId);
            ViewBag.RefuseReason = RefuseReason;

            //管理员信息
            long userId = ShopApplication.GetShopManagers(CurrentSellerManager.ShopId);
            var model = MemberApplication.GetMembers(userId);
            step1.RealName = model.RealName;
            Himall.DTO.MemberAccountSafety mMemberAccountSafety = MemberApplication.GetMemberAccountSafety(userId);
            step1.MemberEmail = mMemberAccountSafety.Email;
            step1.MemberPhone = mMemberAccountSafety.Phone;

            //温馨提示
            ModelState.AddModelError("Phone", "多个联系方式用,号分隔");
            ModelState.AddModelError("ContactPhone", "多个联系方式用,号分隔");
            return View(step1);
        }

        /// <summary>
        /// 个人入驻商家信息
        /// </summary>
        /// <returns></returns>
        public ActionResult Steps1()
        {
            long CompanyRegionId = 0;
            long BusinessLicenceRegionId = 0;
            string RefuseReason = "";
            var step1 = ShopApplication.GetShopProfileSteps1(CurrentSellerManager.ShopId, out CompanyRegionId, out BusinessLicenceRegionId, out RefuseReason);

            ViewBag.CompanyRegionIds = RegionApplication.GetRegionPath((int)CompanyRegionId);
            ViewBag.BusinessLicenceRegionIds = RegionApplication.GetRegionPath((int)BusinessLicenceRegionId);
            ViewBag.RefuseReason = RefuseReason;

            long uid = ShopApplication.GetShopManagers(CurrentSellerManager.ShopId);
            //管理员信息
            var model = MemberApplication.GetMembers(uid);
            step1.RealName = model.RealName;
            Himall.DTO.MemberAccountSafety mMemberAccountSafety = MemberApplication.GetMemberAccountSafety(uid);
            step1.MemberEmail = mMemberAccountSafety.Email;
            step1.MemberPhone = mMemberAccountSafety.Phone;

            //温馨提示
            ModelState.AddModelError("Phone", "多个联系方式用,号分隔");
            ModelState.AddModelError("ContactPhone", "多个联系方式用,号分隔");
            return View(step1);
        }

        /// <summary>
        /// 入驻信息，第三部
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Step2()
        {
            ShopInfo.ShopStage Stage = ShopInfo.ShopStage.Agreement;
            var shop = ShopApplication.GetShopProfileStep2(CurrentSellerManager.ShopId, out Stage);

            if (Stage == ShopInfo.ShopStage.CompanyInfo)
                return RedirectToAction("step1");

            ViewBag.BankRegionIds = RegionApplication.GetRegionPath(shop.BankRegionId);


            return View(shop);
        }

        /// <summary>
        /// 入驻信息，第三部
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Step3()
        {
            ViewBag.ShopGrades = ShopApplication.GetShopGrades();
            ViewBag.ShopCategories = _iCategoryService.GetMainCategory();
            ViewBag.Username = CurrentSellerManager.UserName;

            var shop = ShopApplication.GetShop(CurrentSellerManager.ShopId, true);

            if (shop.Stage == ShopInfo.ShopStage.CompanyInfo)
                return RedirectToAction("step1");

            if (shop.Stage == ShopInfo.ShopStage.FinancialInfo)
                return RedirectToAction("step2");

            var BusinessCategory = new List<CategoryKeyVal>();
            foreach (var key in shop.BusinessCategory.Keys)
            {
                BusinessCategory.Add(new CategoryKeyVal
                {
                    CommisRate = shop.BusinessCategory[key],
                    Name = _iCategoryService.GetCategory(key).Name
                });
            }
            var step3 = new ShopProfileStep3();
            step3.ShopName = shop.ShopName;
            step3.ShopGrade = shop.GradeId;
            step3.BusinessType = shop.BusinessType == null ? Himall.CommonModel.ShopBusinessType.Enterprise : shop.BusinessType.Value;
            step3.BusinessCategory = ShopApplication.GetBusinessCategory(CurrentSellerManager.ShopId);

            return View(step3);
        }


        [HttpGet]
        public ActionResult Step4()
        {
            var shop = ShopApplication.GetShop(CurrentSellerManager.ShopId, true);
            string viewName;
            if (shop.ShopStatus == ShopInfo.ShopAuditStatus.WaitAudit)
            {
                viewName = "step4";
                ViewBag.Text = "已成功提交申请入驻信息，审核通过后将通过短信及邮件通知您，请您关注。";
            }
            else if (shop.ShopStatus == ShopInfo.ShopAuditStatus.Refuse)
            {
                ViewBag.Step = 1;
                ViewBag.MenuStep = 1;
                ViewBag.Frame = "Step1";
                ViewBag.Manager = CurrentSellerManager;
                var step1 = new ShopProfileStep1();
                return View("EditProfile", step1);
            }
            else
                return RedirectToAction("Finish");
            return View(viewName);
        }


        [HttpGet]
        public ActionResult Step5()
        {
            ViewBag.Text = "付款凭证已经提交，请等待管理员核对后为您开通店铺";
            var shop = ShopApplication.GetShop(CurrentSellerManager.ShopId);
            if (shop.ShopStatus == ShopInfo.ShopAuditStatus.WaitConfirm)
                return View("step4");
            else
                return RedirectToAction("Finish");
        }
        #endregion

        #region 其他请求

        /// <summary>
        /// 发送验证码
        /// </summary>
        /// <param name="pluginId"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SendCode(string pluginId, string destination)
        {
            long uid = ShopApplication.GetShopManagers(CurrentSellerManager.ShopId);
            var member = MemberApplication.GetMembers(uid);

            Himall.CommonModel.SendMemberCodeReturn status = MemberApplication.SendMemberCode(pluginId, destination, member.UserName, CurrentSiteSetting.SiteName);

            bool bo = status.Equals(Himall.CommonModel.SendMemberCodeReturn.success);
            return Json(new Result() { success = bo, msg = status.ToDescription() });
        }

        /// <summary>
        /// 跳转记录,记录当前管理员
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Agreement(string agree, int BusinessType)
        {
            if (agree.Equals("on"))
            {
                var seller = ShopApplication.AddSellerManager(CurrentUser.UserName, CurrentUser.Password, CurrentUser.PasswordSalt);

				base.SetSellerAdminLoginCookie(seller.Id, DateTime.Now.AddDays(7));

                var model = ShopApplication.GetShop(CurrentSellerManager.ShopId);
                model.BusinessType = (Himall.CommonModel.ShopBusinessType)BusinessType;
                model.Stage = Himall.Model.ShopInfo.ShopStage.CompanyInfo;
                ShopApplication.UpdateShop(model);
                return RedirectToAction("EditProfile1");
            }
            else
            {
                return RedirectToAction("EditProfile0");
            }
        }


        [HttpPost]
        public JsonResult GetCategories(long? key = null, int? level = -1)
        {
            var categories = _iCategoryService.GetValidBusinessCategoryByParentId(key.GetValueOrDefault());
            var models = categories.Select(item => new KeyValuePair<long, string>(item.Id, item.Name));
            return Json(models);
        }
        public ActionResult Finish()
        {
            return View();
        }
        #endregion

    }

}