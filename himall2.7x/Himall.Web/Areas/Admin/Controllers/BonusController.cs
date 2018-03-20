using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.IServices;
using Himall.Web.Models;
using Himall.Model;
using Himall.Core;
using Himall.Web.Areas.Admin.Models.Market;
using Himall.Core.Helper;
using System.IO;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class BonusController : BaseAdminController
    {
        private IBonusService _iBonusService;
        private ISiteSettingService _iSiteSettingService;

        public BonusController(IBonusService iBonusService, ISiteSettingService iSiteSettingService)
        {
            _iSiteSettingService = iSiteSettingService;
            _iBonusService = iBonusService;
        }

        public ActionResult Management()
        {
            try
            {
                var siteSetting = _iSiteSettingService.GetSiteSettings();
                if (string.IsNullOrEmpty(siteSetting.WeixinAppId) || string.IsNullOrEmpty(siteSetting.WeixinAppSecret))
                {
                    return RedirectToAction("UnConfig");
                }
                return View();
            }
            catch (Exception e)
            {
                Exception innerEx = e.InnerException == null ? e : e.InnerException;
                Log.Info("微信红包进入出错：", innerEx);
                throw e;
            }
        }

        public ActionResult Config()
        {
            throw new NotImplementedException("功能转移至系统-消息设置");
            var siteSetting = _iSiteSettingService.GetSiteSettings();
            return View(siteSetting);
        }
        [HttpPost]
        public ActionResult Config(FormCollection form)
        {
            string wmtid = form["WX_MSGGetCouponTemplateId"];
            _iSiteSettingService.SaveSetting("WX_MSGGetCouponTemplateId", wmtid);
            return RedirectToAction("Config");
        }


        public ActionResult UnConfig()
        {
            return View();
        }



        public ActionResult Detail(long id)
        {
            ViewBag.Id = id;
            return View();
        }

        public ActionResult Add()
        {
            return View();
        }

        public ActionResult Edit(long id)
        {
            var obj = _iBonusService.Get(id);
            BonusModel model = new BonusModel(obj);
            return View(model);
        }

        public ActionResult Apportion(long id)
        {
            var obj = _iBonusService.Get(id);
            BonusModel model = new BonusModel(obj);
            model.QRPath = model.QRPath;
            return View(model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">1 : 关注，2：活动</param>
        /// <param name="state">1：正在进行，2：无效</param>
        [HttpPost]
        public JsonResult List(int type, int state, string name, int page = 1, int rows = 20)
        {
            var result = _iBonusService.Get(type, state, name.Trim(), page, rows);
            var datas = result.Models.ToList().Select(m => new BonusModel()
            {
                Id = m.Id,
                Type = m.Type,
                TypeStr = m.TypeStr,
                Style = m.Style,
                PriceType = (BonusInfo.BonusPriceType)m.PriceType,
                Name = m.Name,
                MerchantsName = m.MerchantsName,
                Remark = m.Remark,
                Blessing = m.Blessing,
                TotalPrice = m.TotalPrice,
                StartTime = m.StartTime,
                EndTime = m.EndTime,
                EndTimeStr = m.EndTimeStr,
                StartTimeStr = m.StartTimeStr,
                FixedAmount = (decimal)m.FixedAmount,
                RandomAmountStart = (decimal)m.RandomAmountStart,
                RandomAmountEnd = (decimal)m.RandomAmountEnd,
                ImagePath = m.ImagePath,
                Description = m.Description,
                ReceiveCount = m.ReceiveCount,
                IsInvalid = m.IsInvalid,
                StateStr = GetStateString(m)
            }).ToList();

            DataGridModel<BonusModel> model = new DataGridModel<BonusModel>
            {
                rows = datas,
                total = result.Total
            };
            return Json(model);
        }


        private string GetStateString(BonusInfo m)
        {
            if (m.EndTime < DateTime.Now)
            {
                return "已过期";
            }
            else if (m.IsInvalid)
            {
                return "已失效";
            }
            return "正在进行";

        }

        [HttpPost]
        public JsonResult DetailList(long id, int page = 1, int rows = 20)
        {
            var result = _iBonusService.GetDetail(id, page, rows);

            var datas = result.Models.ToList().Select(p => new BonusReceiveModel()
            {
                OpenId = p.OpenId,
                Price = p.Price,
                ReceiveTime = p.ReceiveTime == null ? "" : ((DateTime)p.ReceiveTime).ToString("yyyy-MM-dd"),
                UserName = p.Himall_Members == null ? "-" : p.Himall_Members.UserName,
                IsTransformedDeposit = p.IsTransformedDeposit
            }).ToList();
            DataGridModel<BonusReceiveModel> model = new DataGridModel<BonusReceiveModel>
            {
                rows = datas,
                total = result.Total
            };
            return Json(model);
        }

        [HttpPost]
        public JsonResult Invalid(long id)
        {
            _iBonusService.Invalid(id);
            return Json(true);
        }

        [HttpPost]
        public ActionResult Add(BonusModel model)
        {
            if (!string.IsNullOrEmpty(model.ImagePath))
            {
                if (!HimallIO.ExistDir("/Storage/Plat/Bonus"))
                {
                    HimallIO.CreateDir("/Storage/Plat/Bonus");
                }
                //转移图片
                if (model.ImagePath.Contains("/temp/"))
                {
                    string source = model.ImagePath.Substring(model.ImagePath.LastIndexOf("/temp"));
                    string dest = @"/Storage/Plat/Bonus/";
                    model.ImagePath = Path.Combine(dest, Path.GetFileName(source));
                    Core.HimallIO.CopyFile(source, model.ImagePath, true);
                }
                else if (model.ImagePath.Contains("/Storage/"))
                {
                    model.ImagePath = model.ImagePath.Substring(model.ImagePath.LastIndexOf("/Storage"));
                }

            }
            else
            {
                model.ImagePath = "";
            }

            model.StartTime = DateTime.Now;
            if (Himall.Model.BonusInfo.BonusType.Prize == model.Type) {
                model.EndTime = DateTime.Now.AddYears(20);
            }
            string url = "http://" + Request.Url.Host.ToString() + "/m-weixin/bonus/index/";
            _iBonusService.Add(model, url);
            return RedirectToAction("Management");
            throw new HimallException("验证失败");
        }

        [HttpPost]
        public ActionResult CanAdd()
        {
            var result = _iBonusService.CanAddBonus();
            return Json(result);
        }

        [HttpPost]
        public ActionResult EditData(BonusModel model)
        {
            if (ModelState.IsValid)
            {
                _iBonusService.Update(model);
                return RedirectToAction("Management");
            }
            throw new HimallException("验证失败");
        }
    }
}