using Himall.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace Himall.Entity
{
    public static class DbSetExtend
    {
        #region 查询

        /// <summary>
        /// 扩展Entity Framework 中DbSet<TEntity>类型的方法，主要用于查找实体对象的全部记录，不适用任何Where谓词
        /// </summary>
        /// <returns></returns>
        public static IQueryable<TEntity> FindAll<TEntity>(this DbSet<TEntity> dbSet) where TEntity:BaseModel
        {
            return dbSet.Where(item => true);
        }
        /// <summary>
        /// 扩展Entity Framework 中DbSet<TEntity>类型的方法，主要用于查找实体对象的全部记录,支持分页、排序项等功能
        /// </summary>
        /// <typeparam name="TEntity">泛型对象</typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="dbSet">DbSet实体类型</param>
        /// <param name="pageNumber">第几页</param>
        /// <param name="pageSize">一页记录数量</param>
        /// <param name="total">总数量</param>
        /// <param name="orderBy">排序对象</param>
        /// <param name="isAsc">排序方式</param>
        /// <returns></returns>
        public static IQueryable<TEntity> FindAll<TEntity, TKey>(
            this DbSet<TEntity> dbSet, int pageNumber, int pageSize, out int total,Expression<Func<TEntity, TKey>> orderBy, bool isAsc = true
            ) where TEntity : BaseModel
        {
            total = dbSet.Count();
            IQueryable<TEntity> entities = dbSet.Where(item => true);

            if (isAsc)
                entities = entities.OrderBy(orderBy);
            else
                entities = entities.OrderByDescending(orderBy);

            entities = entities.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            return entities;
        }

        /// <summary>
        /// 根据编号查询实体
        /// </summary>
        /// <param name="id">实体编号</param>
        /// <returns></returns>
        public static TEntity FindById<TEntity>(this DbSet<TEntity> dbSet, object id) where TEntity : BaseModel
        {
            //TEntity entity = dbSet.Find(id);
            TEntity entity = dbSet.FirstOrDefault( p => p.Id == id );
            return entity;
        }

        ///// <summary>
        ///// 根据编号查询实体
        ///// </summary>
        ///// <param name="ids">待查询的实体编号集</param>
        ///// <returns></returns>
        //public static IQueryable<TEntity> FindByIds<TEntity>(this DbSet<TEntity> dbSet, params object [] ids) where TEntity : BaseModel
        //{
        //    return dbSet.Where(item => item.Id );
        //}


        /// <summary>
        /// 根据条件查询
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns></returns>
        public static IQueryable<TEntity> FindBy<TEntity>(this DbSet<TEntity> dbSet, Expression<Func<TEntity, bool>> where) where TEntity : BaseModel
        {
            return dbSet.Where(where);
        }


        /// <summary>
        /// 根据条件查询
        /// 
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns></returns>
        public static IQueryable<TEntity> FindBy<TEntity>(this IQueryable<TEntity> entities, Expression<Func<TEntity, bool>> where) where TEntity : BaseModel
        {
            return entities.Where(where);
        }


        /// <summary>
        /// 根据条件查询
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="pageNumber">页码(起始为1)</param>
        /// <param name="pageSize">每页数据行数</param>
        /// <param name="total">符合条件总数据数量</param>
        /// <returns></returns>
        public static IQueryable<TEntity> FindBy<TEntity>(this DbSet<TEntity> dbSet, Expression<Func<TEntity, bool>> where, int pageNumber, int pageSize, out int total) where TEntity : BaseModel
        {
            total = dbSet.Count(where);
            return dbSet.Where(where).OrderBy(item => item.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }

        /// <summary>
        /// 根据条件查询
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="pageNumber">页码(起始为1)</param>
        /// <param name="pageSize">每页数据行数</param>
        /// <param name="total">符合条件总数据数量</param>
        /// <returns></returns>
        public static IQueryable<TEntity> FindBy<TEntity>(this IQueryable<TEntity> entities, Expression<Func<TEntity, bool>> where, int pageNumber, int pageSize, out int total) where TEntity : BaseModel
        {
            total = entities.Count(where);
            return entities.Where(where).OrderBy(item => item.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }

        /// <summary>
        /// 根据条件查询
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="pageNumber">页码(起始为1)</param>
        /// <param name="pageSize">每页数据行数</param>
        /// <param name="total">符合条件总数据数量</param>
        /// <returns></returns>
        public static IQueryable<TEntity> FindBy<TEntity, TKey>(this DbSet<TEntity> dbSet, Expression<Func<TEntity, bool>> where, int pageNumber, int pageSize, out int total, Expression<Func<TEntity, TKey>> orderBy, bool isAsc = true) where TEntity : BaseModel
        {
            total = dbSet.Count(where);
            IQueryable<TEntity> entities;
            if (isAsc)
                entities = dbSet.Where(where).OrderBy(orderBy).Skip((pageNumber - 1) * pageSize).Take(pageSize);
            else
                entities = dbSet.Where(where).OrderByDescending(orderBy).Skip((pageNumber - 1) * pageSize).Take(pageSize);
            return entities;
        }

        /// <summary>
        /// 根据条件查询
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="pageNumber">页码(起始为1)</param>
        /// <param name="pageSize">每页数据行数</param>
        /// <param name="total">符合条件总数据数量</param>
        /// <returns></returns>
        public static IQueryable<TEntity> FindBy<TEntity, TKey>(this IQueryable<TEntity> entities, Expression<Func<TEntity, bool>> where, int pageNumber, int pageSize, out int total, Expression<Func<TEntity, TKey>> orderBy, bool isAsc = true)
        {
            IQueryable<TEntity> newEntities;
            total = entities.Count(where);
            if (isAsc)
                newEntities = entities.Where(where).OrderBy(orderBy).Skip((pageNumber - 1) * pageSize).Take(pageSize);
            else
                newEntities = entities.Where(where).OrderByDescending(orderBy).Skip((pageNumber - 1) * pageSize).Take(pageSize);
            return newEntities;
        }
        /// <summary>
        /// 获取分页结果
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entities"></param>
        /// <param name="total">记录数</param>
        /// <param name="orderBy">排序条件</param>
        /// <param name="pageNumber">页码</param>
        /// <param name="pageSize">每页条数</param>
        /// <returns></returns>
        /// <example>sql = sql.GetPage(out total, d => d.OrderBy(o => o.OrderNum).ThenByDescending(o=>o.Id), 1, 20);</example>
        public static IQueryable<TEntity> GetPage<TEntity>(this IQueryable<TEntity> entities, out int total, Func<IQueryable<TEntity>,
            IOrderedQueryable<TEntity>> orderBy, int pageNumber=1, int pageSize=20)
        {
            if (orderBy == null)
            {
                throw new ArgumentNullException("排序条件不能为空");
            }
            IQueryable<TEntity> newEntities;
            total = entities.Count();
            entities = orderBy(entities);
            newEntities = entities.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            return newEntities;
        }
        /// <summary>
        /// 获取分页结果
        /// <para>默认按Id倒序排列</para>
        /// <para>针对BaseModel，必须有Id字段</para>
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entities"></param>
        /// <param name="total">记录数</param>
        /// <param name="pageNumber">页码</param>
        /// <param name="pageSize">每页条数</param>
        /// <param name="orderBy">排序条件,为null表示按Id倒序</param>
        /// <returns></returns>
        public static IQueryable<TEntity> GetPage<TEntity>(this IQueryable<TEntity> entities, out int total, int pageNumber=1, int pageSize=20, Func<IQueryable<TEntity>,
            IOrderedQueryable<TEntity>> orderBy=null) where TEntity : BaseModel
        {
            if (orderBy == null)
            {
                orderBy = d => d.OrderByDescending(o => o.Id);
            }
            IQueryable<TEntity> newEntities;
            total = entities.Count();
            entities = orderBy(entities);
            newEntities = entities.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            return newEntities;
        }

        #endregion

        #region DML

        //#region 更新

        ///// <summary>
        ///// 更新实体
        ///// </summary>
        ///// <param name="entity">待更新的实体</param>
        //public static void Update<TEntity>(this DbSet<TEntity> dbSet, TEntity entity) where TEntity : class,IBaseModel
        //{
        //    if (!dbSet.Local.Contains(entity))
        //        dbSet.Attach(entity);
        //    DbSet.Add = EntityState.Added;
           
        //}

        ///// <summary>
        ///// 更新实体
        ///// </summary>
        ///// <param name="entities">待更新的实体集</param>
        //public static void Update<TEntity>(this DbSet<TEntity> dbSet, IEnumerable<TEntity> entities) where TEntity : class,IBaseModel
        //{
        //    DbSet<TEntity> dbset = dbSet;
        //    foreach (TEntity entity in entities)
        //    {
        //        dbset.Attach(entity);
        //        context.Entry(entity).State = EntityState.Modified;
        //    }
        //}


        //#endregion

        #region 删除

        /// <summary>
        /// 根据编号删除实体
        /// </summary>
        /// <param name="ids">待删除的实体编号集合</param>
        public static void Remove<TEntity>(this DbSet<TEntity> dbSet, params object[] ids) where TEntity : BaseModel
        {
            List<TEntity> entities = new List<TEntity>();
            foreach (var id in ids)
                entities.Add(dbSet.FindById(id));
            dbSet.RemoveRange(entities);
        }

        /// <summary>
        /// 根据条件删除实体
        /// </summary>
        /// <param name="where">待删除实体需要符合的条件</param>
        public static void Remove<TEntity>(this DbSet<TEntity> dbSet, Expression<Func<TEntity, bool>> where) where TEntity : BaseModel
        {
            IEnumerable<TEntity> entities = dbSet.FindBy(where);
            dbSet.RemoveRange(entities);
        }

        #endregion


        #region 其它

        /// <summary>
        /// 是否存在
        /// </summary>
        /// <param name="where">需要判断是否存在的条件</param>
        /// <returns></returns>
        public static bool Exist<TEntity>(this DbSet<TEntity> dbSet, Expression<Func<TEntity, bool>> where) where TEntity : BaseModel
        {
            return dbSet.Count(where) > 0;
        }

        #endregion

        #endregion
    }
}
