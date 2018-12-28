
using System;
using System.Linq;
using Model;
namespace DAL
{
    public class ExceptionRepository : Repository<ExceptionLog>
    {

        #region 变量
        Context db;
        #endregion

        #region 构造函数
        /// <summary>
        ///  创建实例
        /// </summary>
        /// <param name="db"></param>
        public ExceptionRepository(Context db)
            : base(db)
        {
            this.db = db;
        }
        #endregion

        public IQueryable<ExceptionLog> QueryList(string typeCode, string account, string surmary, DateTime? minTime, DateTime? maxTime)
        {
            var query = db.ExceptionLog.AsQueryable();
            if (!string.IsNullOrEmpty(typeCode))
            {
                query = query.Where(x => x.TypeCode == typeCode);
            }
            if (!string.IsNullOrEmpty(account))
            {
                query = query.Where(x => x.Account == account);
            }
            if (!string.IsNullOrEmpty(surmary))
            {
                query = query.Where(x => x.Surmary.Contains(surmary));
            }
            if (minTime.HasValue)
            {
                query = query.Where(x => x.CreateTime >= minTime.Value);
            }
            if (maxTime.HasValue)
            {
                var temp = maxTime.Value.AddDays(1);
                query = query.Where(x => x.CreateTime < temp);
            }
            return query.OrderByDescending(x => x.CreateTime);
        }
    }
}
