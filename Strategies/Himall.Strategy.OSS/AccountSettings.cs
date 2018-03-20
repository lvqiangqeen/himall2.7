namespace Himall.Strategy
{
    internal class AccountSettings
    {
        public string OssEndpoint { get; set; }
        public string OssAccessKeyId { get; set; }
        public string OssAccessKeySecret { get; set; }

        private AccountSettings()
        {
        }

        public static AccountSettings Load()
        {
            //初始化
            var accountSettings = new AccountSettings();
            accountSettings.OssAccessKeyId = Config.AccessKeyId;
            accountSettings.OssAccessKeySecret = Config.AccessKeySecret;
            accountSettings.OssEndpoint = Config.PrivateEndpoint;
            return accountSettings;
        }



    }
}
