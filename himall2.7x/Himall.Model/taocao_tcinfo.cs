using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    public class taocao_tcinfo : BaseModel
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
        ///sub_title
        ///</summary>
        public string sub_title { get; set; }
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
        ///delete_flag
        ///</summary>
        public int delete_flag { get; set; }
    }
}
