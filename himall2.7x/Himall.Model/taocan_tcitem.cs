using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    public class taocan_tcitem : BaseModel
    {
        ///<summary>
        ///id
        ///</summary>
        long _id;
        public new long Id { get { return _id; } set { _id = value; base.Id = value; } }
        ///<summary>
        ///title
        ///</summary>
        public string title { get; set; }
        ///<summary>
        ///menu_id
        ///</summary>
        public int menu_id { get; set; }
        ///<summary>
        ///brief
        ///</summary>
        public string brief { get; set; }
        ///<summary>
        ///main_content
        ///</summary>
        public string main_content { get; set; }
        ///<summary>
        ///link_url
        ///</summary>
        public string link_url { get; set; }
        ///<summary>
        ///thumbnail
        ///</summary>
        public string thumbnail { get; set; }
        ///<summary>
        ///image
        ///</summary>
        public string image { get; set; }
        ///<summary>
        ///delete_flag
        ///</summary>
        public int delete_flag { get; set; }
    }
}
