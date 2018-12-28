
using System;
using System.Linq;
using Model;
using System.Data.Entity;

namespace DAL
{
    public class MenuRepository : Repository<Menu>
    {

        #region 变量
        Context db;
        #endregion

        #region 构造函数
        /// <summary>
        ///  创建实例
        /// </summary>
        /// <param name="db"></param>
        public MenuRepository(Context db)
            : base(db)
        {
            this.db = db;
        }
        #endregion

        /// <summary>
        /// 删除菜单
        /// </summary>
        /// <param name="menuId"></param>
        public void DeleteMenu(string menuId)
        {
            foreach (var menuOperation in db.MenuOperations.Where(x => x.MenuId == menuId))
            {
                db.MenuOperations.Remove(menuOperation);
            }
            foreach (var info in db.RolePermissions.Where(x => x.MenuId == menuId))
            {
                db.RolePermissions.Remove(info);
            }
            this.DeleteById(menuId);
            this.Commit();
        }

        public bool CheckCodeExists(string id, string code)
        {
            if (string.IsNullOrEmpty(id))
                return db.Menus.Any(x => x.Code.Equals(code));
            else
                return db.Menus.Any(x => x.Code.Equals(code) && !x.Id.Equals(id));
        }

        public void Save(string menuId, string[] OperationIDs)
        {
            foreach (var info in db.MenuOperations.Where(x => x.MenuId == menuId))
            {
                db.MenuOperations.Remove(info);
            }
            foreach (string operationId in OperationIDs)
            {
                this.Create<MenuOperation>(new MenuOperation()
                {
                    Id = Guid.NewGuid().ToString(),
                    MenuId = menuId,
                    OperationId = operationId
                });
            }
        }


        public IQueryable<Menu> GetCurrUserMenu(string userId)
        {
            var query = from rp in db.RolePermissions
                        join ru in db.RoleUsers
                        on rp.RoleId equals ru.RoleId
                        where ru.UserId == userId
                        select new
                        {
                            MenuId = rp.MenuId
                        };
            return db.Set<Menu>().AsNoTracking().Where(x => x.Show == DbFunctions.AsNonUnicode("1") && query.Any(q => q.MenuId == x.Id));
        }

    }
}
