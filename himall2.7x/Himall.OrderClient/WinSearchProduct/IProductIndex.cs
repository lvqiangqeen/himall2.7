using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinSearchProduct
{
    public interface IProductIndex
    {
        /// <summary>
        /// 创建索引的接口
        /// </summary>
        void CreateIndex();

        /// <summary>
        /// 重置索引
        /// </summary>
        void EmptyIndex();
    }
}
