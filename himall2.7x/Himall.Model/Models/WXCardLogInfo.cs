using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Himall.Model
{
    public partial class WXCardLogInfo : BaseModel
    {
        /// <summary>
        /// 优惠券类型
        /// </summary>
        public enum CouponTypeEnum
        {
            /// <summary>
            /// 优惠券
            /// </summary>
            [Description("优惠券")]
            Coupon = 1,
            /// <summary>
            /// 商家红包
            /// </summary>
            [Description("商家红包")]
            Bonus = 2
        }
        /// <summary>
        /// 审核状态
        /// </summary>
        public enum AuditStatusEnum
        {
            /// <summary>
            /// 审核通过
            /// </summary>
            Audited = 1,
            /// <summary>
            /// 审核中
            /// </summary>
            Auditin = 0,
            /// <summary>
            /// 审核未通过
            /// </summary>
            AuditNot = -1
        }

        /// <summary>
        /// 微信卡券颜色表
        /// </summary>
        public static Hashtable WXCardColors
        {
            get
            {
                Hashtable result = new Hashtable();
                result.Add("Color010", "#63b359");
                result.Add("Color020", "#2c9f67");
                result.Add("Color030", "#509fc9");
                result.Add("Color040", "#5885cf");
                result.Add("Color050", "#9062c0");
                result.Add("Color060", "#d09a45");
                result.Add("Color070", "#e4b138");
                result.Add("Color080", "#ee903c");
                result.Add("Color081", "#f08500");
                result.Add("Color082", "#a9d92d");
                result.Add("Color090", "#dd6549");
                result.Add("Color100", "#cc463d");
                result.Add("Color101", "#cf3e36");
                result.Add("Color102", "#5E6671");
                return result;
            }
        }

        #region 传值参数(不入库)
        /// <summary>
        /// 店铺编号
        /// </summary>
        public long ShopId { get; set; }
        /// <summary>
        /// 店铺名
        /// </summary>
        public string ShopName { get; set; }
        /// <summary>
        /// 卡券库存数量(0表示最大)
        /// </summary>
        public int Quantity { get; set; }
        /// <summary>
        ///(停用) 每人限领数量
        /// <para>0表不限领</para>
        /// </summary>
        public int GetLimit { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime BeginTime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndTime { get; set; }
        /// <summary>
        /// (停用) 使用门槛
        /// <para>单位:分</para>
        /// </summary>
        public int LeastCost { get; set; }
        /// <summary>
        /// (停用) 面额
        /// <para>单位:分</para>
        /// </summary>
        public int ReduceCost { get; set; }
        /// <summary>
        /// 优惠详情
        /// </summary>
        public string DefaultDetail { get; set; }
        #endregion
    }
    /// <summary>
    /// 微信同步JSAPI信息(限卡券)
    /// </summary>
    public class WXSyncJSInfoByCard
    {
        public string appid { get; set; }
        public string apiticket { get; set; }
        public string timestamp { get; set; }
        public string nonceStr { get; set; }
        public string signature { get; set; }
    }
    /// <summary>
    /// 微信卡券同步JSAPI信息(卡券领取)
    /// </summary>
    public class WXSyncJSInfoCardInfo
    {
        public string card_id { get; set; }
        public string timestamp { get; set; }
        public string signature { get; set; }
        public string nonce_str { get; set; }
        public int outerid { get; set; }
    }

    /// <summary>
    /// 微信卡券前台信息
    /// </summary>
    public class WXJSCardModel
    {
        public WXJSCardModel()
        {
            cardExt = new WXJSCardExtModel();
        }
        public string cardId { get; set; }
        public WXJSCardExtModel cardExt { get; set; }
    }
    /// <summary>
    /// 微信卡券前台信息扩展
    /// </summary>
    public class WXJSCardExtModel
    {
        public string timestamp { get; set; }
        public string signature { get; set; }
        public string nonce_str { get; set; }
        public int outer_id { get; set; }
    }
}
