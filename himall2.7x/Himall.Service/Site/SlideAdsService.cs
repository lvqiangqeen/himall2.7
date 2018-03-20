using Himall.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Entity;
using Himall.Model;

namespace Himall.Service
{
    public class SlideAdsService : ServiceBase, ISlideAdsService
    {
        #region 手动轮播广告图片

        private void ResetHandSlideAdIndexFrom(long index)
        {
            var data = Context.HandSlideAdInfo.FindBy(s => s.DisplaySequence > index).OrderByDescending(s => s.DisplaySequence).ToList();
            for (int i = 0; i < data.Count(); i++)
            {
                if (i == data.Count() - 1)
                {
                    data[i].DisplaySequence = index;
                }
                else
                {
                    data[i].DisplaySequence = data[i + 1].DisplaySequence;
                }
            }
        }
        public IQueryable<Model.HandSlideAdInfo> GetHandSlidAds()
        {
            return Context.HandSlideAdInfo.FindAll().OrderBy(s => s.DisplaySequence);
        }

        public void AddHandSlidAd(Model.HandSlideAdInfo model)
        {
            var data = Context.HandSlideAdInfo;
            var index = data.Count() == 0 ? 0 : data.Max(s => s.DisplaySequence);

            //修改Model的Index
            model.DisplaySequence = index + 1;

            Context.HandSlideAdInfo.Add(model);
            Context.SaveChanges();
        }

        public void DeleteHandSlidAd(long id)
        {
            var index = Context.HandSlideAdInfo.FirstOrDefault(s => s.Id == id).DisplaySequence;
            Context.HandSlideAdInfo.Remove(Context.HandSlideAdInfo.FirstOrDefault(s => s.Id == id));
            ResetHandSlideAdIndexFrom(index);
            Context.SaveChanges();
        }

        public void UpdateHandSlidAd(Model.HandSlideAdInfo models)
        {
            var slide = Context.HandSlideAdInfo.FirstOrDefault(s => s.Id == models.Id);
            slide.ImageUrl = models.ImageUrl;
            slide.Url = models.Url;
            Context.SaveChanges();
        }

        public void AdjustHandSlidAdIndex(long id, bool direction)
        {
            var slide = Context.HandSlideAdInfo.FirstOrDefault(s => s.Id == id);
            if (direction)
            {
                var prev = Context.HandSlideAdInfo.FirstOrDefault(s => s.DisplaySequence == slide.DisplaySequence - 1);


                slide.DisplaySequence = slide.DisplaySequence - 1;
                prev.DisplaySequence = prev.DisplaySequence + 1;

            }
            else
            {
                var next = Context.HandSlideAdInfo.FirstOrDefault(s => s.DisplaySequence == slide.DisplaySequence + 1);


                slide.DisplaySequence = slide.DisplaySequence + 1;
                next.DisplaySequence = next.DisplaySequence - 1;
            }

            Context.SaveChanges();
        }


        public Model.HandSlideAdInfo GetHandSlidAd(long id)
        {
            return Context.HandSlideAdInfo.FirstOrDefault(s => s.Id == id);
        }

        #endregion

        #region 自动轮播广告图片
        public IQueryable<Model.SlideAdInfo> GetSlidAds(long shopId, SlideAdInfo.SlideAdType type)
        {
            return Context.SlideAdInfo.Where(s => s.ShopId == shopId && s.TypeId == type).OrderBy(t => t.DisplaySequence);
        }

        public void AddSlidAd(Model.SlideAdInfo model)
        {
            string imgUrl = string.Empty;
            var data = Context.SlideAdInfo.Where(s => s.ShopId == model.ShopId);
            var index = data.Count() == 0 ? 0 : data.Max(s => s.DisplaySequence);
            if ((model.TypeId == SlideAdInfo.SlideAdType.VShopHome || model.TypeId == SlideAdInfo.SlideAdType.WeixinHome)
                && Context.SlideAdInfo.Where(item => item.ShopId == model.ShopId && item.TypeId == model.TypeId).Count() + 1 > 5)
                throw new Himall.Core.HimallException("最多只能添加5张轮播图");

            //修改Model的Index
            model.DisplaySequence = index + 1;

            Context.SlideAdInfo.Add(model);
            Context.SaveChanges();
            imgUrl = model.ImageUrl;
            imgUrl = MoveImages(ref imgUrl, model.TypeId, model.ShopId);
            model.ImageUrl = imgUrl;
            Context.SaveChanges();
        }

