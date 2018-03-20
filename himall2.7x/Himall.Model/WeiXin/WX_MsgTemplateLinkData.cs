using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Core.Plugins.Message;


namespace Himall.Model.WeiXin
{
    /// <summary>
    /// 微信与消息类型绑定
    /// </summary>
    public class WX_MsgTemplateLinkData
    {
        /// <summary>
        /// 信息类型
        /// </summary>
        public MessageTypeEnum MsgType { get; set; }
        /// <summary>
        /// 模板短ID
        /// </summary>
        public string MsgTemplateShortId { get; set; }
        /// <summary>
        /// 微信点击跳回网址
        /// </summary>
        public string ReturnUrl { get; set; }

        #region 静态
        /// <summary>
        /// 数据列表
        /// </summary>
        private static List<WX_MsgTemplateLinkData> DataList { get; set; }
        /// <summary>
        /// 静态构造
        /// </summary>
        static WX_MsgTemplateLinkData()
        {
            DataList = new List<WX_MsgTemplateLinkData>();
            WX_MsgTemplateLinkData _tmp;

            #region 订单创建时
            _tmp = new WX_MsgTemplateLinkData();
            _tmp.MsgType = MessageTypeEnum.OrderCreated;
            _tmp.MsgTemplateShortId = "OPENTM207102467";
            _tmp.ReturnUrl = "/m-WeiXin/Order/Detail?id={id}";
            DataList.Add(_tmp);
            #endregion

            #region 订单付款时
            _tmp = new WX_MsgTemplateLinkData();
            _tmp.MsgType = MessageTypeEnum.OrderPay;
            _tmp.MsgTemplateShortId = "OPENTM207185188";
            _tmp.ReturnUrl = "/m-WeiXin/Order/Detail/{id}";
            DataList.Add(_tmp);
            #endregion

            #region 订单发货
            _tmp = new WX_MsgTemplateLinkData();
            _tmp.MsgType = MessageTypeEnum.OrderShipping;
            _tmp.MsgTemplateShortId = "OPENTM202243318";
            _tmp.ReturnUrl = "/m-WeiXin/Order/Detail/{id}";
            DataList.Add(_tmp);
            #endregion

            #region 订单退款
            _tmp = new WX_MsgTemplateLinkData();
            _tmp.MsgType = MessageTypeEnum.OrderRefund;
            _tmp.MsgTemplateShortId = "TM00430";
            _tmp.ReturnUrl = "/m-WeiXin/OrderRefund/RefundDetail/{id}";
            DataList.Add(_tmp);
            #endregion

            #region 售后发货
            _tmp = new WX_MsgTemplateLinkData();
            _tmp.MsgType = MessageTypeEnum.RefundDeliver;
            _tmp.MsgTemplateShortId = "OPENTM203847595";
            DataList.Add(_tmp);
            #endregion

            #region 店铺有新订单
            _tmp = new WX_MsgTemplateLinkData();
            _tmp.MsgType = MessageTypeEnum.ShopHaveNewOrder;
            _tmp.MsgTemplateShortId = "OPENTM200750297";
            DataList.Add(_tmp);
            #endregion

            #region 领取红包通知
            _tmp = new WX_MsgTemplateLinkData();
            _tmp.MsgType = MessageTypeEnum.ReceiveBonus;
            _tmp.MsgTemplateShortId = "TM00251";
            _tmp.ReturnUrl = "/m-WeiXin/Member/Center";
            DataList.Add(_tmp);
            #endregion

            #region 限时购通知
            _tmp = new WX_MsgTemplateLinkData();
            _tmp.MsgType = MessageTypeEnum.LimitTimeBuy;
            _tmp.MsgTemplateShortId = "OPENTM206903698";
            _tmp.ReturnUrl = "/m-wap/limittimebuy/detail/{id}";
            DataList.Add(_tmp);
            #endregion

            #region 订阅限时购
            _tmp = new WX_MsgTemplateLinkData();
            _tmp.MsgType = MessageTypeEnum.SubscribeLimitTimeBuy;
            _tmp.MsgTemplateShortId = "OPENTM201272994";
            DataList.Add(_tmp);
            #endregion

            #region 拼团

            #region 开团成功
            _tmp = new WX_MsgTemplateLinkData();
            _tmp.MsgType = MessageTypeEnum.FightGroupOpenSuccess;
            _tmp.MsgTemplateShortId = "OPENTM400048565";
            _tmp.ReturnUrl = "/m-WeiXin/MyFightGroup/GroupDetail/{gid}?aid={aid}";
            DataList.Add(_tmp);
            #endregion

            #region 参团成功
            _tmp = new WX_MsgTemplateLinkData();
            _tmp.MsgType = MessageTypeEnum.FightGroupJoinSuccess;
            _tmp.MsgTemplateShortId = "OPENTM400048581";
            _tmp.ReturnUrl = "/m-WeiXin/MyFightGroup/GroupDetail/{gid}?aid={aid}";
            DataList.Add(_tmp);
            #endregion

            #region 有新成员参团
            _tmp = new WX_MsgTemplateLinkData();
            _tmp.MsgType = MessageTypeEnum.FightGroupNewJoin;
            _tmp.MsgTemplateShortId = "TM00712";
            _tmp.ReturnUrl = "/m-WeiXin/MyFightGroup/GroupDetail/{gid}?aid={aid}";
            DataList.Add(_tmp);
            #endregion

            #region 拼团失败
            _tmp = new WX_MsgTemplateLinkData();
            _tmp.MsgType = MessageTypeEnum.FightGroupFailed;
            _tmp.MsgTemplateShortId = "OPENTM400232755";
            _tmp.ReturnUrl = "/m-WeiXin/FightGroup/Detail/{aid}";
            DataList.Add(_tmp);
            #endregion

            #region 拼团成功
            _tmp = new WX_MsgTemplateLinkData();
            _tmp.MsgType = MessageTypeEnum.FightGroupSuccess;
            _tmp.MsgTemplateShortId = "OPENTM401153728";
            _tmp.ReturnUrl = "/m-WeiXin/MyFightGroup/GroupDetail/{gid}?aid={aid}";
            DataList.Add(_tmp);
            #endregion
            #endregion

        }
        /// <summary>
        /// 获取消息与微信短编号关联
        /// </summary>
        /// <returns></returns>
        public static List<WX_MsgTemplateLinkData> GetList()
        {
            return DataList;
        }
        #endregion
    }
}
