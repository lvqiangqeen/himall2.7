using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
    public  class MemberGrade
    {
        public  long Id { get; set; }
        public string GradeName { get; set; }
        public int Integral { get; set; }
        public string Remark { get; set; }
    }
}
