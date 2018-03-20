namespace Hishop.Open.Api
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public class trade_list_model
    {
        private List<trade_itme_model> _orders;

        public string buyer_area { get; set; }

        public string buyer_email { get; set; }

        public string buyer_memo { get; set; }

        public string buyer_nick { get; set; }

        public string buyer_uname { get; set; }

        public string close_memo { get; set; }

        public DateTime? consign_time { get; set; }

        public DateTime? created { get; set; }

        public decimal discount_fee { get; set; }

        public DateTime? end_time { get; set; }

        public decimal invoice_fee { get; set; }

        public string invoice_title { get; set; }

        public DateTime? modified { get; set; }

        public List<trade_itme_model> orders
        {
            get
            {
                if (this._orders == null)
                {
                    this._orders = new List<trade_itme_model>();
                }
                return this._orders;
            }
            set
            {
                this._orders = value;
            }
        }

        public DateTime? pay_time { get; set; }

        public decimal payment { get; set; }

        public string receiver_address { get; set; }

        public string receiver_city { get; set; }

        public string receiver_district { get; set; }

        public string receiver_mobile { get; set; }

        public string receiver_name { get; set; }

        public string receiver_state { get; set; }

        public string receiver_town { get; set; }

        public string receiver_zip { get; set; }

        public string seller_flag { get; set; }

        public string seller_id { get; set; }

        public string seller_memo { get; set; }

        public string seller_mobile { get; set; }

        public string seller_name { get; set; }

        public string status { get; set; }

        public string storeId { get; set; }

        public string tid { get; set; }
    }
}

