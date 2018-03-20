using Himall.IServices.QueryModel;
using Himall.Model;
using System.Linq;
using System;
using System.Collections.Generic;
using Himall.CommonModel;

namespace Himall.IServices
{
    public interface IShopService : IService
    {


        #region  店铺服务接口

        /// <summary>
        /// 获取待审核的店铺信息（以分页的形式展示）
        /// </summary>
        /// <param name="shopQueryModel">ShopQuery对象</param>
        /// <returns></returns>
        ObsoletePageModel<ShopInfo> GetAuditingShops(ShopQuery shopQueryModel);


        /// <summary>
        /// 获取店铺信息（以分页的形式展示）
        /// </summary>
        /// <param name="shopQueryModel">ShopQuery对象</param>
        /// <returns></returns>
		QueryPageModel<ShopInfo> GetShops(ShopQuery shopQueryModel);

        /// <summary>
        /// 获取一个店铺信息
        /// </summary>
        /// <param name="id">店铺ID</param>
        /// <returns></returns>
        ShopInfo GetShop(long id, bool businessCategoryOn = false);

		/// <summary>
		/// 根据id获取门店
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
		List<ShopInfo> GetShops(IEnumerable<long> ids);

        /// <summary>
        /// 通过app_key获取店铺信息
        /// </summary>
        /// <param name="appkey"></param>
        /// <returns></returns>
        ShopInfo GetShop(string appkey);

        /// <summary>
        /// 获取店铺基本信息
        /// </summary>
        /// <param name="id">店铺ID</param>
        /// <returns></returns>
        ShopInfo GetShopBasicInfo(long id);

        /// <summary>
        /// 删除一个店铺
        /// </summary>
        /// <param name="id">店铺Id</param>
        void DeleteShop(long id);

        /// <summary>
        /// 更新店铺
        /// </summary>
        /// <param name="shop">店铺Id</param>
        void UpdateShop(ShopInfo shop);

        /// <summary>
        /// 更新店铺
        /// </summary>
        /// <param name="shop">店铺Id</param>
        /// <param name="categoryIds">经营类目</param>
        void UpdateShop(ShopInfo shop, IEnumerable<long> categoryIds);


        /// <summary>
        /// 申请新的经营类目
        /// </summary>
        /// <param name="shop"></param>
        /// <param name="categoryIds"></param>
        void ApplyShopBusinessCate(long shopId, IEnumerable<long> categoryIds);


        /// <summary>
        /// 审核申请的经营类目
        /// </summary>
        /// <param name="applyId"></param>
        void AuditShopBusinessCate(long applyId,BusinessCategoriesApplyInfo.BusinessCateApplyStatus status);


        /// <summary>
        /// 分頁獲取申請的經營類目列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        ObsoletePageModel<BusinessCategoriesApplyInfo> GetBusinessCateApplyList(BussinessCateApplyQuery query);


        /// <summary>
        /// 獲取申請的某個經營列表
        /// </summary>
        /// <param name="applyId"></param>
        /// <returns></returns>
        BusinessCategoriesApplyInfo GetBusinessCategoriesApplyInfo(long applyId);
        

        /// <summary>
        /// 更新店铺发货人信息
        /// </summary>
        /// <param name="shopId">商铺Id</param>
        /// <param name="regionId">地区Id</param>
        /// <param name="address">发货人地址</param>
        /// <param name="senderName">发货人姓名</param>
        /// <param name="senderPhone">发货人电话</param>
        void UpdateShopSenderInfo(long shopId, int regionId, string address, string senderName, string senderPhone);

        /// <summary>
        /// 新建一个店铺
        /// </summary>
        /// <param name="shop">店铺实体模型</param>
        long AddShop(ShopInfo shop);

        /// <summary>
        /// 获取店铺的经营类目
        /// </summary>
        /// <param name="id">店铺Id</param>
        /// <returns></returns>
        IQueryable<BusinessCategoryInfo> GetBusinessCategory(long id);

