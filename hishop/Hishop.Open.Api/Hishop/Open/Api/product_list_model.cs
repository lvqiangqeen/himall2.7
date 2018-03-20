namespace Hishop.Open.Api
{
    using System;
    using System.Collections;
    using System.Runtime.CompilerServices;

    public class product_list_model
    {
        private ArrayList _picurl;

        public string approve_status { get; set; }

        public int brand_id { get; set; }

        public string brand_name { get; set; }

        public string cat_name { get; set; }

        public int cid { get; set; }

        public DateTime? list_time { get; set; }

        public DateTime? modified { get; set; }

        public int num { get; set; }

        public int num_iid { get; set; }

        public string outer_id { get; set; }

        public ArrayList pic_url
        {
            get
            {
                if (this._picurl == null)
                {
                    this._picurl = new ArrayList();
                }
                return this._picurl;
            }
            set
            {
                this._picurl = value;
            }
        }

        public decimal price { get; set; }

        public int sold_quantity { get; set; }

        public string title { get; set; }

        public int type_id { get; set; }

        public string type_name { get; set; }
    }
}

