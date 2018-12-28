
using System;
using System.Linq;
using Model;
using System.Collections.Generic;

namespace DAL
{
    public class RoleUserRepository : Repository<RoleUser>
    {

        #region 变量
        Context db;
        #endregion

        #region 构造函数
        /// <summary>
        ///  创建实例
        /// </summary>
        /// <param name="db"></param>
        public RoleUserRepository(Context db)
            : base(db)
        {
            this.db = db;
        }
        #endregion



        public IQueryable<UserRole> QueryAllUsers()
        {
            var linq = from u in db.Users.AsQueryable()
                       select new UserRole
                       {
                           Id = u.Id,
                           UserNo = u.Account,
                           UserName = u.Name,
                           IsCurrentRoleCheck = false
                       };
            return linq;

        }


        public List<string> QueryUserByRoleId(string roleId)
        {
            var ru = db.RoleUsers.Where(x => x.RoleId == roleId);
            return ru.Select(x => x.UserId).ToList();
        }


        public List<string> QueryRolesByUserId(string userId)
        {
            var query = db.RoleUsers.Where(x=>x.UserId == userId);
            return query.Select(x => x.RoleId).ToList();
        }

        public List<string> QueryRolesNameByUserId(string userId)
        {
            var query = from ru in db.RoleUsers
                        join
                        rl in db.Roles on ru.RoleId equals rl.Id
                        where ru.UserId == userId
                        select rl.Name;
            if (!query.Any())
            {
                return new List<string>();
            }
            return query.ToList();
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="userIds"></param>
        public void Save(string roleId, string userIds)
        {
            foreach (var info in db.RoleUsers.Where(x => x.RoleId == roleId))
            {
                db.RoleUsers.Remove(info);
            }
            foreach (string userId in userIds.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
            {
                this.Create(new RoleUser()
                {
                    Id = Guid.NewGuid().ToString(),
                    RoleId = roleId,
                    UserId = userId
                });
            }
            db.SaveChanges();
        }


        public void SaveUserRoles(string userId, string roleIds)
        {
            foreach (var info in db.RoleUsers.Where(x=>x.UserId == userId))
            {
                db.RoleUsers.Remove(info);
            }
            foreach (string roleId in roleIds.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries))
            {
                this.Create(new RoleUser()
                {
                    Id = Guid.NewGuid().ToString(),
                    RoleId = roleId,
                    UserId = userId
                });
            }
            db.SaveChanges();
        }



    }
}
