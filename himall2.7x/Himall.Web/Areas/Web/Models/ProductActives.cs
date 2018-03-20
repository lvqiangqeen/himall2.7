using Himall.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.Web.Models
{
    public class ProductActives
    {
       public ProductBonusLableModel ProductBonus { set; get; }

        /// <summary>
        /// 满额减
        /// </summary>
        public FullDiscountActive FullDiscount { set; get; }

        /// <summary>
        /// 满额免邮
        /// </summary>
        public decimal freeFreight { set; get; }
    }


  
}