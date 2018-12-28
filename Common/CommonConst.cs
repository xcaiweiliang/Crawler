using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public sealed class CommonConst
    {
        #region MGR
        /// <summary>
        /// 
        /// </summary>
        public const string MgrUserKey = "MGR_USER";
        ////会员注册验证码SESSION key
        //public const string SESSION_SIGNIN_VERIFYCODE = "SESSION_SIGNIN_VERIFYCODE";
        //public const int MESSAGE_PAGESIZE = 10;
        ///// <summary>
        ///// 官网交易记录每页大小
        ///// </summary>
        //public const int WebPageSize = 8;
        ////代理编码
        //public const string COOKIE_AGENT_CODE = "acd";
        #endregion

        //#region 数字字符常量
        //public const string CONST_ZERO = "0";
        //public const string CONST_ONE = "1";
        //#endregion

        ///// <summary>
        ///// des加密默认的KEY
        ///// </summary>
        //public static string GetKey(string mykey)
        //{
        //    if (!"!@#$%^&*()12345".Equals(mykey))
        //    {
        //        return "";
        //    }
        //    string key = "des@K";
        //    return key + "ey5";
        //}

        ///// <summary>
        ///// 注册会员的会员级别ID
        ///// </summary>
        //public const string LevelID = "00000000-0000-0000-0000-000000000000";

        #region 常用操作编码
        /// <summary>
        /// 新增
        /// </summary>
        public const string Add = "Add";
        /// <summary>
        /// 删除
        /// </summary>
        public const string Delete = "Delete";
        /// <summary>
        /// 修改
        /// </summary>
        public const string Update = "Update";
        /// <summary>
        /// 查询
        /// </summary>
        public const string Query = "Query";
        /// <summary>
        /// 审核
        /// </summary>
        public const string Audit = "Audit";
        /// <summary>
        /// 支付
        /// </summary>
        public const string Pay = "Pay";
        /// <summary>
        /// 查看金额
        /// </summary>
        public const string Amount = "Amount";
        /// <summary>
        /// 查看列表所有数据
        /// </summary>
        public const string QueryAll = "QueryAll";
        /// <summary>
        /// 查看资金明细
        /// </summary>
        public const string FundDetail = "FundDetail";
        /// <summary>
        /// 添加转账
        /// </summary>
        public const string AddTransfer = "AddTransfer";
        /// <summary>
        /// 批量锁定会员
        /// </summary>
        public const string LockMember = "LockMember";
        /// <summary>
        /// 查看会员信息
        /// </summary>
        public const string MemberInfo = "MemberInfo";
        /// <summary>
        /// 重置会员密码
        /// </summary>
        public const string ResetPassword = "ResetPassword";

        #endregion

    }

    public enum OSEnum
    {
        MGR
    }
}
