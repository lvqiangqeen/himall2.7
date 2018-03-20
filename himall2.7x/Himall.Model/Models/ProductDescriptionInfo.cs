using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Himall.Core;
    
namespace Himall.Model
{
    public partial class ProductDescriptionInfo
    {
        /// <summary>
        /// 显示手机端描述
        /// <para>后台未添加手机端描述，将显示电脑端描述</para>
        /// </summary>
        //TODO:DZY[150729] 添加
        /* zjt  
         * TODO可移除，保留注释即可
         */
        [NotMapped]
        public string ShowMobileDescription
        {
            get
            {
                string result="";
                if (this != null)
                {
                    result = this.MobileDescription;
                    if (string.IsNullOrWhiteSpace(result)) result = this.Description;
                }
                return result;
            }
        }
    }
}
