using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web
{
    internal class ViewEngineResultCacheKey
    {
        public string ControllerName { get; private set; }
        public string ViewName { get; private set; }

        public ViewEngineResultCacheKey(string controllerName, string viewName)
        {
            this.ControllerName = controllerName ?? string.Empty;
            this.ViewName = viewName ?? string.Empty;
        }
        public override int GetHashCode()
        {
            return this.ControllerName.ToLower().GetHashCode() ^ this.ViewName.ToLower().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            ViewEngineResultCacheKey key = obj as ViewEngineResultCacheKey;
            if (null == key)
            {
                return false;
            }
            return key.GetHashCode() == this.GetHashCode();
        }
    }
}