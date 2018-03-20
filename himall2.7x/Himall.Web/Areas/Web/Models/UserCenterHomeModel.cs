using Himall.Web.Areas.SellerAdmin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Himall.Model;

namespace Himall.Web.Areas.Web.Models
{
    public class UserCenterHomeModel
    {
        public string UserName { get; set; }
        /// <summary>
        /// 头像
        /// </summary>
        public string Logo { get; set; }
        /// <summary>
        /// 购物车商品
        /// </summary>
        public ProductInfo[] ShoppingCartItems { get; set; }
        /// <summary>
        /// 未评论商品数量
        /// </summary>
        public int UnEvaluatProductsNum { get; set; }
        /// <summary>
        /// 用户未评论的3个商品
        /// </summary>
        public OrderItemInfo[] Top3UnEvaluatProducts { get; set; }
        /// <summary>
        /// 最近一次交易的卖家的推荐商品
        /// </summary>
        public ProductInfo[] Top3RecommendProducts { get; set; }
        public List<ProductBrowsedHistoryModel> BrowsingProducts { get; set; }

        public IEnumerable<PluginsInfo> BindContactInfo { get; set; }

        public int OrderCount { get; set; }
        /// <summary>
        /// 待收货
        /// </summary>
        public int OrderWaitReceiving { get; set; }
        /// <summary>
        /// 待付款
        /// </summary>
        public int OrderWaitPay { get; set; }
        /// <summary>
        /// 待评价
        /// </summary>
        public int OrderEvaluationStatus { get; set; }
        /// <summary>
        /// 资产
        /// </summary>
        public decimal Balance { get; set; }
        public MemberAccountSafety memberAccountSafety { get; set; }

        public UserCenterModel userCenterModel { get; set; }
    }

}