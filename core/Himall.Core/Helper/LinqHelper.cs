using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Himall.Core
{
    public static class LinqHelper
    {
        /// <summary>
        /// 去重复项
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
        /// <summary>
        /// 构建初始查询条件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="defultPredicate"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> GetDefaultPredicate<T>(this IQueryable<T> source, bool defultPredicate)
        {
            if (defultPredicate)
            {
                return PredicateExtensions.True<T>();
            }
            else
            {
                return PredicateExtensions.False<T>();
            }
        }
        /// <summary>
        /// 获取默认排序
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        /// <example>var ord=GetDefaultOrder(d=>d.OrderBy(o=>o.Id))</example>
        public static Func<IQueryable<T>, IOrderedQueryable<T>> GetOrderBy<T>(this IQueryable<T> source, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy)
        {
            if (orderBy == null) throw new ArgumentNullException("初始排序不可为空");
            return orderBy;
        }
    }

    /// <summary>
    /// 条件组合扩展
    /// </summary>
    public static class PredicateExtensions
    {
        /// <summary>
        /// 初始True
        /// <para>主针对And拼接</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T, bool>> True<T>() { return f => true; }
        /// <summary>
        /// 初始False
        /// <para>主针对Or拼接</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T, bool>> False<T>() { return f => false; }
        /// <summary>
        /// 条件构建
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="merge"></param>
        /// <returns></returns>
        public static Expression<T> Compose<T>(this Expression<T> first, Expression<T> second, Func<Expression, Expression, Expression> merge)
        {
            // build parameter map (from parameters of second to parameters of first)  
            var map = first.Parameters.Select((f, i) => new { f, s = second.Parameters[i] }).ToDictionary(p => p.s, p => p.f);

            // replace parameters in the second lambda expression with parameters from the first  
            var secondBody = ParameterRebinder.ReplaceParameters(map, second.Body);

            // apply composition of lambda expression bodies to parameters from the first expression   
            return Expression.Lambda<T>(merge(first.Body, secondBody), first.Parameters);
        }
        /// <summary>
        /// And拼接
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.And);
        }
        /// <summary>
        /// Or拼接
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.Or);
        }
    }
    /// <summary>
    /// 条件重组
    /// </summary>
    public class ParameterRebinder : ExpressionVisitor
    {
        private readonly Dictionary<ParameterExpression, ParameterExpression> map;

        public ParameterRebinder(Dictionary<ParameterExpression, ParameterExpression> map)
        {
            this.map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
        }

        public static Expression ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> map, Expression exp)
        {
            return new ParameterRebinder(map).Visit(exp);
        }

        protected override Expression VisitParameter(ParameterExpression p)
        {
            ParameterExpression replacement;
            if (map.TryGetValue(p, out replacement))
            {
                p = replacement;
            }
            return base.VisitParameter(p);
        }
    }  
}
