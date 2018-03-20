using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.DTO;

namespace Himall.API.Model.ParamsModel
{
	public class SetBankAccountPostModel : BankAccount
	{
		/// <summary>
		/// 手机短信验证成功后的凭证
		/// </summary>
		public string Certificate { get; set; }
	}

	public class ApplyWithDrawSubmitPostModel
    {
		/// <summary>
		/// 手机短信验证成功后的凭证
		/// </summary>
		public string Certificate { get; set; }

        /// <summary>
        /// 提现金额
        /// </summary>
        public decimal Amount { get; set; }
    }
}
