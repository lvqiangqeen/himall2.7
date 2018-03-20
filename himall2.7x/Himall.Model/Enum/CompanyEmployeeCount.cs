using System.ComponentModel;
namespace Himall.Model
{
    /// <summary>
    /// 公司员工数量
    /// </summary>
    public enum CompanyEmployeeCount : int
    {
        [Description("5人以下")]
        LessThanFive = 1,

        [Description("5-10人")]
        FiveToTen = 2,

        [Description( "11-50人" )]
        EleToFifty = 3 ,

        [Description("51-100人")] 
        ElevenToFifty = 4,

        [Description("101-200人")]
        OneHundredToTwoHundred = 5,

        [Description("201-300人")]
        TwoHundredToThreeHundred = 6,

        [Description("301-500人")]
        ThreeHundredToFiveHundred = 7,

        [Description("501-1000人")]
        FiveHundredToThousand = 8,

        [Description("1000人以上")]
        MoreThanThousand = 9
    }
}
