
using System.Configuration;
namespace Himall.Model
{
    public partial class HomeCategoryRowInfo
    {

        public string Image1
        {
            get { return ImageServerUrl + image1; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(ImageServerUrl))
                    image1 = value.Replace(ImageServerUrl, "");
                else
                    image1 = value;
            }
        }


        public string Image2
        {
            get { return ImageServerUrl + image2; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(ImageServerUrl))
                    image2= value.Replace(ImageServerUrl, "");
                else
                    image2 = value;
            }
        }
    }
}
