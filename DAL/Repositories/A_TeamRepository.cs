
using System.Linq;
using Model;
using System;
using System.Collections.Generic;

namespace DAL
{
    public class A_TeamRepository : Repository<A_Team>
    {

        #region 变量
        Context db;
        #endregion

        #region 构造函数
        /// <summary>
        ///  创建实例
        /// </summary>
        /// <param name="db"></param>
        public A_TeamRepository(Context db)
            : base(db)
        {
            this.db = db;
        }
        #endregion   
        /// <summary>
        /// 根据联赛ID和名称获取队伍
        /// </summary>
        /// <returns></returns>
        public A_Team GetByLMIDName(string LMID, string name)
        {
            return db.A_Team.Where(x => x.LeagueMatchID == LMID && x.Name == name).FirstOrDefault();
        }
        public List<A_Team> FindAll(string sp, string sportsType)
        {
            var result = db.A_Team.Where(x => x.SourcePlatform == sp);
            if (!string.IsNullOrEmpty(sportsType))
            {
                result = result.Where(x => x.SportsType == sportsType);
            }
            return result.ToList();
        }
    }
}
