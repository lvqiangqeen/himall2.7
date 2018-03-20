using Himall.Entity;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Service.Market.Business
{
    class FixedGeneration : IGenerateDetail
    {
        private readonly decimal _fixedAmount = 0;
        private Entities _context = new Entities();

        public FixedGeneration( decimal fixedAmount )
        {
            this._fixedAmount = fixedAmount;
        }

        public void Generate( long bounsId , decimal totalPrice )
        {
            try
            {
                //红包个数
                int detailCount = ( int )( totalPrice / this._fixedAmount );
                List<BonusReceiveInfo> list = new List<BonusReceiveInfo>();
                //生成固定数量个红包
                for( int i = 0 ; i < detailCount ; i++ )
                {
                    BonusReceiveInfo detail = new BonusReceiveInfo
                    {
                        BonusId = bounsId ,
                        Price = this._fixedAmount ,
                        IsShare = false ,
                        OpenId = null
                    };
                    list.Add( detail );
                    if( list.Count >= 500 )  //500个一次批量提交
                    {
                        this._context.BonusReceiveInfo.AddRange( list );
                        this._context.SaveChanges();
                        list.Clear();
                    }
                }
                this._context.BonusReceiveInfo.AddRange( list );
                this._context.SaveChanges();
            }
            catch
            {
                this._context.BonusReceiveInfo.Remove( p => p.BonusId == bounsId );
                this._context.BonusInfo.Remove( p => p.Id == bounsId );
                this._context.SaveChanges();
            }
            finally
            {
                this._context.Dispose();
            }
        }
    }
}
