using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Framework;
using System.Linq;
using System.Web.Mvc;
using Himall.Core;
using Himall.Web.Areas.Mobile.Models;
using Himall.Application;

namespace Himall.Web.Areas.Mobile.Controllers
{
    /// <summary>
    /// 签到控制器
    /// </summary>
    public class SignInController : BaseMobileMemberController
    {
        private IProductService _iProductService;
        private ITopicService _iTopicService;
        private IMemberSignInService _iMemberSignInService;
        private IMemberService _iMemberService;
        private SiteSignInConfigInfo signConfig;
        public SignInController(ITopicService iTopicService, IProductService iProductService,
            IMemberService iMemberService,
             IMemberSignInService iMemberSignInService)
        {
            _iProductService = iProductService;
            _iTopicService = iTopicService;
            _iMemberSignInService = iMemberSignInService;
            _iMemberService = iMemberService;
            signConfig = _iMemberSignInService.GetConfig();
        }

        public ActionResult Index()
        {
            return RedirectToAction("Detail");
        }

        private int SignIn()
        {
            int result = 0;
            long userid = CurrentUser.Id;
            result = _iMemberSignInService.SignIn(userid);
            return result;
        }

        public JsonResult Sign()
        {
            Result result = new Result { success = false, msg = "未知错误" };
            if (signConfig.IsEnable)
            {
                int signday = SignIn();
                if (signday > 0)
                {
                    string msg = "签到成功！<br>+" + signConfig.DayIntegral.ToString() + "分";
                    if(signConfig.DurationCycle>0 && signConfig.DurationReward>0)
                    {
                        if (signday >= signConfig.DurationCycle)
                        {
                            msg += "<br>并额外获得"+signConfig.DurationReward.ToString()+"分";
                        }
                        else
                        {
                            msg += "<br>再签到" + (signConfig.DurationCycle - signday).ToString() + "天奖" + signConfig.DurationReward.ToString() + "分";
                        }
                    }

                    result.success = true;
                    result.msg = msg;
                }
                else
                {
                    result.success = false;
                    result.msg = "签到失败，请不要重复签到！";
                }
            }
            else
            {
                result.success = false;
                result.msg = "签到失败，签到功能未开启！";
            }
            return Json(result);
        }

        /// <summary>
        /// 签到详情
        /// </summary>
        /// <returns></returns>
        public ActionResult Detail()
        {
            SignInDetailModel model = new SignInDetailModel();
            model.isCurSign = false;
            int signday = SignIn();
            if(signday>0)
            {
                model.isCurSign = true;
            }
            model.SignConfig = signConfig;
            var signinfo = _iMemberSignInService.GetSignInInfo(CurrentUser.Id);
            model.CurSignDurationDay = signinfo.DurationDay;
            model.CurSignDaySum = signinfo.SignDaySum;
            var member = _iMemberService.GetMember(CurrentUser.Id);
            model.UserInfo = member;
            var userInte = MemberIntegralApplication.GetMemberIntegral(member.Id);
            if (userInte != null)
            {
                model.MemberAvailableIntegrals = userInte.AvailableIntegrals;
            }
            return View(model);
        }
    }
}