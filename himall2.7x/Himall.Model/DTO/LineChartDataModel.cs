using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    public class LineChartDataModel<T> where T : struct
    {
        
        /// <summary>
        /// 图表中X轴显示的标题序列
        /// </summary>
        public string[] XAxisData { get; set; }

        /// <summary>
        /// 需要显示的数据，可能有多个比较数据集
        /// </summary>
        public IList<ChartSeries<T>> SeriesData { get; set; }

        /// <summary>
        /// 通用的扩展属性
        /// </summary>
        public string[] ExpandProp { get; set; }
    }

    public class ChartSeries<T> where T : struct
    {
        /// <summary>
        /// 该数据集的名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 需要显示的数据序列
        /// </summary>
        public T[] Data { get; set; }
    }
}
