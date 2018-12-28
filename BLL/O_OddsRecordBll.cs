using System;
using System.Linq;
using DAL;
using System.Collections.Generic;
using Model;
namespace BLL
{
    public class O_OddsRecordBll : Bll<O_OddsRecord>
    {
        public List<O_OddsRecord> FindByDate(DateTime date)
        {
            return new O_OddsRecordRepository(new Context()).FindByDate(date);
        }

        public List<O_OddsRecord> FindAll(string MID, string betCode, string isLive)
        {
            return new O_OddsRecordRepository(new Context()).FindByMID(MID, betCode, isLive);
        }
    }
}


