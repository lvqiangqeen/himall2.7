using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.SmallProgAPI.Model
{
    public class StoreInfoModel
    {
        public string shopName { get; set; }
        public string industry { get; set; }
        public string companyPhone { get; set; }
        public decimal lng { get; set; }
        public decimal lat { get; set; }
        public string companyAddress { get; set; }
        public string openingTime { get; set; }
        public string businessLicenceNumberPhoto { get; set; }
        public string branchImage { get; set; }
        public string shopDescription { get; set; }
        public string contactsName { get; set; }
        public string contactsPhone { get; set; }
        public string contactsPosition { get; set; }
        public string password { get; set; }
    }

    public class StorePasswordInfoModel
    {
        public string password { get; set; }
    }
}
