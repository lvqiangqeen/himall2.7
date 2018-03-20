using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel
{

    /// <summary>
    /// 消费金额
    /// </summary>
    public enum AmountOfConsumption : int
    {
        /// <summary>
        /// 0-500
        /// </summary>
        [Description("0-500")]
        AmountOne = 0,

        /// <summary>
        /// 500-1000
        /// </summary>
        [Description("500-1000")]
        AmountTwo = 1,

        /// <summary>
        /// 1000-3000
        /// </summary>
        [Description("1000-3000")]
        AmountThree = 2,

        /// <summary>
        /// 3000+
        /// </summary>
        [Description("3000+")]
        AmountFour = 3,

        ///// <summary>
        ///// 200-300
        ///// </summary>
        //[Description("3000+")]
        //AmountFive = 4,
    }
}
