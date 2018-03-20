using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinOrderProductStatisticsService
{
    /// <summary>
    /// 统计数据入口 
    /// </summary>
    public interface ISyncData
    {
        /// <summary>
        /// 统计数据接口
        /// </summary>
        void Execute();
    }
}
