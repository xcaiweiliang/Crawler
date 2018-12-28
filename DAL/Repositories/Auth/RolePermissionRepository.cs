
using System;
using System.Linq;
using Model;
namespace DAL
{
public class RolePermissionRepository:Repository<RolePermission>
{
 
       #region 变量
        Context db;
        #endregion

        #region 构造函数
        /// <summary>
        ///  创建实例
        /// </summary>
        /// <param name="db"></param>
        public RolePermissionRepository(Context db)
            : base(db)
        {
            this.db = db;
        }
        #endregion


        /// <summary>
        /// 授权
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="MenuOperation"></param>
        public void SaveRolePermission(string roleId, string[] MenuOperation)
        {
            foreach (var info in db.RolePermissions.Where(x => x.RoleId == roleId))
            {
                db.RolePermissions.Remove(info);
            }

            if (MenuOperation != null)
            {
                foreach (string str in MenuOperation)
                {
                    var arr = str.Split('_');
                    string menuId = arr[0];
                    string operationId = arr[1];
                    var RolePermission = new RolePermission()
                    {
                        Id = Guid.NewGuid().ToString(),
                        RoleId = roleId,
                        MenuId = menuId,
                        OperationId = operationId
                    };
                    this.Create<RolePermission>(RolePermission);
                }
            }
            db.SaveChanges();
        }

    }
}
