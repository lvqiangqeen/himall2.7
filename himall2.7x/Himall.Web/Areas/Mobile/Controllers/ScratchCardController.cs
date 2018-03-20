using Himall.IServices;
using Himall.Model;
using Himall.Web.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using Himall.DTO;
using Himall.CommonModel;
using System;
using Newtonsoft.Json;
using Senparc.Weixin.MP.CommonAPIs;
using Himall.Core;
using Himall.Web.App_Code.Common;
using Himall.Application;

namespace Himall.Web.Areas.Mobile.Controllers
{
    public class ScratchCardController : BaseMobileTemplatesController
    {
        private IBonusService _iBonusService;
        private IMemberIntegralService _iMemberIntegralService;
        private long curUserId;
        private WeiActivityWinModel activityWinModel;
        private int consumePoint;
        private SiteSettingsInfo _siteSetting;
        private ISiteSettingService _iSiteSettingService;

        public ScratchCardController(
 IBonusService iBonusService, IMemberIntegralService iMemberIntegralService,
            ISiteSettingService iSiteSettingService)
        {
            _iBonusService = iBonusService;
            _iMemberIntegralService = iMemberIntegralService;
            _iSiteSettingService = iSiteSettingService;
            this._siteSetting = _iSiteSettingService.GetSiteSettings();
            if (string.IsNullOrWhiteSpace(this._siteSetting.WeixinAppId) || string.IsNullOrWhiteSpace(this._siteSetting.WeixinAppSecret))
            {
                throw new HimallException("未配置公众号参数");
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

        public ActionResult Index(long id)
        {
            //TODO:改成统一的方式取 Token
            var token = AccessTokenContainer.TryGetToken(this._siteSetting.WeixinAppId, this._siteSetting.WeixinAppSecret);

            SceneHelper helper = new SceneHelper();
            var qrTicket = Senparc.Weixin.MP.AdvancedAPIs.QrCode.QrCodeApi.Create(token, 86400, 7758258).ticket;

            ViewBag.QRTicket = qrTicket;

            WeiActivityModel activityModel = WeiActivityApplication.GetActivityModel(id);
            activityModel.userId = curUserId;
            activityModel.winModel = AddWinInfo(activityModel);
            var integrals = _iMemberIntegralService.GetMemberIntegral(curUserId);//用户可用积分
            activityModel.winModel.integrals = integrals == null ? 0 : integrals.AvailableIntegrals;
            activityModel.participationCount = GetParticipationCount(activityModel);
            consumePoint = activityModel.consumePoint;

            return View(activityModel);
        }
        /// <summary>
        /// 计算用户可抽奖次数
        /// </summary>
        /// <param name="activityModel"></param>
        /// <returns></returns>
        public int GetParticipationCount(WeiActivityModel activityModel)
        {
            int participationCount = WeiActivityWinApplication.GetWinModel(activityModel);
            if (participationCount != -1)
            {
                return activityModel.participationCount - participationCount;
            }
            else
            {
                return participationCount;
            }
        }

        /// <summary>
        /// 获取中奖概率
        /// </summary>
        /// <param name="id"></param>
        /// <param name="awardId"></param>
        /// <returns></returns>
        public double GetCount(long id, long awardId)
        {
            var item = WeiActivityApplication.GetActivityModel(id);
            var proportions = 0.0;//抽取完成的概率总和
            float sum = 0;
            List<WeiActivityAwardModel> listModel = item.awards;
            var model = listModel.Where(p => p.Id == awardId).ToList();
            foreach (var t in item.awards)
            {
                bool falg = WeiActivityWinApplication.GetWinNumberByAward(id, t.Id);//查询当前奖等是否有剩余数量
                if (!falg)//无剩余数量
                {

                    proportions += t.proportion;
                    continue;
                }
                if (t.awardLevel < model[0].awardLevel)
                    sum += t.proportion;
            }

            var isOver = WeiActivityWinApplication.GetWinNumberByAward(id, awardId);//是否还有奖品
            if (!isOver)
            {
                return 0;
            }
            else
            {
                return proportions + sum + model[0].proportion;
            }

        }
        public WeiActivityWinModel AddWinInfo(WeiActivityModel activityModel)
        {
            activityWinModel = new WeiActivityWinModel();
            Random r = new Random();
            int num = r.Next(1, 101);//获取随机数做为中奖信息
            activityWinModel.activityId = activityModel.Id;
            activityWinModel.addDate = DateTime.Now;
            activityWinModel.userId = curUserId;

            activityWinModel.isWin = false;
            activityWinModel.awardId = 0;
            activityWinModel.awardName = "未中奖";
            activityWinModel.awardType = WeiActivityAwardType.Integral;
            // var activityModel=WeiActivityWinApplication.GetWinModel()
            foreach (var item in activityModel.awards)
            {
                //获取的随机数在该奖等区域类
                if (num <= GetCount(activityModel.Id, item.Id))
                {
                    if (item.awardType == WeiActivityAwardType.Integral)
                    {
                        activityWinModel.awardName = item.integral.ToString();
                        activityWinModel.awardId = item.Id;
                        activityWinModel.awardType = item.awardType;
                        activityWinModel.isWin = true;
                        return activityWinModel;
                    }
                    else if (item.awardType == WeiActivityAwardType.Bonus)
                    {
                        Model.BonusInfo bonusInfo = _iBonusService.Get(item.bonusId);
                        string Surplus = _iBonusService.GetBonusSurplus(item.bonusId);
                        if (bonusInfo.IsInvalid)//红包已经失效 返回 未中奖
                        {
                            break;
                        }
                        if (Convert.ToInt32(Surplus) <= 0)//当前红包已经领取完 设置未中奖
                        {
                            break;
                        }

                        //获取红包名称
                        activityWinModel.awardName = bonusInfo.Himall_BonusReceive.Where(p => p.IsShare == false && p.UserId == null).FirstOrDefault().Price.ToString() + "元红包";
                        activityWinModel.awardId = item.Id;
                        activityWinModel.awardType = item.awardType;
                        activityWinModel.bonusId = bonusInfo.Id;
                        activityWinModel.isWin = true;
                        return activityWinModel;
                    }
                    else if (item.awardType == WeiActivityAwardType.Coupon)
                    {
                        CouponModel couponModel = CouponApplication.Get(item.couponId);
                        int perMax = WeiActivityWinApplication.GetCouPonMax(activityWinModel.userId, activityWinModel.activityId, item.Id);

                        if (couponModel == null)
                        {
                            break;
                        }
                        if (couponModel.Num <= 0)//无库存 返回 未中奖
                        {
                            break;
                        }
                        if (couponModel.EndTime < DateTime.Now)//优惠券失效
                        {
                            break;
                        }
                        if (couponModel.perMax != 0 && perMax >= couponModel.perMax)
                        {
                            break;
                        }

                        //获取优惠券名称
                        string awardName = couponModel.CouponName;
                        if (couponModel.ShopName != "")
                        {
                            awardName = awardName + "(" + couponModel.ShopName + ")";
                        }
                        if (couponModel.OrderAmount != "")
                        {
                            activityWinModel.amount = "(" + couponModel.OrderAmount + ")";
                        }
                        activityWinModel.awardName = awardName;
                        activityWinModel.awardId = item.Id;
                        activityWinModel.awardType = item.awardType;
                        activityWinModel.couponId = couponModel.Id;
                        activityWinModel.isWin = true;
                        return activityWinModel;
                    }

                }
            }
            return activityWinModel;
        }
        public ActionResult Add(string productCommentsJSON)
        {
            //用于提交时 积分参数为抽奖需消耗积分
            var productComments = JsonConvert.DeserializeObject<WeiActivityWinModel>(productCommentsJSON);
            if (productComments != null)
            {
                WeiActivityModel activityModel = WeiActivityApplication.GetActivityModel(productComments.activityId);
                int participationCount = activityModel != null ? GetParticipationCount(activityModel) : 0;
                if (participationCount > 0)
                    WeiActivityWinApplication.WinnerSubmit(productComments);
            }

            return Json(new Result() { success = true, msg = "成功" });
        }

    }
}