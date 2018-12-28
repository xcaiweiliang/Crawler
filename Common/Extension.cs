using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class Extension
    {

        #region 时间扩展方法
        public static string ToShortDate(this DateTime obj)
        {
            return obj.ToString("yyyy-MM-dd");
        }
        public static string ToShortTime(this DateTime obj)
        {
            return obj.ToString("yyyy-MM-dd HH:mm:ss");
        }
        public static string ToShortDate(this DateTime? obj)
        {
            if (!obj.HasValue)
            {
                return string.Empty;
            }
            return obj.Value.ToString("yyyy-MM-dd");
        }
        /// <summary>
        /// DateTime?
        /// </summary>
        /// <param name="obj">DateTime?</param>
        /// <returns></returns>
        public static string ToShortTime(this DateTime? obj)
        {
            return obj.ToShortTime("yyyy-MM-dd HH:mm:ss");
        }
        public static string ToShortTime(this DateTime? obj, string format)
        {
            if (!obj.HasValue)
            {
                return string.Empty;
            }
            return obj.Value.ToString(format);
        }
     
          
      
        #endregion
        #region  LINQ 动态排序


        /// <summary>
        /// LINQ 动态排序
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="field"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        public static IQueryable<T> CreateSortQuery<T>(this IQueryable<T> query, string field, string sort)
        {
            ParameterExpression param = Expression.Parameter(typeof(T), field);
            System.Reflection.PropertyInfo pi = typeof(T).GetProperty(field);
            Type[] types = new Type[2];
            types[0] = typeof(T);
            types[1] = pi.PropertyType;
            string sortwith = sort.ToUpper() == "ASC" ? "OrderBy" : "OrderByDescending";
            Expression exp = Expression.Call(typeof(Queryable), sortwith, types, query.Expression, Expression.Lambda(Expression.Property(param, field), param));
            return query.AsQueryable().Provider.CreateQuery<T>(exp);
        }

        public static IQueryable<T> CreateSortQuery<T>(this IOrderedQueryable<T> q, string fieldName, string direction)
        {
            var param = Expression.Parameter(typeof(T), "p");
            var prop = Expression.Property(param, fieldName);
            var exp = Expression.Lambda(prop, param);
            string method = direction.ToLower() == "asc" ? "ThenBy" : "ThenByDescending";
            Type[] types = new Type[] { q.ElementType, exp.Body.Type };
            var mce = Expression.Call(typeof(Queryable), method, types, q.Expression, exp);
            return q.Provider.CreateQuery<T>(mce);
        }
        #endregion

        #region 生成验证码
        ///// <summary>
        ///// 生成随机数
        ///// </summary>
        ///// <param name="controller"></param>
        ///// <returns></returns>
        //private static string GenerateCheckCode(this Controller controller)
        //{
        //    int number;
        //    char code;
        //    string checkCode = String.Empty;
        //    System.Random random = new Random();
        //    for (int i = 0; i < 4; i++)
        //    {
        //        number = random.Next();
        //        code = (char)('0' + (char)(number % 10));
        //        checkCode += code.ToString();
        //    }
        //    controller.Session.Remove(CommonConst.SESSION_SIGNIN_VERIFYCODE);
        //    controller.Session[CommonConst.SESSION_SIGNIN_VERIFYCODE] = checkCode;
        //    return checkCode;
        //}
        ///// <summary>
        ///// 生成验证码图片
        ///// </summary>
        ///// <param name="controller"></param>
        ///// <returns></returns>
        //public static byte[] CreateCheckCodeImage(this Controller controller)
        //{
        //    string checkCode = controller.GenerateCheckCode();
        //    if (checkCode == null || checkCode.Trim() == String.Empty)
        //    {
        //        return null;
        //    }
        //    System.Drawing.Bitmap image = new System.Drawing.Bitmap((int)Math.Ceiling((checkCode.Length * 18.5)), 34);
        //    System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(image);
        //    try
        //    {
        //        //生成随机生成器
        //        Random random = new Random();
        //        //清空图片背景色
        //        g.Clear(System.Drawing.Color.White);
        //        //画图片的背景噪音线
        //        for (int i = 0; i < 1; i++)
        //        {
        //            int x1 = random.Next(image.Width);
        //            int x2 = random.Next(image.Width);
        //            int y1 = random.Next(image.Height);
        //            int y2 = random.Next(image.Height);
        //            g.DrawLine(new System.Drawing.Pen(System.Drawing.Color.DarkRed), x1, y1, x2, y2);
        //        }
        //        System.Drawing.Font font = new System.Drawing.Font("Arial", 20, (System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic));
        //        System.Drawing.Drawing2D.LinearGradientBrush brush = new System.Drawing.Drawing2D.LinearGradientBrush(new System.Drawing.Rectangle(0, 0, image.Width, image.Height), System.Drawing.Color.Red, System.Drawing.Color.Red, 1.2f, true);
        //        g.DrawString(checkCode, font, brush, 2, 2);
        //        //画图片的前景噪音点
        //        for (int i = 0; i < 5; i++)
        //        {
        //            int x = random.Next(image.Width);
        //            int y = random.Next(image.Height);
        //            image.SetPixel(x, y, System.Drawing.Color.FromArgb(random.Next()));
        //        }
        //        //画图片的边框线
        //        g.DrawRectangle(new System.Drawing.Pen(System.Drawing.Color.DarkRed), 0, 0, image.Width - 1, image.Height - 1);
        //        System.IO.MemoryStream ms = new System.IO.MemoryStream();
        //        image.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
        //        //HttpContext.Current.Response.ClearContent();
        //        //HttpContext.Current.Response.ContentType = "image/Gif";
        //        //HttpContext.Current.Response.BinaryWrite(ms.ToArray());
        //        return ms.ToArray();
        //    }
        //    finally
        //    {
        //        g.Dispose();
        //        image.Dispose();
        //    }
        //}
        #endregion

        #region decimal
        public static string DecimalToString(this decimal? dec)
        {
            if (dec.HasValue)
            {
                return dec.Value.DecimalToString();
            }
            return string.Empty;
        }
        public static string DecimalToString(this decimal dec)
        {
            string tmp = dec.ToString();
            if (tmp.Contains("."))
            {
                tmp = tmp.TrimEnd('0');
                if (tmp.EndsWith("."))
                {
                    tmp = tmp.TrimEnd('.');
                }
            }
            return tmp;
        }
        #endregion



    }

}
