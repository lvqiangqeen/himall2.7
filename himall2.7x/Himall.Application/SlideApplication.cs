using Himall.Core;
using Himall.DTO;
using Himall.IServices;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Application
{
    public class SlideApplication
    {
        private static ISlideAdsService _iSlideAdsService = ObjectContainer.Current.Resolve<ISlideAdsService>();
        public static void AddGuidePages(List<SlideAdModel> model)
        {
            var guides = model.Where(a => !string.IsNullOrWhiteSpace(a.ImageUrl)).ToList();
            List<SlideAdInfo> Ads = new List<SlideAdInfo>();
            var index = 1;
            foreach (var m in model)
            {
                var ad = new Himall.Model.SlideAdInfo();
                var pic = m.ImageUrl;
                if (!string.IsNullOrWhiteSpace(pic))
                {
                    //转移图片
                    if (pic.Contains("/temp/"))
                    {
                        string source = pic.Substring(pic.LastIndexOf("/temp"));
                        string dest = @"/Storage/Plat/ImageAd/";
                        pic = Path.Combine(dest, Path.GetFileName(source));
                        Core.HimallIO.CopyFile(source, pic, true);
                    }
                    else if (pic.Contains("/Storage/"))
                    {
                        pic = pic.Substring(pic.LastIndexOf("/Storage"));
                    }
                }
                ad.ImageUrl = pic;
                ad.TypeId = SlideAdInfo.SlideAdType.AppGuide;
                ad.ShopId = 0;
                ad.DisplaySequence = index;
                ad.Url = string.Empty;
                index++;
                Ads.Add(ad);
            }
            _iSlideAdsService.AddGuidePages(Ads);
        }


        public static  List<Himall.DTO.SlideAdModel>  GetGuidePages()
        {
            var models = _iSlideAdsService.GetSlidAds(0, SlideAdInfo.SlideAdType.AppGuide).OrderBy(a=>a.DisplaySequence).ToList();
            var m = models.Select(a => new Himall.DTO.SlideAdModel() { Id = a.Id, ShopId = a.ShopId,
                DisplaySequence = a.DisplaySequence, ImageUrl = a.ImageUrl,TypeId=a.TypeId }).ToList();
            return m;
        }
    }
}
