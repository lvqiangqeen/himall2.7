using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Service.Market.Business
{
    interface IGenerateDetail
    {
        /// <summary>
        /// 生成红包详情
        /// </summary>
        /// <param name="bounsId">红包ID</param>
        /// <param name="totalPrice">红包总额</param>
        void Generate( long bounsId , decimal totalPrice );
    }
}
