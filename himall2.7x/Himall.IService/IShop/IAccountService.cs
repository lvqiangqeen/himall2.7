using Himall.IServices.QueryModel;
using Himall.Model;
using System.Linq;
using System;
using System.Collections.Generic;

namespace Himall.IServices
{
    public interface IAccountService : IService
    {
        /// <summary>
        /// 获取结算列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        ObsoletePageModel<AccountInfo> GetAccounts(AccountQuery query);

        /// <summary>
        /// 获取单个结算
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        AccountInfo GetAccount(long id);

        /// <summary>
        /// 获取结算订单明细列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        ObsoletePageModel<AccountDetailInfo> GetAccountDetails(AccountQuery query);


        /// <summary>
        /// 获取结算采购明细列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        ObsoletePageModel<AccountPurchaseAgreementInfo> GetAccountPurchaseAgreements(AccountQuery query);
        /// <summary>
        /// 取服务费用
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        ObsoletePageModel<AccountMetaModel> GetAccountMeta(AccountQuery query);


        /// <summary>
        /// 取分销佣金
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        ObsoletePageModel<BrokerageModel> GetBrokerageList(AccountQuery query);


        /// <summary>
        /// 确认结算
        /// </summary>
        /// <param name="id"></param>
        /// <param name="managerRemark"></param>
        void ConfirmAccount(long id, string managerRemark);
    }
}
