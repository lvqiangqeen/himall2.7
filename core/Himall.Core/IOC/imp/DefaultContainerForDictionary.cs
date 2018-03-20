using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Core
{
    class DefaultContainerForDictionary : IinjectContainer
    {
        private static IDictionary<Type , object> objectDefine = new Dictionary<Type , object>();

        public void RegisterType<T>()
        {
            if( !objectDefine.ContainsKey( typeof( T ) ) )
            {
                objectDefine[ typeof( T ) ] = Activator.CreateInstance( typeof( T ) );
            }
        }

        public T Resolve<T>()
        {
            if( objectDefine.ContainsKey( typeof( T ) ) )
            {
                return ( T )objectDefine[ typeof( T ) ];
            }
            throw new Exception( "该服务未在框架中注册" );
        }
    }
}
