
namespace Himall.Web.Framework
{
    /// <summary>
    /// Cookie集合
    /// </summary>
    public class CookieKeysCollection
    {
        /// <summary>
        /// 平台管理员登录标识
        /// </summary>
        public const string PLATFORM_MANAGER = "Himall-PlatformManager";

        /// <summary>
        /// 商家管理员登录标识
        /// </summary>
        public const string SELLER_MANAGER = "Himall-SellerManager";

        /// <summary>
        /// 会员登录标识
        /// </summary>
        public const string HIMALL_USER = "Himall-User";
        /// <summary>
        /// 会员登录标识
        /// </summary>
        public const string HIMALL_ACTIVELOGOUT = "d783ea20966909ff";  //HIMALL_ACTIVELOGOUT做MD5后的16位字符
        /// <summary>
        /// 分销合作者编号
        /// </summary>
        public const string HIMALL_DISTRIBUTIONUSERLINKIDS = "d2cccb104922d434";   //HIMALL_DISTRIBUTIONUSERLINKIDS做MD5后的16位字符
        /// <summary>
        /// 不同平台用户key
        /// </summary>
        /// <param name="platform"></param>
        /// <returns></returns>
        public static string HIMALL_USER_KEY(string platform)
        {
            return string.Format("Himall-{0}User", platform);
        }
        /// <summary>
        /// 
        /// </summary>
        public const string HIMALL_USER_OpenID = "Himall-User_OpenId";
        /// <summary>
        /// 购物车
        /// </summary>
        public const string HIMALL_CART = "HIMALL-CART";

        /// <summary>
        /// 商品浏览记录
        /// </summary>
        public const string HIMALL_PRODUCT_BROWSING_HISTORY = "Himall_ProductBrowsingHistory";
        
        /// <summary>
        /// 最后产生访问时间
        /// </summary>
        public const string HIMALL_LASTOPERATETIME = "Himall_LastOpTime";

        /// <summary>
        /// 标识是平台还是商家公众号
        /// </summary>
        public const string MobileAppType = "Himall-Mobile-AppType";
        /// <summary>
        /// 访问的微店标识
        /// </summary>
        public const string HIMALL_VSHOPID = "Himall-VShopId";

		/// <summary>
		/// 用户角色(Admin)
		/// </summary>
		public const string USERROLE_ADMIN = "0";
		/// <summary>
		/// 用户角色(SellerAdmin)
		/// </summary>
		public const string USERROLE_SELLERADMIN = "1";
		/// <summary>
		/// 用户角色(User)
		/// </summary>
		public const string USERROLE_USER = "2";
    }
}