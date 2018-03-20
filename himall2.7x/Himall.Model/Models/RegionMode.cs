using System.Collections.Generic;

namespace Himall.Model
{
    /// <summary>
    ///  省
    /// </summary>
    public class ProvinceMode
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public IEnumerable<CityMode> City { get; set; }
    }

    /// <summary>
    /// 市
    /// </summary>
    public class CityMode
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public IEnumerable<CountyMode> County { get; set; }
    }

    /// <summary>
    /// 区
    /// </summary>
    public class CountyMode
    {
        public long Id { get; set; }

        public string Name { get; set; }
    }
}
