using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.API.Model
{
    public class MemberOrderGetStatusModel
    {
        public long orderId { get; set; }
        public int status { get; set; }
        public long activeId { get; set; }
        public long groupId { get; set; }
    }
}
