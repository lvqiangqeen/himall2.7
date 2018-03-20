using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    /// <summary>
    /// 限时购活动的基础设置
    /// </summary>
    public class LimitTimeBuySettingModel
    {
        public decimal Price { get; set; }
        public int ReviceDays { get; set; }
    }
}
