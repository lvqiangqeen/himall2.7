using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel
{
    public  class AppletShopDetail
    {
               
        public string Id { get; set; }
      
        public string Logo { get; set; }        
      
        public string ShopName { get; set; }

        /// <summary>
        /// 是否关注 1 已关注，0 未关注
        /// </summary>
      
        public string IsSubscribe
        {
            get;
            set;
        }
 
        public string WelcomeTitle
        {
            get;
            set;
        }

      
        public string ContactsPhone
        {
            get;
            set;
        }

       
        public string CompanyAddress
        {
            get;
            set;
        }
        public string CompanyRegionId
        {
            get;
            set;
        }

        public string CompanyRegionAddress
        {
            get;
            set;
        }

        public string Lng
        {
            get;
            set;
        }

        
        public string Lat
        {
            get;
            set;
        }

        public decimal Distance
        { get; set; }


        public string OpeningTime
        {
            get;
            set;
        }
         /// <summary>
         /// 推荐商品
         /// </summary>
        public string RecommendProductInfo { get; set; }
        /// <summary>
        /// 自由拼团
        /// </summary>
        public string FreeProductInfo { get; set; }
    }
}
