namespace Hishop.Open.Api
{
    using System;
    using System.Runtime.CompilerServices;

    public class product_sku_model
    {
        public string outer_sku_id { get; set; }

        public decimal price { get; set; }

        public int quantity { get; set; }

        public string sku_id { get; set; }

        public string sku_properties_name { get; set; }
    }
}

