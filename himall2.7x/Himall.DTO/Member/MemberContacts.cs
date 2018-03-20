using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
    /// <summary>
    /// 邮件、短信认证
    /// </summary>
    public class MemberContacts
    {
        long _id;
        /// <summary>
        /// 主键ID
        /// </summary>
        public long Id { get { return _id; } set { _id = value; } }

        /// <summary>
        /// 会员ID
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 插件名称
        /// </summary>
        public string ServiceProvider { get; set; }
        /// <summary>
        /// 联系号码
        /// </summary>
        public string Contact { get; set; }
        /// <summary>
        /// 用户类型
        /// </summary>
        public Himall.Model.MemberContactsInfo.UserTypes UserType { get; set; }
    }
}
