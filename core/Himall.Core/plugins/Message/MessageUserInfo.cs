using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Core.Plugins.Message
{
   public class MessageUserInfo
    {
       public string UserName { set; get; }

       /// <summary>
       /// 验证码
       /// </summary>
       public string CheckCode { set; get; }


       /// <summary>
       /// 商城名称
       /// </summary>
       public string SiteName { set; get; }
    }
}
