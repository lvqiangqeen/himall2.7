using System.Configuration;

namespace Himall.Model
{
    public partial class ImageAdInfo
    {

        /// <summary>
        /// 图片路径
        /// </summary>
        public string ImageUrl
        {
            get { return ImageServerUrl + imageUrl; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(ImageServerUrl))
                    imageUrl = value.Replace(ImageServerUrl, "");
                else
                    imageUrl = value;
            }
        }
    }
}
