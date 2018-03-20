using Himall.IServices;
using Himall.Model;
using Himall.Web.Areas.SellerAdmin.Models;
using Himall.Web.Framework;
using Himall.Web.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using Himall.Core.Plugins.Payment;
using Himall.Web.Areas.Web.Models;
using Himall.Core;
using Himall.IServices.QueryModel;
using Himall.Application;

namespace Himall.Web.Areas.SellerAdmin.Controllers
{
    public class ShopController : BaseSellerController
    {
        private IShopService _iShopService;
        private ICategoryService _iCategoryService;
        private IRegionService _iRegionService;

        public ShopController(
            IShopService iShopService,  
            ICategoryService iCategoryService, 
            IRegionService iRegionService)
        {
            _iShopService = iShopService;
            _iCategoryService = iCategoryService;
            _iRegionService = iRegionService;
        }

        // GET: SellerAdmin/Shop
        public ActionResult FreightSetting()
        {
            ShopInfo shop = _iShopService.GetShop(CurrentSellerManager.ShopId);
            var shopModel = new ShopFreightModel()
            {
                FreeFreight = shop.FreeFreight,
                Freight = shop.Freight,
            };
            return View(shopModel);
        }

        [HttpPost]
        [UnAuthorize]
        // GET: SellerAdmin/Shop
        public JsonResult SaveFreightSetting(ShopFreightModel shopFreight)
        {
            _iShopService.UpdateShopFreight(CurrentSellerManager.ShopId, shopFreight.Freight, shopFreight.FreeFreight);
            Cache.Remove(CacheKeyCollection.CACHE_SHOP(CurrentSellerManager.ShopId, false));
            return Json(new { success = true });
        }

        public ActionResult ShopDetail()
        {
            //Note:DZY[151010] 有form数据返回，传参暂时不能改
            var shopid = CurrentSellerManager.ShopId;
            var shop = _iShopService.GetShop(shopid, true);
            var model = new ShopModel(shop);
            model.BusinessCategory = new List<CategoryKeyVal>();
            foreach (var key in shop.BusinessCategory.Keys)
            {
                model.BusinessCategory.Add(new CategoryKeyVal
                {
                    CommisRate = shop.BusinessCategory[key],
                    Name = _iCategoryService.GetCategory(key).Name
                });
            }
            ViewBag.CompanyRegionIds = _iRegionService.GetRegionPath(shop.CompanyRegionId);
            ViewBag.BusinessLicenseCert = shop.BusinessLicenseCert;
            //var model= _iShopService.GetShopBasicInfo(shopid);

            string businessLicenseCerts = "";
            string productCerts = "";
            string otherCerts = "";
            for (int i = 0; i < 3; i++)
            {
                if (HimallIO.ExistFile(shop.BusinessLicenseCert + string.Format("{0}.png", i + 1)))
                    businessLicenseCerts += HimallIO.GetImagePath(shop.BusinessLicenseCert + string.Format("{0}.png", i + 1)) + ",";
                else
                    businessLicenseCerts += "null,";
                if (HimallIO.ExistFile(shop.ProductCert + string.Format("{0}.png", i + 1)))
                    productCerts += HimallIO.GetImagePath(shop.ProductCert + string.Format("{0}.png", i + 1)) + ",";
                else
                    productCerts += "null,";
                if (HimallIO.ExistFile(shop.OtherCert + string.Format("{0}.png", i + 1)))
                    otherCerts = HimallIO.GetImagePath(shop.OtherCert + string.Format("{0}.png", i + 1)) + ",";
                else
                    otherCerts += "null,";
            }
            ViewBag.BusinessLicenseCerts = businessLicenseCerts.TrimEnd(',');
            ViewBag.ProductCerts = productCerts.TrimEnd(',');
            ViewBag.OtherCerts = otherCerts.TrimEnd(',');

            //管理员信息
            long uid = ShopApplication.GetShopManagers(CurrentSellerManager.ShopId);
            var mUser = MemberApplication.GetMembers(uid);
            ViewBag.RealName = mUser.RealName;
            Himall.DTO.MemberAccountSafety mMemberAccountSafety = MemberApplication.GetMemberAccountSafety(uid);
            ViewBag.MemberEmail = mMemberAccountSafety.Email;
            ViewBag.MemberPhone = mMemberAccountSafety.Phone;

            if (model.BusinessType.Equals(Himall.CommonModel.ShopBusinessType.Enterprise))
            {
                return View(model);
            }
            else
            {
                return View("ShopPersonalDetail", model);
            }
        }

