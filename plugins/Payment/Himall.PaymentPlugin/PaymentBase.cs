using Himall.Core;
using Himall.Core.Plugins;
using Himall.Core.Plugins.Payment;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.PaymentPlugin
{
    public class PaymentBase<T> where T : ConfigBase, new()
    {
        static Dictionary<string, string> workDirectories = new Dictionary<string, string>();


        public string WorkDirectory
        {
            get
            {
                string name = this.GetType().FullName;
                Log.Info("get WorkDirectory:" + name);
                if (!string.IsNullOrWhiteSpace(name) && workDirectories.ContainsKey(name))
                    return workDirectories[name];
                else
                    return null;
            }
            set
            {
                string name = this.GetType().FullName;
                Log.Info("set WorkDirectory:" + name);
                if (!workDirectories.ContainsKey(name))
                    workDirectories.Add(name, value);
            }
        }

        public void Disable(PlatformType platform)
        {
            EnableOrDisable(platform, false);
        }

        public void Enable(PlatformType platform)
        {
            EnableOrDisable(platform, true);
        }

        void EnableOrDisable(PlatformType platform, bool enable)
        {
            T config = Utility<T>.GetConfig(WorkDirectory);
            if (!config.SupportPlatfoms.Contains(platform))//检查是否支持该类型
                throw new Himall.Core.PlatformNotSupportedException(platform);

            if (config.OpenStatus.ContainsKey(platform))
                config.OpenStatus[platform] = enable;
            else
                config.OpenStatus.Add(platform, enable);
            Utility<T>.SaveConfig(config, WorkDirectory);
        }

        public IEnumerable<PlatformType> SupportPlatforms
        {
            get
            {
                T config = Utility<T>.GetConfig(WorkDirectory);
                var supportPlatforms = config.SupportPlatfoms;
                if (supportPlatforms == null || supportPlatforms.Count() == 0)//当未配置时，默认认为是PC
                {
                    config.SupportPlatfoms = new PlatformType[] { PlatformType.PC };
                    Utility<T>.SaveConfig(config, WorkDirectory);
                }
                return config.SupportPlatfoms;
            }
        }


        public bool IsEnable(PlatformType platform)
        {
            T config = Utility<T>.GetConfig(WorkDirectory);
            return config.OpenStatus.ContainsKey(platform) && config.OpenStatus[platform];
        }
        /// <summary>
        /// 退款入口
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        public virtual RefundFeeReturnModel ProcessRefundFee(PaymentPara para)
        {
            throw new PluginException("未实现此方法");
        }
        /// <summary>
        /// 处理退款返回结果
        /// </summary>
        /// <param name="context">请求</param>
        /// <exception cref="ApplicationException"></exception>
        /// <returns></returns>
        public virtual PaymentInfo ProcessRefundReturn(HttpRequestBase context)
        {
            throw new PluginException("未实现此方法");
        }

        /// <summary>
        /// 处理退款异步通知结果
        /// </summary>
        /// <param name="queryString">请求参数</param>
        /// <exception cref="ApplicationException"></exception>
        /// <returns></returns>
        public virtual PaymentInfo ProcessRefundNotify(HttpRequestBase context)
        {
            throw new PluginException("未实现此方法");
        }

        public virtual PaymentInfo EnterprisePay(EnterprisePayPara para)
        {
            throw new PluginException("未实现此方法");
        }
    }
}
