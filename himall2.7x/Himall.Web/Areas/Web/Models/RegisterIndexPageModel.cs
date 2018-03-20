using Himall.Web.Areas.SellerAdmin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Himall.Model;

namespace Himall.Web.Areas.Web.Models
{
    public class RegisterIndexPageModel
    {
        public bool MobileVerifOpen { get; set; }
        public long Introducer { get; set; }
        public SiteSettingsInfo.RegisterTypes RegisterType { get; set; }
        public bool RegisterEmailRequired { get; set; }
        public bool EmailVerifOpen { get; set; }
    }

}