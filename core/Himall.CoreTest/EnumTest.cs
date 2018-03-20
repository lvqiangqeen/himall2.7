using System;
using Himall.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Himall.Core.Helper;

namespace Himall.CoreTest
{
    [TestClass]
    public class EnumTest
    {
        enum myEnum
        {
            [System.ComponentModel.Description("This is enumItem1")]
            enumItem1,

            [System.ComponentModel.Description("This is enumItem2")]
            enumItem2
        }

        [Flags]
        public enum ShopAuditStatus
        {
            /// <summary>
            /// 不可用
            /// </summary>
            [System.ComponentModel.Description("不可用")]
            Unusable = 1,

            /// <summary>
            /// 待审核
            /// </summary>
            [System.ComponentModel.Description("待审核")]
            WaitAudit = 2,

            ///// <summary>
            /// 待付款
            /// </summary>
          [System.ComponentModel.Description("待付款")]
            WaitPay = 4,

            /// <summary>
            /// 被拒绝
            /// </summary>
           [System.ComponentModel.Description("被拒绝")]
            Refuse = 8,

            /// <summary>
            /// 待确认
            /// </summary>
            [System.ComponentModel.Description("待确认")]
            WaitConfirm = 16,
            /// <summary>
            /// 冻结
            /// </summary>
            [System.ComponentModel.Description("冻结")]
            Freeze = 32,

            /// <summary>
            /// 开启
            /// </summary>
            [System.ComponentModel.Description("开启")]
            Open = 64,

            [System.ComponentModel.Description("所有待审核状态，包括（待审核、待确认、待付款）")]
            NeedAuditing = WaitAudit | WaitPay | WaitConfirm

        }

        [TestMethod]
        public void Core_Enum_DescriptionTest()
        {
            string expected = "This is enumItem2";
            string actual = myEnum.enumItem2.ToDescription();
            Assert.AreEqual(expected, actual);
        }
        [TestMethod]
        public void Core_Enum_SelectListTest()
        {
           // var a = ShopAuditStatus.NeedAuditing.ToSelectList(onlyFlag:true);
            
        }

     
    }
}
