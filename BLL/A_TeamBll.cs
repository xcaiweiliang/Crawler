using System;
using System.Linq;
using DAL;
using System.Collections.Generic;
using Model;
namespace BLL
{
    public class A_TeamBll : Bll<A_Team>
    {
        public A_Team GetByLMIDName(string LMID, string name)
        {
            return new A_TeamRepository(new Context()).GetByLMIDName(LMID, name);
        }

        public List<A_Team> FindAll(string sp, string sportsType)
        {
            return new A_TeamRepository(new Context()).FindAll(sp, sportsType);
        }
    }
}


