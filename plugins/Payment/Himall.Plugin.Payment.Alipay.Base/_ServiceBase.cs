using Himall.Core;
using Himall.Core.Plugins;
using Himall.Core.Plugins.Payment;
using Himall.Core.Helper;
using Himall.PaymentPlugin;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Text;

namespace Himall.Plugin.Payment.Alipay.Base
{
    public class _ServiceBase : PaymentBase<Config>
    {
        #region 配置信息
        private Config __config;
        protected Config _Config
        {
            get
            {
                if (__config == null)
                {
                    __config = GetConfig();
                }
                return __config;
            }
        }
        #endregion

        /// <summary>
        /// 获取配置信息
        /// </summary>
        /// <returns></returns>
        public Config GetConfig()
        {
            Config config = Utility<Config>.GetConfig(WorkDirectory);
            return config;
        }

        /// <summary>
        /// 退款入口
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        public override RefundFeeReturnModel ProcessRefundFee(PaymentPara para)
        {
            //退款网关
            string gateway = "https://mapi.alipay.com/gateway.do";
            //数据初始
            RefundFeeReturnModel paymentInfo = new RefundFeeReturnModel();
            paymentInfo.RefundResult = RefundState.Failure;
            paymentInfo.RefundMode = RefundRunMode.Async;
            string refund_batch_no=para.out_refund_no;

            string strResult = string.Empty;
            Config _config = GetConfig();
            if (_config.Sign_type.ToLower() == "md5")
            {
                //多种签名机制参与参数不一样
                if (string.IsNullOrEmpty(_config.Key))
                    throw new PluginException("未设置Key");
            }
            else
            {
                if (string.IsNullOrEmpty(_config.Private_key))
                    throw new PluginException("未设置私钥");
            }

            Dictionary<string, string> dicPara = new Dictionary<string, string>();
            //整理基础数据
            dicPara.Add("service", "refund_fastpay_by_platform_pwd");//服务固定，退款
            dicPara.Add("partner", _config.Partner);//合作者ID，支付宝提供
            dicPara.Add("_input_charset", _config.Input_charset);
            dicPara.Add("notify_url", para.notify_url);

            //整理业务数据
            dicPara.Add("seller_user_id", _config.Partner);
            dicPara.Add("refund_date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            dicPara.Add("batch_no", refund_batch_no);
            dicPara.Add("batch_num", "1");
            dicPara.Add("detail_data", para.pay_trade_no.Trim() + "^" + para.refund_fee.ToString("F2").Trim() + "^平台协商退款".Trim());   //要去除非格式时用“^”、“|”、“$”、“#”
            string refundurl = Submit.BuildRequestUrl(dicPara, _config, gateway);
            paymentInfo.RefundResult = RefundState.Success;
            paymentInfo.ResponseContentWhenFinished = refundurl;
            paymentInfo.RefundNo = refund_batch_no;
            return paymentInfo;
        }
        /// <summary>
        /// 退款异步通知
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override PaymentInfo ProcessRefundNotify(HttpRequestBase context)
        {
            PaymentInfo result = null;

            NameValueCollection coll = context.Form;
            Dictionary<string, string> paras = new Dictionary<string, string>();
            foreach (string key in coll.AllKeys)
            {
                paras.Add(key, coll[key]);
            }
            Config _config = _Config;
            Notify notify = new Notify(WorkDirectory);
            string notifyid = paras["notify_id"];
            string notifytype = paras["notify_type"];
            if (notifytype == null)
            {
                notifytype = "";
            }

            bool isSign = notify.Verify(paras, notifyid, (string)coll["sign"], _config);
            if (isSign && notifytype.ToLower() == "batch_refund_notify")
            {
                string batch_no = paras["batch_no"];
                string notify_time = paras["notify_time"];
                string success_num = paras["success_num"];
                int _snum = 0;
                if (int.TryParse(success_num, out _snum))
                {
                    if (_snum > 0)
                    {
                        string result_details = paras["result_details"];  //未使用，为退款详情，可以记录日志
#if DEBUG
                        Log.Info("支付宝退款：[" + batch_no + "]" + result_details);  //调试时记录日志
#endif
                        result = new PaymentInfo();
                        result.TradNo = batch_no;
                        result.TradeTime = DateTime.Parse(notify_time);
                        result.ResponseContentWhenFinished = "success";
                    }
                }
            }

            return result;
        }

        string _logo;

        public string Logo
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_logo))
                {
                    Config _config = GetConfig();
                    _logo = _config.Logo;
                }
                return _logo;
            }
            set
            {
                _logo = value;
            }
        }

        public string ConfirmPayResult()
        {
            return "success";
        }




        public virtual UrlType RequestUrlType
        {
            get { return UrlType.Page; }
        }


        public virtual string HelpImage
        {
            get { throw new NotImplementedException(); }
        }
    }
}
