using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Model;

namespace Himall.API.Model
{
    public class UserCenterGetIntegralRecordModel
    {
        public long Id { get; set; }
        public MemberIntegral.IntegralType TypeId { get; set; }
        public string ShowType { get; set; }
        public int Integral { get; set; }
        public Nullable<System.DateTime> RecordDate { get; set; }
        public string ReMark { get; set; }
    }
}
