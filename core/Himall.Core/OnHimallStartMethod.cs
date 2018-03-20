using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace Himall.Core
{
    /// <summary>
    /// 提供对Himall程序启动的扩展支持
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class OnHimallStartMethodAttribute:Attribute
    {
        /// <summary>
        /// 没有返回值的空参数签名
        /// </summary>
        public string MethodName { get; private set; }

        /// <summary>
        /// 一个描述启动方法的类型的对象
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// 启动顺序,数字越小越先启动
        /// </summary>
        public int Order { get; private set; }

        /// <summary>
        /// 提供对Himall程序启动的扩展支持
        /// </summary>
        /// <param name="type">一个描述启动方法的类型的对象</param>
        /// <param name="methodName">没有返回值的空参数签名</param>
        /// <param name="order">启动顺序,数字越小越先启动</param>
        public OnHimallStartMethodAttribute(Type type, string methodName, int order = 0)
        {
            this.MethodName = methodName;
            this.Type = type;
            this.Order = order;
        }

        static bool IsStarted = false;

        /// <summary>
        /// 启动初始化
        /// </summary>
        public static void Start()
        {
            if (!IsStarted)
            {
                StartImp();
                IsStarted = true;
            }
        }

        static void StartImp()
        {
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            var thisType = typeof(OnHimallStartMethodAttribute);
            assemblies = assemblies.Where(item => item.CustomAttributes.Any(t => t.AttributeType == thisType)).ToArray();
            var allAttributes = new List<Attribute>();
            foreach (var assembly in assemblies)
            {
                var attributes = assembly.GetCustomAttributes(thisType);
                allAttributes.AddRange(attributes);
            }
            var himallStartMethodAttributes = allAttributes.Select(item => item as OnHimallStartMethodAttribute).OrderBy(item => item.Order);
            foreach (var atrribute in himallStartMethodAttributes)
            {
                Type type = atrribute.Type;
                MethodInfo mf = type.GetMethod(atrribute.MethodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static, null, new Type[] { }, null);
                if (mf != null)
                    mf.Invoke(null, null);
            }
        }

    }
}
