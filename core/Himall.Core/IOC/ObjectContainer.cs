using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Core
{
    public class ObjectContainer
    {
        private static ObjectContainer current;
        private static IinjectContainer container;
        public static void ApplicationStart( IinjectContainer c )
        {
            container = c;
            current = new ObjectContainer( container );
        }

        public static ObjectContainer Current
        {
            get
            {
                if( current == null )
                {
                    ApplicationStart( container );
                }
                return current;
            }
        }

        protected IinjectContainer Container
        {
            get;
            set;
        }

        protected ObjectContainer()
        {
            Container = new DefaultContainerForDictionary();
        }

        protected ObjectContainer( IinjectContainer inversion )
        {
            Container = inversion;
        }

        public void RegisterType<T>()
        {
            Container.RegisterType<T>();
        }

        public T Resolve<T>()
        {
            return Container.Resolve<T>();
        }


    }
}
