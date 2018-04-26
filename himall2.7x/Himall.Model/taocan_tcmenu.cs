using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    public class taocan_tcmenu : BaseModel
    {
        ///<summary>
        ///Id
        ///</summary>
        long _id;
        public new long Id { get { return _id; } set { _id = value; base.Id = value; } }
        ///<summary>
        ///title
        ///</summary>
        public string title { get; set; }
        ///<summary>
        ///parent_id
        ///</summary>
        public int parent_id { get; set; }
        ///<summary>
        ///is_parent
        ///</summary>
        public int is_parent { get; set; }
        ///<summary>
        ///link_url
        ///</summary>
        public string link_url { get; set; }
        ///<summary>
        ///delete_flag
        ///</summary>
        public int delete_flag { get; set; }
    }
}
