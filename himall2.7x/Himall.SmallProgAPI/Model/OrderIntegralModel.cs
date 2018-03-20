using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.SmallProgAPI.Model
{
    public class OrderIntegralModel
    {
        public OrderIntegralModel()
        {
            IntegralPerMoney = 0;
            UserIntegrals = 0;
        }

        public decimal IntegralPerMoney { get; set; }

        public decimal UserIntegrals { get; set; }
    }
}
