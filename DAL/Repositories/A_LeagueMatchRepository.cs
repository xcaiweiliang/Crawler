
using System.Linq;
using Model;
using System;
using System.Collections.Generic;

namespace DAL
{
    public class A_LeagueMatchRepository : Repository<A_LeagueMatch>
    {

        #region 变量
        Context db;
        #endregion

        #region 构造函数
        /// <summary>
        ///  创建实例
        /// </summary>
        /// <param name="db"></param>
        public A_LeagueMatchRepository(Context db)
            : base(db)
        {
            this.db = db;
        }
        #endregion        

        /// <summary>
        /// 根据名称获取联赛
        /// </summary>
        /// <returns></returns>
        public A_LeagueMatch GetByName(string name,string sp)
        {
            return db.A_LeagueMatch.Where(x => x.Name == name && x.SourcePlatform == sp).FirstOrDefault();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<A_LeagueMatch> FindAll(string sp, string sportsType)
        {
            var result = db.A_LeagueMatch.Where(x => x.SourcePlatform == sp);
            if (!string.IsNullOrEmpty(sportsType))
            {
                result = result.Where(x => x.SportsType == sportsType);
            }
            return result.ToList();
        }
    }
}
