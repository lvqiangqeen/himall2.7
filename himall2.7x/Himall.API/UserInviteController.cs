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
using Himall.Core;

namespace Himall.API
{
    public class UserInviteController : BaseApiController
    {
        public UserInviteController()
        {
        }
        public object Get()
        {
            CheckUserLogin();
            var userId = CurrentUser.Id;
            //var model = _iMemberInviteService.GetMemberInviteInfo(userId);
            var _iMemberInviteService = ServiceProvider.Instance<IMemberInviteService>.Create;
            var _iMemberIntegralService = ServiceProvider.Instance<IMemberIntegralService>.Create;
            var rule = _iMemberInviteService.GetInviteRule();
            var Integral = _iMemberIntegralService.GetIntegralChangeRule() ;
            string IntergralMoney = "0";
            if (Integral != null && Integral.IntegralPerMoney > 0)
            {
                IntergralMoney = (rule.InviteIntegral.Value / Integral.IntegralPerMoney).ToString("f2");
            }
            return Json(new { Success = true, InviteIntegral = rule.InviteIntegral, IntergralMoney = IntergralMoney, RegIntegral = rule.RegIntegral, ShareRule = rule.ShareRule });
        }
    }
}