using Himall.Web.Framework;
using System.Web.Mvc;

namespace Himall.Web.Areas.Web
{
    public class WebAreaRegistration : AreaRegistrationOrder 
    {
        public override string AreaName 
        {
            get 
            {
                return "Web";
            }
        }

        public override void RegisterAreaOrder(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Web_default",
                "{controller}/{action}/{id}",
                new { controller = "home", action = "Index", id = UrlParameter.Optional }
            );
        }

        public override int Order
        {
            get { return 999; }
        }
    }
}