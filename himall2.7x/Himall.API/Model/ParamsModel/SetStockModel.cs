using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.API.Model.ParamsModel
{
    /// <summary>
    /// 
    /// </summary>
    public class SetProductStockModel
    {
       public string pids {get;set;}
       public int stock {get;set;}
       public int optype { get; set; }
    }

    public class SetSkuStockModel
    {
        public string skus { get; set; }
        public string stock { get; set; }
        public int optype { get; set; }
    }
}
