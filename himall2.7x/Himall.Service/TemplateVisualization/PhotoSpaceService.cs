using Himall.IServices;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Entity;

namespace Himall.Service
{
    public class PhotoSpaceService : ServiceBase, IPhotoSpaceService
    {

        public PhotoSpaceCategoryInfo AddPhotoCategory(string name, long shopId = 0)
        {
            var photoSpace = Context.PhotoSpaceCategoryInfo.Add(new Model.PhotoSpaceCategoryInfo
            {
                PhotoSpaceCatrgoryName = name,
                ShopId = shopId,
                DisplaySequence = Context.PhotoSpaceCategoryInfo.Select(p => p.DisplaySequence).DefaultIfEmpty().Max() + 1
            });
            Context.SaveChanges();
            return photoSpace;
        }

        public void MovePhotoType(List<long> pList, int pTypeId, long shopId = 0)
        {
            var oldPhotos = Context.PhotoSpaceInfo.Where(p => pList.Contains(p.Id) && p.ShopId == shopId);
            foreach (var item in oldPhotos)
            {
                item.PhotoCategoryId = pTypeId;
                item.LastUpdateTime = DateTime.Now;
            }
            Context.SaveChanges();
        }

        public void UpdatePhotoCategories(Dictionary<long, string> photoCategorys, long shopId = 0)
        {
            foreach (var item in photoCategorys)
            {
                var cate = Context.PhotoSpaceCategoryInfo.FirstOrDefault(p => p.Id.Equals(item.Key) && p.ShopId == shopId);
                if (null != cate)
                {
                    cate.PhotoSpaceCatrgoryName = item.Value;
                }
            }
            Context.SaveChanges();
        }

        public void DeletePhotoCategory(long categoryId, long shopId = 0)
        {
            var photoCate = Context.PhotoSpaceCategoryInfo.FirstOrDefault(p => p.Id.Equals(categoryId) && p.ShopId.Equals(shopId));
            if (null != photoCate)
            {
                Context.PhotoSpaceCategoryInfo.Remove(photoCate);
            }
            Context.SaveChanges();
        }

        public IQueryable<Model.PhotoSpaceCategoryInfo> GetPhotoCategories(long shopId = 0)
        {
            return Context.PhotoSpaceCategoryInfo.Where(p => p.ShopId == shopId);
        }

        public ObsoletePageModel<Model.PhotoSpaceInfo> GetPhotoList(string keyword, int pageIndex, int pageSize, int order, long categoryId = 0, long shopId = 0)
        {
            IQueryable<PhotoSpaceInfo> complaints = Context.PhotoSpaceInfo.AsQueryable();

            #region 条件组合
            complaints = complaints.Where(item => shopId == item.ShopId);
            if (categoryId != 0)
            {
                complaints = complaints.Where(item => item.PhotoCategoryId == categoryId);
            }
            if (!string.IsNullOrEmpty(keyword))
            {
                complaints = complaints.Where(item => item.PhotoName.Contains(keyword));
            }
            #endregion

            int total;
            complaints = complaints.GetPage(out total, p => p.OrderByDescending(o => o.UploadTime.Value), pageIndex, pageSize);

            ObsoletePageModel<PhotoSpaceInfo> pageModel = new ObsoletePageModel<PhotoSpaceInfo>() { Models = complaints, Total = total };
            return pageModel;
        }

        public void AddPhote(long categoryId, string photoName, string photoPath, int fileSize, long shopId = 0)
        {
            Context.PhotoSpaceInfo.Add(new Model.PhotoSpaceInfo
            {
                FileSize = fileSize,
                LastUpdateTime = DateTime.Now,
                UploadTime = DateTime.Now,
                PhotoCategoryId = categoryId,
                PhotoName = photoName,
                PhotoPath = photoPath,
                ShopId = shopId
            });
            Context.SaveChanges();
        }

        public void DeletePhoto(long photoId, long shopId = 0)
        {
            var photo = Context.PhotoSpaceInfo.FirstOrDefault(p => p.Id.Equals(photoId) && p.ShopId.Equals(shopId));
            if (null != photo)
            {
                Context.PhotoSpaceInfo.Remove(photo);
            }
            Context.SaveChanges();
        }

        public void RenamePhoto(long photoId, string newName, long shopId = 0)
        {
            var photo = Context.PhotoSpaceInfo.FirstOrDefault(p => p.Id.Equals(photoId) && p.ShopId.Equals(shopId));
            if (null != photo)
            {
                photo.PhotoName = newName;
            }
            Context.SaveChanges();
        }

        public string GetPhotoPath(long photoId, long shopId = 0)
        {
            throw new NotImplementedException();
        }

        public int GetPhotoCount(long shopId = 0)
        {
            throw new NotImplementedException();
        }

        public int GetDefaultPhotoCount(long shopId = 0)
        {
            throw new NotImplementedException();
        }
    }
}
