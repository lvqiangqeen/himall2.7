using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.Admin.Models
{
    public class PhotoCategoryAjaxModel
    {
        public string status { get; set; }
        public photoCateNumber data { get; set; }
        public string msg { get; set; }
    }

    public class photoCateNumber
    {
        public string total { get; set; }
        public List<photoCateContent> tree { get; set; }
    }

    public class photoCateContent
    {
        public string name { get; set; }
        public int parent_id { get; set; }
        public string id { get; set; }
        public string picNum { get; set; }
    }
}