using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace Common
{

    public static class EnumExtension
    {
        /// <summary>
        /// 获取枚举描述信息
        /// </summary>
        /// <param name="enumeration"></param>
        /// <returns></returns>
        public static string GetDescription(this Enum enumeration)
        {
            string description = enumeration.GetAttributeValue<DescriptionAttribute, string>(x => x.Description);
            if (String.IsNullOrEmpty(description))
            {
                description = enumeration.ToString();
            }
            return description;
        }

        //public static string GetWebDescription<T>(this Controller controller, string value)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(value))
        //        {
        //            return "";
        //        }
        //        else
        //        {
        //            Enum ee = (Enum)Enum.Parse(typeof(T), value);
        //            return ee.GetDescription();
        //        }
        //    }
        //    catch
        //    {
        //        return "";
        //    }
        //}

        public static string GetIntValue(this Enum v)
        {
            return Convert.ToInt32(v).ToString();
        }
        public static T ConvertToEnum<T>(string v)  
        {
            return (T)Enum.Parse(typeof(T), v);
        }

        /// <summary>
        /// GetAttributeValue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="Expected"></typeparam>
        /// <param name="enumeration"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static Expected GetAttributeValue<T, Expected>(this Enum enumeration, Func<T, Expected> expression) where T : Attribute
        {
            var arr = enumeration.GetType()
                .GetMember(enumeration.ToString());
            if (arr.Length == 0)
            {
                return default(Expected);
            }
            T attribute = arr[0].GetCustomAttributes(typeof(T), false)
                .Cast<T>().SingleOrDefault();

            if (attribute == null)
                return default(Expected);
            return expression(attribute);
        }

        #region 获取枚举列表

        public static ICollection<ListItem> GetEnumList<T>()
        {
            if (typeof(T).IsEnum)
            {
                ICollection<ListItem> list = new HashSet<ListItem>();
                Array array = Enum.GetValues(typeof(T));
                for (int i = 0; i < array.Length; i++)
                {
                    ListItem item = new ListItem();
                    string val = array.GetValue(i).ToString();

                    Enum ee = (Enum)Enum.Parse(typeof(T), val);
                    item.Value = Convert.ToInt32(ee).ToString();
                    item.Text = ee.GetDescription();
                    list.Add(item);
                }
                return list;
            }
            return null;
        }

        public static ICollection<ListItem> GetEnumNameList<T>()
        {
            if (typeof(T).IsEnum)
            {
                ICollection<ListItem> list = new HashSet<ListItem>();
                var array = Enum.GetNames(typeof(T));
                foreach (var v in array)
                {
                    ListItem item = new ListItem();
                    Enum ee = (Enum)Enum.Parse(typeof(T), v);
                    item.Value = v;
                    item.Text = ee.GetDescription();
                    list.Add(item);
                }
                return list;
            }
            return null;
        }

        public static Dictionary<string,string> GetEnumNameDict<T>()
        {
            if (typeof(T).IsEnum)
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();
                var array = Enum.GetNames(typeof(T));
                foreach (var v in array)
                {
                    Enum ee = (Enum)Enum.Parse(typeof(T), v);
                    dict.Add(v, ee.GetDescription());
                }
                return dict;
            }
            return null;
        }
        #endregion
    }

}
