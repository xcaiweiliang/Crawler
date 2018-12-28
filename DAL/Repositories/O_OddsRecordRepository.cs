
using System.Linq;
using Model;
using System;
using System.Collections.Generic;

namespace DAL
{
    public class O_OddsRecordRepository : Repository<O_OddsRecord>
    {

        #region 变量
        Context db;
        #endregion

        #region 构造函数
        /// <summary>
        ///  创建实例
        /// </summary>
        /// <param name="db"></param>
        public O_OddsRecordRepository(Context db)
            : base(db)
        {
            this.db = db;
        }
        #endregion
        /// <summary>
        /// 获取比赛的玩法赔率
        /// </summary>
        /// <returns></returns>
        public List<O_OddsRecord> FindByMID(string MID, string betCode = "", string isLive = "")
        {
            var result = db.O_OddsRecord.Where(x => x.MatchID == MID);
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
        public List<O_OddsRecord> FindByDate(DateTime date)
        {
            //date = date.AddDays(-1);
            return db.O_OddsRecord.Where(x => x.CreateTime >= date).ToList();
        }
    }
}
