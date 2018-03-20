using Himall.Entity;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Service.Market.Business
{
    class RandomlyGeneration : IGenerateDetail
    {
        private  decimal _randomAmountStart = 0;
        private  decimal _randomAmountEnd = 0;
        private Entities _context = new Entities();

        static long tick = DateTime.Now.Ticks;
        private Random _random = new Random( ( int )( tick & 0xffffffffL ) | ( int )( tick >> 32 ) );

        public RandomlyGeneration( decimal randomAmountStart , decimal randomAmountEnd )
        {
            this._randomAmountStart = randomAmountStart;
            this._randomAmountEnd = randomAmountEnd;
        }


        /*
         * 算法逻辑：
         * 1 产生随机金额后添加一个红包详情，总金额递减
         * 2 如果当前总金额 <= 随机金额+设定的最大随机金额，证明后续生成的红包不会超过2个
         *    2.1 如果随机金额 <= 设定的最大随机金额，证明已是最后一次生成随机红包
         *    2.2 如果随机金额 >= 设定的起始金额，进行平均分配，作为最后两个红包的金额
         */
        //public void Generate( long bounsId , decimal totalPrice )
        //{
        //    try
        //    {
        //        List<BonusReceiveInfo> list = new List<BonusReceiveInfo>();
        //        while( totalPrice != 0 )
        //        {
        //            decimal randomAmount = GenerateRandomAmountPrice();

        //            if( totalPrice <= randomAmount + this._randomAmountEnd )
        //            {
        //                if( totalPrice <= this._randomAmountEnd )  //剩余金额不够
        //                {
        //                    randomAmount = totalPrice;
        //                }
        //                else if( randomAmount >= this._randomAmountStart )
        //                {
        //                    randomAmount = totalPrice / 2;
        //                }

        //            }

        //            totalPrice -= randomAmount;

        //            BonusReceiveInfo detail = new BonusReceiveInfo
        //            {
        //                BonusId = bounsId ,
        //                Price = randomAmount ,
        //                IsShare = false ,
        //                OpenId = null
        //            };

        //            list.Add( detail );
        //            if( list.Count >= 500 )  //500个一次批量提交
        //            {
        //                this._context.BonusReceiveInfo.AddRange( list );
        //                this._context.SaveChanges();
        //                list.Clear();
        //            }
        //        }
        //        this._context.BonusReceiveInfo.AddRange( list );
        //        this._context.SaveChanges();
        //    }
        //    catch
        //    {
        //        this._context.BonusReceiveInfo.Remove( p => p.BonusId == bounsId );
        //        this._context.BonusInfo.Remove( p => p.Id == bounsId );
        //        this._context.SaveChanges();
        //    }
        //    finally
        //    {
        //        this._context.Dispose();
        //    }
        //}

        public void Generate( long bounsId , decimal totalPrice )
        {
            try
            {
                List<BonusReceiveInfo> list = new List<BonusReceiveInfo>();
                while( totalPrice != 0 )
                {
                    decimal randomAmount = 0;
                    if( totalPrice <= this._randomAmountEnd && this._randomAmountEnd > this._randomAmountStart )
                    {
                        this._randomAmountEnd = totalPrice;
                    }

                    randomAmount = GenerateRandomAmountPrice();

                    if( totalPrice - randomAmount < this._randomAmountStart )
                    {
                        randomAmount = totalPrice;
                    }

                    totalPrice -= randomAmount;

                    BonusReceiveInfo detail = new BonusReceiveInfo
                    {
                        BonusId = bounsId ,
                        Price = randomAmount ,
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

        /// <summary>
        /// 获取随机金额
        /// </summary>
        private decimal GenerateRandomAmountPrice()
        {

            decimal temp = _random.Next( ( int )this._randomAmountStart , ( int )this._randomAmountEnd );
            string startF = String.Format( "{0:N2} " , this._randomAmountStart );
            string endF = String.Format( "{0:N2} " , this._randomAmountEnd );
            startF = startF.Substring( startF.IndexOf( '.' ) + 1 , 2 );//小数位
            endF = endF.Substring( endF.IndexOf( '.' ) + 1 , 2 );//小数位
            if (this._randomAmountStart!=this._randomAmountEnd )
            {
                if (int.Parse(startF) == 0)
                {
                    startF = "1";
                }
                if (int.Parse(endF) < int.Parse(startF))
                {
                    endF = "100";
                }
            }
            
            decimal tempF = ( decimal )_random.Next( int.Parse( startF ) , int.Parse( endF ) ) / 100;

            temp += tempF;
            return temp;
        }


    }
}
