using Himall.IServices;
using Himall.Model;
using System.Collections.Generic;
using System.Web;

namespace Himall.Web.Framework
{
    public static class ServiceHelper
    {
        public static T Create<T>() where T : IService
        {

            T t = Himall.ServiceProvider.Instance<T>.Create;
            if (HttpContext.Current != null&&HttpContext.Current.Session!=null)
            {
                List<IService> ts = HttpContext.Current.Session["_serviceInstace"] as List<IService>;
                if (ts == null)
                {
                    ts = new List<IService>() { t };
                }
                else
                    ts.Add(t);
                HttpContext.Current.Session["_serviceInstace"] = ts;
            }
            return t;
        }       
    }
}
