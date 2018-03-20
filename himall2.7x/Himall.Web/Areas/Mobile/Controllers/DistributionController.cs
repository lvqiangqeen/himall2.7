using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.Model;
using System.IO;
using Senparc.Weixin.MP.Helpers;
using Himall.Core;
using Senparc.Weixin.MP.CommonAPIs;
using Himall.Web.App_Code.Common;
using Himall.Core.Plugins.Message;
using Himall.Web.Areas.Mobile.Models;
using Himall.Application;

namespace Himall.Web.Areas.Mobile.Controllers
{
    public class DistributionController : BaseMobileMemberController
    {
        private SiteSettingsInfo _siteSetting = null;
        private ISiteSettingService _iSiteSettingService;
        private IMemberService _iMemberService;
        private IRegionService _iRegionService;
        private IMessageService _iMessageService;
        private IDistributionService _iDistributionService;
        private const string SMSPLUGIN = "Himall.Plugin.Message.SMS";
        private long curUserId;
        private DistributorSettingInfo _distributionsetting = null;
        public DistributionController(IDistributionService iDistributionService, IMemberService iMemberService, IMessageService iMessageService, IRegionService iRegionService, ISiteSettingService iSiteSettingService)
        {
            _iDistributionService = iDistributionService;
            _iMemberService = iMemberService;
            _iMessageService = iMessageService;
            _iRegionService = iRegionService;
            _iSiteSettingService = iSiteSettingService;
            this._siteSetting = _iSiteSettingService.GetSiteSettings();
            this._distributionsetting = _iDistributionService.GetDistributionSetting();
            if (this._distributionsetting == null)
            {
                throw new HimallException("平台未开启分销！");
            }
        }
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (CurrentUser != null)
            {
                curUserId = CurrentUser.Id;
            }
        }
        public ActionResult WaitAudit()
        {
            return View();
        }

        public ActionResult Apply(long productId = 0)
        {
            var setting = _iDistributionService.GetRecruitmentSetting();
            if (setting == null)
            {
                throw new HimallException("平台未设置招募审核！");
            }
            PromoterModel model = new PromoterModel();
            model.Member = MemberApplication.GetMember(curUserId);
            model.RecruitSetting = setting;
            if (setting.MustAddress)
                model.RegionPath = _iRegionService.GetRegionPath(model.Member.RegionId);
            if (setting.MustMobile)
            {
                var mobile = _iMessageService.GetDestination(curUserId, SMSPLUGIN, Himall.Model.MemberContactsInfo.UserTypes.General);
                model.IsBindMobile = !string.IsNullOrEmpty(mobile);
            }
            var promoter = _iDistributionService.GetPromoterByUserId(curUserId);
            model.IsHavePostData = false;
            if (promoter != null)
            {
                model.ShopName = promoter.ShopName;
                model.Status = promoter.Status;
                model.IsHavePostData = true;
                if (promoter.Status == PromoterInfo.PromoterStatus.Refused)
                {
                    model.IsRefused = true;
                }

                switch (model.Status)
                {
                    case PromoterInfo.PromoterStatus.Audited:
                        return RedirectToAction("Index", "DistributionMarket");
                        break;
                    case PromoterInfo.PromoterStatus.NotAvailable:
                        return RedirectToAction("Performance");
                        break;
                    case PromoterInfo.PromoterStatus.UnAudit:
                        return RedirectToAction("WaitAudit");
                        break;
                }
            }
            else
            {
                model.RegionPath = string.Empty;//申请新销售员时，取消默认地址显示
            }

            //处理无必填自动完成
            ViewBag.ProductId = productId;
            return View(model);
        }

        private void CheckMobile(string mobile)
        {
            _iMemberService.CheckContactInfoHasBeenUsed(SMSPLUGIN, mobile);
        }

