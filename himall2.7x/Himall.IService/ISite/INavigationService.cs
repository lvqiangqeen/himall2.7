using Himall.Core;
using Himall.Model;
using System.Linq;

namespace Himall.IServices
{
    public interface INavigationService : IService
    {
        /// <summary>
        /// 开关一个平台导航
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        void OpenOrClose(long id, bool status);

        /// <summary>
        /// 添加一个平台导航
        /// </summary>
        /// <param name="model"></param>
        void AddPlatformNavigation(BannerInfo model);

        /// <summary>
        /// 添加一个平台导航
        /// </summary>
        /// <param name="model"></param>
        void AddSellerNavigation(BannerInfo model);

        /// <summary>
        /// 编辑一个平台导航
        /// </summary>
        /// <param name="model"></param>
        void UpdatePlatformNavigation(BannerInfo model);

        /// <summary>
        /// 编辑一个平台导航
        /// </summary>
        /// <param name="model"></param>
        void UpdateSellerNavigation(BannerInfo model);

        /// <summary>
        /// 删除一个平台导航
        /// </summary>
        /// <param name="id"></param>
        void DeletePlatformNavigation(long id);

        /// <summary>
        /// 删除一个店铺导航
        /// </summary>
        /// <param name="id"></param>
        void DeleteSellerformNavigation(long shopId, long id);

        /// <summary>
        /// 交换两个导航的排序
        /// </summary>
        /// <param name="id"></param>
        /// <param name="id2"></param>
        void SwapPlatformDisplaySequence(long id, long id2);

        /// <summary>
        /// 交换两个导航的排序
        /// </summary>
        /// <param name="shopId">店铺id</param>
        /// <param name="id"></param>
        /// <param name="id2"></param>
        void SwapSellerDisplaySequence(long shopId,long id, long id2);

        /// <summary>
        /// 获取平台导航列表
        /// </summary>
        /// <returns></returns>
        IQueryable<BannerInfo> GetPlatNavigations();

      /// <summary>
      /// 获取商家导航列表
      /// </summary>
      /// <param name="shopid">店铺ID</param>
      /// <returns></returns>
        IQueryable<BannerInfo> GetSellerNavigations(long shopId, PlatformType plat=PlatformType.PC);

        /// <summary>
        /// 获取商家单个导航
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        BannerInfo GetSellerNavigation(long id);
    }
}
