using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Model;

namespace Himall.API.Model
{
    public class GiftsDetailModel: GiftModel
    {
        /// <summary>
        /// 是否可以购买
        /// </summary>
        public bool CanBuy { get; set; }
        /// <summary>
        /// 不可购买原因
        /// </summary>
        public string CanNotBuyDes { get; set; }
        public List<string> Images { get; set; }
    }
}
