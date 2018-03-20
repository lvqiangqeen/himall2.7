using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Core.Plugins.Message;

namespace Himall.Model.WeiXin
{
    public class WXApplet_MsgTemplateLinkData
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
        private static List<WXApplet_MsgTemplateLinkData> DataList { get; set; }
        /// <summary>
        /// 静态构造
        /// </summary>
        static WXApplet_MsgTemplateLinkData()
        {
            DataList = new List<WXApplet_MsgTemplateLinkData>();
            WXApplet_MsgTemplateLinkData _tmp;

            #region 订单创建时  	待付款提醒
            _tmp = new WXApplet_MsgTemplateLinkData();
            _tmp.MsgType = MessageTypeEnum.OrderCreated;
            _tmp.MsgTemplateShortId = "AT0008";
            _tmp.ReturnUrl = "pages/orderdetails/orderdetails?orderid={id}";
            DataList.Add(_tmp);
            #endregion

            #region 订单付款时  订单支付成功通知
            _tmp = new WXApplet_MsgTemplateLinkData();
            _tmp.MsgType = MessageTypeEnum.OrderPay;
            _tmp.MsgTemplateShortId = "AT0009";
            _tmp.ReturnUrl = "pages/orderdetails/orderdetails?orderid={id}";
            DataList.Add(_tmp);
            #endregion

            #region 订单发货  订单发货提醒
            _tmp = new WXApplet_MsgTemplateLinkData();
            _tmp.MsgType = MessageTypeEnum.OrderShipping;
            _tmp.MsgTemplateShortId = "AT0007";
            _tmp.ReturnUrl = "pages/orderdetails/orderdetails?orderid={id}";
            DataList.Add(_tmp);
            #endregion

            #region 订单退款  退款通知
            _tmp = new WXApplet_MsgTemplateLinkData();
            _tmp.MsgType = MessageTypeEnum.OrderRefund;
            _tmp.MsgTemplateShortId = "AT0313";
            _tmp.ReturnUrl = "pages/returndetail/returndetail?orderid={id}";
            DataList.Add(_tmp);
            #endregion

            #region 售后发货
            //_tmp = new WXApplet_MsgTemplateLinkData();
            //_tmp.MsgType = MessageTypeEnum.RefundDeliver;
            //_tmp.MsgTemplateShortId = "OPENTM203847595";
            //DataList.Add(_tmp);
            #endregion

            #region 店铺有新订单
            //_tmp = new WXApplet_MsgTemplateLinkData();
            //_tmp.MsgType = MessageTypeEnum.ShopHaveNewOrder;
            //_tmp.MsgTemplateShortId = "OPENTM200750297";
            //DataList.Add(_tmp);
            #endregion

            #region 领取红包通知
            //_tmp = new WXApplet_MsgTemplateLinkData();
            //_tmp.MsgType = MessageTypeEnum.ReceiveBonus;
            //_tmp.MsgTemplateShortId = "TM00251";
            //_tmp.ReturnUrl = "/m-WeiXin/Member/Center";
            //DataList.Add(_tmp);
            #endregion

            #region 限时购通知
            //_tmp = new WXApplet_MsgTemplateLinkData();
            //_tmp.MsgType = MessageTypeEnum.LimitTimeBuy;
            //_tmp.MsgTemplateShortId = "OPENTM206903698";
            //_tmp.ReturnUrl = "/m-wap/limittimebuy/detail/{id}";
            //DataList.Add(_tmp);
            #endregion

            #region 订阅限时购
            //_tmp = new WXApplet_MsgTemplateLinkData();
            //_tmp.MsgType = MessageTypeEnum.SubscribeLimitTimeBuy;
            //_tmp.MsgTemplateShortId = "OPENTM201272994";
            //DataList.Add(_tmp);
            #endregion

            #region 拼团

            #region 开团成功
            //_tmp = new WXApplet_MsgTemplateLinkData();
            //_tmp.MsgType = MessageTypeEnum.FightGroupOpenSuccess;
            //_tmp.MsgTemplateShortId = "OPENTM400048565";
            //_tmp.ReturnUrl = "/m-WeiXin/MyFightGroup/GroupDetail/{gid}?aid={aid}";
            //DataList.Add(_tmp);
            #endregion

            #region 参团成功
            //_tmp = new WXApplet_MsgTemplateLinkData();
            //_tmp.MsgType = MessageTypeEnum.FightGroupJoinSuccess;
            //_tmp.MsgTemplateShortId = "OPENTM400048581";
            //_tmp.ReturnUrl = "/m-WeiXin/MyFightGroup/GroupDetail/{gid}?aid={aid}";
            //DataList.Add(_tmp);
            #endregion

            #region 有新成员参团
            //_tmp = new WXApplet_MsgTemplateLinkData();
            //_tmp.MsgType = MessageTypeEnum.FightGroupNewJoin;
            //_tmp.MsgTemplateShortId = "TM00712";
            //_tmp.ReturnUrl = "/m-WeiXin/MyFightGroup/GroupDetail/{gid}?aid={aid}";
            //DataList.Add(_tmp);
            #endregion

            #region 拼团失败
            //_tmp = new WXApplet_MsgTemplateLinkData();
            //_tmp.MsgType = MessageTypeEnum.FightGroupFailed;
            //_tmp.MsgTemplateShortId = "OPENTM400232755";
            //_tmp.ReturnUrl = "/m-WeiXin/FightGroup/Detail/{aid}";
            //DataList.Add(_tmp);
            #endregion

            #region 拼团成功
            //_tmp = new WXApplet_MsgTemplateLinkData();
            //_tmp.MsgType = MessageTypeEnum.FightGroupSuccess;
            //_tmp.MsgTemplateShortId = "OPENTM401153728";
            //_tmp.ReturnUrl = "/m-WeiXin/MyFightGroup/GroupDetail/{gid}?aid={aid}";
            //DataList.Add(_tmp);
            #endregion
            #endregion

        }
        /// <summary>
        /// 获取消息与微信短编号关联
        /// </summary>
        /// <returns></returns>
        public static List<WXApplet_MsgTemplateLinkData> GetList()
        {
            return DataList;
        }
        #endregion
    }
}
