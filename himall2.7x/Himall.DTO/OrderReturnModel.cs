using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.ViewModel
{
    public class OrderReturnModel
    {
        public long[] Ids
        {
            get;
            set;
        }

        public bool Redirect
        {
            get;
            set;
        }
    }

    public class OrderReturnModel2
    {
        public IEnumerable<long> orderIds
        {
            get;
            set;
        }

        public bool success
        {
            get;
            set;
        }
    }
}

