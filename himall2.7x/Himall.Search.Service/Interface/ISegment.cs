using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Search.Service.Interface
{
    public interface ISegment
    {
        /// <summary>
        /// 分词
        /// </summary>
        /// <param name="str">需分词的文本</param>
        /// <returns>分词结果</returns>
        List<string> DoSegment(string str);
    }
}
