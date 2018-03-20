using System.ComponentModel.DataAnnotations.Schema;

namespace Himall.Model
{
    public partial class ShippingAddressInfo
    {
        [NotMapped]
        public string RegionFullName { get; set; }

        [NotMapped]
        public string RegionIdPath { set; get; }

    }
}
