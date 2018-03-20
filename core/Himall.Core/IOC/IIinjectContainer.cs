using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Core
{
    /// <summary>
    /// 依赖注入接口
    /// </summary>
    public interface IinjectContainer
    {
        void RegisterType<T>();

        T Resolve<T>();
    }
}
