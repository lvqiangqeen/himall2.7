using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model.Models
{
    public class FlashSaleRedisInfo
    {
        /// <summary>
        /// 库存标识
        /// </summary>
        public string SkuId
        {
            get; set;
        }

        /// <summary>
        /// 限时构ID
        /// </summary>
        public long FlashSaleId
        {
            get; set;
        }

        /// <summary>
        /// 库存数量
        /// </summary>
        public long Stock
        {
            get; set;
        }

        /// <summary>
        /// 限时购开始时间
        /// </summary>
        public DateTime BeginDate
        {
            get; set;
        }

        /// <summary>
        /// 限时购结束时间
        /// </summary>
        public DateTime EndDate
        {
            get; set;
        }
    }
}
