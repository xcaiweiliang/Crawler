
using System.Linq;
using Model;
using System;
using System.Collections.Generic;

namespace DAL
{
    public class A_MatchResultRepository : Repository<A_MatchResult>
    {

        #region 变量
        Context db;
        #endregion

        #region 构造函数
        /// <summary>
        ///  创建实例
        /// </summary>
        /// <param name="db"></param>
        public A_MatchResultRepository(Context db)
            : base(db)
        {
            this.db = db;
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<A_MatchResult> FindByDate(string sp, string sportsType, DateTime date)
        {
            var result = db.A_MatchResult.Where(x => x.SourcePlatform == sp && x.CreateTime >= date);
            if (!string.IsNullOrEmpty(sportsType))
            {
                result = result.Where(x => x.SportsType == sportsType);
            }
            return result.ToList();
        }
    }
}
