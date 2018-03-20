using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
   public class UserIntegral
    {
        public long Id { get; set; }
        public Nullable<long> MemberId { get; set; }
        public string UserName { get; set; }
        public int HistoryIntegrals { get; set; }
        public int AvailableIntegrals { get; set; }
    }
}
