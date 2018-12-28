
using System.Linq;
using Model;
using System;
using System.Collections.Generic;

namespace DAL
{
    public class S_BetCodeRepository : Repository<S_BetCode>
    {

        #region 变量
        Context db;
        #endregion

        #region 构造函数
        /// <summary>
        ///  创建实例
        /// </summary>
        /// <param name="db"></param>
        public S_BetCodeRepository(Context db)
            : base(db)
        {
            this.db = db;
        }
        #endregion   
        
        public List<S_BetCode> FindList()
        {
            return db.S_BetCode.ToList();
        }
        
    }
}
