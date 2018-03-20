using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Himall.Core;
using System.ComponentModel;
using System.Reflection;
using Himall.Web.Framework;
using Himall.Model;

namespace Himall.Web.Framework
{
    public class ActionPermission
    {
        public string ActionName { set; get; }
        public string ControllerName { set; get; }
        public string Description { set; get; }
    }

    public static class AdminPermission
    {
        private readonly static Dictionary<AdminPrivilege, IEnumerable<ActionPermission>> privileges;
        private readonly static IEnumerable<ActionPermission> ActionPermissions;
        static AdminPermission()
        {
            ActionPermissions = GetAllActionByAssembly();
            privileges = new Dictionary<AdminPrivilege,IEnumerable<ActionPermission>>();
            var AdminPrivileges = PrivilegeHelper.GetPrivileges<AdminPrivilege>().Privilege.Select(a => a.Items);         
            foreach (var privilege in AdminPrivileges)
            {
                foreach (var item in privilege)
                {
                    List<ActionPermission> actions = new List<ActionPermission>();
                    var ctrls = item.Controllers;
                    foreach (var ctrl in ctrls)
                    {
                        foreach (string act in ctrl.ActionNames)
                        {
                            var acts = GetActionByControllerName(ctrl.ControllerName, act);
                            actions.AddRange(acts);
                        }
                    }
                    privileges.Add((AdminPrivilege)item.PrivilegeId, actions);
                }
            }

            //首页4
            //privileges.Add(AdminPrivilege.ConSole, GetActionByControllerName("home","console"));
            //privileges.Add(AdminPrivilege.CopyRight, GetActionByControllerName("home", "copyright"));
            //privileges.Add(AdminPrivilege.AboutUs, GetActionByControllerName("home","about"));
            //商品7
            //  privileges.Add(AdminPrivilege.ProductManage, GetActionByControllerName("product"));
            //  privileges.Add(AdminPrivilege.CategoryManage, GetActionByControllerName("category"));
            //  privileges.Add(AdminPrivilege.BrandManage, GetActionByControllerName("brand"));
            //  privileges.Add(AdminPrivilege.ProductTypeManage, GetActionByControllerName("producttype"));
            //  privileges.Add(AdminPrivilege.ConsultationManage, GetActionByControllerName("productconsultation"));
            //  privileges.Add(AdminPrivilege.CommentManage, GetActionByControllerName("ProductComment"));
            //  //交易5
            //  privileges.Add(AdminPrivilege.OrderManage, GetActionByControllerName("order"));
            //  privileges.Add(AdminPrivilege.ReturnManage, GetActionByControllerName("orderrefund"));
            //  privileges.Add(AdminPrivilege.OrderComment, GetActionByControllerName("ordercomment"));
            //  privileges.Add(AdminPrivilege.OrderComplaint, GetActionByControllerName("ordercomplaint"));
            //  privileges.Add(AdminPrivilege.PaymentManage, GetActionByControllerName("payment"));
            //  privileges.Add(AdminPrivilege.ExpressTemplate, GetActionByControllerName("ExpressTemplate"));
            //  //会员2
            //  privileges.Add(AdminPrivilege.MemberManage, GetActionByControllerName("member"));
            //  privileges.Add(AdminPrivilege.OpenIDManage, GetActionByControllerName("oauth"));
            //  //店铺3
            //  privileges.Add(AdminPrivilege.ShopManage, GetActionByControllerName("shop"));
            //  privileges.Add(AdminPrivilege.ShopPackage, GetActionByControllerName("shopgrade"));
            //  privileges.Add(AdminPrivilege.SettlementManage, GetActionByControllerName("account"));
            //  //统计4
            ////  privileges.Add(AdminPrivilege.TrafficStatistics, GetActionByControllerName("chart"));
            //  privileges.Add(AdminPrivilege.MemberStatistics, GetActionByControllerName("statistics", "member"));
            //  privileges.Add(AdminPrivilege.ShopStatistics, GetActionByControllerName("statistics", "newshop"));
            //  privileges.Add(AdminPrivilege.SalesAnalysis, GetActionByControllerName("statistics", "productsaleranking"));
            //  //网站3
            //  privileges.Add(AdminPrivilege.PageSetting, GetActionByControllerName("PageSettings"));
            //  privileges.Add(AdminPrivilege.AritcleManage, GetActionByControllerName("aritcle"));
            //  privileges.Add(AdminPrivilege.AritcleCategoryManage, GetActionByControllerName("aritclecategory"));
            //  //系统4
            //  privileges.Add(AdminPrivilege.SiteSetting, GetActionByControllerName("SiteSetting"));
            //  privileges.Add(AdminPrivilege.AdminManage, GetActionByControllerName("Manager"));
            //  privileges.Add(AdminPrivilege.PrivilegesManage, GetActionByControllerName("privilege"));
            //  privileges.Add(AdminPrivilege.OperationLog, GetActionByControllerName("OperationLog"));
            //  privileges.Add(AdminPrivilege.MessageSetting, GetActionByControllerName("Message"));

            //  /*营销*/
            //  privileges.Add(AdminPrivilege.LimitTimeBuy, GetActionByControllerName("LimitTimeBuy"));
            //  privileges.Add(AdminPrivilege.Coupon, GetActionByControllerName("Coupon"));

            //  /*微商城*/
            //  privileges.Add(AdminPrivilege.Vshop, GetActionByControllerName("WeiXin").Concat(GetActionByControllerName("Vshop")));

            //  /*专题*/
            //  privileges.Add(AdminPrivilege.MobileTopic, GetActionByControllerName("MobileTopic"));
            //  privileges.Add(AdminPrivilege.PCTopic, GetActionByControllerName("Topic"));
        }




