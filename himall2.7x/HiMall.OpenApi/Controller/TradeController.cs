using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json;
using Hishop.Open.Api;
using Himall.Core;
using Himall.OpenApi.Model.Parameter;

namespace Himall.OpenApi
{
    /// <summary>
    /// 交易控制器
    /// </summary>
    [RoutePrefix("OpenApi")]
    public class TradeController : HiOpenAPIController
    {
        private TradeHelper tradeService;

        public TradeController()
        {
            tradeService = new TradeHelper();
        }

        /// <summary>
        /// 获取当前商家的订单列表
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        public object GetSoldTrades([FromUri]GetSoldTradesParameterModel para)
        {
            #region 参数初始
            if(para==null)
            {
                para = new GetSoldTradesParameterModel();
            }
            para.ValueInit();   //初始基础值
            para.CheckParameter();
            #endregion

            var data = tradeService.GetSoldTrades(para.app_key, para.start_created, para.end_created, para.status, para.buyer_uname, para.page_no.Value, para.page_size.Value);
            List<trade_list_model> datalist = new List<trade_list_model>();
            if (data.Total > 0)
            {
                datalist = data.Models.ToList();
            }
            bool has_next = para.page_no.Value * para.page_size.Value < data.Total;
            var result = new { trades_sold_get_response = new { total_results = data.Total, has_next= has_next, trades = datalist } };
            return Json(result);
        }
        /// <summary>
        /// 查询订单的增量交易数据
        /// </summary>
        /// <param name="app_key"></param>
        /// <param name="timestamp"></param>
        /// <param name="sign"></param>
        /// <param name="start_modified"></param>
        /// <param name="end_modified"></param>
        /// <param name="status"></param>
        /// <param name="buyer_uname"></param>
        /// <param name="page_no"></param>
        /// <param name="page_size"></param>
        /// <returns></returns>
        public object GetIncrementSoldTrades([FromUri]GetIncrementSoldTradesParameterModel para)
        {
            #region 参数初始
            if (para == null)
            {
                para = new GetIncrementSoldTradesParameterModel();
            }
            para.ValueInit();   //初始基础值
            para.CheckParameter();
            #endregion
            
            var data = tradeService.GetIncrementSoldTrades(para.app_key, para.start_modified.Value, para.end_modified.Value, para.status, para.buyer_uname, para.page_no.Value, para.page_size.Value);
            List<trade_list_model> datalist = new List<trade_list_model>();
            if (data.Total > 0)
            {
                datalist = data.Models.ToList();
            }
            bool has_next = para.page_no.Value * para.page_size.Value < data.Total;
            var result = new { trades_sold_get_response = new { total_results = data.Total, has_next = has_next, trades = datalist } };
            return Json(result);
        }
        /// <summary>
        /// 获取单笔交易的详细信息
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        public object GetTrade([FromUri]GetTradeParameterModel para)
        {
            #region 参数初始
            if (para == null)
            {
                para = new GetTradeParameterModel();
            }
            para.ValueInit();   //初始基础值
            para.CheckParameter();   //参数检测
            #endregion

            long orderId = long.Parse(para.tid);

            var result = tradeService.GetTrade(para.app_key, orderId);
            return Json(new { trade_get_response = new { trade = result } });
        }
        /// <summary>
        /// 修改交易备注
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        [HttpPost]
        public object UpdateTradeMemo(UpdateTradeMemoParameterModel para)
        {
            #region 参数初始
            if (para == null)
            {
                para = new UpdateTradeMemoParameterModel();
            }
            para.ValueInit();   //初始基础值
            para.CheckParameter();
            #endregion

            long orderId = long.Parse(para.tid);

            var result = tradeService.UpdateTradeMemo(para.app_key, orderId, para.memo);
            return Json(new { trade_memo_update_response = new { trade = new { tid = para.tid, modified = result } } });
        }
        /// <summary>
        /// 订单发货
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        [HttpPost]
        public object SendLogistic(SendLogisticParameterModel para)
        {
            #region 参数初始
            if (para == null)
            {
                para = new SendLogisticParameterModel();
            }
            para.ValueInit();   //初始基础值
            para.CheckParameter();   //参数检测
            #endregion

            long orderId = long.Parse(para.tid);

            var result = tradeService.SendLogistic(para.app_key, orderId, para.company_name, para.out_sid);

            return Json(new { logistics_send_response = new { shipping = new { is_success = result } } });
        }
        /// <summary>
        /// 更新物流信息
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        [HttpPost]
        public object ChangLogistics(ChangLogisticsParameterModel para)
        {
            #region 参数初始
            if (para == null)
            {
                para = new ChangLogisticsParameterModel();
            }
            para.ValueInit();   //初始基础值
            para.CheckParameter();   //参数检测
            #endregion

            long orderId = long.Parse(para.tid);

            var result = tradeService.ChangLogistics(para.app_key, orderId, para.company_name, para.out_sid);
            
            return Json(new { logistics_change_response = new { shipping = new { is_success = result } } });
        }
    }
}
