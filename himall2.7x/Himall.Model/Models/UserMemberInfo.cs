using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Himall.Model
{
    public partial class UserMemberInfo : BaseModel
    {
        /// <summary>
        /// 头像
        /// </summary>
        [NotMapped]
        public string Photo
        {
            get { return ImageServerUrl + photo; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(ImageServerUrl))
                    photo = value.Replace(ImageServerUrl, "");
                else
                    photo = value;
            }
        }
        /// <summary>
        /// 显示昵称
        /// <para>无昵称则显示用户名</para>
        /// </summary>
        public string ShowNick
        {
            get
            {
                string result = this.Nick;
                if (string.IsNullOrWhiteSpace(result))
                {
                    result = this.UserName;
                }
                return result;
            }
        }

        /// <summary>
        /// 是否是微信用户
        /// </summary>
        public bool IsWeiXinUser
        {
            get
            {
                return !string.IsNullOrEmpty(this.PasswordSalt) && this.PasswordSalt.StartsWith("o");
            }
        }
        /// <summary>
        /// 会员折扣(0.00-1)
        /// </summary>
        public decimal MemberDiscount { get; set; }


        public string Hobby { get; set; }
    }

}
