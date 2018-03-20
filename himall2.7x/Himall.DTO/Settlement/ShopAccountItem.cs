using Himall.CommonModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Core;

namespace Himall.DTO
{
	/// <summary>
	/// 店铺收支详情
	/// </summary>
	public class ShopAccountItem
	{
		/// <summary>
		/// 详情ID
		/// </summary>
		public long Id { set; get; }

		/// <summary>
		/// 店铺ID
		/// </summary>
		public long ShopId { set; get; }

		public string ShopName { get; set; }

		/// <summary>
		/// 时间
		/// </summary>
		public DateTime CreateTime { set; get; }

		/// <summary>
		/// 资金流水号
		/// </summary>
		public string AccountNo { set; get; }

		/// <summary>
		/// 类型
		/// </summary>
		public string AccountType { get { return ShopAccountType.ToDescription(); } }

		/// <summary>
		/// 类型枚举对象
		/// </summary>
		public ShopAccountType ShopAccountType { set; get; }

		/// <summary>
		/// 余额
		/// </summary>
		public decimal Balance { set; get; }

		/// <summary>
		/// 详情ID
		/// </summary>
		public string DetailId { set; get; }

		/// <summary>
		/// 详情链接
		/// </summary>
		public string DetailLink
		{
			get
			{
				var detailLink = string.Empty;
				switch (this.ShopAccountType)
				{
					case ShopAccountType.MarketingServices:
						detailLink = "/SellerAdmin/billing/MarketServiceRecordInfo/" + this.DetailId;
						break;
					case ShopAccountType.PlatCommissionRefund:
					case ShopAccountType.DistributorCommissionRefund:
					case ShopAccountType.Refund:
						detailLink = "/SellerAdmin/orderrefund/management?orderid=" + this.DetailId;
						break;
					case ShopAccountType.Recharge:
						detailLink = "";
						break;
					case ShopAccountType.SettlementIncome:
						detailLink = "/SellerAdmin/Billing/SettlementOrders?detailId=" + this.DetailId;
						break;
					case ShopAccountType.WithDraw:
						detailLink = "/SellerAdmin/ShopAccount/Management?id=" + this.DetailId;
						break;
				}
				return detailLink;
			}
		}

		public bool IsIncome { get; set; }

		public decimal Amount { get; set; }

		/// <summary>
		/// 收入
		/// </summary>
		public string Income
		{
			get
			{
				if (IsIncome)
					return Amount.ToString();
				return string.Empty;
			}
		}

		/// <summary>
		/// 支出
		/// </summary>
		public string Expenditure
		{
			get
			{
				if (!IsIncome)
					return Amount.ToString();
				return string.Empty;
			}
		}

		public string ReMark { get; set; }

		public long AccoutID { get; set; }

		/// <summary>
		/// 结算周期(以天为单位)
		/// </summary>
		public int SettlementCycle { get; set; }
	}

}
