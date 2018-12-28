using System;
using System.Linq;
using DAL;
using System.Collections.Generic;
using Model;
namespace BLL
{
    public class S_BetCodeBll : Bll<S_BetCode>
    {
        public List<S_BetCode> FindList()
        {
            return new S_BetCodeRepository(new Context()).FindList();
        }
        
    }
}


