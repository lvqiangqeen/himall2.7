using System.Configuration;

namespace Himall.Model
{
    public partial class TopicInfo
    {

        public string TopImage
        {
            get { return Core.HimallIO.GetImagePath(topImage); }
            set
            {
                if (!string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(ImageServerUrl))
                    topImage = value.Replace(ImageServerUrl, "");
                else
                    topImage = value;
            }
        }


        public string BackgroundImage
        {
            get { return  Core.HimallIO.GetImagePath(backgroundImage); }
            set
            {
                if (!string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(ImageServerUrl))
                    backgroundImage = value.Replace(ImageServerUrl, "");
                else
                    backgroundImage = value;
            }
        }


        public string FrontCoverImage
        {
            get { return  Core.HimallIO.GetImagePath(frontCoverImage); }
            set
            {
                if (!string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(ImageServerUrl))
                    frontCoverImage = value.Replace(ImageServerUrl, "");
                else
                    frontCoverImage = value;
            }
        }


    }
}
