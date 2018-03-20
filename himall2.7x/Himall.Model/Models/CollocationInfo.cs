using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class CollocationInfo : BaseModel
    {

        [NotMapped]
        public long ProductId { set; get; }

        [NotMapped]
        public string ShopName { set; get; }
        /// <summary>
        /// 组合购状态
        /// </summary>
        public string Status
        {
            get
            {

                if (this.EndTime < DateTime.Now)
                {
                    return "已结束";
                }
                else if (this.StartTime > DateTime.Now)
                {
                    return "未开始";
                }
                else
                {
                    return "进行中";
                }
            }
        }
    }
}