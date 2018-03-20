using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.SellerAdmin.Models
{
    public class MenuManageModel
    {
        public long ID { get; set; }
        public string TopMenuName { get; set; }

        public IEnumerable<Himall.Model.MenuInfo> SubMenu { get; set; }

        public Nullable<MenuInfo.UrlTypes> LinkType { get; set; }

        public string URL { get; set; }
    }
}