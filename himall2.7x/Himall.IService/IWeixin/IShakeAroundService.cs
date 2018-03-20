using Senparc.Weixin.MP;
using Senparc.Weixin.MP.AdvancedAPIs.ShakeAround;
using Senparc.Weixin.MP.AppStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.IServices
{
    public interface IShakeAroundService : IService
    {
        #region 设备管理
        DeviceListResultJson UnauthorizedTest();

        void init( string appid , string secret );

        bool AddDevice( int quantity , string applyReason , string comment = null , long? poiId = null );

        bool UpdateDevice( long deviceId , string uuId , long major , long minor , string comment , long poiId = 0 );

        List<DeviceModel> GetDeviceAll();

        DeviceList2ResultJson GetDeviceAll( int page , int rows );

        DeviceModel GetDeviceById( long id );

        bool DeviceBindLocatoin( long deviceId , string uuId , long major , long minor , long poiId );

        string UploadImage( string file );
        #endregion

        #region 页面管理

        bool AddPage( string title , string description , string pageUrl , string iconUrl , string comment = null );

        bool UpdatePage( long pageId , string title , string description , string pageUrl , string iconUrl , string comment = null );

        bool DeletePage( List<long> ids );

        SearchPages_Data GetPageAll();
        SearchPages_Data GetPageAll( int page , int rows );

        List<SearchPages_Data_Page> GetPageById( long[] ids );
        #endregion

        #region 配置关系

        List<long> GetPageids( DeviceModel model );

        bool SetRelationship( DeviceApply_Data_Device_Identifiers deviceIdentifier , long[] pageIds , ShakeAroundBindType type );
        #endregion

    }
}
