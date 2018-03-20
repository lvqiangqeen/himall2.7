/*----------------------------------------------------------------
    Copyright (C) 2015 Senparc
    
    文件名：TempleteModel.cs
    文件功能描述：模板消息接口需要的数据
    
    
    创建标识：Senparc - 20150211
    
    修改标识：Senparc - 20150303
    修改描述：整理接口
----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Senparc.Weixin.MP.AdvancedAPIs.TemplateMessage
{
    public class AddTempleteModel
    {
        /// <summary>
        /// 模板短编号
        /// </summary>
        public string template_id_short { get; set; }
    }
}
