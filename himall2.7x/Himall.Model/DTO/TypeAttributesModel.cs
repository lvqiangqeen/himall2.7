using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Model
{
    public class TypeAttributesModel
    {
        public long AttrId { get; set; }
        public string Selected { get; set; }
        public bool IsMulti { get; set; }
        public string Name { get; set; }
        public List<TypeAttrValue> AttrValues { get; set; }
    }

    public class TypeAttrValue
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}