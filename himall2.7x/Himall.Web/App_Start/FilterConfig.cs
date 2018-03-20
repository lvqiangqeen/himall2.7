using System.Web;
using System.Web.Mvc;
using Himall.Web.Framework;

namespace Himall.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new GZipAttribute());
            filters.Add(new HandleErrorAttribute());
           
        }
    }


}
