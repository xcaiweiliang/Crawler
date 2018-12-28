using System;
using System.Linq;
using Model;
namespace DAL
{
    public class RoleRepository : Repository<Role>
    {

        #region 变量
        Context db;
        #endregion

        #region 构造函数
        /// <summary>
        ///  创建实例
        /// </summary>
        /// <param name="db"></param>
        public RoleRepository(Context db)
            : base(db)
        {
            this.db = db;
        }
        #endregion

        /// <summary>
        /// 获取可查询的
        /// </summary>
        /// <returns></returns>
        public IQueryable<Role> QueryList(string name)
        {
            var query = db.Roles.AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(x => x.Name.Contains(name.Trim()));
            }
            return query;
        }

        //public IQueryable<AuthVM> GetRoleAuth(string roleId)
        //{
        //    return from mo in db.MenuOperations
        //           join o in db.Operations on mo.OperationId equals o.Id
        //           join r in db.RolePermissions.Where(x => x.RoleId == roleId) on new { MenuId = mo.MenuId, OperationId = mo.OperationId } equals new { MenuId = r.MenuId, OperationId = r.OperationId } into temp
        //           from r in temp.DefaultIfEmpty()
        //           select new AuthVM
        //           {
        //               Id = mo.Id,
        //               MenuId = mo.MenuId,
        //               OperationName = o.Name,
        //               OperationCode = o.Code,
        //               OperationId = mo.OperationId,
        //               HasAuth = r != null
        //           };
        //}


        public void DeleteRole(string roleId)
        {
            var rolePer = db.RolePermissions.Where(x=>x.RoleId == roleId);
            foreach (var rp in rolePer)
            {
                db.RolePermissions.Remove(rp);
            }
            var roleUser = db.RoleUsers.Where(x=>x.RoleId == roleId);
            foreach (var ru in roleUser)
            {
                db.RoleUsers.Remove(ru);
            }
            this.DeleteById(roleId);
        }
    }
}
