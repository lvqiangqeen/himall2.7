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
    public class WX_MsgFightGroupNewJoinDataModel
    {
        public WX_MsgFightGroupNewJoinDataModel()
        {
            this.first = new WX_MSGItemBaseModel();
            this.time = new WX_MSGItemBaseModel();
            this.number = new WX_MSGItemBaseModel();
            this.remark = new WX_MSGItemBaseModel();
        }
        public WX_MSGItemBaseModel first { get; set; }
        public WX_MSGItemBaseModel time { get; set; }
        public WX_MSGItemBaseModel number { get; set; }
        public WX_MSGItemBaseModel remark { get; set; }
    }
}