        [HttpPost]
        public ActionResult ApplyForDistributor(Model.PromoterModel model)
        {
            var promoter = _iDistributionService.GetPromoterByUserId(curUserId);


            if (promoter != null && (promoter.Status == PromoterInfo.PromoterStatus.NotAvailable || promoter.Status == PromoterInfo.PromoterStatus.Audited))
            {
                return Json(new Result() { success = false, msg = "你已经是销售员了！" });
            }
            var setting = _iDistributionService.GetRecruitmentSetting();
            if (setting == null)
            {
                return Json(new Result() { success = false, msg = "平台未设置招募审核！" });
            }
            if (setting.MustRealName && string.IsNullOrWhiteSpace(model.RealName))
            {
                return Json(new Result() { success = false, msg = "真实姓名必须填写！" });
            }

            if (setting.MustMobile && string.IsNullOrEmpty(model.Mobile))
            {
                var mobile = _iMessageService.GetDestination(curUserId, SMSPLUGIN, Himall.Model.MemberContactsInfo.UserTypes.General);
                if (string.IsNullOrEmpty(mobile))
                {
                    return Json(new Result() { success = false, msg = "手机必须必须填写！" });
                }
                model.Mobile = mobile;
            }
            if (setting.MustAddress)
            {
                if (model.RegionId == 0)
                {
                    return Json(new Result() { success = false, msg = "地址必须填写！" });
                }
                model.TopRegionId = Himall.Application.RegionApplication.GetRegion(model.RegionId, CommonModel.Region.RegionLevel.Province).Id;
            }
            model.UserId = curUserId;
            _iDistributionService.ApplyForDistributor(model);
            var result = new Result() { success = true, msg = "提交成功！", status = 1 };
            promoter = _iDistributionService.GetPromoterByUserId(curUserId);
            if (promoter != null)
            {
                if (promoter.Status == PromoterInfo.PromoterStatus.UnAudit)
                {
                    result = new Result() { success = true, msg = "提交成功！", status = 2 };
                }
            }
            return Json(result);
        }
        [HttpPost]
        public JsonResult SendCode(string destination)
        {
            CheckMobile(destination);
            string pluginId = SMSPLUGIN;
            var timeout = CacheKeyCollection.MemberPluginCheckTime(destination, pluginId);
            if (Core.Cache.Get(timeout) != null)
            {
                return Json(new Result() { success = false, msg = "120秒内只允许请求一次，请稍后重试!" });
            }
            var checkCode = new Random().Next(10000, 99999);
            var cacheTimeout = DateTime.Now.AddMinutes(15);
            string cachename = CacheKeyCollection.MemberPluginCheck(destination, pluginId);
            Core.Cache.Insert(cachename, checkCode, cacheTimeout);
#if DEBUG
            Core.Log.Debug("[MDCP]" + cachename + "__" + checkCode);
#endif
            var user = new MessageUserInfo() { UserName = "", SiteName = CurrentSiteSetting.SiteName, CheckCode = checkCode.ToString() };
            _iMessageService.SendMessageCode(destination, pluginId, user);
            string timecachename = CacheKeyCollection.MemberPluginCheckTime(destination, pluginId);
            Core.Cache.Insert(timecachename, checkCode, DateTime.Now.AddSeconds(120));
            return Json(new Result() { success = true, msg = "发送成功" });
        }

        [HttpPost]
        public JsonResult CheckCode(string code, string destination)
        {
            string pluginId = SMSPLUGIN;
            var cache = CacheKeyCollection.MemberPluginCheck(destination, pluginId);
            var cacheCode = Core.Cache.Get(cache);
            if (cacheCode != null && cacheCode.ToString() == code)
            {
                return Json(new Result() { success = true, msg = "验证正确" });
            }
            else
            {
                return Json(new Result() { success = false, msg = "验证码不正确或者已经超时" });
            }
        }

        public ActionResult BillDetail()
        {
            throw new HimallException("未启用");
            //return View();
        }

