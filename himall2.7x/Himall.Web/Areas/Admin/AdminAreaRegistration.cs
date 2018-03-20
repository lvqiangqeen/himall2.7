using Himall.Web.Framework;
using System.Web.Mvc;

namespace Himall.Web.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistrationOrder 
    {
        public override string AreaName 
        {
            get 
            {
                return "Admin";
            }
        }

        public override void RegisterAreaOrder(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Admin_default",
                "Admin/{controller}/{action}/{id}",
                new { controller = "home", action = "Index", id = UrlParameter.Optional }
            );
        }

        public override int Order
        {
            get { return 0; }
        }
    }
}