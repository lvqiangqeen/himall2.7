using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    public partial class FreightAreaContentInfo
    {
        public  FreightAreaContentInfo()
        {
            FreightAreaDetailInfo = new List<Model.FreightAreaDetailInfo>();
        }
       public List<FreightAreaDetailInfo> FreightAreaDetailInfo { set; get; }
    }
}
