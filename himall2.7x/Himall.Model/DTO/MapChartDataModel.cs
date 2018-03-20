using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    public class MapChartDataModel
    {
        public int RangeMin { get; set; }
        public int RangeMax { get; set; }

        public MapChartSeries Series { get; set; }
    }

    public class MapChartSeries
    {
        /// <summary>
        /// 该数据集的名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 需要显示的数据序列
        /// </summary>
        public MapChartSeriesData[] Data { get; set; }
    }

    public class MapChartSeriesData
    {
        public string name { get; set; }
        public decimal value { get; set; }
    }
}
