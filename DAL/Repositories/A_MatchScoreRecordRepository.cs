
using System.Linq;
using Model;
using System;
using System.Collections.Generic;

namespace DAL
{
    public class A_MatchScoreRecordRepository : Repository<A_MatchScoreRecord>
    {

        #region 变量
        Context db;
        #endregion

        #region 构造函数
        /// <summary>
        ///  创建实例
        /// </summary>
        /// <param name="db"></param>
        public A_MatchScoreRecordRepository(Context db)
            : base(db)
        {
            this.db = db;
        }
        #endregion
        /// <summary>
        /// 获取比赛最新的得分记录
        /// </summary>
        /// <returns></returns>
        public A_MatchScoreRecord GetByMID(string MID)
        {
            return db.A_MatchScoreRecord.Where(x => x.MatchID == MID).OrderByDescending(x => x.CreateTime).FirstOrDefault();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<A_MatchScoreRecord> FindByDate(string sp, string sportsType, DateTime date)
        {
            var result = db.A_MatchScoreRecord.Where(x => x.SourcePlatform == sp && x.CreateTime >= date);
            if (!string.IsNullOrEmpty(sportsType))
            {
                result = result.Where(x => x.SportsType == sportsType);
            }
            return result.ToList();
        }
    }
}
