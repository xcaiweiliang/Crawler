using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class Utility
    {
        #region  MD5加密
        
        public static string MD5(this string str)
        {
            //return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(str, "MD5").ToLower();
            byte[] result = Encoding.UTF8.GetBytes(str);
            var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] output = md5.ComputeHash(result);
            return BitConverter.ToString(output).Replace("-", "").ToLower();
        }
        #endregion

        #region DESC 加解密
        static string DESCKEY = "GKdnjJHf";
        /// <summary>
        /// DESC 加密字符串
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string Encrypt(this string content)
        {
            return Encrypt(DESCKEY, content);
        }
        /// <summary>
        /// DESC 加密字符串
        /// </summary>
        /// <param name="key">加密密码，8位字符</param>
        /// <param name="s">需要加密的内容</param>
        /// <returns></returns>
        public static string Encrypt(string key, string content)
        {
            if (content == null)
            {
                return null;
            }
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] inputByteArray = System.Text.Encoding.Default.GetBytes(content);
            des.Key = System.Text.ASCIIEncoding.ASCII.GetBytes(key);
            des.IV = System.Text.ASCIIEncoding.ASCII.GetBytes(key);

            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();

            System.Text.StringBuilder sb_Ret = new System.Text.StringBuilder();
            foreach (byte b in ms.ToArray())
            {
                sb_Ret.AppendFormat("{0:X2}", b);
            }
            sb_Ret.ToString();

            return sb_Ret.ToString();
        }
        /// <summary>
        /// DESC 解密字符串
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string Decrypt(this string content)
        {
            return Decrypt(content, DESCKEY);
        }
        /// <summary>
        /// DESC 解密字符串
        /// 如果失败，则返回null值
        /// </summary>
        /// <param name="key">解密密码，8位字符</param>
        /// <param name="s">需要解密的内容</param>
        /// <returns></returns>
        public static string Decrypt(this string content, string key)
        {
            try
            {
                if (content == null)
                {
                    return null;
                }
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                byte[] inputByteArray = new byte[content.Length / 2];
                for (int x = 0; x < content.Length / 2; x++)
                {
                    int i = (Convert.ToInt32(content.Substring(x * 2, 2), 16));
                    inputByteArray[x] = (byte)i;
                }
                des.Key = System.Text.ASCIIEncoding.ASCII.GetBytes(key);
                des.IV = System.Text.ASCIIEncoding.ASCII.GetBytes(key);

                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();

                return System.Text.Encoding.Default.GetString(ms.ToArray());
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 加密
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="info"></param>
        /// <param name="propertyNames"></param>
        /// <returns></returns>
        public static void Encrypt<T>(this T info, params string[] propertyNames) where T : class
        {
            Type type = typeof(T);
            var propertys = type.GetProperties();
            foreach (var name in propertyNames)
            {
                var property = propertys.Where(x => x.Name.ToLower() == name.ToLower()).SingleOrDefault();
                if (property != null)
                {
                    object value = property.GetValue(info);
                    if (value != null)
                    {
                        property.SetValue(info, value.ToString().Encrypt());
                    }
                }
            }
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="info"></param>
        /// <param name="propertyNames"></param>
        public static void Decrypt<T>(this T info, params string[] propertyNames) where T : class
        {
            Type type = typeof(T);
            var propertys = type.GetProperties();
            foreach (var name in propertyNames)
            {
                var property = propertys.Where(x => x.Name.ToLower() == name.ToLower()).SingleOrDefault();
                if (property != null)
                {
                    object value = property.GetValue(info);
                    if (value != null)
                    {
                        property.SetValue(info, value.ToString().Decrypt());
                    }
                }
            }
        }
        #endregion

        public static void ClearCache()
        {
            System.Web.Caching.Cache Cache = System.Web.HttpRuntime.Cache;
            System.Collections.IDictionaryEnumerator enumerator = Cache.GetEnumerator();
            while (enumerator.MoveNext())
            {
                Cache.Remove(enumerator.Key.ToString());
            }
        }

        #region 常用类型转换
        /// <summary>
        /// 对象类型转int, 失败返回0
        /// </summary>
        /// <param name="obj">转换的对象</param>
        /// <returns></returns>
        public static int ObjConvertToInt(object obj)
        {
            if (obj == null)
            {
                return 0;
            }
            int i = 0;
            int.TryParse(obj.ToString(), out i);
            return i;
        }
        /// <summary>
        /// 对象类型转string，失败返回空字符串
        /// </summary>
        /// <param name="obj">转换的对象</param>
        /// <returns></returns>
        public static string ObjToDateTime(object obj)
        {
            if (obj == null)
            {
                return "";
            }
            DateTime dt = DateTime.Now;
            if (DateTime.TryParse(obj.ToString(), out dt))
            {
                return dt.ToString();
            }
            else
            {
                return "";
            }
        }
        /// <summary>
        /// 对象类型转bool，失败返回 false
        /// </summary>
        /// <param name="obj">转换的对象</param>
        /// <returns></returns>
        public static bool ObjConvertToBool(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            bool flag = false;
            bool.TryParse(obj.ToString(), out flag);
            return flag;
        }
        /// <summary>
        /// 对象类型转string，失败返回空字符
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ObjConvertToString(object obj)
        {
            if (obj == null)
            {
                return "";
            }
            return obj.ToString().Trim();
        }
        /// <summary>
        /// 对象类型转decimal，失败返回0
        /// 如果转换成功，则返回值包含与 s 中包含的数值等效的 Decimal 数；如果转换失败，则返回值包含零。如果 s 参数为 null，格式不符合 style，或者表示的数字小于 MinValue 或大于 MaxValue，则转换将失败。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static decimal ObjConvertToDecimal(object obj)
        {
            if (obj == null)
            {
                return 0;
            }
            decimal d = 0;
            decimal.TryParse(obj.ToString(), out d);
            return d;
        }
        /// <summary>
        /// 对象类型转DateTime，失败返回当前时间
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static DateTime ObjConvertToDateTime(object obj)
        {
            DateTime dt;
            if (obj == null)
            {
                return DateTime.Now;
            }
            if (DateTime.TryParse(obj.ToString(), out dt))
            {
                return dt;
            }
            else
            {
                return DateTime.Now;
            }
        }

        /// <summary>
        ///  转换Unix时间戳,错误返回空子符
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        public static string StampToDateTime(string timeStamp)
        {
            try
            {
                DateTime dateTimeStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
                long lTime = long.Parse(timeStamp + "0000000");
                TimeSpan toNow = new TimeSpan(lTime);
                return dateTimeStart.Add(toNow).ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch (Exception)
            {

                return string.Empty;
            }
        }

        /// <summary>
        /// 获得Unix时间戳
        /// </summary>
        /// <returns></returns>
        public static int ObjConvertToTimeStamp(DateTime times)
        {
            return (int)((times.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds) / 1000);
        }
        #endregion

        #region 连接字符串
        /// <summary>
        /// 获取连接字符串
        /// </summary>
        /// <returns></returns>
        public static string GetConnectionString()
        {
            string connstrEncrypt = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            string connstr = Decrypt(connstrEncrypt);
            if (string.IsNullOrEmpty(connstr))
            {
                throw new Exception("连接字符串错误");
            }
            return connstr;
        }

        #endregion

        #region 文件操作
        /// <summary>
        /// 创建文件
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns>创建失败返回false</returns>
        public static bool CreateFile(string fileName)
        {
            try
            {
                if (File.Exists(fileName)) return false;
                var fs = File.Create(fileName);
                fs.Close();
                fs.Dispose();
            }
            catch (IOException ioe)
            {
                throw new IOException(ioe.Message);
            }

            return true;
        }


        /// <summary>
        /// 读文件内容,转化为字符类型
        /// </summary>
        /// <param name="fileName">文件路径</param>
        /// <returns></returns>
        public static string Read(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return null;
            }
            //将文件信息读入流中
            using (var fs = new FileStream(fileName, FileMode.Open))
            {
                return new StreamReader(fs).ReadToEnd();
            }
        }

        /// <summary>
        /// 写文件
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="content">文件内容</param>
        /// <returns></returns>
        public static bool Write(string fileName, string content)
        {
            if (File.Exists(fileName))
            {
                CreateFile(fileName);
            }
            try
            {
                //将文件信息读入流中
                //初始化System.IO.FileStream类的新实例与指定路径和创建模式
                using (var fs = new FileStream(fileName, FileMode.OpenOrCreate))
                {
                    //锁住流
                    lock (fs)
                    {
                        if (!fs.CanWrite)
                        {
                            throw new System.Security.SecurityException("文件fileName=" + fileName + "是只读文件不能写入!");
                        }

                        var buffer = Encoding.Default.GetBytes(content);
                        fs.Write(buffer, 0, buffer.Length);
                        return true;
                    }
                }
            }
            catch (IOException ioe)
            {
                throw new Exception(ioe.Message);
            }

        }
        public static bool AppendWrite(string fileName, string content)
        {
            if (File.Exists(fileName))
            {
                CreateFile(fileName);
            }
            try
            {
                //将文件信息读入流中
                //初始化System.IO.FileStream类的新实例与指定路径和创建模式
                using (var fs = new FileStream(fileName, FileMode.OpenOrCreate))
                {
                    //锁住流
                    lock (fs)
                    {
                        if (!fs.CanWrite)
                        {
                            throw new System.Security.SecurityException("文件fileName=" + fileName + "是只读文件不能写入!");
                        }
                        //设定书写的开始位置为文件的末尾  
                        fs.Position = fs.Length;
                        //将待写入内容追加到文件末尾  
                        var buffer = Encoding.Default.GetBytes(content);
                        fs.Write(buffer, 0, buffer.Length);
                        return true;
                    }
                }
            }
            catch (IOException ioe)
            {
                throw new Exception(ioe.Message);
            }

        }
        #endregion        

        /// <summary>
        /// 利用反射实现深拷贝
        /// </summary>
        /// <param name="_object"></param>
        /// <returns></returns>
        public static object DeepCopy(object _object)
        {
            Type T = _object.GetType();
            object o = Activator.CreateInstance(T);
            PropertyInfo[] PI = T.GetProperties();
            for (int i = 0; i < PI.Length; i++)
            {
                PropertyInfo P = PI[i];
                P.SetValue(o, P.GetValue(_object));
            }
            return o;
        }
    }

}