        private static IEnumerable<ActionPermission> GetActionByControllerName(string controllername, string actionname = "")
        {
            return ActionPermissions.Where(item => item.ControllerName.ToLower() == controllername.ToLower() && (actionname == "" || item.ActionName.ToLower() == actionname.ToLower()));
        }

        public static Dictionary<AdminPrivilege, IEnumerable<ActionPermission>> Privileges { get { return privileges; } }


        private static IList<ActionPermission> GetAllActionByAssembly()
        {
            var result = new List<ActionPermission>();
            var types = Assembly.Load("Himall.Web").GetTypes().Where(a => a.BaseType != null && a.BaseType.Name == "BaseAdminController");

            foreach (var type in types)
            {
                var members = type.GetMethods();
                foreach (var member in members)
                {
                    if (member.ReturnType.Name == "ActionResult" || member.ReturnType.Name == "JsonResult")//如果是Action
                    {
                        var ap = new ActionPermission();

                        ap.ActionName = member.Name;
                        ap.ControllerName = member.DeclaringType.Name.Substring(0, member.DeclaringType.Name.Length - 10); // 去掉“Controller”后缀

                        object[] attrs = member.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), true);
                        if (attrs.Length > 0)
                            ap.Description = (attrs[0] as System.ComponentModel.DescriptionAttribute).Description;

                        result.Add(ap);
                    }
                }
            }
            return result;
        }


        /// <summary>
        /// 检查是否有权限访问该动作的授权
        /// </summary>
        /// <param name="controllerName"></param>
        /// <param name="actionName"></param>
        /// <returns></returns>
        public static bool CheckPermissions(List<AdminPrivilege> userprivileages, string controllerName, string actionName)
        {
            if (userprivileages.Contains(0))
                return true;

            return privileges.Where(a => userprivileages.Contains(a.Key)).Any(b => b.Value.Any(c => c.ControllerName.ToLower() == controllerName.ToLower() && c.ActionName.ToLower() == actionName.ToLower()));
        }

    }
}