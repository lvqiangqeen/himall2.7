using Aliyun.OSS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Strategy
{
    internal static class OssClientFactory
    {
        public static IOss CreateOssClient()
        {
            return CreateOssClient(AccountSettings.Load());
        }

        public static IOss CreateOssClient(AccountSettings settings)
        {
            return new OssClient(settings.OssEndpoint,
                                 settings.OssAccessKeyId,
                                 settings.OssAccessKeySecret);
        }
    }
}
