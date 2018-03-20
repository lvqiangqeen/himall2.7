using Himall.IServices;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.Model;
using Himall.Web.Areas.Admin.Models;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class IntegralRuleController : BaseAdminController
    {
        private IMemberIntegralService _iMemberIntegralService;
        public IntegralRuleController(IMemberIntegralService iMemberIntegralService)
        {
            _iMemberIntegralService = iMemberIntegralService;
        }
        // GET: Admin/IntegralRule
        public ActionResult Management()
        {
            var model = _iMemberIntegralService.GetIntegralRule();
            IntegralRule rule = new IntegralRule();
            var bindWX = model.FirstOrDefault(a => a.TypeId == (int)MemberIntegral.IntegralType.BindWX);
            var reg = model.FirstOrDefault(a => a.TypeId == (int)MemberIntegral.IntegralType.Reg);
            var login = model.FirstOrDefault(a => a.TypeId == (int)MemberIntegral.IntegralType.Login);
            var comment = model.FirstOrDefault(a => a.TypeId == (int)MemberIntegral.IntegralType.Comment);
            var share = model.FirstOrDefault(a => a.TypeId == (int)MemberIntegral.IntegralType.Share);

            rule.BindWX = bindWX == null ? 0 : bindWX.Integral;
            rule.Reg = reg == null ? 0 : reg.Integral;
            rule.Comment = comment == null ? 0 : comment.Integral; ;
            rule.Login = login == null ? 0 : login.Integral;
            rule.Share = share == null ? 0 : share.Integral; ;
            var info = _iMemberIntegralService.GetIntegralChangeRule();
            if (info != null)
            {
                rule.MoneyPerIntegral = info.MoneyPerIntegral;
            }
            return View(rule);
        }

        [HttpPost]
        public JsonResult Management(IntegralRule rule)
        {
            List<MemberIntegralRule> rules = new List<MemberIntegralRule>();
            rules.Add(new MemberIntegralRule() { Integral = rule.Reg, TypeId = (int)MemberIntegral.IntegralType.Reg });
            rules.Add(new MemberIntegralRule() { Integral = rule.BindWX, TypeId = (int)MemberIntegral.IntegralType.BindWX });
            rules.Add(new MemberIntegralRule() { Integral = rule.Login, TypeId = (int)MemberIntegral.IntegralType.Login });
            rules.Add(new MemberIntegralRule() { Integral = rule.Comment, TypeId = (int)MemberIntegral.IntegralType.Comment });
            rules.Add(new MemberIntegralRule() { Integral = rule.Share, TypeId = (int)MemberIntegral.IntegralType.Share });
            _iMemberIntegralService.SetIntegralRule(rules);
            var info = _iMemberIntegralService.GetIntegralChangeRule();
            if (info != null)
            {
                info.MoneyPerIntegral = rule.MoneyPerIntegral;
            }
            else
            {
                info = new MemberIntegralExchangeRules();
                info.MoneyPerIntegral = rule.MoneyPerIntegral;
            }
            _iMemberIntegralService.SetIntegralChangeRule(info);
            return Json(new Result() { success = true, msg = "保存成功" });
        }

        public ActionResult Change()
        {
            var model = _iMemberIntegralService.GetIntegralChangeRule();
            return View(model);
        }

        [HttpPost]
        public JsonResult Change(int IntegralPerMoney)
        {
            var info = _iMemberIntegralService.GetIntegralChangeRule();
            if (info != null)
            {
                info.IntegralPerMoney = IntegralPerMoney;
            }
            else
            {
                info = new MemberIntegralExchangeRules();
                info.IntegralPerMoney = IntegralPerMoney;
            }
            _iMemberIntegralService.SetIntegralChangeRule(info);
            return Json(new Result() { success = true, msg = "保存成功" });
        }
    }
}