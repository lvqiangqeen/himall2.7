using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
    /// <summary>
    /// (提现页面显示)提现帐户
    /// </summary>
    public class WithDrawAccount
    {
        /// <summary>
        /// 银行名
        /// </summary>
        public string BankName { set; get; }

        /// <summary>
        /// 开户姓名
        /// </summary>
        public string AccountName { set; get; }

        /// <summary>
        /// 开户支行完整名
        /// </summary>
        public string AccountBranch { set; get; }

        /// <summary>
        ///银行帐号
        /// </summary>
        public string CardNo { set; get; }

        /// <summary>
        /// 开户银行所在地
        /// </summary>
        public string BankAddress { set; get; }

        /// <summary>
        /// 微信真实姓名
        /// </summary>
        public string WeiChatName { set; get; }
  
    }
}
