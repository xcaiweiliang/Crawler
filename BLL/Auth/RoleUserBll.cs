using System;
using System.Linq;
using Model;
using DAL;
using System.Collections.Generic;
namespace BLL
{

    public class RoleUserBll : Bll<RoleUser>
    {
        public IQueryable<UserRole> QueryAllUsers()
        {
            return new RoleUserRepository(new Context()).QueryAllUsers();
        }


        public List<string> QueryUserByRoleId(string roleId)
        {
            return new RoleUserRepository(new Context()).QueryUserByRoleId(roleId);
        }


        public void Save(string roleId, string userIds)
        {
            new RoleUserRepository(new Context()).Save(roleId, userIds);
            
        }

        public List<string> QueryRolesByUserId(string userId)
        {
            using (Context db = new Context())
            {
                return new RoleUserRepository(db).QueryRolesByUserId(userId);
            }
                
        }

        public List<string> QueryRolesNameByUserId(string userId)
        {
            using (Context db = new Context())
            {
                return new RoleUserRepository(db).QueryRolesNameByUserId(userId);
            }

        }

        public void SaveUserRoles(string userId,string roleIds)
        {
            new RoleUserRepository(new Context()).SaveUserRoles(userId, roleIds);
        }

    }
}
