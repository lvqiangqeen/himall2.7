using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Himall.CommonModel
{
    public  class AppletShopInfo
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
        public int CompanyRegionId
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

        public string Distance
        { get; set; }

        public decimal DistanceMi
        { get; set; }

        public string OpeningTime
        {
            get;
            set;
        }

        public int FavoriteShopCount { get; set; }

        public int MsgCount { get; set; }

    }
}
