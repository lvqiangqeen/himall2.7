using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    public partial class AttributeInfo
    {
        [NotMapped]
        public string AttrValue { get; set; }
    }
}
