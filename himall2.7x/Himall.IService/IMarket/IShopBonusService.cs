using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.IServices.QueryModel;

namespace Himall.IServices
{
    public interface IShopBonusService : IService
    {
        /// <summary>
        ///  获取红包列表
        /// </summary>
        ObsoletePageModel<ShopBonusInfo> Get( long shopid , string name , int state , int pageIndex , int pageSize );

        List<ShopBonusReceiveInfo> GetCanUseDetailByUserId( long userid );
        List<ShopBonusReceiveInfo> GetDetailByUserId( long userid );
        ShopBonusReceiveInfo GetDetailById( long userid , long id );
        List<ShopBonusReceiveInfo> GetDetailToUse( long shopid , long userid , decimal sumprice );
        ObsoletePageModel<ShopBonusReceiveInfo> GetDetailByQuery( CouponRecordQuery query );
        ShopBonusGrantInfo GetByOrderId( long orderid );
        decimal GetUsedPrice( long orderid , long userid );

        ShopBonusGrantInfo GetGrantByUserOrder( long orderid , long userid );

        /// <summary>
        /// 获取红包
        /// </summary>
        ShopBonusInfo Get( long id );

        /// <summary>
        /// 根据grantid获取
        /// </summary>
        ShopBonusInfo GetByGrantId( long grantid );

        long GetGrantIdByOrderId( long orderid );

        void SetBonusToUsed( long userid , List<OrderInfo> orders , long rid );

        /// <summary>
        /// 检查是否能添加
        /// </summary>
        bool IsAdd( long shopid );

        /// <summary>
        ///  获取红包详情
        /// </summary> 
        ObsoletePageModel<ShopBonusReceiveInfo> GetDetail( long bonusid , int pageIndex , int pageSize );

        List<ShopBonusReceiveInfo> GetDetailByGrantId( long grantid );

        /// <summary>
        ///  添加红包 
        /// </summary>
        void Add( ShopBonusInfo model , long shopid );

        /// <summary>
        ///  修改红包
        /// </summary>
        void Update( ShopBonusInfo model );

        /// <summary>
        ///  失效
        /// </summary>
        void Invalid( long id );

        /// <summary>
        /// 领取
        /// </summary>
        object Receive( long grantid , string openId , string wxhead , string wxname );

        /// <summary>
        /// 新增红包时判断是否超出服务费用结束日期
        /// </summary>
        /// <returns></returns>
        bool IsOverDate( DateTime bonusDateEnd , DateTime dateEnd , long shopid );

        ActiveMarketServiceInfo GetShopBonusService( long shopId );

        /// <summary>
        /// 订单支付完成时，生成红包详情
        /// </summary>
        long GenerateBonusDetail( ShopBonusInfo model  , long orderid , string receiveurl );

        ShopBonusInfo GetByShopId( long shopid );
    }
}
