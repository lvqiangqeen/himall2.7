using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    public class ComboDetail
    {
        [NotMapped]
        public int Id { get; set; }
        [NotMapped]
        public string ComboName { get; set; }
        [NotMapped]
        public int ComboQuantity { get; set; }
        [NotMapped]
        public decimal ComboPrice { get; set; }

        [NotMapped]
        public int GroupActiveId { get; set; }

    }
}
