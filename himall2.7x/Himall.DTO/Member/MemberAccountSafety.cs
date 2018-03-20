using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
    /// <summary>
    /// 会员邮箱、手机认证
    /// </summary>
    public class MemberAccountSafety
    {
        /// <summary>
        /// 会员ID
        /// </summary>
        public long UserId { get; set; }

        private string _Email = "";
        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { get { return _Email; } set { _Email = value; } }

        private bool _BindEmail = false;
        /// <summary>
        /// 邮件是否认证
        /// </summary>
        public bool BindEmail { get { return _BindEmail; } set { _BindEmail = value; } }

        private string _Phone = "";
        /// <summary>
        /// 手机号
        /// </summary>
        public string Phone { get { return _Phone; } set { _Phone = value; } }

        private bool _BindPhone = false;
        /// <summary>
        /// 电话是否认证
        /// </summary>
        public bool BindPhone { get { return _BindPhone; } set { _BindPhone = value; } }

    }
}
