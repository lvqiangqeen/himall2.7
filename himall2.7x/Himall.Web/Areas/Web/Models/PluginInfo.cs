using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.Web.Models
{
    public class PluginsInfo
    {
        public string ShortName { get; set; }
        public string PluginId { get; set; }

        public bool Enable { get; set; }

        public bool IsBind { set; get; }

        public bool IsSettingsValid { set; get; }
    }
}