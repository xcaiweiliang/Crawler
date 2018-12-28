
using System;
using System.Linq;
using Model;
namespace DAL
{
    public class OperationRepository : Repository<Operation>
    {

        #region 变量
        Context db;
        #endregion

        #region 构造函数
        /// <summary>
        ///  创建实例
        /// </summary>
        /// <param name="db"></param>
        public OperationRepository(Context db)
            : base(db)
        {
            this.db = db;
        }
        #endregion

        /// <summary>
        /// 获取可查询的
        /// </summary>
        /// <returns></returns>
        public IQueryable<Operation> QueryList(string code, string name)
        {
            var query = db.Operations.AsQueryable();
            if (!string.IsNullOrEmpty(code))
            {
                query = query.Where(x => x.Code.ToUpper().Contains(code.ToUpper().Trim()));
            }
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(x => x.Name.Contains(name.Trim()));
            }
            return query;
        }

        public void DeleteOperation(string operateId)
        {
            var menuOper = db.MenuOperations.Where(x => x.OperationId == operateId);
            foreach (var mo in menuOper)
            {
                db.MenuOperations.Remove(mo);
            }
            var rolePer = db.RolePermissions.Where(x=>x.OperationId == operateId);
            foreach (var rp in rolePer)
            {
                db.RolePermissions.Remove(rp);
            }
            this.DeleteById(operateId);
        }


    }
}
