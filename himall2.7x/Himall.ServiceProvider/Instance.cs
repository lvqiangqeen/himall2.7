using Autofac;
using Autofac.Configuration;
using Himall.IServices;
using System;
using System.Collections;
using System.Configuration;
using System.Linq;
using Himall.Core;

namespace Himall.ServiceProvider
{
    public class Instance<T> where T : IService
    {
        public static T Create
        {
            //get
            //{
            //    return ObjectContainer.Current.Resolve<T>();
            //}

            get
            {
                var builder = new ContainerBuilder();
                ConfigurationSettingsReader reader = new ConfigurationSettingsReader("autofac");
                SectionHandler handler = reader.SectionHandler;
                var element = handler.Components.FirstOrDefault(item => item.Service.Contains(typeof(T).FullName));
                GetServiceProviders();
                Autofac.IContainer container = null;
                try
                {
                    T t;
                    if (element == null)//配置文件中未配置，采用默认注册
                    {
                        string iserviceName = typeof(T).Name;
                        string fullName = typeof(T).FullName;
                        string namespaceName = fullName.Substring(0, fullName.LastIndexOf('.'));
                        string implementClass = ServiceProviders[namespaceName] as string;
                        if (implementClass == null)
                            throw new ApplicationException("未配置" + fullName + "的实现");
                        string nameSpace = implementClass.Split(',')[0];
                        string assembly = implementClass.Split(',')[1];
                        string implementName = iserviceName.Substring(1);
                        string className = string.Format("{0}.{1}, {2}", nameSpace, implementName, assembly);
                        Type implementType = Type.GetType(className);
                        if (implementType == null)
                            throw new NotImplementedException("未找到" + className);
                        builder.RegisterType(implementType).As<T>().InstancePerLifetimeScope();
                    }
                    else
                    {
                        builder.RegisterType<T>();
                        builder.RegisterModule(reader);
                    }
                    container = builder.Build();

                    t = container.Resolve<T>();
                    return t;
                    //var Isproxy = ConfigurationManager.AppSettings[ "IsAopProxy" ];
                    //if( !string.IsNullOrEmpty( Isproxy ) && Convert.ToBoolean( Isproxy ) )
                    //{
                    //    var proxy = new AopProxy<T>( t );
                    //    return ( T )proxy.GetTransparentProxy();
                    //}
                    //else
                    //{
                    //    return t;
                    //}
                }
                catch (Exception ex)
                {
                    throw new ServiceInstacnceCreateException(typeof(T).Name + "服务实例创建失败", ex);
                }
            }
        }





        static object locker = new object();

        static Hashtable ServiceProviders = null;

        static void GetServiceProviders()
        {
            if (ServiceProviders == null)
            {
                lock (locker)
                {
                    if (ServiceProviders == null)
                    {
                        ServiceProviders = new Hashtable();
                        var config = ConfigurationManager.GetSection("serviceProvider") as ServiceProviderConfig;
                        foreach (var item in config.Items)
                        {
                            if (ServiceProviders.ContainsKey(item.Interface))
                                throw new ApplicationException("配置文件中最多只能配置一个" + item.Interface + "的实现");
                            ServiceProviders.Add(item.Interface, item.NameSpace + "," + item.Assembly);
                        }
                    }
                }
            }
        }
    }
}

