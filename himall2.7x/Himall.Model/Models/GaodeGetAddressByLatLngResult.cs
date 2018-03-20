using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    public class GaodeGetAddressByLatLngResult
    {
        public Int32 status { get; set; }
        public string info { get; set; }
        public Regeocode regeocode { get; set; }
    }
    public class Regeocode
    {
        public string formatted_address { get; set; }
        public AddressComponent addressComponent { get; set; }
    }

    public class AddressComponent
    {
        public string province { get; set; }

        public string city { get; set; }

        public string district { get; set; }

        public string township { get; set; }

        public Building building { get; set; }

        public Neighborhood neighborhood { get; set; }
    }
    public class Building
    {
        public string name { get; set; }
    }
    public class Neighborhood
    {
        public string name { get; set; }
    }
}
