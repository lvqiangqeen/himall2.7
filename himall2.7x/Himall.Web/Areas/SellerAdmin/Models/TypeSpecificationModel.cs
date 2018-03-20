using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.SellerAdmin.Models
{

    public class SpecJosnModel
    {
        public List<TypeSpecificationModel> json { get; set; }
        public tableDataModel tableData { get; set; }
    }

    public class tableDataModel
    {
        public long productId { get; set; }
        public List<SKUSpecModel> cost { get; set; }
        public List<SKUSpecModel> stock { get; set; }
        public List<SKUSpecModel> sku { get; set; }
        public List<SKUSpecModel> mallPrice { get; set; }
    }

    [Serializable]
    public class SKUSpecModel
    {
        public string index { get; set; }
        public List<string> ValueSet { get; set; }
    }

    public class TypeSpecificationModel
    {
        public string Name { get; set; }
        public long SpecId { get; set; }
        public List<Specification> Values { get; set; }
    }

    public class Specification
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public bool Selected { get; set; }
        public bool isPlatform { get; set; }
    }
}