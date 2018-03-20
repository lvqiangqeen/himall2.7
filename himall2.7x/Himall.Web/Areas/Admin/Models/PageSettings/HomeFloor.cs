using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.Admin.Models
{
    public class HomeFloor
    {
        public long StyleLevel
        {
            get;
            set;
        }

        public string Name { get; set; }

        public long DisplaySequence { get; set; }

        public bool Enable { get; set; }


        public long Id { get; set; }
    }
}