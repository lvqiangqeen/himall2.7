using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model.DTO
{
    public class AppendCommentModel
    {
        public string[] Images { set; get; }
        public string[] WXmediaId { set; get; }

        public string AppendContent { get; set; }

        public long Id { set; get; }

        public long UserId { set; get; }
    }
}