        /// <summary>
        /// 获取选择的类目下的所有三级类目（不包含已申请的类目)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        List<CategoryRateModel> GetThirdBusinessCategory(long id,long shopId);
        /// <summary>
        /// 是否可以删除经营类目
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="bCategoryId"></param>
        /// <returns></returns>
        bool CanDeleteBusinessCategory(long shopId, long bCategoryId);

        /// <summary>
        /// 保存指定店铺的经营类目
        /// </summary>
        /// <param name="shopId">店铺Id</param>
        /// <param name="bCategoryList"></param>
        void SaveBusinessCategory(long shopId, Dictionary<long, decimal> bCategoryList);

        /// <summary>
        /// 修改指定经营类目分佣比例
        /// </summary>
        /// <param name="id">经营类目Id</param>
        /// <param name="commisRate">分佣比例</param>
        void SaveBusinessCategory(long id, decimal commisRate);

        /// <summary>
        /// 更新店铺的状态
        /// </summary>
        /// <param name="shopId">店铺Id</param>
        /// <param name="status">状态</param>
        /// <param name="comments">注释</param>
        void UpdateShopStatus(long shopId, ShopInfo.ShopAuditStatus status, string comments = "", int TrialDays = 0);

        /// <summary>
        /// 商家后台控制台
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        SellerConsoleModel GetSellerConsoleModel(long shopId);

        /// <summary>
        /// 平台后台控制台
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        PlatConsoleModel GetPlatConsoleMode();

        /// <summary>
        /// 判断店铺名称是否存在
        /// </summary>
        /// <param name="shopName"></param>
        /// <returns></returns>
        bool ExistShop(string shopName, long shopId = 0);
        /// <summary>
        /// 是否已过期
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        bool IsExpiredShop(long shopId);
        /// <summary>
        /// 是否冻结
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        bool IsFreezeShop(long shopId);
        /// <summary>
        /// 是否官方自营店
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        bool IsSelfShop(long shopId);
        /// <summary>
        /// 判断公司名称是否存在
        /// </summary>
        /// <param name="shopName"></param>
        /// <returns></returns>
        bool ExistCompanyName(string companyName, long shopId = 0);


        /// <summary>
        /// 检测营业执照号是否重复
        /// </summary>
        /// <param name="BusinessLicenceNumber">营业执照号</param>
        /// <param name="shopId"></param>
        bool ExistBusinessLicenceNumber(string BusinessLicenceNumber, long shopId = 0);

        ObsoletePageModel<ShopInfo> GetSellers(SellerQuery sellerQueryModel);

        #endregion

        #region 更新店铺运费

        /// <summary>
        /// 更新店铺运费
        /// </summary>
        /// <param name="shopId">店铺id</param>
        /// <param name="freight">运费</param>
        /// <param name="freeFreight">满额免运费</param>
        void UpdateShopFreight(long shopId, decimal freight, decimal freeFreight);

        #endregion

        #region 店铺等级服务接口

        /// <summary>
        /// 获取所有店铺等级列表
        /// </summary>
        /// <returns></returns>
        IQueryable<ShopGradeInfo> GetShopGrades();

        /// <summary>
        /// 获取指定店铺等级信息
        /// </summary>
        /// <param name="id">店铺等级Id</param>
        /// <returns></returns>
        ShopGradeInfo GetShopGrade(long id);

        /// <summary>
        /// 新建一个店铺等级
        /// </summary>
        /// <param name="shopGrade"></param>
        void AddShopGrade(ShopGradeInfo shopGrade);

        /// <summary>
        /// 删除一个指定的店铺等级
        /// </summary>
        /// <param name="id">店铺等级Id</param>
        void DeleteShopGrade(long id, out string msg);


        /// <summary>
        /// 更新店铺等级
        /// </summary>
        /// <param name="shopGrade"></param>
        void UpdateShopGrade(ShopGradeInfo shopGrade);

        #endregion

        #region 获取店铺已使用空间大小（超过返回-1）
        /// <summary>
        /// 获取店铺已使用空间大小（超过返回-1）
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        long GetShopSpaceUsage(long shopId);
        #endregion

        #region 获取店铺的关注人数
        long GetShopConcernedCount(long shopId);
        #endregion

