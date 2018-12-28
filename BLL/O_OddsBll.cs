using System;
using System.Linq;
using DAL;
using System.Collections.Generic;
using Model;
namespace BLL
{
    public class O_OddsBll : Bll<O_Odds>
    {
        public List<O_Odds> FindByDate(string sp, string sportsType, DateTime date)
        {
            return new O_OddsRepository(new Context()).FindByDate(sp, sportsType, date);
        }

        public List<O_Odds> FindAll(string MID, string betCode, string isLive)
        {
            return new O_OddsRepository(new Context()).FindByMID(MID, betCode, isLive);
        }
    }
}