        public ActionResult ShareInfo()
        {
            var share = _iDistributionService.GetDistributionShare();
            if (share == null)
            {
                share = new DistributionShareSetting();
            }
            if (string.IsNullOrWhiteSpace(share.DisShareTitle))
            {
                share.DisShareTitle = CurrentSiteSetting.SiteName + "分销市场";
            }
            if (string.IsNullOrWhiteSpace(share.DisShareDesc))
            {
                share.DisShareDesc = "免费加盟代理，一键注册0成本当老板，让电商更简单！";
            }
            if (string.IsNullOrWhiteSpace(share.DisShareLogo))
            {
                share.DisShareLogo = CurrentSiteSetting.WXLogo;
            }
            return View(share);
        }

        /// <summary>
        /// 业绩账单
        /// </summary>
        /// <returns></returns>
        public ActionResult Billlist()
        {
            return View();
        }

        /// <summary>
        /// 获取业绩账单
        /// </summary>
        /// <returns></returns>
        public JsonResult GetBilllist(int page = 1, int? state = null)
        {
            List<DistributionFeatModel> result = new List<DistributionFeatModel>();
            DistributionUserBillQuery query = new DistributionUserBillQuery { UserId = curUserId, PageNo = page, PageSize = 6 };
            List<BrokerageIncomeInfo.BrokerageStatus> states = new List<BrokerageIncomeInfo.BrokerageStatus>();
            if (state != null)
            {
                switch (state)
                {
                    case 1:
                        states.Add(BrokerageIncomeInfo.BrokerageStatus.Settled);
                        break;
                    case 0:
                        states.Add(BrokerageIncomeInfo.BrokerageStatus.NotAvailable);
                        states.Add(BrokerageIncomeInfo.BrokerageStatus.NotSettled);
                        break;
                }
            }
            query.SettleState = states;
            var datasql = _iDistributionService.GetUserBillList(query);
            result = datasql.Models.ToList();
            var siteconfig = this.CurrentSiteSetting;
            int SalesRefundTimeout = siteconfig.SalesReturnTimeout;
            int NoReceivingTimeout = siteconfig.NoReceivingTimeout;
            //需要计算维权期
            foreach (var item in result)
            {
                if (SalesRefundTimeout < 0)
                {
                    SalesRefundTimeout = 0;
                }
                if (!item.LastRightsTime.HasValue)
                {
                    item.LastRightsTime = item.CreateTime;
                    item.LastRightsTime = item.LastRightsTime.Value.AddDays(SalesRefundTimeout);
                }
                item.LastRightsTime = item.LastRightsTime.Value.AddDays(SalesRefundTimeout);
            }
            return Json(result);
        }

        /// <summary>
        /// 分销业绩
        /// </summary>
        /// <returns></returns>
        public ActionResult Performance()
        {
            var _curuser = _iMemberService.GetMember(curUserId);
            var promoter = _curuser.Himall_Promoter.FirstOrDefault();
            CheckPromoter(promoter);
            DistributionUserPerformanceSetModel result = new DistributionUserPerformanceSetModel();
            result = _iDistributionService.GetUserPerformance(curUserId);
            ViewBag.NotAvailable = (promoter.Status == PromoterInfo.PromoterStatus.NotAvailable);
            bool isCanAgent = true;
            if (!_distributionsetting.Enable)
            {
                isCanAgent = false;
            }
            if (isCanAgent)
            {
                if (promoter.Status != PromoterInfo.PromoterStatus.Audited)
                {
                    isCanAgent = false;
                }
            }
            ViewBag.isCanAgent = isCanAgent;
            return View(result);
        }
        /// <summary>
        /// 检测销售员信息
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public void CheckPromoter(PromoterInfo info)
        {
            string result = "";
            if (info == null)
            {
                Response.Clear();
                Response.BufferOutput = true;
                result = @Url.Action("Apply");
                Response.Redirect(result);
                Response.End();
            }
            switch (info.Status)
            {
                case PromoterInfo.PromoterStatus.UnAudit:
                    result = @Url.Action("Apply");
                    break;
                case PromoterInfo.PromoterStatus.Refused:
                    result = @Url.Action("Apply");
                    break;
                case PromoterInfo.PromoterStatus.NotAvailable:
                    if (RouteData.Values["action"].ToString().ToLower() != "performance")
                    {
                        result = @Url.Action("Performance");
                    }
                    break;
            }
            if (!string.IsNullOrWhiteSpace(result))
            {
                Response.Clear();
                Response.BufferOutput = true;
                Response.Redirect(result);
                Response.End();
            }
        }

