using DAL;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public class ExceptionBll : Bll<Model.ExceptionLog>
    {

        public IQueryable<ExceptionLog> QueryList(string typeCode, string account, string surmary, DateTime? minTime, DateTime? maxTime)
        {
            return new ExceptionRepository(new Context()).QueryList(typeCode, account, surmary, minTime, maxTime);
        }

        public void LogError(string os, string account, string surmary, string description, string path)
        {
            using (Context db = new Context())
            {
                try
                {
                    ExceptionLog info = new ExceptionLog();
                    info.Id = Guid.NewGuid().ToString();
                    info.TypeCode = os;
                    info.Account = account;
                    info.Surmary = surmary;
                    info.Description = description;
                    info.Path = path;
                    info.CreateTime = this.GetServerDateTime();
                    db.ExceptionLog.Add(info);
                    this.Commit(db);
                }
                //catch (System.Data.Entity.Validation.DbEntityValidationException e)
                //{
                //    foreach (var eve in e.EntityValidationErrors)
                //    {
                //        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                //            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                //        foreach (var ve in eve.ValidationErrors)
                //        {
                //            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                //                ve.PropertyName, ve.ErrorMessage);
                //        }
                //    }
                //    throw;
                //}
                catch (Exception ex)
                {

                }
            }
        }

    }
}
