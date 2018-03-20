using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    public partial class BonusInfo
    {

        public enum BonusType
        {
            [Description("活动红包")]
            Activity = 1,

            [Description("关注红包")]
            Attention = 2,

           [Description("奖品红包")]
            Prize=3
        }

        public enum BonusStyle
        {
            [Description( "模板1" )]
            TempletOne = 1 ,

            [Description( "模板2" )] 
            TempletTwo = 2 ,
        }

        public enum BonusPriceType 
        { 
            [Description( "固定" )]
            Fixed = 1 ,

            [Description( "随机" )]
            Random = 2 ,
        }


        public string TypeStr
        {
            get;
            set;
        }

        public string StartTimeStr
        {
            get;
            set;
        }

        public string EndTimeStr
        {
            get;
            set;
        }
    }


}
