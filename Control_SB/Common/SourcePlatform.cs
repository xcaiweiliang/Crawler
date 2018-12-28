using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Control_SB
{
    public class SourcePlatform
    {
        public string key { get; set; }
        public string name { get; set; }
        /// <summary>
        /// 沙巴地址
        /// </summary>
        public string url { get; set; }//加上&lang=cs
        /// <summary>
        /// 没有登录的地址
        /// </summary>
        public string nologinurl { get; set; }//加上&lang=cs
        /// <summary>
        /// 平台登录地址
        /// </summary>
        public string loginurl { get; set; }
        /// <summary>
        /// 赛果地址
        /// </summary>
        public string resulturl { get; set; }
        /// <summary>
        /// 维护信息地址
        /// </summary>
        public string UMurl { get; set; }
        /// <summary>
        /// 公告地址
        /// </summary>
        public string messageurl { get; set; }

        public string loginname { get; set; }
        public string loginpassword { get; set; }

        public SourcePlatform(string key)
        {
            string congfigFile = Application.StartupPath + "\\SP.config";
            XmlDocument document = new XmlDocument();
            document.Load(congfigFile);
            XmlNodeList nodes = document.SelectNodes("/configuration/SourcePlatform/add");
            foreach (XmlNode item in nodes)
            {
                if (item.Attributes["key"].Value == key)
                {
                    this.key = key;
                    this.name = item.Attributes["name"].Value;
                    this.url = item.Attributes["url"].Value + "&lang=cs";
                    this.nologinurl = item.Attributes["nologinurl"].Value + "&lang=cs";
                    this.loginurl = item.Attributes["loginurl"].Value;
                    this.resulturl= item.Attributes["resulturl"].Value;
                    this.UMurl= item.Attributes["umurl"].Value;
                    this.messageurl = item.Attributes["messageurl"].Value;//登录后才能拿到中文公告
                    this.loginname = item.Attributes["loginname"].Value;
                    this.loginpassword = item.Attributes["loginpassword"].Value;
                    break;
                }
            }
        }
    }
}
