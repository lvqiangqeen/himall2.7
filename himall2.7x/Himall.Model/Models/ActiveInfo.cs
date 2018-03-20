using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    public partial class ActiveInfo
    {
        /// <summary>
        /// 满减规则(需自行填充)
        /// </summary>
        /// 

        [NotMapped]
        public  List<FullDiscountRulesInfo> Rules { set; get; }

        /// <summary>
        /// 满减商品(需自行填充)
        /// </summary>

        [NotMapped]
        public List<ActiveProductInfo> Products { set; get; }
    }
}
