using Himall.IServices;
using Himall.Model;
using Himall.Web.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using Himall.DTO;
using Himall.CommonModel;
using System;
using Senparc.Weixin.MP.CommonAPIs;
using Himall.Core;
using Himall.Web.App_Code.Common;
using Himall.Application;

namespace Himall.Web.Areas.Mobile.Controllers
{
    public class BigWheelController : BaseMobileTemplatesController
    {
        private IBonusService _iBonusService;
        private IMemberIntegralService _iMemberIntegralService;
        private long curUserId;
        private WeiActivityWinModel activityWinModel;
        private int consumePoint;
        private SiteSettingsInfo _siteSetting;
        private ISiteSettingService _iSiteSettingService;
        public BigWheelController( IBonusService iBonusService, IMemberIntegralService iMemberIntegralService,
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
            WeiActivityModel activityModel = WeiActivityApplication.GetActivityModelByBigWheel(id);
            activityModel.userId = curUserId;
            var integrals = _iMemberIntegralService.GetMemberIntegral(curUserId).AvailableIntegrals;//用户可用积分
            activityModel.participationCount = GetParticipationCount(activityModel);
            consumePoint = activityModel.consumePoint;

            ViewBag.Integrals = integrals;

            //TODO:改成统一的方式取 Token
            var token = AccessTokenContainer.TryGetToken(this._siteSetting.WeixinAppId, this._siteSetting.WeixinAppSecret);
            var qrTicket = Senparc.Weixin.MP.AdvancedAPIs.QrCode.QrCodeApi.Create(token, 86400, 7758259).ticket;
            ViewBag.QRTicket = qrTicket;

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
        public ActionResult Add(long id, long userId)
        {
            WeiActivityModel activityModel = WeiActivityApplication.GetActivityModel(id);
            activityWinModel = new WeiActivityWinModel();
            Random r = new Random();
            int num = r.Next(1, 100);//获取随机数做为中奖信息
            activityWinModel.activityId = id;
            activityWinModel.addDate = DateTime.Now;
            activityWinModel.userId = userId;
            activityWinModel.integrals = activityModel.consumePoint;
            //int activityNum = 0;

            activityWinModel.isWin = false;
            activityWinModel.awardId = 0;
            activityWinModel.awardName = "未中奖";
            foreach (var item in activityModel.awards)
            {
                //累加计算奖等区域
                //activityNum = activityNum + GetCount(activityModel.Id, item.Id);
                //获取的随机数在该奖等区域类
                if (num <= GetCount(activityModel.Id, item.Id))
                {
                    activityWinModel.isWin = true;

                    if (item.awardType == WeiActivityAwardType.Integral)
                    {
                        activityWinModel.awardName = item.integral.ToString();
                        activityWinModel.awardId = item.Id;
                        activityWinModel.awardType = item.awardType;
                        break;
                    }
                    else if (item.awardType == WeiActivityAwardType.Bonus)
                    {
                        Model.BonusInfo bonusInfo = _iBonusService.Get(item.bonusId);
                        if (bonusInfo == null)
                        {
                            //activityNum = 0;
                            activityWinModel.isWin = false;
                            break;
                        }
                        string Surplus = _iBonusService.GetBonusSurplus(item.bonusId);
                        if (bonusInfo.IsInvalid)//红包已经失效 返回 未中奖
                        {
                            //activityNum = 0;
                            activityWinModel.isWin = false;
                            break;
                        }
                        if (Convert.ToInt32(Surplus) <= 0)//当前红包已经领取完 设置未中奖
                        {
                            //activityNum = 0;
                            activityWinModel.isWin = false;
                            break;
                        }

                        //获取红包名称
                        activityWinModel.awardName = bonusInfo.Name;
                        activityWinModel.awardId = item.Id;
                        activityWinModel.awardType = item.awardType;
                        activityWinModel.bonusId = item.bonusId;
                        break;
                    }
                    else if (item.awardType == WeiActivityAwardType.Coupon)
                    {
                        CouponModel couponModel = CouponApplication.Get(item.couponId);
                        if (couponModel == null)
                        {
                            //activityNum = 0;
                            activityWinModel.isWin = false;
                            break;
                        }
                        int perMax = WeiActivityWinApplication.GetCouPonMax(activityWinModel.userId, activityWinModel.activityId, item.Id);
                        if (couponModel.Num <= 0)//优惠券无库存 返回 未中奖
                        {
                            //activityNum = 0;
                            activityWinModel.isWin = false;
                            break;
                        }
                        if (couponModel.perMax != 0 && perMax >= couponModel.perMax)
                        {
                            //activityNum = 0;
                            activityWinModel.isWin = false;
                            break;
                        }
                        if (couponModel.EndTime < DateTime.Now)//优惠券已经失效
                        {
                            //activityNum = 0;
                            activityWinModel.isWin = false;
                            break;
                        }

                        string awardName = couponModel.CouponName;
                        if (couponModel.ShopName != "")
                        {
                            awardName = awardName + "(" + couponModel.ShopName + ")";
                        }
                        if (couponModel.OrderAmount != "")
                        {
                            awardName = awardName + "(" + couponModel.OrderAmount + ")";
                        }
                        //获取红包名称
                        activityWinModel.awardName = awardName;
                        activityWinModel.awardId = item.Id;
                        activityWinModel.awardType = item.awardType;
                        activityWinModel.couponId = item.couponId;
                        break;
                    }


                }
            }

            WeiActivityWinApplication.WinnerSubmit(activityWinModel);
            decimal bonusPrice = 0;
            if (activityWinModel.bonusId > 0)
            {
                bonusPrice = _iBonusService.GetReceivePriceByUserId(activityWinModel.bonusId, userId);
            }

            return Json(new Result() { success = true, msg = activityWinModel.awardId.ToString() + "," + bonusPrice.ToString() });
        }

        public string GetCouponName(long id)
        {
            return CouponApplication.Get(id).CouponName;
        }


    }
}