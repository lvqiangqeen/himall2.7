using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    public class FlashSaleConfigModel
    {
        public FlashSaleConfigModel()
        {

        }

        public FlashSaleConfigModel( int preheat , bool isNormalPurchase )
        {
            Preheat = preheat;
            IsNormalPurchase = isNormalPurchase;
        }

        public int Preheat { get; set; }

        public bool IsNormalPurchase { get; set; }
    }
}
