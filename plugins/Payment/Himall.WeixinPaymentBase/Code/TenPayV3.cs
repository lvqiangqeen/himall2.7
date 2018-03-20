using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Himall.WeixinPaymentBase
{
    /// <summary>
    /// 微信支付接口，官方API：https://mp.weixin.qq.com/paymch/readtemplate?t=mp/business/course2_tmpl&lang=zh_CN&token=25857919#4
    /// </summary>
    public static class TenPayV3
    {
        /// <summary>
        /// 统一支付接口
        /// 统一支付接口，可接受JSAPI/NATIVE/APP 下预支付订单，返回预支付订单号。NATIVE 支付返回二维码code_url。
        /// </summary>
        /// <param name="data">微信支付需要post的xml数据</param>
        /// <returns></returns>
        public static string Unifiedorder(string data)
        {
            var urlFormat = "https://api.mch.weixin.qq.com/pay/unifiedorder";

            var formDataBytes = data == null ? new byte[0] : Encoding.UTF8.GetBytes(data);
            MemoryStream ms = new MemoryStream();
            ms.Write(formDataBytes, 0, formDataBytes.Length);
            ms.Seek(0, SeekOrigin.Begin);//设置指针读取位置
            return RequestUtility.HttpPost(urlFormat, null, ms);
        }
        public static string Refund(string data, string cert, string password)
        {
            string url = "https://api.mch.weixin.qq.com/secapi/pay/refund";

            var formDataBytes = data == null ? new byte[0] : Encoding.UTF8.GetBytes(data);
            MemoryStream ms = new MemoryStream();
            ms.Write(formDataBytes, 0, formDataBytes.Length);
            ms.Seek(0, SeekOrigin.Begin);//设置指针读取位置
            
            return RequestUtility.HttpPost(url, cert, password, null, ms);
        }

        public static string transfers(string data, string cert, string password)
        {
            string url = "https://api.mch.weixin.qq.com/mmpaymkttransfers/promotion/transfers";

            var formDataBytes = data == null ? new byte[0] : Encoding.UTF8.GetBytes(data);
            MemoryStream ms = new MemoryStream();
            ms.Write(formDataBytes, 0, formDataBytes.Length);
            ms.Seek(0, SeekOrigin.Begin);//设置指针读取位置

            return RequestUtility.HttpPost(url, cert, password, null, ms);
        }
    }
}