        /// <summary>
        /// 添加APP引导页
        /// </summary>
        /// <param name="models"></param>
        public void AddGuidePages(List<Model.SlideAdInfo> models)
        {
            Context.Database.ExecuteSqlCommand("delete FROM Himall_SlideAds  where TypeId=12");
            Context.SlideAdInfo.AddRange(models);
            Context.SaveChanges();         
        }

        private void ResetSlideAdIndexFrom(long shopId, long index)
        {
            var data = Context.SlideAdInfo.FindBy(s => s.DisplaySequence > index && s.ShopId == shopId).
                OrderByDescending(s => s.DisplaySequence).ToList();
            for (int i = 0; i < data.Count(); i++)
            {
                if (i == data.Count() - 1)
                {
                    data[i].DisplaySequence = index;
                }
                else
                {
                    data[i].DisplaySequence = data[i + 1].DisplaySequence;
                }
            }
        }

        public void DeleteSlidAd(long shopId, long id)
        {
            var index = Context.SlideAdInfo.FirstOrDefault(s => s.ShopId == shopId && s.Id == id).DisplaySequence;
            Context.SlideAdInfo.Remove(Context.SlideAdInfo.FirstOrDefault(s => s.ShopId == shopId && s.Id == id));
            ResetSlideAdIndexFrom(shopId, index);
            Context.SaveChanges();
        }

        public void UpdateSlidAd(Model.SlideAdInfo models)
        {
            string imgUr = string.Empty;
            var slide = Context.SlideAdInfo.FirstOrDefault(s => s.ShopId == models.ShopId && s.Id == models.Id);
            slide.Description = models.Description;
            slide.ImageUrl = models.ImageUrl;
            slide.Url = models.Url;
            Context.SaveChanges();

            imgUr = models.ImageUrl;
            imgUr = MoveImages(ref imgUr, models.TypeId, models.ShopId);
            slide.ImageUrl = imgUr;
            Context.SaveChanges();
        }

        public void AdjustSlidAdIndex(long shopId, long id, bool direction, SlideAdInfo.SlideAdType type)
        {
            var slide = Context.SlideAdInfo.FirstOrDefault(s => s.ShopId == shopId && s.Id == id && s.TypeId == type);
            if (direction)
            {
                //var prev = context.SlideAdInfo.FirstOrDefault(s => s.ShopId == shopId && s.DisplaySequence == slide.DisplaySequence - 1 && s.TypeId == type);
                var prev = Context.SlideAdInfo.OrderByDescending(s => s.DisplaySequence).FirstOrDefault(s => s.ShopId == shopId && s.TypeId == type && s.DisplaySequence < slide.DisplaySequence);
                if (null != prev)
                {

                    slide.DisplaySequence = slide.DisplaySequence - 1;
                    prev.DisplaySequence = prev.DisplaySequence + 1;
                }

            }
            else
            {
                var next = Context.SlideAdInfo.OrderBy(s => s.DisplaySequence).FirstOrDefault(s => s.ShopId == shopId && s.TypeId == type && s.DisplaySequence > slide.DisplaySequence);

                if (null != next)
                {
                    slide.DisplaySequence = slide.DisplaySequence + 1;
                    next.DisplaySequence = next.DisplaySequence - 1;
                }
            }

            Context.SaveChanges();
        }


        public Model.SlideAdInfo GetSlidAd(long shopId, long id)
        {
            return Context.SlideAdInfo.FirstOrDefault(s => s.ShopId == shopId && s.Id == id);
        }

        #endregion


        #region 普通广告图片

        public void UpdateImageAd(ImageAdInfo model)
        {
            var imageAd = Context.ImageAdInfo.FirstOrDefault(i => i.ShopId == model.ShopId && i.Id == model.Id);
            if (null != imageAd && imageAd.Id == model.Id)
            {
                imageAd.ImageUrl = model.ImageUrl;
                imageAd.Url = model.Url;
            }
            Context.SaveChanges();

        }

        public ImageAdInfo GetImageAd(long shopId, long id)
        {
            return Context.ImageAdInfo.FirstOrDefault(item => item.Id == id && item.ShopId == shopId);
        }

