using System;
using System.Linq;
using Model;
using DAL;
using System.Collections.Generic;
namespace BLL
{
    public class OperationBll : Bll<Operation>
    {
        /// <summary>
        /// ≤È—Ø¡–±Ì
        /// </summary>
        /// <param name="code"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public IQueryable<Operation> QueryList(string code, string name)
        {
                return new OperationRepository(new Context()).QueryList(code, name);
        }

        public void DeleteOperations(params string[] ids)
        {
            using (Context db = new Context())
            {
                var oRep = new OperationRepository(db);
                foreach (var id in ids)
                {
                    oRep.DeleteOperation(id);
                }
                oRep.Commit();
            }
        }


    }
}
