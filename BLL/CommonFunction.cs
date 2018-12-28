using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace BLL
{
    public static partial class CommonFunction
    {
        #region 获取服务器时间
        public static DateTime GetServerDateTime<T>(this Bll<T> b) where T : class
        {
            using (Context db = new Context())
            {
                return new Repository<T>(db).GetServerDateTime();
            }
        }
        public static DateTime GetServerDateTime()
        {
            using (Context db = new Context())
            {
                return new Repository<Model.A_LeagueMatch>(db).GetServerDateTime();
            }
        }
        #endregion

        #region 时间生成随机数
        public static string CreateUniqueNumber<T>(this Bll<T> b) where T : class
        {
            return CreateUniqueNumber();
        }
        /// <summary>
        /// 生成15位不一样的数字(格式为：121019184858111)
        /// </summary>
        /// <returns></returns>
        public static string CreateUniqueNumber()
        {
            DateTime dt = GetServerDateTime();
            //System.Text.StringBuilder _sb = new System.Text.StringBuilder();
            //_sb.Append((dt.Year - 2000).ToString()); // 年
            //_sb.Append(AppendZero(dt.Month)); // 月
            //_sb.Append(AppendZero(dt.Day)); // 日
            //_sb.Append(AppendZero(dt.Hour)); // 时
            //_sb.Append(AppendZero(dt.Minute)); // 分
            //_sb.Append(AppendZero(dt.Second)); // 秒

            //随机3位数
            long tick = DateTime.Now.Ticks;
            Random ran = new Random((int)(tick & 0xffffffffL) | (int)(tick >> 32));
            int Millisecond = ran.Next(1, 999);
            return dt.ToString("yyMMddHHmmss") + Millisecond.ToString("000");
            //if (Millisecond < 10)
            //{
            //    _sb.Append("00" + Millisecond.ToString());
            //}
            //else if (Millisecond < 100)
            //{
            //    _sb.Append("0" + Millisecond.ToString());
            //}
            //else
            //{
            //    _sb.Append(Millisecond.ToString());
            //}
            //return _sb.ToString();
        }
        ///// <summary>
        ///// 前面补零
        ///// </summary>
        ///// <param name="num">number</param>
        ///// <returns></returns>
        //public static string AppendZero(int num)
        //{
        //    return (num < 10 ? ("0" + num) : num.ToString());
        //}
        #endregion
    }
}