        public IEnumerable<ImageAdInfo> GetImageAds(long shopId, Himall.CommonModel.ImageAdsType? ImageAdsType = Himall.CommonModel.ImageAdsType.Initial)
        {
            IEnumerable<ImageAdInfo> imageAdInfos = Context.ImageAdInfo.FindBy(i => i.ShopId == shopId).OrderBy(i => i.Id);
            if (ImageAdsType != Himall.CommonModel.ImageAdsType.Initial)
            {
                imageAdInfos = imageAdInfos.Where(item => item.TypeId == ImageAdsType);
            }
            if (imageAdInfos.Count() == 0)
            {
                ImageAdInfo[] newImageAdInfos = new ImageAdInfo[5];
                for (int i = 0; i < 4; i++)
                {
                    newImageAdInfos[i] = new ImageAdInfo() { ImageUrl = "", ShopId = shopId, Url = "", IsTransverseAD = false };
                }
                newImageAdInfos[4] = new ImageAdInfo() { ImageUrl = "", ShopId = shopId, Url = "", IsTransverseAD = true };
                Context.ImageAdInfo.AddRange(newImageAdInfos);
                Context.SaveChanges();
                imageAdInfos = newImageAdInfos;
            }
            if (!imageAdInfos.Any(p => p.IsTransverseAD))
            {
                ImageAdInfo info = new ImageAdInfo { ImageUrl = "", ShopId = shopId, Url = "", IsTransverseAD = true };
                Context.ImageAdInfo.Add(info);
                Context.SaveChanges();
            }
            return imageAdInfos;
        }

        #endregion


        public void UpdateWeixinSlideSequence(long shopId, long sourceSequence, long destiSequence, SlideAdInfo.SlideAdType type)
        {
            var souce = Context.SlideAdInfo.FirstOrDefault(item => item.DisplaySequence == sourceSequence && item.ShopId == shopId && item.TypeId == type);
            var destination = Context.SlideAdInfo.FirstOrDefault(item => item.DisplaySequence == destiSequence && item.ShopId == shopId && item.TypeId == type);
            souce.DisplaySequence = destiSequence;
            destination.DisplaySequence = sourceSequence;
            Context.SaveChanges();
        }


        /// <summary>
        /// 图片转移
        /// </summary>
        /// <param name="topicId">专题编号</param>
        /// <param name="backGroundImage">临时背景图地址，返回正式地址</param>
        /// <param name="topImage">临时top图地址，返回正式地址</param>
        /// <returns>专题图片目录</returns>
        string MoveImages(ref string backGroundImage, SlideAdInfo.SlideAdType type, long shopId)
        {
            string imageDir = string.Empty;
            string relativeDir = string.Empty;
            if (type == SlideAdInfo.SlideAdType.WeixinHome)
            {
                //转移图片
                imageDir = "/Storage/Plat/Weixin/SlidAd/";
                relativeDir = "/Storage/Plat/Weixin/SlidAd/";
            }

            if (type == SlideAdInfo.SlideAdType.VShopHome)
            {
                imageDir = "/Storage/Shop/" + shopId + "/VShop/";
                relativeDir = "/Storage/Shop/" + shopId + "/VShop/";

            }
            if (type == SlideAdInfo.SlideAdType.IOSShopHome)
            {
                imageDir = "/Storage/Plat/APP/SlidAd/";
                relativeDir = "/Storage/Plat/APP/SlidAd/";
            }
            backGroundImage = backGroundImage.Replace("\\", "/");

            if (!string.IsNullOrWhiteSpace(imageDir))//只有在临时目录中的图片才需要复制
            {
                if (backGroundImage.Contains("/temp/"))
                {
                    if (!string.IsNullOrWhiteSpace(backGroundImage))
                    {
                        string logoname = backGroundImage.Substring(backGroundImage.LastIndexOf('/') + 1);
                        string oldlogo = backGroundImage.Substring(backGroundImage.LastIndexOf("/temp"));
                        string newLogo = imageDir + logoname;
                        Core.HimallIO.CopyFile(oldlogo, newLogo, true);
                        backGroundImage = newLogo;
                    }
                }
                else if (backGroundImage.Contains("/Storage/"))
                {
                    backGroundImage = backGroundImage.Substring(backGroundImage.LastIndexOf("/Storage"));
                }
            }

            return backGroundImage;
        }
    }
}
