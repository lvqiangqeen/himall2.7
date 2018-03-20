using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model.Models
{
    public class OrderCommentInfo
    {
        public long Id
        {
            get;
            set;
        }

        public long OrderId
        {
            get;
            set;
        }

        public string ReviewContent
        {
            get;
            set;
        }

        public DateTime ReviewDate
        {
            get;
            set;
        }
    }
}
