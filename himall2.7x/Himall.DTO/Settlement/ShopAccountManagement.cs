using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
    /// <summary>
    /// 店铺帐号管理
    /// </summary>
    public class ShopAccountManagement
    {
        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { set; get; }

        /// <summary>
        /// 手机
        /// </summary>
        public string Mobile { set; get;}

        /// <summary>
        /// 微信帐户
        /// </summary>
        public WeChatAccount WeiChatAccount { set; get; }

        /// <summary>
        /// 银行帐户
        /// </summary>
        public BankAccount BankAccount { set; get; }

    }

    /// <summary>
    /// 微信帐户
    /// </summary>
    public class WeChatAccount
    {

        /// <summary>
        /// 店铺Id
        /// </summary>
        public long ShopId { set; get; }
        /// <summary>
        /// 微信昵称
        /// </summary>
        public string WeiXinNickName { set; get; }

        /// <summary>
        /// 微信真实姓名
        /// </summary>
        public string  WeiXinRealName { set; get; }

        /// <summary>
        /// 微信OpenId
        /// </summary>
        public string WeiXinOpenId { set; get; }

        /// <summary>
        /// 姓别
        /// </summary>
        public string  Sex  { set; get; }

        /// <summary>
        /// 所在地
        /// </summary>
        public string Address { set; get; }

        /// <summary>
        /// 微信头像
        /// </summary>
        public string Logo { set; get; }

    }

    /// <summary>
    /// 银行帐户
    /// </summary>
    public class BankAccount
    {
        /// <summary>
        /// 店铺Id
        /// </summary>
        public long ShopId { set; get; }

        /// <summary>
        /// 银行名
        /// </summary>
        public string BankAccountName { set; get; }

        /// <summary>
        /// 开户姓名
        /// </summary>
        public string AccountName { set; get; }

        /// <summary>
        /// 开户支行完整名
        /// </summary>
        public string BankName { set; get; }

        /// <summary>
        /// 支行联行号
        /// </summary>
        public string BankCode { get; set; }

        /// <summary>
        ///银行帐号
        /// </summary>
        public string BankAccountNumber { set; get; }

        /// <summary>
        /// 开户行所在地
        /// </summary>
        public int BankRegionId { get; set; }

        /// <summary>
        /// 开户银行所在地
        /// </summary>
        public string BankAddress { set; get; }
        /// <summary>
        /// 开户银行许可证
        /// </summary>
        public string BankPhoto { set; get; }
    }
}