        #region 获取用户关注的店铺
        ObsoletePageModel<FavoriteShopInfo> GetUserConcernShops(long userId, int pageNo, int pageSize);
        #endregion

        #region 取消用户关注的店铺
        void CancelConcernShops(IEnumerable<long> ids, long userId);

        void CancelConcernShops(long shopI, long userId);
        #endregion

        #region 累加浏览次数

        /// <summary>
        /// 累计店铺的浏览次数
        /// </summary>
        /// <param name="shopId"></param>
        void LogShopVisti(long shopId);

        #endregion

        #region 创建店铺

        /// <summary>
        /// 创建一个空店铺
        /// </summary>
        /// <returns></returns>
        ShopInfo CreateEmptyShop();



        #endregion

        #region 添加店铺关注

        /// <summary>
        /// 添加店铺关注
        /// </summary>
        /// <param name="memberId">会员Id</param>
        /// <param name="shopId">店铺Id</param>
        void AddFavoriteShop(long memberId, long shopId);

        bool IsFavoriteShop(long memberId, long shopId);
        #endregion

        #region 获取店铺关注

        /// <summary>
        /// 获取指定会员所有店铺关注
        /// </summary>
        /// <param name="memberId">会员Id</param>
        /// <returns></returns>
        IQueryable<FavoriteShopInfo> GetFavoriteShopInfos(long memberId);
        #endregion

        #region 获取店铺的关注度
        int GetShopFavoritesCount(long shopId);
        #endregion

        #region 获取店铺宝贝数
        int GetShopProductCount(long shopId);
        #endregion

        ShopInfo GetSelfShop();

        string GetShopName(long id);

        /// <summary>
        /// 获取商铺免邮活动的邮费
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        decimal GetShopFreeFreight(long id);

        /// <summary>
        /// 获取店铺账户信息
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        List<ShopAccountInfo> GetShopAccounts(IEnumerable<long> ids);

		/// <summary>
		/// 获取商家名称
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
		Dictionary<long, string> GetShopNames(IEnumerable<long> ids);

        void UpdateLogo(long shopId, string img);

        #region  店铺的评分统计

        IQueryable<StatisticOrderCommentsInfo> GetShopStatisticOrderComments(long shopId);
        #endregion

        /// <summary>
        /// 检测并初始店铺模板
        /// </summary>
        /// <param name="shopId"></param>
        void CheckInitTemplate(long shopId);

        ShopInfo.ShopVistis GetShopVistiInfo(DateTime startDate, DateTime endDate, long shopId);

        /// <summary>
        /// 获取店铺的订单销量
        /// </summary>
        int GetSales(long id);

        #region 添加店铺续费记录
        void AddShopRenewRecord(ShopRenewRecord record);
        #endregion

        #region 店铺续费
        void ShopReNew(long shopid, int year);
        #endregion

        #region 店铺升级
        void ShopUpGrade(long shopid, long gradeid);
        #endregion

        #region 获取店铺续费记录
        ObsoletePageModel<ShopRenewRecord> GetShopRenewRecords(ShopQuery query);
        #endregion
        /// <summary>
        /// 冻结/解冻店铺
        /// </summary>
        /// <param name="id"></param>
        /// <param name="state">true冻结 false解冻</param>
        void FreezeShop(long id, bool state);
        /// <summary>
        /// 将所有在售的商品下架
        /// </summary>
        /// <param name="id"></param>
        void SaleOffAllProduct(long id);
        /// <summary>
        /// 关闭过期或冻结店铺的活动(JOB)
        /// </summary>
        void AutoCloseMarketingActionByShopExpiredOrFreeze();
        /// <summary>
        /// 下架过期或冻结店铺的商品(JOB)
        /// </summary>
        void AutoSaleOffProductByShopExpiredOrFreeze();

        /// <summary>
        /// 获取单条入驻缴费记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        ShopRenewRecord GetShopRenewRecord(long id);

        /// <summary>
        /// 获取商铺管理员会员ID
        /// </summary>
        /// <param name="ShopId"></param>
        /// <returns></returns>
        long GetShopManagers(long ShopId);
    }
}
