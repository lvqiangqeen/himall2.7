using Himall.IServices;
using Himall.Model;
using Himall.Web.Areas.Admin.Models;
using Himall.Web.Framework;
using Himall.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.DTO;
using Himall.Application;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class AdvancePaymentController : BaseAdminController
    {
        // GET: Admin/AdvancePayment
        ISiteSettingService _iSiteSettingService;
        IRefundService _iRefundService;
        ICustomerService _iCustomerService;
        public AdvancePaymentController(ISiteSettingService iSiteSettingService, ICustomerService iCustomerService, IRefundService iRefundService)
        {
            _iSiteSettingService = iSiteSettingService;
            _iCustomerService = iCustomerService;
            _iRefundService = iRefundService;
        }

        public ActionResult edit()
        {
            var siteSetting = _iSiteSettingService.GetSiteSettings();
            var siteSettingModel = new SiteSettingModel()
            {
                AdvancePaymentPercent = siteSetting.AdvancePaymentPercent,
                AdvancePaymentLimit = siteSetting.AdvancePaymentLimit,
                UnpaidTimeout = siteSetting.UnpaidTimeout,
                NoReceivingTimeout = siteSetting.NoReceivingTimeout,
                OrderCommentTimeout = siteSetting.OrderCommentTimeout,
                SalesReturnTimeout = siteSetting.SalesReturnTimeout,
                AS_ShopConfirmTimeout = siteSetting.AS_ShopConfirmTimeout,
                AS_SendGoodsCloseTimeout = siteSetting.AS_SendGoodsCloseTimeout,
                AS_ShopNoReceivingTimeout = siteSetting.AS_ShopNoReceivingTimeout
            };
            return View(siteSettingModel);
        }

        [UnAuthorize]
        [HttpPost]
        [ValidateInput(false)]
        public JsonResult Edit(SiteSettingModel siteSettingModel)
        {
            Result result = new Result();
            var siteSetting = _iSiteSettingService.GetSiteSettings();
            bool isdataok = true;

            if (isdataok)
            {
                if (siteSettingModel.UnpaidTimeout < 0)
                {
                    isdataok = false;
                    result.success = false;
                    result.msg = "错误的未付款超时，不可为负";
                }
            }

            if (isdataok)
            {
                if (siteSettingModel.NoReceivingTimeout < 0)
                {
                    isdataok = false;
                    result.success = false;
                    result.msg = "错误的确认收货超时，不可为负";
                }
            }
            if (isdataok)
            {
                if (siteSettingModel.OrderCommentTimeout < 0)
                {
                    isdataok = false;
                    result.success = false;
                    result.msg = "错误的关闭评价通道时限，不可为负";
                }
            }

            if (isdataok)
            {
                if (siteSettingModel.SalesReturnTimeout < 0)
                {
                    isdataok = false;
                    result.success = false;
                    result.msg = "错误的订单退货期限，不可为负";
                }
            }

            if (isdataok)
            {
                if (siteSettingModel.AS_ShopConfirmTimeout < 0)
                {
                    isdataok = false;
                    result.success = false;
                    result.msg = "错误的商家自动确认售后时限，不可为负";
                }
            }
            if (isdataok)
            {
                if (siteSettingModel.AS_SendGoodsCloseTimeout < 0)
                {
                    isdataok = false;
                    result.success = false;
                    result.msg = "错误的用户发货限时，不可为负";
                }
            }
            if (isdataok)
            {
                if (siteSettingModel.AS_ShopNoReceivingTimeout < 0)
                {
                    isdataok = false;
                    result.success = false;
                    result.msg = "错误的商家确认到货时限，不可为负";
                }
            }

            if (isdataok)
            {
                //siteSetting.AdvancePaymentLimit = siteSettingModel.AdvancePaymentLimit;
                //siteSetting.AdvancePaymentPercent = siteSettingModel.AdvancePaymentPercent;
                siteSetting.NoReceivingTimeout = siteSettingModel.NoReceivingTimeout;
                siteSetting.UnpaidTimeout = siteSettingModel.UnpaidTimeout;
                siteSetting.OrderCommentTimeout = siteSettingModel.OrderCommentTimeout;

                siteSetting.SalesReturnTimeout = siteSettingModel.SalesReturnTimeout;
                siteSetting.AS_ShopConfirmTimeout = siteSettingModel.AS_ShopConfirmTimeout;
                siteSetting.AS_SendGoodsCloseTimeout = siteSettingModel.AS_SendGoodsCloseTimeout;
                siteSetting.AS_ShopNoReceivingTimeout = siteSettingModel.AS_ShopNoReceivingTimeout;

                _iSiteSettingService.SetSiteSettings(siteSetting);
                result.success = true;
            }
            return Json(result);
        }

        #region 售后原因设置
        public ActionResult RefundReason()
        {
            var datalist = _iRefundService.GetRefundReasons();
            return View(datalist);
        }
        [HttpPost]
        public JsonResult SaveRefundReason(string reason, long id = 0)
        {
            Result r = new Result { msg = "售后原因处理成功", status = 0, success = true };
            bool isdataok = true;
            if (isdataok)
            {
                if (string.IsNullOrWhiteSpace(reason))
                {
                    isdataok = false;
                    r = new Result { msg = "请填写售后原因", status = -2, success = false };
                }
            }
            if (isdataok)
            {
                if (reason.Length > 20)
                {
                    isdataok = false;
                    r = new Result { msg = "售后原因限20字符", status = -2, success = false };
                }
            }
            _iRefundService.UpdateAndAddRefundReason(reason, id);
            return Json(r);
        }
        [HttpPost]
        public JsonResult DeleteRefundReason(long id)
        {
            Result r = new Result { msg = "删除成功", status = 0, success = true };
            _iRefundService.DeleteRefundReason(id);
            return Json(r);
        }

        public ActionResult CustomerServicesEdit()
        {
			var css =CustomerServiceApplication.GetPlatformCustomerService();

			var count = 3;//客服个数，可设计成从站点配置取
			var models = new PlatformCustomerServiceModel[count];
			for (int i = 0; i < css.Count; i++)
			{
				models[i] = new PlatformCustomerServiceModel();
				css[i].Map(models[i]);
			}

			for (int i = css.Count; i < models.Length; i++)
			{
				models[i] = new PlatformCustomerServiceModel();
				models[i].CreateId = Guid.NewGuid();
				models[i].Tool = CustomerServiceInfo.ServiceTool.QQ;
			}

			if (!models.Any(p => p.Tool == CustomerServiceInfo.ServiceTool.MeiQia))
				models.First(p => p.Id == 0).Tool = CustomerServiceInfo.ServiceTool.MeiQia;//设置其中一个客服为美洽客服

			return View(models);
        }

        [HttpPost]
        public JsonResult CustomerServicesEdit(PlatformCustomerServiceModel[] css)
        {
			var count = 3;//客服个数，可设计成从站点配置取
			css = css.Where(p =>!string.IsNullOrEmpty(p.Name) && !string.IsNullOrEmpty(p.AccountCode)).Take(count).ToArray();

			foreach (var item in css)
			{
				item.ShopId = 0;
				if (!string.IsNullOrEmpty(item.Name))
					item.Name = item.Name.Trim();
				if (!string.IsNullOrEmpty(item.AccountCode))
					item.AccountCode = item.AccountCode.Trim();
			}

			var newIds = new Dictionary<Guid, long>();
			if (css.Any(p => p.Id == 0))
			{
				foreach (var item in css.Where(p=>p.Id==0))
				{
					var newId = CustomerServiceApplication.AddPlateCustomerService(item.Map<CustomerService>());
					newIds.Add(item.CreateId, newId);
				}
			}
			if (css.Any(p => p.Id > 0))
				Application.CustomerServiceApplication.UpdatePlatformService(css.Where(p => p.Id > 0));

			return Json(new
			{
				Success = true,
				NewIds = newIds
			},true);
        }
        #endregion
    }
}