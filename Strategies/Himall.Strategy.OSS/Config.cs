using Himall.Core;
using Himall.Strategy;
using System.Configuration;

namespace Himall.Strategy
{
    /// <summary>
    /// 配置类
    /// </summary>
    public static class Config
    {
        /// <summary>
        /// OSS 内网Endpoint地址
        /// </summary>
        public static readonly string PrivateEndpoint = ConfigurationManager.AppSettings["PrivateEndpoint"];

        /// <summary>
        /// 文件服务器域名（即OSS外网域名）
        /// </summary>
        public static readonly string FileServerDomain = ConfigurationManager.AppSettings["FileServerDomain"];

        /// <summary>
        /// 阿里云AccessKeyId
        /// </summary>
        public static readonly string AccessKeyId = ConfigurationManager.AppSettings["AccessKeyId"];

        /// <summary>
        /// 阿里云AccessKeySecret
        /// </summary>
        public static readonly string AccessKeySecret = ConfigurationManager.AppSettings["AccessKeySecret"];

        /// <summary>
        /// OSS BucketName
        /// </summary>
        public static readonly string BucketName = ConfigurationManager.AppSettings["BucketName"];

        /// <summary>
        /// 图片服务器域名（开通OSS图片服务后给出的图片服务域名）
        /// </summary>
        public static readonly string ImageServerDomain = ConfigurationManager.AppSettings["ImageServerDomain"];

        static Config()
        {
            if (string.IsNullOrWhiteSpace(PrivateEndpoint))
                throw new HimallIOException("未配置PrivateEndpoint节点");
            if (string.IsNullOrWhiteSpace(FileServerDomain))
                throw new HimallIOException("未配置FileServerDomain节点");
            if (string.IsNullOrWhiteSpace(AccessKeyId))
                throw new HimallIOException("未配置AccessKeyId节点");
            if (string.IsNullOrWhiteSpace(AccessKeySecret))
                throw new HimallIOException("未配置AccessKeySecret节点");
            if (string.IsNullOrWhiteSpace(BucketName))
                throw new HimallIOException("未配置BucketName节点");
        }

    }
}
