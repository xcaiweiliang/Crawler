using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class Repository<T> where T : class
    {
        private Context dbContext { get; set; }
        private readonly IDbSet<T> dbset;

        #region 构造函数
        /// <summary>
        ///  创建实例
        /// </summary>
        /// <param name="dbContext"></param>
        public Repository(Context dbContext)
        {
            this.dbContext = dbContext;
            this.dbset = this.dbContext.Set<T>();

        }
        #endregion

        public void Create(T model)
        {
            dbContext.Set<T>().Add(model);
        }
        public void Create<F>(F model) where F : class
        {
            dbContext.Set<F>().Add(model);
        }
        public void CreateRange(IEnumerable<T> models)
        {
            dbContext.Set<T>().AddRange(models);
        }
        public void CreateRange<F>(IEnumerable<F> models) where F : class
        {
            dbContext.Set<F>().AddRange(models);
        }
        public void Update(T model)
        {
            if (dbContext.Entry<T>(model).State == EntityState.Detached)
            {
                dbContext.Set<T>().Attach(model);
                dbContext.Entry<T>(model).State = EntityState.Modified;
            }
        }
        public void Update<F>(F model) where F : class
        {
            if (dbContext.Entry<F>(model).State == EntityState.Detached)
            {
                dbContext.Set<F>().Attach(model);
                dbContext.Entry<F>(model).State = EntityState.Modified;
            }
        }

        public void Delete(T model)
        {
            dbContext.Set<T>().Remove(model);
        }

        public void Delete(Expression<Func<T, bool>> filter)
        {
            dbContext.Set<T>().RemoveRange(dbContext.Set<T>().Where(filter).ToList());
        }

        public void Delete(params string[] ids)
        {
            foreach (string id in ids)
            {
                T v = dbContext.Set<T>().Find(id);
                if (v == null)
                {
                    continue;
                }
                dbContext.Set<T>().Remove(v);
            }
        }



        public void DeleteById(string id)
        {
            T v = dbContext.Set<T>().Find(id);
            dbContext.Set<T>().Remove(v);
        }

        public T Get(string id)
        {
            return dbContext.Set<T>().Find(id);
        }


        public T Get(Expression<Func<T, bool>> filter)
        {
            return this.dbset.Where(filter).SingleOrDefault();
        }
        public T Get(Expression<Func<T, bool>> filter, params string[] includes)
        {
            var result = filter == null ? this.dbset : this.dbset.Where(filter);
            if (includes != null && includes.Length > 0)
            {
                foreach (var include in includes)
                {
                    result = result.Include(include);
                }
            }
            return result.SingleOrDefault();
        }

        public IQueryable<T> Filter(Expression<Func<T, bool>> predicate)
        {
            return dbset.Where(predicate).AsQueryable<T>();
        }
        public IQueryable<T> GetWithRawSql(string query, params object[] parameters)
        {
            return dbContext.Database.SqlQuery<T>(query, parameters).AsQueryable();
        }
        public IQueryable<F> GetWithRawSql<F>(string query, params object[] parameters)
        {
            return dbContext.Database.SqlQuery<F>(query, parameters).AsQueryable();
        }



        public IQueryable<T> GetAll()
        {
            return dbContext.Set<T>().AsQueryable();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="KProperty"></typeparam>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="total"></param>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <param name="ascending">是否升序</param>
        /// <param name="includes"></param>
        /// <returns></returns>
        public IEnumerable<T> GetPaged<KProperty>(int pageIndex, int pageSize, out int total, Expression<Func<T, bool>> filter, Expression<Func<T, KProperty>> orderBy, bool ascending = true, string[] includes = null)
        {
            pageIndex = pageIndex > 0 ? pageIndex : 1;
            var result = this.GetFiltered(filter, orderBy, ascending, includes);
            total = result.Count();
            return result.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        }

        public IEnumerable<T> GetFiltered<KProperty>(Expression<Func<T, bool>> filter, Expression<Func<T, KProperty>> orderBy, bool ascending = true, string[] includes = null)
        {
            var result = filter == null ? this.dbset : this.dbset.Where(filter);

            if (ascending)
            {
                result = result.OrderBy(orderBy);
            }
            else
            {
                result = result.OrderByDescending(orderBy);
            }
            if (includes != null && includes.Length > 0)
            {
                foreach (var include in includes)
                {
                    result = result.Include(include);
                }
            }
            return result.ToList();
        }

        public IEnumerable<T> GetPaged(int pageIndex, int pageSize, out int total, Expression<Func<T, bool>> filter, string field, bool ascending = true, string[] includes = null)
        {
            pageIndex = pageIndex > 0 ? pageIndex : 1;
            var result = filter == null ? this.dbset : this.dbset.Where(filter);
            //var result = this.GetFiltered(filter, orderBy, ascending, includes);
            total = result.Count();
            if (includes != null && includes.Length > 0)
            {
                foreach (var include in includes)
                {
                    result = result.Include(include);
                }
            }
            result = CreateSortQuery(result, field, ascending);
            return result.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        }


        /// <summary>
        /// LINQ 动态排序
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="field"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        public IQueryable<T> CreateSortQuery(IQueryable<T> query, string field, bool ascending = true)
        {
            ParameterExpression param = Expression.Parameter(typeof(T), field);
            System.Reflection.PropertyInfo pi = typeof(T).GetProperty(field);
            Type[] types = new Type[2];
            types[0] = typeof(T);
            types[1] = pi.PropertyType;

            string sortwith = ascending ? "OrderBy" : "OrderByDescending";
            Expression exp = Expression.Call(typeof(Queryable), sortwith, types, query.Expression, Expression.Lambda(Expression.Property(param, field), param));
            return query.AsQueryable().Provider.CreateQuery<T>(exp);
        }

        public IQueryable<F> CreateSortQuery<F>(IQueryable<F> query, string field, bool ascending = true)
        {
            ParameterExpression param = Expression.Parameter(typeof(F), field);
            System.Reflection.PropertyInfo pi = typeof(F).GetProperty(field);
            Type[] types = new Type[2];
            types[0] = typeof(T);
            types[1] = pi.PropertyType;

            string sortwith = ascending ? "OrderBy" : "OrderByDescending";
            Expression exp = Expression.Call(typeof(Queryable), sortwith, types, query.Expression, Expression.Lambda(Expression.Property(param, field), param));
            return query.AsQueryable().Provider.CreateQuery<F>(exp);
        }

        public void Commit()
        {
            //try
            //{
            dbContext.SaveChanges();
            //}
            //catch (DbEntityValidationException ex)
            //{
            //    StringBuilder errors = new StringBuilder();
            //    IEnumerable<DbEntityValidationResult> validationResult = ex.EntityValidationErrors;
            //    foreach (DbEntityValidationResult result in validationResult)
            //    {
            //        ICollection<DbValidationError> validationError = result.ValidationErrors;
            //        foreach (DbValidationError err in validationError)
            //        {
            //            errors.Append(err.PropertyName + ":" + err.ErrorMessage + "\r\n");
            //        }
            //    }
            //    string msg = errors.ToString();
            //}
        }

        public DateTime GetServerDateTime()
        {
            return dbContext.Database.SqlQuery<DateTime>("select getdate()").First();
        }
    }
}
