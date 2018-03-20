using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.IServices.QueryModel
{
    public partial class ConsultationQuery : QueryBase
    {
      public bool?IsReply{set;get;}

       //店铺ID
      public long ShopID { set; get; }
       //关键字
      public string KeyWords { set; get; }

       //用户ID
      public long UserID { set; get; }

       //产品ID
      public long ProductID { set; get; }
    }
}
