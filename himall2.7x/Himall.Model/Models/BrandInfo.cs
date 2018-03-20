using System.ComponentModel;
using System.Configuration;

namespace Himall.Model
{
    public partial class BrandInfo
    {
        /// <summary>
        /// 品牌Logo
        /// </summary>
        public string Logo
        {
            get { return ImageServerUrl + logo; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(ImageServerUrl))
                    logo = value.Replace(ImageServerUrl, "");
                else
                    logo = value;
            }
        }
    }
}
