using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    /// <summary>
    /// IP数据模型
    /// </summary>
    public class IPDataModel
    {
        public string Address { get; set; }
        public string Area { get; set; }
        public string City { get; set; }
        public string CityCountry { get; set; }
        public string Country { get; set; }
        public string FullAddress { get; set; }
        public string IPAddress { get; set; }
        public string ISP { get; set; }
        public string Province { get; set; }

    }
}
