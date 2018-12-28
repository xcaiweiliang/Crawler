using System;
using System.Linq;
using DAL;
using System.Collections.Generic;
using Model;
namespace BLL
{
    public class A_MatchScoreRecordBll : Bll<A_MatchScoreRecord>
    {
        public A_MatchScoreRecord GetByMID(string MID)
        {
            return new A_MatchScoreRecordRepository(new Context()).GetByMID(MID);
        }

        public List<A_MatchScoreRecord> FindByDate(string sp, string sportsType, DateTime date)
        {
            return new A_MatchScoreRecordRepository(new Context()).FindByDate(sp, sportsType, date);
        }
    }
}


