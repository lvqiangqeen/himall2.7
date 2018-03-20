using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Web.Framework
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
   
    ///不验证权限
    public class UnAuthorize : Attribute
    {
        public UnAuthorize()
        {

        }
    }
}

