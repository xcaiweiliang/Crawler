using System;
using System.Linq;
using DAL;
using System.Collections.Generic;
using Model;
using Common;

namespace BLL
{
    public class UserBll : Bll<User>
    {
        private const string key = "G0q)U*7Pq@z@Dw7";
        /// <summary>
        /// 锁定时间
        /// </summary>
        private const int Locked = 1;
        public IQueryable<User> QueryList(string userNO, string UserName)
        {
            return new UserRepository(new Context()).QueryList(userNO, UserName);
        }

        public User GetUserByUserNo(string userNO)
        {
            using (var db = new Context())
            {
                return new UserRepository(db).GetUserByUserNo(userNO);
            }
        }


        public bool Authorization(string userId, string menuCode, string operation = "")
        {
            using (Context db = new Context())
            {
                return new UserRepository(db).Authorization(userId, menuCode, operation);
            }
        }

        public string Login(string userNO, string pwd)
        {
            using (var db = new Context())
            {
                var user = new UserRepository(db).GetUserByUserNo(userNO, "");
                if (user != null)
                {
                    if (false)
                    {
                        throw new Exception();
                    }
                    if ((!user.IsEnable && user.DisabledTime.HasValue && user.DisabledTime.Value < DateTime.Now.AddHours(-Locked).AddSeconds(-DateTime.Now.Second)) || user.IsEnable)
                    {
                        if (user.Password == GeneratePwd(userNO, pwd))
                        {
                            System.Web.HttpContext.Current.Session[Common.CommonConst.MgrUserKey] = user;
                            user.TryNum = 0;
                            user.DisabledTime = null;
                            user.LastLoginTime = DateTime.Now;
                            //插入登录日志
                            db.SaveChanges();
                            return "success";
                        }
                        else
                        {
                            user.TryNum += 1;
                            db.SaveChanges();
                        }
                        return "密码错误";
                    }
                    else if (user.DisabledTime.HasValue)//无效用户
                    {
                        return "请" + Math.Ceiling((user.DisabledTime.Value.AddHours(Locked) - user.DisabledTime.Value).TotalMinutes) + "分钟之后在登录";
                    }
                    else
                    {
                        return "账号异常";
                    }
                }
                return "账号错误";
            }
        }

        public string GeneratePwd(string account, string pwd)
        {
            return (account.Trim().ToLower() + pwd + key).MD5();
        }

        public void DeleteUsers(params string[] ids)
        {
            using (Context db = new Context())
            {
                var uRep = new UserRepository(db);
                foreach (var id in ids)
                {
                    uRep.DeleteUser(id);
                }
                uRep.Commit();
            }
        }
    }
}


