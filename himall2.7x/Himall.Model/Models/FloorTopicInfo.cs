using System.Configuration;

namespace Himall.Model
{
    public partial  class FloorTopicInfo
    {
        //topicImage

        /// <summary>
        /// 专题图片
        /// </summary>
        public string TopicImage
        {
            get { return ImageServerUrl + topicImage; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(ImageServerUrl))
                    topicImage = value.Replace(ImageServerUrl, "");
                else
                    topicImage = value;
            }
        }
    }
}
