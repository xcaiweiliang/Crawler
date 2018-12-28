
using System.Linq;
using Model;
using System;
using System.Collections.Generic;

namespace DAL
{
    public class O_OddsRepository : Repository<O_Odds>
    {

        #region 变量
        Context db;
        #endregion

        #region 构造函数
        /// <summary>
        ///  创建实例
        /// </summary>
        /// <param name="db"></param>
        public O_OddsRepository(Context db)
            : base(db)
        {
            this.db = db;
        }
        #endregion
        /// <summary>
        /// 获取比赛的玩法赔率
        /// </summary>
        /// <returns></returns>
        public List<O_Odds> FindByMID(string MID, string betCode, string isLive)
        {
            var result = db.O_Odds.Where(x => x.MatchID == MID);
            if (!string.IsNullOrEmpty(betCode))
            {
                result = result.Where(x => x.BetCode == betCode);
            }
            if (!string.IsNullOrEmpty(isLive))
            {
                result = result.Where(x => x.IsLive == isLive);
            }
            return result.ToList();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<O_Odds> FindByDate(string sp, string sportsType, DateTime date)
        {
            var result = db.O_Odds.Where(x => x.SourcePlatform == sp && x.ModifyTime >= date);
            if (!string.IsNullOrEmpty(sportsType))
            {
                result = result.Where(x => x.SportsType == sportsType);
            }
            return result.ToList();
        }
        
    }
}
