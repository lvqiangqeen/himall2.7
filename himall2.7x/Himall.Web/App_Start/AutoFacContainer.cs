using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using Himall.Core;
using Autofac.Configuration;

namespace Himall.Web
{
    public class AutoFacContainer : IinjectContainer
    {
		#region 字段
		private ContainerBuilder builder;
		private IContainer container;
		#endregion

		#region 构造函数
		public AutoFacContainer()
		{
			builder = new ContainerBuilder();
			SetupResolveRules(builder);  //注入
			builder.RegisterControllers(Assembly.GetExecutingAssembly());  //注入所有Controller
			container = builder.Build();
			DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
		}
		#endregion

		#region IinjectContainer 成员
		public void RegisterType<T>()
		{
			builder.RegisterType<T>();
		}

		public T Resolve<T>()
		{
			return container.Resolve<T>();
		}

		public object Resolve(Type type)
		{
			return container.Resolve(type);
		}
		#endregion

		#region 私有方法
		private void SetupResolveRules(ContainerBuilder builder)
		{
			var services = Assembly.Load("Himall.Service");
			builder.RegisterAssemblyTypes(services).Where(t => t.GetInterface(typeof(Himall.IServices.IService).Name)!=null).AsImplementedInterfaces().InstancePerLifetimeScope();
			var reader = new ConfigurationSettingsReader("autofac");
			builder.RegisterModule(reader);
		}
		#endregion
	}
}