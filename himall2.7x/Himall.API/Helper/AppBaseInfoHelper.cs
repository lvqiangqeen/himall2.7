using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Himall.Core;
using Himall.Model;
using Himall.IServices;

namespace Himall.API.Helper
{
    public class AppBaseInfoHelper
    {
        private IAppBaseService _iAppBaseService;

        private string _AppKey { get; set; }
        /// <summary>
        /// Key
        /// </summary>
        public string AppKey
        {
            get
            {
                return _AppKey;
            }
        }
        private string _AppSecret { get; set; }
        /// <summary>
        /// 公钥
        /// </summary>
        public string AppSecret
        {
            get
            {
                return _AppSecret;
            }
        }
        public AppBaseInfoHelper(string appkey)
        {
            _iAppBaseService = Himall.ServiceProvider.Instance<IAppBaseService>.Create;
            _AppKey = appkey;
            if (string.IsNullOrWhiteSpace(_AppKey))
            {
                throw new HimallApiException(ApiErrorCode.Missing_App_Key, "app_key");
            }
            _AppSecret = "";
            try
            {
                _AppSecret = _iAppBaseService.GetAppSecret(_AppKey);
            }
            catch (Exception ex)
            {
                throw new HimallApiException(ApiErrorCode.Invalid_App_Key, "app_key");
            }
            if (string.IsNullOrWhiteSpace(_AppSecret))
            {
                throw new HimallApiException(ApiErrorCode.Insufficient_ISV_Permissions, "not set app_secreat");
            }
        }
    }
}
