using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.IServices
{
    public interface IPhotoSpaceService : IService
    {
        /// <summary>
        /// 添加新图片分类
        /// </summary>
        /// <param name="name"></param>
        PhotoSpaceCategoryInfo AddPhotoCategory(string name,long shopId=0);
        /// <summary>
        /// 移动图片到新另一个分类
        /// </summary>
        /// <param name="pList"></param>
        /// <param name="pTypeId"></param>
        void MovePhotoType(List<long> pList, int pTypeId,long shopId=0);
        /// <summary>
        /// 修改图片分类名称
        /// </summary>
        /// <param name="photoCategorys"></param>
        void UpdatePhotoCategories(Dictionary<long, string> photoCategorys,long shopId=0);
        /// <summary>
        /// 删除图片分类
        /// </summary>
        /// <param name="categoryId"></param>
        void DeletePhotoCategory(long categoryId,long shopId=0);

        /// <summary>
        /// 获取所有图片分类
        /// </summary>
        /// <returns></returns>
        IQueryable<PhotoSpaceCategoryInfo> GetPhotoCategories(long shopId=0);

        /// <summary>
        /// 获取图片列表
        /// </summary>
        /// <param name="keyword">图片名称关键字</param>
        /// <param name="categoryId">分类编号</param>
        /// <param name="pageIndex">页号</param>
        /// <param name="order">排序类型</param>
        /// <returns></returns>
        ObsoletePageModel<PhotoSpaceInfo> GetPhotoList(string keyword, int pageIndex, int pageSize, int order, long categoryId = 0, long shopId = 0);

        /// <summary>
        /// 添加图片
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="photoName"></param>
        /// <param name="photoPath"></param>
        /// <param name="fileSize"></param>
        /// <returns></returns>
        void AddPhote(long categoryId, string photoName, string photoPath, int fileSize, long shopId = 0);

        /// <summary>
        /// 删除图片
        /// </summary>
        /// <param name="photoId"></param>
        /// <param name="shopId"></param>
        void DeletePhoto(long photoId, long shopId = 0);

        /// <summary>
        /// 图片改名
        /// </summary>
        /// <param name="photoId"></param>
        /// <param name="newName"></param>
        /// <param name="shopId"></param>
        void RenamePhoto(long photoId, string newName, long shopId=0);

        /// <summary>
        /// 获取图片路径
        /// </summary>
        /// <param name="photoId"></param>
        /// <returns></returns>
        string GetPhotoPath(long photoId, long shopId = 0);

        /// <summary>
        /// 统计图片个数
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        int GetPhotoCount(long shopId = 0);

        /// <summary>
        /// 统计默认分类的图片个数
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        int GetDefaultPhotoCount(long shopId = 0);



    }
}
