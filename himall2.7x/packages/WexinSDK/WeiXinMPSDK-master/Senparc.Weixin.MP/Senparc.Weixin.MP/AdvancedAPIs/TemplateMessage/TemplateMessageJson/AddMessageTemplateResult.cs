/*----------------------------------------------------------------
    Copyright (C) 2015 Senparc
    
    文件名：SendTemplateMessageResult.cs
    文件功能描述：发送模板消息结果
    
    
    创建标识：Senparc - 20150211
    
    修改标识：Senparc - 20150303
    修改描述：整理接口
----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Senparc.Weixin.Entities;

namespace Senparc.Weixin.MP.AdvancedAPIs.TemplateMessage
{
    /// <summary>
    /// 添加消息模板返回结果
    /// </summary>
    public class AddMessageTemplateResult : WxJsonResult
    {
        /// <summary>
        /// template_id
        /// </summary>
        public string template_id { get; set; }
    }
}
