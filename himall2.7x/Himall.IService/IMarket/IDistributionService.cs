using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.IServices.QueryModel;

namespace Himall.IServices
{
    public interface IDistributionService : IService
    {
        #region 平台中心
        /// <summary>
        /// 平台招募设置
        /// </summary>
        /// <param name="model"></param>
        void UpdateRecruitmentSetting(RecruitSettingInfo model);
        /// <summary>
        /// 获取平台招募设置
        /// </summary>
        /// <returns></returns>
        RecruitSettingInfo GetRecruitmentSetting();

        //招募计划设置
        void UpdateRecruitmentPlan(RecruitPlanInfo model);
        /// <summary>
        /// 获取平台招募计划
        /// </summary>
        /// <returns></returns>
        RecruitPlanInfo GetRecruitmentPlan();

        //平台分销板块设置
        void UpdateDistributorSetting(DistributorSettingInfo model);
        /// <summary>
        /// 获取平台分销板块设置
        /// </summary>
        /// <returns></returns>
        DistributorSettingInfo GetDistributionSetting();

        //推广信息设置
        void UpdateDistributionShare(DistributionShareSetting model);
        /// <summary>
        /// 获取推广信息
        /// </summary>
        /// <returns></returns>
        DistributionShareSetting GetDistributionShare();

        //分页获取推广员列表
        ObsoletePageModel<PromoterInfo> GetPromoterList(PromoterQuery query);

        /// <summary>
        /// 平台清退用户 
        /// /// </summary>
        /// <param name="Id"></param>
        void DisablePromoter(long Id);

        /// <summary>
        /// 获取分销员ID或者分销员信息
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        PromoterInfo GetPromoter(long id);

        /// <summary>
        /// 根据用户ID获取分销员信息
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>

        PromoterInfo GetPromoterByUserId(long userId);


        /// <summary>
        /// 拒绝用户申请
        /// </summary>
        /// <param name="Id"></param>
        void RefusePromoter(long Id);

        /// <summary>
        /// 审核平台用户
        /// </summary>
        /// <param name="userId"></param>
        void AduitPromoter(long Id);


        //获取销售员总数，三天，七天新增数
        PromoterStatistics GetPromoterStatistics();

        //获取分销列表
        ObsoletePageModel<ProductsDistributionModel> GetDistributionlist(DistributionQuery query);

        //更新分销产品排序
        void UpdateProductsDistributionOrder(long productId, int sort);


        //获取业绩列表
        ObsoletePageModel<ProformanceModel> GetPerformanceList(ProformanceQuery query);

        //获取用户业绩详情
        ObsoletePageModel<UserProformanceModel> GetPerformanceDetail(UserProformanceQuery query);
        #endregion

        #region 卖家中心
        /// <summary>
        /// 获取商家分销设置
        /// </summary>
        /// <returns></returns>
        ShopDistributorSettingInfo getShopDistributorSettingInfo(long shopid);
        /// <summary>
        /// 商家聚合页推广设置
        /// </summary>
        /// <param name="model"></param>
        void UpdateShopDistributor(ShopDistributorSettingInfo model);
        /// <summary>
        /// 设置商家默认分佣比例
        /// </summary>
        /// <param name="rate"></param>
        void UpdateDefaultBrokerage(decimal rate, long shopid);

		/// <summary>
		/// 是否为分销员
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		bool IsPromoter(long userId);

        /// <summary>
        /// 获取当前所有分销商品编号
        /// </summary>
        /// <param name="shopid"></param>
        /// <returns></returns>
        List<long> GetAllDistributionProductIds(long shopid);

        /// <summary>
        /// 添加分销商品
        /// <para>功能未实现</para>
        /// </summary>
        void AddDistributionProducts(ProductBrokerageInfo model);
        /// <summary>
        /// 批量添加分销商品
        /// </summary>
        /// <param name="productids"></param>
        /// <param name="shopid"></param>
        /// <param name="rate"></param>
        void BatAddDistributionProducts(IEnumerable<long> productids, long shopid, decimal rate);

        /// <summary>
        /// 分销订单明细
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        ObsoletePageModel<object> GetDistributionOrders(dynamic query);

        /// <summary>
        /// (批量)取消商品分销推广
        /// </summary>
        /// <param name="ProductIds"></param>
        /// <param name="shopId">店铺编号,null表示所有店铺</param>
        void CancelDistributionProduct(IEnumerable<long> ProductIds, long? shopId = null);


        /// <summary>
        /// （批量）设置商品的分佣比例
        /// </summary>
        /// <param name="percent"></param>
        /// <param name="ProductIds"></param>
        /// <param name="shopId"></param>
        void SetProductBrokerage(decimal percent, IEnumerable<long> ProductIds, long? shopId = null);

        /// <summary>
        /// 商品分销明细
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        ObsoletePageModel<object> GetDistributionProductsDetail(dynamic query);
        #endregion

        #region 个人中心
        void ApplyForDistributor(PromoterModel model);

        /// <summary>
        /// 获取分销用户业绩
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        DistributionUserPerformanceSetModel GetUserPerformance(long userId);


        /// <summary>
        /// 分销产品的转发次数
        /// </summary>
        /// <param name="productId"></param>
        void UpdateProductShareNum(long productId);

        /// <summary>
        /// 获取用户帐单列表
        /// </summary>
        /// <param name="query"></param>
        /// 
        /// <returns></returns>
        ObsoletePageModel<DistributionFeatModel> GetUserBillList(DistributionUserBillQuery query);

