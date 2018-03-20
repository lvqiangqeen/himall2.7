using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
   public class UserOrderCommentModel
    {

       public long OrderId { set; get; }
      
       
       /// <summary>
       /// 评论时间
       /// </summary>
       public DateTime CommentTime { set; get; }
    }
}
