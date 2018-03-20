using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    /// <summary>
    /// 微信模板信息模型
    /// <para>keyword请按顺序对应，如果无内容保持空</para>
    /// </summary>
    public class WX_MsgTemplateKey2DataModel
    {
        public WX_MsgTemplateKey2DataModel()
        {
            this.first = new WX_MSGItemBaseModel();
            this.keyword1 = new WX_MSGItemBaseModel();
            this.keyword2 = new WX_MSGItemBaseModel();
            this.remark = new WX_MSGItemBaseModel();
        }
        public WX_MSGItemBaseModel first { get; set; }
        public WX_MSGItemBaseModel keyword1 { get; set; }
        public WX_MSGItemBaseModel keyword2 { get; set; }
        public WX_MSGItemBaseModel remark { get; set; }
    }
}
