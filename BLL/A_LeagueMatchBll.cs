using System;
using System.Linq;
using DAL;
using System.Collections.Generic;
using Model;
namespace BLL
{
    public class A_LeagueMatchBll : Bll<A_LeagueMatch>
    {
        public A_LeagueMatch GetByName(string name, string sp)
        {
            return new A_LeagueMatchRepository(new Context()).GetByName(name,sp);
        }

        public List<A_LeagueMatch> FindAll(string sp, string sportsType)
        {
            return new A_LeagueMatchRepository(new Context()).FindAll(sp, sportsType);
        }
    }
}