        /// <summary>
        /// 公司信息保存
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult EditProfile1()
        {

            string CompanyName = Request.Params["CompanyName"] ?? "";
            string CompanyAddress = Request.Params["CompanyAddress"] ?? "";
            string strCompanyRegionId = Request.Params["CompanyRegionId"] ?? "";
            strCompanyRegionId = string.IsNullOrWhiteSpace(strCompanyRegionId) ? Request.Params["NewCompanyRegionId"] : strCompanyRegionId;
            int CompanyRegionId = 0;
            if (!int.TryParse(strCompanyRegionId, out CompanyRegionId))
            {
                CompanyRegionId = 0;
            }
            string CompanyRegionAddress = Request.Params["CompanyRegionAddress"] ?? "";
            string CompanyEmployeeCount = Request.Params["CompanyEmployeeCount"] ?? "";

            string BusinessLicenseCert = Request.Params["BusinessLicenseCert"] ?? "";
            string ProductCert = Request.Params["ProductCert"] ?? "";
            string OtherCert = Request.Params["OtherCert"] ?? "";

            ShopInfo shopInfo = new ShopInfo()
            {
                Id = CurrentSellerManager.ShopId,
                CompanyName = CompanyName,
                CompanyAddress = CompanyAddress,
                CompanyRegionId = CompanyRegionId,
                CompanyRegionAddress = CompanyRegionAddress,
                CompanyEmployeeCount = (CompanyEmployeeCount)int.Parse(CompanyEmployeeCount),
                BusinessLicenseCert = BusinessLicenseCert,
                ProductCert = ProductCert,
                OtherCert = OtherCert
            };

            _iShopService.UpdateShop(shopInfo);
            return Json(new { success = true });
        }


        /// <summary>
        /// 公司信息保存
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Edit1()
        {

            string CompanyName = Request.Params["CompanyName"] ?? "";
            string CompanyAddress = Request.Params["CompanyAddress"] ?? "";
            string strCompanyRegionId = Request.Params["CompanyRegionId"] ?? "";
            strCompanyRegionId = string.IsNullOrWhiteSpace(strCompanyRegionId) ? Request.Params["NewCompanyRegionId"] : strCompanyRegionId;
            int CompanyRegionId = 0;
            if (!int.TryParse(strCompanyRegionId, out CompanyRegionId))
            {
                CompanyRegionId = 0;
            }
            string CompanyEmployeeCount = Request.Params["CompanyEmployeeCount"] ?? "";


            ShopInfo shopInfo = new ShopInfo()
            {
                Id = CurrentSellerManager.ShopId,
                CompanyName = CompanyName,
                CompanyAddress = CompanyAddress,
                CompanyRegionId = CompanyRegionId,
                CompanyEmployeeCount = (CompanyEmployeeCount)int.Parse(CompanyEmployeeCount)
            };

            _iShopService.UpdateShop(shopInfo);
            return Json(new { success = true });
        }


        /// <summary>
        /// 个人信息保存
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult EditPersonal1()
        {

            string CompanyName = Request.Params["CompanyName"] ?? "";
            string CompanyAddress = Request.Params["CompanyAddress"] ?? "";
            string strCompanyRegionId = Request.Params["CompanyRegionId"] ?? "";
            strCompanyRegionId = string.IsNullOrWhiteSpace(strCompanyRegionId) ? Request.Params["NewCompanyRegionId"] : strCompanyRegionId;
            int CompanyRegionId = 0;
            if (!int.TryParse(strCompanyRegionId, out CompanyRegionId))
            {
                CompanyRegionId = 0;
            }


            ShopInfo shopInfo = new ShopInfo()
            {
                Id = CurrentSellerManager.ShopId,
                CompanyName = CompanyName,
                CompanyAddress = CompanyAddress,
                CompanyRegionId = CompanyRegionId
            };

            _iShopService.UpdateShop(shopInfo);
            return Json(new { success = true });
        }

