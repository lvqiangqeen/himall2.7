using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    public class PaymentType
    {
        public PaymentType()
        {

        }

        public PaymentType( string id, string name)
        {
            Id = id;
            DisplayName = name;
        }

        public string Id { get; set; }

        public string DisplayName { get; set; }
    }
}
