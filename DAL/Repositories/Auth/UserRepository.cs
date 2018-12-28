
using System;
using System.Linq;
using Model;
namespace DAL
{
    public class UserRepository : Repository<User>
    {

        #region 变量
        Context db;
        #endregion

        #region 构造函数
        /// <summary>
        ///  创建实例
        /// </summary>
        /// <param name="db"></param>
        public UserRepository(Context db)
            : base(db)
        {
            this.db = db;
        }
        #endregion
        public IQueryable<User> QueryList(string userNO, string userName)
        {
            var query = db.Users.AsQueryable();//.Where(x => x.SystemUser != "1")
            if (!string.IsNullOrEmpty(userNO))
            {
                query = query.Where(d => d.Account.Contains(userNO.Trim()));
            }
            if (!string.IsNullOrEmpty(userName))
            {
                query = query.Where(d => d.Name.Contains(userName.Trim()));
            }
            return query;
        }

        /// <summary>
        /// 根据账号获取用户数据
        /// </summary>
        /// <param name="userNo"></param>
        /// <returns></returns>
        public User GetUserByUserNo(string userNo)
        {
            return db.Users.Where(x => x.Account == userNo).FirstOrDefault();
        }

        /// <summary>
        /// 总商户平台编码为空
        /// </summary>
        /// <param name="userNo"></param>
        /// <param name="platCode"></param>
        /// <returns></returns>
        public User GetUserByUserNo(string userNo, string platCode)
        {
            return db.Users.Where(x => x.Account == userNo && x.PlatCode == platCode).SingleOrDefault();
        }

        /// <summary>
        /// 判断用户权限
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="menuCode">菜单编码</param>
        /// <param name="operation">操作编码</param>
        /// <returns></returns>
        public bool Authorization(string userId, string menuCode, string operationCode = "")
        {
            var linq = from ru in db.RoleUsers
                       join p in db.RolePermissions on ru.RoleId equals p.RoleId
                       join m in db.Menus on p.MenuId equals m.Id
                       where ru.UserId == userId && m.Code == menuCode
                       select new { OperationId = p.OperationId, MenuId = p.MenuId };
            if (string.IsNullOrEmpty(operationCode))
            {
                return linq.Any();
            }
            linq = from l in linq
                   join o in db.Operations on l.OperationId equals o.Id
                   join mo in db.MenuOperations on new { MenuId = l.MenuId, OperationId = l.OperationId } equals new { MenuId = mo.MenuId, OperationId = mo.OperationId }
                   where o.Code == operationCode
                   select l;
            return linq.Any();
        }

        public void DeleteUser(string userId)
        {
            var roleUser = db.RoleUsers.Where(x => x.UserId == userId);
            foreach (var ru in roleUser)
            {
                db.RoleUsers.Remove(ru);
            }
            this.DeleteById(userId);
        }
    }
}


