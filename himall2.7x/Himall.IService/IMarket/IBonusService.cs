using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.IServices
{
    public interface IBonusService : IService
    {
        /// <summary>
        ///  获取红包列表
        /// </summary>
        ObsoletePageModel<BonusInfo> Get( int type ,int state , string name , int pageIndex , int pageSize );

        /// <summary>
        /// 获取红包
        /// </summary>
        BonusInfo Get( long id );

        /// <summary>
        ///  获取红包详情
        /// </summary>
        ObsoletePageModel<BonusReceiveInfo> GetDetail( long bonusId , int pageIndex , int pageSize );

        /// <summary>
        ///  添加红包
        /// </summary>
        void Add( BonusInfo model , string baseAddress );

        /// <summary>
        ///  修改红包
        /// </summary>
        void Update( BonusInfo model );

        /// <summary>
        ///  红包失效
        /// </summary>
        void Invalid( long id );

        /// <summary>
        /// 领取活动红包
        /// </summary>
        object Receive( long id , string openId );


        /// <summary>
        /// 关注送红包
        /// </summary>
        string Receive( string openId );

        /// <summary>
        /// 获取某用户领取的金额
        /// </summary>
        decimal GetReceivePriceByOpendId( long id , string openId );

        /// <summary>
        /// 能否添加红包
        /// </summary>
        bool CanAddBonus();


        void SetShare( long id , string openId );
        /// <summary>
        /// 刮刮卡新增红包
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isShare"></param>
        /// <param name="userId"></param>
        /// <param name="openId"></param>
        void SetShareByUserId(long id, bool isShare, long userId);
        void DepositToRegister( long userid );
        /// <summary>
        /// 获取红包集合 
        /// </summary>
        IEnumerable<BonusInfo> GetBonusByType(BonusInfo.BonusType bonusType);

        /// <summary>
        /// 获取红包剩余数量
        /// </summary>
        string GetBonusSurplus(long bonusId);

        decimal GetReceivePriceByUserId(long id, long userId);

        /// <summary>
        /// 增加红包增加个数
        /// </summary>
        /// <param name="id">红包Id</param>
        void IncrReceiveCount(long id);
    }
}
