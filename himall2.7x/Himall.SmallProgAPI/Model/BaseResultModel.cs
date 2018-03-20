using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.SmallProgAPI.Model
{
    public class BaseResultModel
    {
        private bool _Status { get; set; }
        public string Status { get
            {
                return _Status? "OK" : "NO";
            }
        }
        public object Message { get; set; }

        public BaseResultModel(bool status)
        {
            _Status = status;
        }
        /// <summary>
        /// 设置状态
        /// </summary>
        /// <param name="status"></param>
        public void SetStatus(bool status)
        {
            _Status = status;
        }
    }
}
