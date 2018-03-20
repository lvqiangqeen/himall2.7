using Himall.Model.Models;
using Senparc.Weixin.MP.AdvancedAPIs.Poi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.IServices
{
    public interface IPoiService : IService 
    {
        void init( string appid , string secret );

        bool AddPoi( CreateStoreData createStoreData );

        bool UpdatePoi( UpdateStoreData updateStoreData );

        bool DeletePoi( string poiId );

        string UploadImage( string filePath );

        GetStoreListResultJson GetPoiList( int page , int rows );

        List<GetStoreList_BaseInfo> GetPoiList();

        GetStoreBaseInfo GetPoi( string poiId );

        List<WXCategory> GetCategory();

    }
}
