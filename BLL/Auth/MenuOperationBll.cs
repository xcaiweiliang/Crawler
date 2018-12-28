using System;
using System.Linq;
using DAL;
using System.Collections.Generic;
using Model;
namespace BLL
{

    public class MenuOperationBll : Bll<MenuOperation>
    {
        public IQueryable<MenuOperation> QueryList()
        {
            return new MenuOperationRepository(new Context()).QueryList();
        }

    }
}
