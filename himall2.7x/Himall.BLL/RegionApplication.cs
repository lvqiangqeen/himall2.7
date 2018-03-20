using Himall.Core;
using Himall.IServices;

namespace Himall.Application
{
    public class RegionApplication
    {
        private static IRegionService _iRegionService = ObjectContainer.Current.Resolve<IRegionService>();

        /// <summary>
        /// 获取省 市 区 的编号，中间用逗号隔开
        /// </summary>
        /// <param name="regionId"></param>
        /// <returns></returns>
        public static string GetRegionIdPath(long regionId)
        {
            return _iRegionService.GetRegionIdPath(regionId);
        }
    }
}
