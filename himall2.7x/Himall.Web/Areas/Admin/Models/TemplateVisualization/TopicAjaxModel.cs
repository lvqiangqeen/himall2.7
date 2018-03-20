using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.Admin.Models
{
    public class TopicAjaxModel
    {
        public int status { get; set; }
        public List<TopicContent> list { get; set; }
        public string page { get; set; }
    }


    public class TopicContent
    {
        public long item_id { get; set; }
        public string title { get; set; }
        public string create_time { get; set; }
        public string link { get; set; }
        public string pic { get; set; }
        public string tag { get; set; }
    }
}