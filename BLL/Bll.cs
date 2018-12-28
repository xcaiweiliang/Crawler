using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public class Bll<T> where T : class
    {
        private string ReturnValue { get; set; }
        /// <summary>
        /// 创建就提交
        /// </summary>
        /// <param name="model"></param>
        public string Create(T model)
        {            
            try
            {
                using (Context db = new Context())
                {
                    var rep = new Repository<T>(db);
                    rep.Create(model);
                    rep.Commit();
                    ReturnValue = string.Empty;
                }
            }
            catch (Exception ex)
            {
                ReturnValue = ex.Message;
            }
            return ReturnValue;
        }
        /// <summary>
        /// 创建就提交
        /// </summary>
        /// <param name="models"></param>
        public string CreateRange(IEnumerable<T> models)
        {
            try
            {
                using (Context db = new Context())
                {
                    var rep = new Repository<T>(db);
                    rep.CreateRange(models);
                    rep.Commit();
                    ReturnValue = string.Empty;
                }
            }
            catch (Exception ex)
            {
                ReturnValue = ex.Message;
            }
            return ReturnValue;
        }

        /// <summary>
        /// 提交
        /// </summary>
        /// <param name="model"></param>
        public string Update(T model)
        {
            return Update<T>(model);
        }
        /// <summary>
        /// 修改并提交
        /// </summary>
        /// <typeparam name="F"></typeparam>
        /// <param name="model"></param>
        public string Update<F>(F model) where F : class
        {
            try
            {
                using (Context db = new Context())
                {
                    var rep = new Repository<T>(db);
                    rep.Update<F>(model);
                    rep.Commit();
                    ReturnValue = string.Empty;
                }
            }
            catch (Exception ex)
            {
                ReturnValue = ex.Message;
            }
            return ReturnValue;
        }


        /// <summary>
        /// 删除并提交
        /// </summary>
        /// <param name="id"></param>
        public string DeleteById(string id)
        {
            try
            {
                using (Context db = new Context())
                {
                    var rep = new Repository<T>(db);
                    rep.DeleteById(id);
                    rep.Commit();
                    ReturnValue = string.Empty;
                }
            }
            catch (Exception ex)
            {
                ReturnValue = ex.Message;
            }
            return ReturnValue;
        }

        /// <summary>
        /// 删除并提交
        /// </summary>
        /// <param name="ids"></param>
        public void DeleteByIds(params string[] ids)
        {
            using (Context db = new Context())
            {
                var rep = new Repository<T>(db);
                foreach (var id in ids)
                {
                    rep.DeleteById(id);
                }
                rep.Commit();
            }
        }

        public T Get(string id)
        {
            using (Context db = new Context())
            {
                return new Repository<T>(db).Get(id);
            }
        }

        public T Get(Expression<Func<T, bool>> filter)
        {
            using (Context db = new Context())
            {
                return new Repository<T>(db).Get(filter);
            }
        }
        public T Get(Expression<Func<T, bool>> filter, params string[] includes)
        {
            using (Context db = new Context())
            {
                return new Repository<T>(db).Get(filter, includes);
            }
        }
        public IEnumerable<T> Filter(Expression<Func<T, bool>> predicate)
        {
            using (Context db = new Context())
            {
                return new Repository<T>(db).Filter(predicate).ToList();
            }
        }


        //public IEnumerable<Model.SystemManage.User> Orderby(Expression<Func<Model.SystemManage.User, bool>> predicate)
        //{
        //    using (Context db = new Context())
        //    {
        //        return db.Users.OrderBy(x => x.Id).ToList();
        //    }
        //}

        public IQueryable<F> GetWithRawSql<F>(string query, params object[] parameters)
        {
            using (Context db = new Context())
            {
                return new Repository<T>(db).GetWithRawSql<F>(query, parameters);
            }
        }


        public IEnumerable<T> GetAll()
        {
            using (Context db = new Context())
            {
                return new Repository<T>(db).GetAll().ToList();
            }
        }

        public IEnumerable<T> GetPaged(int pageIndex, int pageSize, out int total, Expression<Func<T, bool>> filter, string field, bool ascending = true, string[] includes = null)
        {
            using (Context db = new Context())
            {
                return new Repository<T>(db).GetPaged(pageIndex, pageSize, out total, filter, field, ascending, includes);
            }
        }

        public IEnumerable<T> GetPaged<KProperty>(int pageIndex, int pageSize, out int total, Expression<Func<T, bool>> filter, Expression<Func<T, KProperty>> orderBy, bool ascending = true, string[] includes = null)
        {
            using (Context db = new Context())
            {
                return new Repository<T>(db).GetPaged(pageIndex, pageSize, out total, filter, orderBy, ascending, includes);
            }
        }

        public IEnumerable<T> GetFiltered<KProperty>(Expression<Func<T, bool>> filter, Expression<Func<T, KProperty>> orderBy, bool ascending = true, string[] includes = null)
        {
            using (Context db = new Context())
            {
                return new Repository<T>(db).GetFiltered(filter, orderBy, ascending, includes);
            }
        }

        public void Commit(Context db)
        {
            new Repository<T>(db).Commit();
        }

        public string ExecuteBySQL(string sql, out int count)
        {
            count = 0;
            try
            {
                using (Context db = new Context())
                {
                    count = db.Database.ExecuteSqlCommand(sql);
                    ReturnValue = string.Empty;
                }
            }
            catch (Exception ex)
            {
                ReturnValue = ex.Message;
            }
            return ReturnValue;
        }

    }
}
