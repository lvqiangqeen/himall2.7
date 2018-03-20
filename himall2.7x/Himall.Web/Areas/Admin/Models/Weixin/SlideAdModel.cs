using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.Admin.Models
{
    public class SlideAdModel
    {
        public long ID { get; set; }

        public string Description { get; set; }

        public string imgUrl { get; set; }

        public long DisplaySequence { get; set; }

        public string Url { get; set; }
    }
}