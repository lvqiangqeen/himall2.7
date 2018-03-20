using Himall.IServices.QueryModel;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Himall.IServices
{
    public interface IVShopService : IService
    {

        /// <summary>
        /// 根据条件查询所有微店
        /// </summary>
        /// <returns></returns>
        ObsoletePageModel<VShopInfo> GetVShopByParamete(VshopQuery vshopQuery);

        /// <summary>
        /// 获取热门微店
        /// </summary>
        /// <returns></returns>
		IEnumerable<VShopInfo> GetHotShop(VshopQuery vshopQuery, DateTime? startTime, DateTime? endTime,out int total);

        /// <summary>
        /// 获取热门微店
        /// </summary>
        /// <param name="page">页码</param>
        /// <param name="pageSize">页面大小</param>
        /// <param name="total">热门微店总数</param>
        /// <returns></returns>
        IQueryable<VShopInfo> GetHotShops(int page, int pageSize, out int total);

        /// <summary>
        /// 获取所有微店
        /// </summary>
        /// <returns></returns>
        IQueryable<VShopInfo> GetVShops();

		/// <summary>
		/// 根据商家id获取微店信息
		/// </summary>
		/// <param name="shopIds"></param>
		/// <returns></returns>
		List<VShopInfo> GetVShopsByShopIds(IEnumerable<long> shopIds);

        /// <summary>
        /// 获取微店
        /// </summary>
        /// <param name="id">微店Id</param>
        /// <returns></returns>
        VShopInfo GetVShop(long id);

        /// <summary>
        /// 获取所有微店(分页)
        /// </summary>
        /// <param name="page">页号</param>
        /// <param name="pageSize">页面大小</param>
        /// <param name="total">微店总数</param>
        /// <returns></returns>
        IQueryable<VShopInfo> GetVShops(int page, int pageSize, out int total);


        /// <summary>
        /// 获取所有微店(分页)
        /// </summary>
        /// <param name="page">页号</param>
        /// <param name="pageSize">页面大小</param>
        /// <param name="total">微店总数</param>
        /// <param name="state">微店状态</param>
        /// <returns></returns>
        IQueryable<VShopInfo> GetVShops(int page, int pageSize, out int total, VShopInfo.VshopStates state);


        /// <summary>
        /// 获取主推微店
        /// </summary>
        /// <returns></returns>
        VShopInfo GetTopShop();


        /// <summary>
        /// 设为主推
        /// </summary>
        void SetTopShop(long vshopId);

        /// <summary>
        /// 设为热门
        /// </summary>
        /// <param name="vshopId">微店ID</param>
        void SetHotShop(long vshopId);

        /// <summary>
        /// 下架微店
        /// </summary>
        void CloseShop(long vshopId);

        /// <summary>
        /// 上架微店
        /// </summary>
        void SetShopNormal( long vshopId );

        /// <summary>
        /// 删除热门微店
        /// </summary>
        /// <param name="vshopId">微店ID</param>
        void DeleteHotShop(long vshopId);

        /// <summary>
        /// 替换热门微店
        /// </summary>
        /// <param name="oldHotVShopId">替换前的热门微店ID</param>
        /// <param name="newHotVshopId">要替换的热门微店ID</param>
        void ReplaceHotShop(long oldHotVShopId, long newHotVshopId);

        /// <summary>
        /// 更改热门微店排序
        /// </summary>
        /// <param name="vshopId">微店ID</param>
        /// <param name="sequence">排序数字</param>
        void UpdateSequence(long vshopId, int? sequence);

        /// <summary>
        /// 审核通过
        /// </summary>
        /// <param name="vshopId">微店ID</param>
        void AuditThrough(long vshopId);

        /// <summary>
        /// 审核拒绝
        /// </summary>
        /// <param name="vshopId">微店ID</param>
        void AuditRefused(long vshopId);

        /// <summary>
        /// 替换主推微店
        /// </summary>
        /// <param name="oldTopVshopId">替换前的主推微店ID</param>
        /// <param name="newTopVshopId">要替换的主推微店ID</param>
        void ReplaceTopShop(long oldTopVshopId, long newTopVshopId);

        /// <summary>
        /// 删除主推微店
        /// </summary>
        /// <param name="vshopId">微店ID</param>
        void DeleteTopShop(long vshopId);

        /// <summary>
        /// 根据店铺Id获取微店
        /// </summary>
        /// <param name="shopId">店铺Id</param>
        /// <returns></returns>
        VShopInfo GetVShopByShopId(long shopId);

        /// <summary>
        /// 创建微店
        /// </summary>
        /// <param name="vshopInfo">微店信息</param>
        void CreateVshop(VShopInfo vshopInfo);

        /// <summary>
        /// 更新微店
        /// </summary>
        /// <param name="vshopInfo">微店信息</param>
        void UpdateVShop(VShopInfo vshopInfo);

        /// <summary>
        /// 添加微店访问数量
        /// </summary>
        /// <param name="vshopId"></param>
        void AddVisitNumber(long vshopId);

        /// <summary>
        /// 添加微店购买数量
        /// </summary>
        /// <param name="vshopId"></param>
        void AddBuyNumber(long vshopId);

        /// <summary>
        /// 获取用户关注的微店
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        IQueryable<VShopInfo> GetUserConcernVShops(long userId, int pageNo, int pageSize);

        /// <summary>
        /// 获取微店的配置信息
        /// </summary>
        /// <param name="shopId">微店ID</param>
        /// <returns></returns>
        WXShopInfo GetVShopSetting(long shopId);
        /// <summary>
        /// 获取微店优惠卷 设置 信息
        /// </summary>
        /// <param name="shopid"></param>
        /// <returns></returns>
        IQueryable<CouponSettingInfo> GetVShopCouponSetting(long shopid);
        /// <summary>
        /// 更新店铺优惠卷 设置 信息
        /// </summary>
        /// <param name="infolist"></param>
        void SaveVShopCouponSetting(IEnumerable<CouponSettingInfo> infolist);
        void SaveVShopSetting(WXShopInfo wxShop);
        /// <summary>
        /// 增加访问记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        int LogVisit(long id);

        string GetVShopLog( long vshopid );
    }
}
