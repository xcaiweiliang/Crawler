
using System;
using System.Linq;
using Model;

namespace DAL
{
    public class MenuOperationRepository : Repository<MenuOperation>
    {

        #region 变量
        Context db;
        #endregion

        #region 构造函数
        /// <summary>
        ///  创建实例
        /// </summary>
        /// <param name="db"></param>
        public MenuOperationRepository(Context db)
            : base(db)
        {
            this.db = db;
        }
        #endregion

        public IQueryable<MenuOperation> QueryList()
        {
            var query = db.MenuOperations.AsQueryable();
            return query;
        }
    }
}
