using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    public class ManagersGrade : BaseModel
    {
        long _id;
        public new long Id { get { return _id; } set { _id = value; base.Id = value; } }
        public string MGradeName { get; set; }
        public int Integral { get; set; }
        public string Remark { get; set; }
        public decimal Discount { get; set; }
        public int GradeType { get; set; }
    }
}
