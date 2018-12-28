using System;
using System.Linq;
using Model;
using DAL;
using System.Collections.Generic;
namespace BLL
{

    public class RolePermissionBll : Bll<RolePermission>
    {
        public void SaveRolePermission(string roleId, string[] MenuOperation)
        {
            using (Context db = new Context())
            {
                new RolePermissionRepository(db).SaveRolePermission(roleId, MenuOperation);
            }
        }
    }
}
