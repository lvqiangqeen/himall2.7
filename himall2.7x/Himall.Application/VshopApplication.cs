using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Core;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;

namespace Himall.Application
{
    public class VshopApplication
    {
        private static IVShopService _iVShopService = ObjectContainer.Current.Resolve<IVShopService>();
        /// <summary>
        /// 根据条件查询所有微店
        /// </summary>
        /// <returns></returns>
        public static ObsoletePageModel<VShopInfo> GetVShopByParamete(VshopQuery vshopQuery)
        {
            return _iVShopService.GetVShopByParamete(vshopQuery);
        }

        /// <summary>
        /// 获取热门微店
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<VShopInfo> GetHotShop(VshopQuery vshopQuery, DateTime? startTime, DateTime? endTime, out int total)
        {
            return _iVShopService.GetHotShop(vshopQuery, startTime, endTime, out total);
        }

        /// <summary>
        /// 获取热门微店
        /// </summary>
        /// <param name="page">页码</param>
        /// <param name="pageSize">页面大小</param>
        /// <param name="total">热门微店总数</param>
        /// <returns></returns>
        public static IQueryable<VShopInfo> GetHotShops(int page, int pageSize, out int total)
        {
            return _iVShopService.GetHotShops(page, pageSize, out total);
        }

        /// <summary>
        /// 获取所有微店
        /// </summary>
        /// <returns></returns>
        public static IQueryable<VShopInfo> GetVShops()
        {
            return _iVShopService.GetVShops();
        }

		/// <summary>
		/// 根据商家id获取微店信息
		/// </summary>
		/// <param name="shopIds"></param>
		/// <returns></returns>
		public static List<DTO.VShop> GetVShopsByShopIds(IEnumerable<long> shopIds)
		{
			return _iVShopService.GetVShopsByShopIds(shopIds).Map<List<DTO.VShop>>();
		}

        /// <summary>
        /// 获取微店
        /// </summary>
        /// <param name="id">微店Id</param>
        /// <returns></returns>
        public static VShopInfo GetVShop(long id)
        {
            return _iVShopService.GetVShop(id);
        }

        /// <summary>
        /// 获取所有微店(分页)
        /// </summary>
        /// <param name="page">页号</param>
        /// <param name="pageSize">页面大小</param>
        /// <param name="total">微店总数</param>
        /// <returns></returns>
        public static IQueryable<VShopInfo> GetVShops(int page, int pageSize, out int total)
        {
            return _iVShopService.GetVShops(page, pageSize, out total);
        }


        /// <summary>
        /// 获取所有微店(分页)
        /// </summary>
        /// <param name="page">页号</param>
        /// <param name="pageSize">页面大小</param>
        /// <param name="total">微店总数</param>
        /// <param name="state">微店状态</param>
        /// <returns></returns>
        public static IQueryable<VShopInfo> GetVShops(int page, int pageSize, out int total, VShopInfo.VshopStates state)
        {
           return  _iVShopService.GetVShops(page, pageSize, out total, state);
        }


        /// <summary>
        /// 获取主推微店
        /// </summary>
        /// <returns></returns>
        public static VShopInfo GetTopShop()
        {
           return  _iVShopService.GetTopShop();
        }


        /// <summary>
        /// 设为主推
        /// </summary>
        public static void SetTopShop(long vshopId)
        {
            _iVShopService.SetTopShop(vshopId);
        }

        /// <summary>
        /// 设为热门
        /// </summary>
        /// <param name="vshopId">微店ID</param>
        public static void SetHotShop(long vshopId)
        {
            _iVShopService.SetHotShop(vshopId);
        }

        /// <summary>
        /// 下架微店
        /// </summary>
        public static void CloseShop(long vshopId)
        {
            _iVShopService.CloseShop(vshopId);
        }

        /// <summary>
        /// 上架微店
        /// </summary>
        public static void SetShopNormal(long vshopId)
        {
            _iVShopService.SetShopNormal(vshopId);
        }

        /// <summary>
        /// 删除热门微店
        /// </summary>
        /// <param name="vshopId">微店ID</param>
        public static void DeleteHotShop(long vshopId)
        {
            _iVShopService.DeleteHotShop(vshopId);
        }

        /// <summary>
        /// 替换热门微店
        /// </summary>
        /// <param name="oldHotVShopId">替换前的热门微店ID</param>
        /// <param name="newHotVshopId">要替换的热门微店ID</param>
        public static void ReplaceHotShop(long oldHotVShopId, long newHotVshopId)
        {
            _iVShopService.ReplaceHotShop(oldHotVShopId, newHotVshopId);
        }

