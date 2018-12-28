using System;
using System.Linq;
using DAL;
using System.Collections.Generic;
using Model;
namespace BLL
{
    public class A_MatchResultBll : Bll<A_MatchResult>
    {
        public List<A_MatchResult> FindByDate(string sp, string sportsType, DateTime date)
        {
            return new A_MatchResultRepository(new Context()).FindByDate(sp, sportsType, date);
        }
    }
}


