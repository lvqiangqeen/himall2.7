using System.Collections.Generic;
using System.Linq;
using Himall.Model;
using Himall.IServices.QueryModel;
using System;
using Himall.Model.Models;

namespace Himall.IServices
{
    /// <summary>
    /// 限时购服务
    /// </summary>
    public interface ILimitTimeBuyService : IService
    {
        #region 平台

        FlashSaleConfigModel GetConfig();

        void UpdateConfig( FlashSaleConfigModel data );

        void AuditItem( long Id , LimitTimeMarketInfo.LimitTimeMarketAuditStatus status , string message );

        /// <summary>
        /// 更新限时购服务设置
        /// </summary>
        void UpdateServiceSetting( LimitTimeBuySettingModel model );

        /// <summary>
        /// 获取限时购服务设置
        /// </summary>
        /// <returns></returns>
        LimitTimeBuySettingModel GetServiceSetting();



        /// <summary>
        /// 更新限时购服务分类
        /// </summary>
        /// <param name="categoryId"></param>
        void AddServiceCategory( string categoryName );

        /// <summary>
        /// 删除一个限时购服务分类
        /// </summary>
        /// <param name="categoryId"></param>
        void DeleteServiceCategory( string categoryName );

        /// <summary>
        /// 获取限时购服务分类
        /// </summary>
        /// <returns></returns>
        string[] GetServiceCategories();


        /// <summary>
        /// 获取指定营销类型服务的已购买商家列表
        /// </summary>
        /// <param name="MarketBoughtQuery">营销查询对象</param>
        /// <returns></returns>
        ObsoletePageModel<ActiveMarketServiceInfo> GetBoughtShopList( MarketBoughtQuery query );

        /// <summary>
        /// 获取参加限时购的所有活动商品列表
        /// </summary>
        /// <param name="query">限时购活动查询对象</param>
        /// <returns></returns>
        ObsoletePageModel<LimitTimeMarketInfo> GetItemList( LimitTimeQuery query );

        #endregion


        #region 商家


        /// <summary>
        /// 添加一个限时购活动
        /// </summary>
        /// <param name="model">限时购对象</param>
        void AddLimitTimeItem( LimitTimeMarketInfo model );

        void UpdateLimitTimeItem( LimitTimeMarketInfo model );

        /// <summary>
        /// 根据店铺Id获取该店铺购买的限时购营销服务信息
        /// </summary>
        /// <param name="shopId">店铺Id</param>
        /// <returns></returns>
        ActiveMarketServiceInfo GetMarketService( long shopId );

        /// <summary>
        /// 为指定的店铺开通限时购服务
        /// </summary>
        /// <param name="monthCount">时长（以月为单位）</param>
        /// <param name="shopId">店铺Id</param>
        void EnableMarketService( int monthCount , long shopId );


        #endregion


        #region 前台

        /// <summary>
        /// 获取一个限时购的详细信息
        /// </summary>
        /// <param name="id">限时购活动Id</param>
        /// <returns></returns>
        LimitTimeMarketInfo GetLimitTimeMarketItem( long id );


        /// <summary>
        ///  根据商品Id获取一个限时购的详细信息
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        FlashSaleInfo GetLimitTimeMarketItemByProductId( long pid );

        /// <summary>
        /// 判断商品是否正在做限时购
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool IsLimitTimeMarketItem( long id );

        #endregion

        #region 获取指定限时购活动商品Id的销售量(单个用户)

        int GetMarketSaleCountForUserId( long pId , long userId );

        #endregion


        #region 新版本限时购

        FlashSaleDetailInfo GetDetail( string skuid );

        /// <summary>
        /// 后台分页取数据
        /// </summary>
        ObsoletePageModel<FlashSaleInfo> GetAll(  int status , string shopname , string title , int pageIndex , int pageSize );

        /// <summary>
        /// 店铺分页取某店数据
        /// </summary>
        ObsoletePageModel<FlashSaleInfo> GetAll(long shopid, int? status, string productName, DateTime? StartDate, DateTime? EndDate, int pageIndex, int pageSize);

        /// <summary>
        /// 前端获取参加限时购的所有活动商品列表
        /// </summary>
        /// <returns></returns>
        ObsoletePageModel<FlashSaleInfo> GetAll( FlashSaleQuery query );

        /// <summary>
        /// 获取正在进行的活动
        /// </summary>
        ObsoletePageModel<FlashSaleInfo> GetStartData( int index , int size , string cname ); 
        FlashSaleModel GetFlaseSaleByProductId( long pid );
        FlashSaleModel GetDetailInfo( long productId ); 

        /// <summary>
        /// 获取单条记录
        /// </summary>
        FlashSaleModel Get( long id );

        /// <summary>
        /// 增加限时购
        /// </summary>
        void AddFlashSale( FlashSaleModel model );

        /// <summary>
        /// 更新限时购
        /// </summary>
        void UpdateFlashSale( FlashSaleModel model );

        /// <summary>
        /// 通过
        /// </summary>
        void Pass( long id );

        /// <summary>
        /// 拒绝
        /// </summary>
        void Refuse( long id );

        /// <summary>
        /// 取消
        /// </summary>
        void Cancel( long id );


        /// <summary>
        /// 删除
        /// </summary>
        void Delete(long id,long shopId);

        /// <summary>
        /// 判断某产品是否可以作为限时购添加
        /// </summary>
        bool IsAdd(long productid);
        /// <summary>
        /// 判断某产品是否可以作为限时购
        /// </summary>
        bool IsEdit(long productid,long id); 

        /// <summary>
        /// 判断某个产品是否是未开始的限时购
        /// </summary>
        FlashSaleModel IsFlashSaleDoesNotStarted( long productid );

        /// <summary>
        /// 根据商品id获取限时购价格
        /// </summary>
        /// <param name="ids">商品id集合</param> 
        /// <returns>key = productid , value = price</returns>
        List<FlashSalePrice> GetPriceByProducrIds( List<long> ids );
        
        /// <summary>
        /// 增加一条需要发送的开团提醒记录
        /// </summary>
        /// <param name="flashSaleId"></param>
        /// <param name="openId"></param>
        void AddRemind( long flashSaleId , string openId );



        /// <summary>
        /// 增加销量
        /// </summary>
        void IncreaseSaleCount( List<long> orderids );

        /// <summary>
        /// 获取所有正在进行或未开始的限时购活动缓存信息
        /// </summary>
        List<FlashSaleRedisInfo> GetAllStartData();

        /// <summary>
        /// 获取最近5个限时购
        /// </summary>
        /// <returns></returns>
        List<FlashSaleModel> GetRecentFlashSale();
        #endregion

    }
}