        /// <summary>
        /// 更改热门微店排序
        /// </summary>
        /// <param name="vshopId">微店ID</param>
        /// <param name="sequence">排序数字</param>
        public static void UpdateSequence(long vshopId, int? sequence)
        {
            _iVShopService.UpdateSequence(vshopId, sequence);
        }

        /// <summary>
        /// 审核通过
        /// </summary>
        /// <param name="vshopId">微店ID</param>
        public static void AuditThrough(long vshopId)
        {
            _iVShopService.AuditThrough(vshopId);
        }

        /// <summary>
        /// 审核拒绝
        /// </summary>
        /// <param name="vshopId">微店ID</param>
        public static void AuditRefused(long vshopId)
        {
            _iVShopService.AuditRefused(vshopId);
        }

        /// <summary>
        /// 替换主推微店
        /// </summary>
        /// <param name="oldTopVshopId">替换前的主推微店ID</param>
        /// <param name="newTopVshopId">要替换的主推微店ID</param>
        public static void ReplaceTopShop(long oldTopVshopId, long newTopVshopId)
        {
            _iVShopService.ReplaceTopShop(oldTopVshopId, newTopVshopId);
        }

        /// <summary>
        /// 删除主推微店
        /// </summary>
        /// <param name="vshopId">微店ID</param>
        public static void DeleteTopShop(long vshopId)
        {
            _iVShopService.DeleteTopShop(vshopId);
        }

        /// <summary>
        /// 根据店铺Id获取微店
        /// </summary>
        /// <param name="shopId">店铺Id</param>
        /// <returns></returns>
        public static VShopInfo GetVShopByShopId(long shopId)
        {
           return  _iVShopService.GetVShopByShopId(shopId);
        }

        /// <summary>
        /// 创建微店
        /// </summary>
        /// <param name="vshopInfo">微店信息</param>
        public static void CreateVshop(VShopInfo vshopInfo)
        {
            _iVShopService.CreateVshop(vshopInfo);
        }

        /// <summary>
        /// 更新微店
        /// </summary>
        /// <param name="vshopInfo">微店信息</param>
        public static void UpdateVShop(VShopInfo vshopInfo)
        {
            _iVShopService.UpdateVShop(vshopInfo);
        }

        /// <summary>
        /// 添加微店访问数量
        /// </summary>
        /// <param name="vshopId"></param>
        public static void AddVisitNumber(long vshopId)
        {
            _iVShopService.AddBuyNumber(vshopId);
        }

        /// <summary>
        /// 添加微店购买数量
        /// </summary>
        /// <param name="vshopId"></param>
        public static void AddBuyNumber(long vshopId)
        {
            _iVShopService.AddBuyNumber(vshopId);
        }

        /// <summary>
        /// 获取用户关注的微店
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public static IQueryable<VShopInfo> GetUserConcernVShops(long userId, int pageNo, int pageSize)
        {
           return  _iVShopService.GetUserConcernVShops(userId, pageNo, pageSize);
        }

        /// <summary>
        /// 获取微店的配置信息
        /// </summary>
        /// <param name="shopId">微店ID</param>
        /// <returns></returns>
        public static WXShopInfo GetVShopSetting(long shopId)
        {
           return   _iVShopService.GetVShopSetting(shopId);
        }
        /// <summary>
        /// 获取微店优惠卷 设置 信息
        /// </summary>
        /// <param name="shopid"></param>
        /// <returns></returns>
        public static IQueryable<CouponSettingInfo> GetVShopCouponSetting(long shopid)
        {
          return    _iVShopService.GetVShopCouponSetting(shopid);
        }
        /// <summary>
        /// 更新店铺优惠卷 设置 信息
        /// </summary>
        /// <param name="infolist"></param>
        public static void SaveVShopCouponSetting(IEnumerable<CouponSettingInfo> infolist)
        {
            _iVShopService.SaveVShopCouponSetting(infolist);
        }
        public static void SaveVShopSetting(WXShopInfo wxShop)
        {
            _iVShopService.SaveVShopSetting(wxShop);
        }
        /// <summary>
        /// 增加访问记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static int LogVisit(long id)
        {
           return  _iVShopService.LogVisit(id);
        }

        public static string GetVShopLog(long vshopid)
        {
           return  _iVShopService.GetVShopLog(vshopid);
        }
    }
}
