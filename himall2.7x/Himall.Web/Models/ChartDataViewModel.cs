using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Himall.Model;

namespace Himall.Web.Models
{

    public class SeriesViewModel
    {
        public string Name { get; set; }
        public string SeriesData { get; set; }
    }
    public class ChartDataViewModel
    {
        public string xAxis { get; set; }
        public List<SeriesViewModel> SeriesData { get; set; }

        public ChartDataViewModel()
        {

        }
        public ChartDataViewModel(LineChartDataModel<int> chart)
            : this()
        {
            this.SeriesData = new List<SeriesViewModel>();
            foreach (var item in chart.XAxisData)
            {
                this.xAxis += ("'" + item + "',");
            }

            foreach (var item in chart.SeriesData)
            {
                var series = new SeriesViewModel { Name = item.Name, SeriesData = "" };
                foreach (var s in item.Data)
                {
                    series.SeriesData += (s.ToString() + ",");
                }
                this.SeriesData.Add(series);
            }

        }

        public ChartDataViewModel(LineChartDataModel<float> chart)
            : this()
        {
            this.SeriesData = new List<SeriesViewModel>();
            foreach (var item in chart.XAxisData)
            {
                this.xAxis += ("'" + item + "',");
            }

            foreach (var item in chart.SeriesData)
            {
                var series = new SeriesViewModel { Name = item.Name, SeriesData = "" };
                foreach (var s in item.Data)
                {
                    series.SeriesData += (s.ToString() + ",");
                }
                this.SeriesData.Add(series);
            }

        }
    }

}