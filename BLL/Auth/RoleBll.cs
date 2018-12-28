using System;
using System.Linq;
using Model;
using DAL;
using System.Collections.Generic;
namespace BLL
{

    public class RoleBll : Bll<Role>
    {
        public IQueryable<Role> QueryList(string name)
        {
            return new RoleRepository(new Context()).QueryList(name);
        }

        //public Tuple<List<Menu>, List<AuthVM>> GetRoleAuth(string roleId)
        //{
        //    using (Context db = new Context())
        //    {
        //        var menuRep = new MenuRepository(db);
        //        var menuList = menuRep.GetAll().OrderBy(x => x.Sort).ToList();
        //        var authList = new RoleRepository(db).GetRoleAuth(roleId).ToList();

        //        return Tuple.Create(menuList, authList);
        //    }
        //}

        public void DeleteRoles(params string[] ids)
        {
            using (Context db = new Context())
            {
                var rRep = new RoleRepository(db);
                foreach (var id in ids)
                {
                    rRep.DeleteRole(id);
                }
                rRep.Commit();
            }
        }

    }
}
