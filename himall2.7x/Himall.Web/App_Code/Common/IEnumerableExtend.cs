using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Common
{
    public static class EnumerableExtend
    {
        /// <summary>
        /// 返回序列中满足条件的第一个元素；如果未找到这样的元素，则返回默认值。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <param name="defaultTSource">默认值</param>
        /// <returns></returns>
        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate,TSource defaultTSource)
        {
            var result = source.FirstOrDefault(predicate);
            if (result == null)
                result = defaultTSource;
            return result;
        }
    }
}