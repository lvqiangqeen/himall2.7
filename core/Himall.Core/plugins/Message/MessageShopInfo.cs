using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Core.Plugins.Message
{
   public  class MessageShopInfo
    {
       public string UserName { set; get; }

       /// <summary>
       /// 店铺名称
       /// </summary>
       public string ShopName { set; get; }

       /// <summary>
       /// 店铺ID
       /// </summary>
       public long ShopId { set; get; }

       /// <summary>
       /// 商城名称
       /// </summary>
       public string SiteName { set; get; }

    }
}
