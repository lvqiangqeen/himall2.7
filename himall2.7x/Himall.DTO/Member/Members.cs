using Himall.CommonModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
    /// <summary>
    /// 会员信息
    /// </summary>
    public class Members
    {
        /// <summary>
        /// 会员ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 真实姓名
        /// </summary>
        public string RealName { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 电话
        /// </summary>
        public string CellPhone { get; set; }

        /// <summary>
        /// QQ
        /// </summary>

        public string QQ { set; get; }

        /// <summary>
        /// 登陆用户名
        /// </summary>
        public string UserName { get; set; }

        ///// <summary>
        ///// 登陆密码
        ///// </summary>
        //public string Password { get; set; }

        ///// <summary>
        ///// 密码加密
        ///// </summary>
        //public string PasswordSalt { get; set; }

        /// <summary>
        /// 性别 1男  2女，默认0
        /// </summary>
        public SexType Sex { get; set; }

        /// <summary>
        /// 生日
        /// </summary>
        public Nullable<System.DateTime> BirthDay { get; set; }


        /// <summary>
        /// 是否被冻结
        /// </summary>
        public bool Disabled { get; set; }

        /// <summary>
        /// 最后登录时间
        /// </summary>
        public System.DateTime LastLoginDate { get; set; }

        /// <summary>
        /// 职业
        /// </summary>
        public string Occupation { set; get; }

   
        /// <summary>
        /// 总消费金额（不排除退款）
        /// </summary>
        public decimal TotalAmount { set; get; }


        /// <summary>
        /// 净消费金额（排除退款后的消费金额）
        /// </summary>
        public decimal NetAmount { set; get; }


        /// <summary>
        /// 注册日期
        /// </summary>
        public DateTime CreateDate { set; get; }

        public string CreateDateStr { get { return CreateDate.ToString("yyyy-MM-dd HH:mm:ss"); } }

        /// <summary>
        /// 会员等级名称
        /// </summary>
        public string GradeName { set; get; }

        /// <summary>
        /// 会员图像
        /// </summary>
        public string Photo { get; set; }
        /// <summary>
        /// 昵称
        /// </summary>
        public string Nick { get; set; }
        /// <summary>
        /// 会员的标签信息
        /// </summary>
        public IEnumerable<LabelModel> MemberLabels { get; set; }
        /// <summary>
        /// 推荐人
        /// </summary>
        public string InviteUserName { get; set; }

        /// <summary>
        /// 分销员ID
        /// </summary>
        public long ShareUserId { get; set; }
        /// <summary>
        /// 会员可用积分
        /// </summary>
        public long AvailableIntegral { get; set; }

        /// <summary>
        /// 会员历史积分
        /// </summary>
        public long HistoryIntegral { set; get; }

        /// <summary>
        /// 订单数
        /// </summary>
        public int OrderNumber { get; set; }

        /// <summary>
        /// 最后消费时间
        /// </summary>
        public DateTime? LastConsumptionTime { get; set; }
    }
}
