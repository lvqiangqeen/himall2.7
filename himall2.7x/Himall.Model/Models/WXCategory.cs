using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model.Models
{
    public class WXCategory
    {
        public WXCategory()
        {
            Child = new List<WXCategory>();
        }

        public string Id
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public List<WXCategory> Child
        {
            get;
            set;
        }
    }
}
