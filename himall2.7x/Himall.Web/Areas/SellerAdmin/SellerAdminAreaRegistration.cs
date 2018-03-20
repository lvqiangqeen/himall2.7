using Himall.Web.Framework;
using System.Web.Mvc;

namespace Himall.Web.Areas.SellerAdmin
{
    public class SellerAdminAreaRegistration : AreaRegistrationOrder 
    {
        public override string AreaName 
        {
            get 
            {
                return "SellerAdmin";
            }
        }

        public override void RegisterAreaOrder(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "SellerAdmin_default",
                "SellerAdmin/{controller}/{action}/{id}",
                new { controller = "home", action = "Index", id = UrlParameter.Optional }
            );
        }

        public override int Order
        {
            get { return 1; }
        }
    }
}