        /// <summary>
        /// 获取用户帐单详情
        /// </summary>
        /// <param name="billId"></param>
        /// <returns></returns>
        object GetUserBillDetail(long billId);

        /// <summary>
        /// 获取用户代理的商品
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        ObsoletePageModel<object> GetUserAgentProducts(long userId);
        /// <summary>
        /// 获取productIds中可以代理的商品编号
        /// </summary>
        /// <param name="productIds">商品编号范围</param>
        /// <param name="userId"></param>
        /// <returns>可以代理的商品编号</returns>
        IEnumerable<long> GetCanAgentProductId(IEnumerable<long> productIds, long userId);

        /// <summary>
        /// （批量）移除用户代理的商品
        /// </summary>
        /// <param name="ProductIds"></param>
        /// <param name="userId"></param>
        void RemoveAgentProducts(IEnumerable<long> productIds, long userId);

        /// <summary>
        /// （批量）添加代理商品
        /// </summary>
        /// <param name="productIds"></param>

        void AddAgentProducts(IEnumerable<long> productIds, long userId);

        /// <summary>
        /// 添加一笔用户结算收入流水
        /// </summary>
        /// <param name="model"></param>
        void AddUserDistribution(BrokerageIncomeInfo model);

        /// <summary>
        /// 添加分销退款
        /// </summary>
        /// <param name="model"></param>
        void AddDistributionRefund(long OrderItemId, decimal RefundPrice, decimal Brokerage, long RefundId);
        /// <summary>
        /// 修改分销退款
        /// </summary>
        /// <param name="model"></param>
        void UpdateDistributionRefund(long OrderItemId, decimal RefundPrice, decimal Brokerage, long RefundId);
        /// <summary>
        /// 关闭分销退款
        /// </summary>
        /// <param name="OrderItemId"></param>
        void CloseDistributionRefund(long OrderItemId);
        /// <summary>
        /// 完成分销退款
        /// </summary>
        /// <param name="OrderItemId"></param>
        void OverDistributionRefund(long OrderItemId, decimal RefundAmount, long RefundQuantity);

        /// <summary>
        /// 发生退款退货行为时，改变结算状态为不可结算
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="orderItemId">订单条目ID</param>
        void DisabledSettlement(long userId, long orderItemId);

        /// <summary>
        /// 退款处理完成变为可结算状态
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="orderItemId"></param>
        void EnableSettlement(long userId, long orderItemId);


        /// <summary>
        /// 用户佣金结算
        /// </summary>
        /// <param name="userId"></param>
        void UserBrokerageSettlement(long userId);
        #endregion

        #region  分销市场

        /// <summary>
        /// 获取所有的分销商品
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        ObsoletePageModel<ProductBrokerageInfo> GetDistributionProducts(ProductBrokerageQuery query);
        /// <summary>
        /// 获取分销商品信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        ProductBrokerageInfo GetDistributionProductInfo(long productId);

        /// <summary>
        /// 批量获取分销商品信息
        /// </summary>
        /// <param name="productIds">分销商品ids</param>
        /// <returns></returns>
        List<ProductBrokerageInfo> GetDistributionProductInfo(IEnumerable<long> productIds);

        /// <summary>
        /// 获取店铺列表的接口
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        ObsoletePageModel<DistributionShopModel> GetShopDistributionList(DistributionShopQuery query);
        /// <summary>
        /// 获取店铺分销商品数量
        /// <para>仅统计可以正常购买的</para>
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        long GetShopDistributionProductCount(long shopId);

        /// <summary>
        /// 获取所有分销首页商品
        /// </summary>
        /// <returns></returns>
        List<DistributionProductsInfo> GetDistributionProducts();

        /// <summary>
        /// 添加首页商品设置
        /// </summary>
        /// <param name="mDistributionProductsInfo"></param>
        void AddDistributionProducts(DistributionProductsInfo mDistributionProductsInfo);

        /// <summary>
        /// 移除分销首页商品
        /// </summary>
        /// <param name="Ids"></param>
        void RemoveDistributionProducts(IEnumerable<long> Ids);

        /// <summary>
        /// 获取分销首页所有商品
        /// </summary>
        /// <param name="page">分页页码</param>
        /// <param name="rows">每页行数</param>
        /// <param name="keyWords">搜索关键字</param>
        /// <param name="categoryId">3级分类</param>
        Himall.CommonModel.QueryPageModel<Himall.Model.DistributionProductsInfo> GetDistributionProducts(int page, int rows, string keyWords, long? categoryId = null);

        /// <summary>
        /// 获取分销首页数据
        /// </summary>
        /// <param name="status">分销商品状态</param>
        /// <returns></returns>
        List<Himall.Model.DistributionProductsInfo> GetDistributionProducts(ProductBrokerageInfo.ProductBrokerageStatus? status = null);

        /// <summary>
        /// 删除设置
        /// </summary>
        /// <param name="Id"></param>
        void DelDistributionProducts(long Id);

        /// <summary>
        /// 获取分销设置对象
        /// </summary>
        /// <param name="Id">主键ID</param>
        /// <returns></returns>
        Himall.Model.DistributionProductsInfo GetDistributionProductsInfo(long Id);

        /// <summary>
        /// 修改分销首页设置
        /// </summary>
        /// <param name="model">分销实体</param>
        void UpdateDistributionProducts(Himall.Model.DistributionProductsInfo model);
        #endregion
    }
}
