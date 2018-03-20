using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Model;

namespace Himall.IServices
{
    public interface IWXCardService : IService
    {
        /// <summary>
        /// 添加卡券
        /// </summary>
        /// <param name="info"></param>
        bool Add(WXCardLogInfo info);
        /// <summary>
        /// 修改卡券限领
        /// </summary>
        /// <param name="num">null表示不限领取数量</param>
        /// <param name="codeid"></param>
        /// <param name="stock">库存数</param>
        void EditGetLimit(int? num, string cardid);
        /// <summary>
        /// 修改卡券限领
        /// </summary>
        /// <param name="num">null表示不限领取数量</param>
        /// <param name="id"></param>
        /// <param name="stock">库存数</param>
        void EditGetLimit(int? num, long id);
        /// <summary>
        /// 修改卡券库存
        /// </summary>
        /// <param name="num"></param>
        /// <param name="cardid"></param>
        void EditStock(int num, string cardid);
        /// <summary>
        /// 修改卡券库存
        /// </summary>
        /// <param name="num"></param>
        /// <param name="id"></param>
        void EditStock(int num, long id);
        /// <summary>
        /// 删除卡券
        /// </summary>
        /// <param name="cardid"></param>
        void Delete(string cardid);
        /// <summary>
        /// 删除卡券
        /// </summary>
        /// <param name="id"></param>
        void Delete(long id);
        /// <summary>
        /// 是否可以同步微信
        /// </summary>
        /// <param name="couponid"></param>
        /// <param name="couponcodeid"></param>
        /// <param name="couponType"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        WXSyncJSInfoByCard GetSyncWeiXin(long couponid, long couponcodeid, WXCardLogInfo.CouponTypeEnum couponType, string url);
        /// <summary>
        /// 获取同步微信JS
        /// </summary>
        /// <param name="couponcodeid"></param>
        /// <param name="couponType"></param>
        /// <returns></returns>
        WXJSCardModel GetJSWeiXinCard(long couponid, long couponcodeid, WXCardLogInfo.CouponTypeEnum couponType);
        /// <summary>
        /// 获取卡券领取地址
        /// </summary>
        /// <param name="cardid"></param>
        /// <param name="couponRecordId">红包记录号</param>
        /// <param name="couponType">红包类型</param>
        /// <returns></returns>
        string GetCardReceiveUrl(string cardid, long couponRecordId, WXCardLogInfo.CouponTypeEnum couponType);
        /// <summary>
        /// 获取卡券领取地址
        /// </summary>
        /// <param name="cardid"></param>
        /// <param name="couponRecordId">红包记录号</param>
        /// <param name="couponType">红包类型</param>
        /// <returns></returns>
        string GetCardReceiveUrl(long id, long couponRecordId, WXCardLogInfo.CouponTypeEnum couponType);
        /// <summary>
        /// 使用卡券
        /// <para>核销卡券</para>
        /// </summary>
        /// <param name="code"></param>
        /// <param name="cardid"></param>
        /// <returns></returns>
        void Consume(string cardid, string code);
        /// <summary>
        /// 使用卡券
        /// <para>核销卡券</para>
        /// </summary>
        /// <param name="id">投放记录编号</param>
        void Consume(long id);/// <summary>
        /// 使用卡券
        /// <para>核销卡券</para>
        /// </summary>
        /// <param name="couponcodeid">红包记录号</param>
        /// <param name="coupontype">红包类型</param>
        void Consume(long couponcodeid, WXCardLogInfo.CouponTypeEnum coupontype);
        /// <summary>
        /// 卡券Code失效
        /// </summary>
        /// <param name="code"></param>
        /// <param name="cardid"></param>
        void Unavailable(string cardid, string code);
        /// <summary>
        /// 卡券Code失效
        /// </summary>
        /// <param name="id">投放记录编号</param>
        void Unavailable(long id);

        #region 获取
        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        WXCardLogInfo Get(long id);
        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        WXCardLogInfo Get(string cardid);
        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        WXCardLogInfo Get(long couponId, WXCardLogInfo.CouponTypeEnum couponType);
        /// <summary>
        /// 获取领取记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        WXCardCodeLogInfo GetCodeInfo(long id);
        /// <summary>
        /// 获取领取记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        WXCardCodeLogInfo GetCodeInfo(string cardid,string code);
        /// <summary>
        /// 获取领取记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        WXCardCodeLogInfo GetCodeInfo(long couponCodeId, WXCardLogInfo.CouponTypeEnum couponType);
        #endregion

        /// <summary>
        /// 用户主动删除卡包内优惠券
        /// </summary>
        /// <param name="cardid"></param>
        /// <param name="code"></param>
        void Event_Unavailable(string cardid, string code);
        /// <summary>
        /// 投放卡券
        /// <para>由事件推送调用</para>
        /// </summary>
        /// <param name="cardid"></param>
        /// <param name="code"></param>
        void Event_Send(string cardid, string code, string openid, int outerid);
        /// <summary>
        /// 审核卡券
        /// <para>由事件推送调用</para>
        /// </summary>
        /// <param name="cardid"></param>
        /// <param name="auditstatus"></param>
        void Event_Audit(string cardid, WXCardLogInfo.AuditStatusEnum auditstatus);
        //修改卡券(暂不实现)
    }
}
