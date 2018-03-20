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
    public class CashDepositsApplication
    {
        private static ICashDepositsService _iCashDepositsService = ObjectContainer.Current.Resolve<ICashDepositsService>();

        /// <summary>
        /// 获取保证金列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static ObsoletePageModel<CashDepositInfo> GetCashDeposits(CashDepositQuery query)
        {
            return _iCashDepositsService.GetCashDeposits(query);
        }

        /// <summary>
        /// 根据保证金ID获取明细列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static ObsoletePageModel<CashDepositDetailInfo> GetCashDepositDetails(CashDepositDetailQuery query)
        {
            return _iCashDepositsService.GetCashDepositDetails(query);
        }

        /// <summary>
        /// 新增类目保证金
        /// </summary>
        /// <param name="model"></param>
        public static void AddCategoryCashDeposits(CategoryCashDepositInfo model)
        {
            _iCashDepositsService.AddCategoryCashDeposits(model);
        }

        /// <summary>
        /// 根据一级分类删除类目保证金
        /// </summary>
        /// <param name="categoryId"></param>
        public static void DeleteCategoryCashDeposits(long categoryId)
        {
            _iCashDepositsService.DeleteCategoryCashDeposits(categoryId);
        }
        /// <summary>
        /// 根据保证金ID查询实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static CashDepositInfo GetCashDeposit(long id)
        {
            return _iCashDepositsService.GetCashDeposit(id);
        }

        /// <summary>
        /// 第一次充值保证金时插入记录
        /// </summary>
        /// <param name="cashDeposit"></param>
        public static void AddCashDeposit(CashDepositInfo cashDeposit)
        {
            _iCashDepositsService.AddCashDeposit(cashDeposit);
        }

        /// <summary>
        /// 插入一条保证金流水号信息
        /// </summary>
        /// <param name="cashDepositDetail"></param>
        public static void AddCashDepositDetails(CashDepositDetailInfo cashDepositDetail)
        {
            _iCashDepositsService.AddCashDepositDetails(cashDepositDetail);
        }

        /// <summary>
        /// 根据店铺ID获取保证金
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static CashDepositInfo GetCashDepositByShopId(long shopId)
        {
            return _iCashDepositsService.GetCashDepositByShopId(shopId);
        }

        /// <summary>
        /// 更新标识的显示状态
        /// </summary>
        /// <param name="id"></param>
        /// <param name="EnableLabels"></param>
        public static void UpdateEnableLabels(long id, bool enableLabels)
        {
            _iCashDepositsService.UpdateEnableLabels(id, enableLabels);
        }

        #region 类目保证金
        /// <summary>
        /// 获取店铺应缴保证金
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static Decimal GetNeedPayCashDepositByShopId(long shopId)
        {
            return _iCashDepositsService.GetNeedPayCashDepositByShopId(shopId);
        }

        /// <summary>
        /// 获取分类保证金列表
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<CategoryCashDepositInfo> GetCategoryCashDeposits()
        {
            return _iCashDepositsService.GetCategoryCashDeposits();
        }
        /// <summary>
        /// 更新所需缴纳保证金
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="CashDeposits">保证金金额</param>
        public static void UpdateNeedPayCashDeposit(long categoryId, decimal CashDeposit)
        {
            _iCashDepositsService.UpdateNeedPayCashDeposit(categoryId, CashDeposit);
        }

        /// <summary>
        /// 开启七天无理由退换货
        /// </summary>
        /// <param name="id"></param>
        public static void OpenNoReasonReturn(long id)
        {
            _iCashDepositsService.OpenNoReasonReturn(id);
        }

        /// <summary>
        /// 关闭七天无理由退换货
        /// </summary>
        /// <param name="id"></param>
        public static void CloseNoReasonReturn(long id)
        {
            _iCashDepositsService.CloseNoReasonReturn(id);
        }


        #endregion

        /// <summary>
        /// 获取提供特殊服务实体
        /// </summary>
        /// <param name="productId"></param>
        public static CashDepositsObligation GetCashDepositsObligation(long productId)
        {
            return _iCashDepositsService.GetCashDepositsObligation(productId);
        }
    }
}