        public ActionResult ShopManagement(string skey)
        {
            DistributionShopShowModel model = new DistributionShopShowModel();

            var _curuser = _iMemberService.GetMember(curUserId);
            var promoter = _curuser.Himall_Promoter.FirstOrDefault();
            CheckPromoter(promoter);


            #region 二维码
            var curhttp = System.Web.HttpContext.Current;
            string url = curhttp.Request.Url.Scheme + "://" + curhttp.Request.Url.Authority; ;
            url = url + Url.Action("Promoter", "DistributionShop", new { id = curUserId });
            model.ShopQCodeUrl = url;
            var map = Core.Helper.QRCodeHelper.Create(url);
            //TODO:将二维码生成临时文件，lly
            //MemoryStream ms = new MemoryStream();
            //map.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            string fileName = "/temp/" + curUserId + DateTime.Now.ToString("yyMMddHHmmssffffff") + ".jpg";
            map.Save(Server.MapPath(fileName));
            //  将图片内存流转成base64,图片以DataURI形式显示  
            //string strUrl = "data:image/gif;base64," + Convert.ToBase64String(ms.ToArray());
            //ms.Dispose();
            map.Dispose();
            model.ShopQCode = fileName;
            #endregion

            model.ShopName = promoter.ShopName;
            model.SearchKey = skey;
            model.UserId = curUserId;
            return View(model);
        }
        /// <summary>
        /// 获取二维码
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public FileResult GetQCode(long id)
        {
            var curhttp = System.Web.HttpContext.Current;
            string url = curhttp.Request.Url.Scheme + "://" + curhttp.Request.Url.Authority; ;
            url = url + Url.Action("Promoter", "DistributionShop", new { id = id });
            var map = Core.Helper.QRCodeHelper.Create(url);
            MemoryStream ms = new MemoryStream();
            map.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            byte[] imgbits = ms.ToArray();
            ms.Dispose();
            return File(imgbits, "image/png");
        }
        /// <summary>
        /// 删除代理商品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DeleteAgentProduct(long id)
        {
            Result result = new Result { success = false, msg = "错误的商品编号" };
            if (id > 0)
            {
                List<long> ids = new List<long>();
                ids.Add(id);
                _iDistributionService.RemoveAgentProducts(ids, curUserId);
                result = new Result { success = true, msg = "移除代理成功" };
            }
            return Json(result);
        }
        /// <summary>
        /// 订单列表
        /// </summary>
        /// <param name="state">状态</param>
        /// <param name="stime">开始时间</param>
        /// <param name="etime">结束时间</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult OrderList(BrokerageIncomeInfo.BrokerageStatus? state = null, DateTime? stime = null, DateTime? etime = null)
        {
            Result result = new Result { success = false, msg = "未知错误" };

            return Json(result);
        }

        public ActionResult RateDetail()
        {
            var model = _iDistributionService.GetDistributionSetting();
            return View(model);
        }
        [HttpGet]
        public ActionResult InitRegion(string fromLatLng)
        {
            string address = string.Empty, province = string.Empty, city = string.Empty, district = string.Empty, street = string.Empty, newStreet = string.Empty;
            Common.ShopbranchHelper.GetAddressByLatLng(fromLatLng, ref address, ref province, ref city, ref district, ref street);
            if (district == "" && street != "")
            {
                district = street;
                street = "";
            }
            string fullPath = RegionApplication.GetAddress_Components(city, district, street, out newStreet);
            if (fullPath.Split(',').Length <= 3) newStreet = string.Empty;//如果无法匹配街道，则置为空
            return Json(new { fullPath = fullPath, showCity = string.Format("{0} {1} {2}", province, city, district), street = newStreet }, JsonRequestBehavior.AllowGet);
        }
    }
}