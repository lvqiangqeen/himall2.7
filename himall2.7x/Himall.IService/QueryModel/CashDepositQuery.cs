using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.IServices.QueryModel
{
    public partial class CashDepositQuery:QueryBase
    {
        public string ShopName { get; set; }

        /// <summary>
        /// 缴纳状态,true代表正常,false代表欠费
        /// </summary>
        public bool? Type { get; set; }
    }
}