        /// <summary>
        /// 营业执照信息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Edit2()
        {
            string BusinessLicenseCert = Request.Params["BusinessLicenseCert"] ?? "";
            string ProductCert = Request.Params["ProductCert"] ?? "";
            string OtherCert = Request.Params["OtherCert"] ?? "";


            ShopInfo shopInfo = new ShopInfo()
            {
                Id = CurrentSellerManager.ShopId,
                BusinessLicenseCert = BusinessLicenseCert,
                ProductCert = ProductCert,
                OtherCert = OtherCert
            };

            _iShopService.UpdateShop(shopInfo);
            return Json(new { success = true });
        }


        /// <summary>
        /// 修改真实姓名
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Edit5(string RealName)
        {
            if (!RealName.Equals(""))
            {
                long uid = ShopApplication.GetShopManagers(CurrentSellerManager.ShopId);
                var member = MemberApplication.GetMembers(uid);
                member.RealName = RealName;
                MemberApplication.UpdateMember(member);
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, msg = "真实姓名不能为空" });
            }
        }

        public ActionResult Renew()
        {
            //店铺当前信息
            ShopRenewModel model = new ShopRenewModel();
            model.ShopId = CurrentSellerManager.ShopId;
            var oldShopInfo = _iShopService.GetSellerConsoleModel(CurrentSellerManager.ShopId);
            var shopInfo = _iShopService.GetShop(CurrentSellerManager.ShopId);
            model.ShopName = shopInfo.ShopName;
            model.ShopCreateTime = shopInfo.CreateDate.ToString("yyyy/MM/dd");
            model.ShopEndTime = shopInfo.EndDate.HasValue ? shopInfo.EndDate.Value.ToString("yyyy/MM/dd") : string.Empty;
            model.GradeId = shopInfo.GradeId;
            var shopGrade = _iShopService.GetShopGrades().Where(c => c.Id == shopInfo.GradeId).FirstOrDefault();
            model.GradeName = shopGrade != null ? shopGrade.Name : string.Empty;
            model.ProductLimit = oldShopInfo.ProductLimit;
            model.ImageLimit = (int)oldShopInfo.ImageLimit;

            //续费时间
            List<SelectListItem> yearList = new List<SelectListItem> { };
            yearList.Add(new SelectListItem() { Selected = true, Text = "一年", Value = "1" });
            yearList.Add(new SelectListItem() { Selected = false, Text = "两年", Value = "2" });
            yearList.Add(new SelectListItem() { Selected = false, Text = "三年", Value = "3" });
            yearList.Add(new SelectListItem() { Selected = false, Text = "四年", Value = "4" });
            yearList.Add(new SelectListItem() { Selected = false, Text = "五年", Value = "5" });
            ViewBag.YearList = yearList;

            //可升级套餐
            List<SelectListItem> gradeList = new List<SelectListItem>() { new SelectListItem() { Selected = true, Text = "请选择升级套餐", Value = "0" } };
            var enableGrade = _iShopService.GetShopGrades().Where(c => c.ChargeStandard > shopGrade.ChargeStandard);
            foreach (var item in enableGrade)
            {
                gradeList.Add(new SelectListItem() { Selected = false, Text = item.Name, Value = item.Id.ToString() });
            }

            ViewBag.GradeList = gradeList;
            ViewBag.HasOver = shopInfo.EndDate.Value <= DateTime.Now;

            return View(model);
        }

        [HttpPost]
        public JsonResult GetInfoAfterTimeSelect(int year)
        {
            var shopInfo = _iShopService.GetShop(CurrentSellerManager.ShopId);
            var shopGrade = _iShopService.GetShopGrades().Where(c => c.Id == shopInfo.GradeId).FirstOrDefault();
            DateTime endtime = shopInfo.EndDate.Value;
            if (shopInfo.EndDate.Value < DateTime.Now)
                endtime = DateTime.Now;
            string newEndTime = endtime.AddYears(year).ToString("yyyy/MM/dd");
            string amount = (shopGrade.ChargeStandard * year).ToString("F2");
            return Json(new { success = true, amount = amount, endtime = newEndTime });
        }

        [HttpPost]
        public JsonResult GetInfoAfterGradeSelect(int grade)
        {
            var shopInfo = _iShopService.GetShop(CurrentSellerManager.ShopId);
            var shopGrade = _iShopService.GetShopGrades().Where(c => c.Id == shopInfo.GradeId).FirstOrDefault();
            var newGrade = _iShopService.GetShopGrades().Where(c => c.Id == (long)grade).FirstOrDefault();

            //差价
            decimal spread = newGrade.ChargeStandard - shopGrade.ChargeStandard;
            DateTime endTime = shopInfo.EndDate.Value;
            string amount = (endTime.Subtract(DateTime.Now).Days * spread / 365).ToString("F2");
            string newGradeInfo = "可发布商品 " + newGrade.ProductLimit + "个，使用图片空间 " + newGrade.ImageLimit + "M";

            return Json(new { success = true, amount = amount, gradeTip = newGradeInfo });
        }

        public JsonResult PaymentList(decimal balance, int type, int value)
        {
            string webRoot = Request.Url.Scheme + "://" + HttpContext.Request.Url.Host + (HttpContext.Request.Url.Port == 80 ? "" : (":" + HttpContext.Request.Url.Port.ToString()));

            //获取同步返回地址
            string returnUrl = webRoot + "/SellerAdmin/Shop/ReNewPayReturn/{0}?balance={1}";

            //获取异步通知地址
            string payNotify = webRoot + "/pay/ReNewPayNotify/{0}?str={1}";

            var payments = Core.PluginsManagement.GetPlugins<IPaymentPlugin>(true).Where(item => item.Biz.SupportPlatforms.Contains(PlatformType.PC));

            const string RELATEIVE_PATH = "/Plugins/Payment/";

            //不重复数字
            string ids = DateTime.Now.ToString("yyyyMMddmmss") + CurrentSellerManager.ShopId.ToString();

            var models = payments.Select(item =>
            {
                string requestUrl = string.Empty;
                try
                {
                    requestUrl = item.Biz.GetRequestUrl(string.Format(returnUrl, EncodePaymentId(item.PluginInfo.PluginId), balance),
                                                        string.Format(payNotify, EncodePaymentId(item.PluginInfo.PluginId), balance + "-" + CurrentSellerManager.UserName + "-" + CurrentSellerManager.ShopId + "-" + type + "-" + value), ids, balance, "店铺续费");
                }
                catch (Exception ex)
                {
                    Core.Log.Error("支付页面加载支付插件出错", ex);
                }
                return new PaymentModel()
                {
                    Logo = RELATEIVE_PATH + item.PluginInfo.ClassFullName.Split(',')[1] + "/" + item.Biz.Logo,
                    RequestUrl = requestUrl,
                    UrlType = item.Biz.RequestUrlType,
                    Id = item.PluginInfo.PluginId
                };
            });
            models = models.Where(item => !string.IsNullOrEmpty(item.RequestUrl) && item.Id != "Himall.Plugin.Payment.WeiXinPay" && item.Id != "Himall.Plugin.Payment.WeiXinPay_Native");//只选择正常加载的插件
            return Json(models);
        }

        /// <summary>
        /// 检测公司名是否重复
        /// </summary>
        /// <param name="companyName"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult CheckCompanyName(string companyNameT)
        {
            var exist = ShopApplication.ExistCompanyName(companyNameT.Trim(), CurrentSellerManager.ShopId);
            return Json(!exist, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 检测营业执照号是否重复
        /// </summary>
        /// <param name="BusinessLicenceNumber"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult CheckBusinessLicenceNumber(string BusinessLicenceNumberT)
        {
            var exist = ShopApplication.ExistBusinessLicenceNumber(BusinessLicenceNumberT.Trim(), CurrentSellerManager.ShopId);
            return Json(!exist, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 检测营业执照号是否重复
        /// </summary>
        /// <param name="BusinessLicenceNumber"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult CheckBusinessLicenceNumbers(string BusinessLicenceNumber)
        {
            var exist = ShopApplication.ExistBusinessLicenceNumber(BusinessLicenceNumber.Trim(), CurrentSellerManager.ShopId);
            return Json(!exist, JsonRequestBehavior.AllowGet);
        }

        string EncodePaymentId(string paymentId)
        {
            return paymentId.Replace(".", "-");
        }

        string DecodePaymentId(string paymentId)
        {
            return paymentId.Replace("-", ".");
        }

        public ActionResult ReNewPayReturn(string id, decimal balance)
        {
            id = DecodePaymentId(id);
            string errorMsg = string.Empty;

            try
            {
                var payment = Core.PluginsManagement.GetPlugin<IPaymentPlugin>(id);
                var payInfo = payment.Biz.ProcessReturn(HttpContext.Request);
                ShopRenewRecord model = new ShopRenewRecord();
                bool result = Cache.Get(CacheKeyCollection.PaymentState(string.Join(",", payInfo.OrderIds))) == null ? false : true;
                if (!result)
                {
                    throw new Exception("支付未成功");

                    #region  "废弃,因为参数不足，无法在这里处理续费逻辑"
                    ////添加店铺续费记录
                    //model.ShopId = CurrentSellerManager.ShopId;
                    //model.OperateDate = DateTime.Now;
                    //model.Operator = CurrentSellerManager.UserName;
                    //model.Amount = balance;
                    ////续费操作
                    //if (type == 1)
                    //{

                    //    model.OperateType = ShopRenewRecord.EnumOperateType.ReNew;
                    //    var shopInfo = _iShopService.GetShop(CurrentSellerManager.ShopId);
                    //    string strNewEndTime = shopInfo.EndDate.Value.AddYears(value).ToString("yyyy-MM-dd");
                    //    model.OperateContent = "续费 " + value + " 年至 " + strNewEndTime;
                    //    _iShopService.AddShopRenewRecord(model);

                    //    //店铺续费
                    //    _iShopService.ShopReNew(CurrentSellerManager.ShopId, value);
                    //}
                    ////升级操作
                    //else
                    //{
                    //    model.ShopId = CurrentSellerManager.ShopId;
                    //    model.OperateType = ShopRenewRecord.EnumOperateType.Upgrade;
                    //    var shopInfo = _iShopService.GetShop(CurrentSellerManager.ShopId);
                    //    var shopGrade = _iShopService.GetShopGrades().Where(c => c.Id == shopInfo.GradeId).FirstOrDefault();
                    //    var newshopGrade = _iShopService.GetShopGrades().Where(c => c.Id == (long)value).FirstOrDefault();
                    //    model.OperateContent = "将套餐‘" + shopGrade.Name + "'升级为套餐‘" + newshopGrade.Name + "'";
                    //    _iShopService.AddShopRenewRecord(model);

                    //    //店铺升级
                    //    _iShopService.ShopUpGrade(CurrentSellerManager.ShopId, (long)value);
                    //}



                    ////写入支付状态缓存
                    //string payStateKey = CacheKeyCollection.PaymentState(string.Join(",", payInfo.OrderIds));//获取支付状态缓存键
                    //Cache.Insert(payStateKey, true);//标记为已支付
                    #endregion
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
            }
            ViewBag.Error = errorMsg;
            return View();
        }

        public ActionResult ShopRenewRecords()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Recordlist(int page, int rows)
        {
            ShopQuery query = new ShopQuery()
            {
                BrandId = CurrentSellerManager.ShopId,
                PageNo = page,
                PageSize = rows
            };

            ObsoletePageModel<ShopRenewRecord> accounts = _iShopService.GetShopRenewRecords(query);
            IList<ShopRecordModel> models = new List<ShopRecordModel>();
            foreach (var item in accounts.Models.ToArray())
            {
                ShopRecordModel model = new ShopRecordModel();
                model.Id = (int)item.Id;
                model.OperateType = item.OperateType.ToDescription();
                model.OperateDate = item.OperateDate.ToString("yyyy-MM-dd HH:mm");
                model.Operate = item.Operator;
                model.Content = item.OperateContent;
                model.Amount = item.Amount;
                models.Add(model);
            }
            return Json(new { rows = models, total = accounts.Total });
        }

        public ActionResult ShopOverview()
        {
            Himall.Web.Models.ShopModel result = new ShopModel();
            return View(result);
        }
    }
}