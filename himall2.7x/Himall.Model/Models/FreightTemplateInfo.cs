using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Himall.Core;
using Himall.CommonModel;

namespace Himall.Model
{
    public partial class FreightTemplateInfo
    {

        /// <summary>
        /// 仓库地址
        /// <para>手动补充</para>
        /// </summary>
        [NotMapped]
        public string DepotAddress { get; set; }


        /// <summary>
        /// 获取发货时间
        /// </summary>
        public SendTimeEnum? GetSendTime
        {
            get
            {
                SendTimeEnum? result = null;
                if (!string.IsNullOrWhiteSpace(this.SendTime))
                {
                    int num = 0;
                    if (int.TryParse(this.SendTime, out num))
                    {
                        if (Enum.IsDefined(typeof(SendTimeEnum), num))
                        {
                            result = (SendTimeEnum)num;
                        }
                    }
                }
                return result;
            }
        }
    }
}
