using BLL;
using Common;
using Model;
using OpenQA.Selenium;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Control_SB
{
    public partial class Main : Form
    {
        //多线程中会修改到的变量加volatile修饰符

        private static object lockwin = new object();
        private static object locklist = new object();

        private DateTime _runstarttime = DateTime.Now;
        private volatile SourcePlatform _SourcePlatform = null;
        private string _browser = "gc";

        private string _ProxyIP = string.Empty;

        private static volatile bool _IsRunning = false;//是否正在运行
        private static volatile bool _IsMaintenance = false;//是否正在维护
        private static volatile bool _IsLogining = false;//是否正在登录

        private volatile bool _EnableParallelLeagueMatch = false;//联赛循环是否启用并行
        private volatile bool _EnableParallelMatch = false;//赛事循环是否启用并行

        private volatile bool _SaveRunningLog = false;//是否保存运行日志

        private volatile List<IWebDriver> _WebDriverList = new List<IWebDriver>();
        private List<Log> _LogWinList = new List<Log>();

        private bool _ZP_AllDay = false;//早盘，所有日期

        private int _ZP_MS = 1000;//早盘抓取间隔，毫秒
        private int _JR_MS = 1000;//今日赛事抓取间隔，毫秒
        private int _GQ_MS = 1000;//滚球抓取间隔，毫秒
        private int _SG_MS = 1000;//赛果抓取间隔，毫秒

        private int _WebDriver_MS = 50;//判断元素时需等待的时间，毫秒

        private string _LogPath = System.Windows.Forms.Application.StartupPath + "/log/";

        private volatile List<Cookie> _CookieList = new List<Cookie>();
        private volatile List<Cookie> _ResultCookieList = new List<Cookie>();

        string[,,] menuarr = new string[,,]
        {
            {{"早盘","今日赛事","滚球","","","",""}},
            {{"足球","篮球、美式足球","网球","排球","棒球","羽毛球","乒乓球"}},
            {{"独赢&让球&大小&单双","半场全场","波胆","总入球","冠军","",""}},
        };
        //联盟
        private static volatile Dictionary<string, List<A_LeagueMatch>> Temp_A_LeagueMatch_Dict = new Dictionary<string, List<A_LeagueMatch>>();
        //private static volatile Dictionary<string, ConcurrentBag<A_LeagueMatch>> Temp_A_LeagueMatch_Dict = new Dictionary<string, ConcurrentBag<A_LeagueMatch>>();

        //队伍
        private static volatile Dictionary<string, List<A_Team>> Temp_A_Team_Dict = new Dictionary<string, List<A_Team>>();
        //private static volatile Dictionary<string, ConcurrentBag<A_Team>> Temp_A_Team_Dict = new Dictionary<string, ConcurrentBag<A_Team>>();

        //比赛
        private static volatile Dictionary<string, List<A_Match>> Temp_A_Match_Dict = new Dictionary<string, List<A_Match>>();
        //private static volatile Dictionary<string, ConcurrentBag<A_Match>> Temp_A_Match_Dict = new Dictionary<string, ConcurrentBag<A_Match>>();

        //赛果
        private static volatile Dictionary<string, List<A_MatchResult>> Temp_A_MatchResult_Dict = new Dictionary<string, List<A_MatchResult>>();
        //private static volatile Dictionary<string, ConcurrentBag<A_MatchResult>> Temp_A_MatchResult_Dict = new Dictionary<string, ConcurrentBag<A_MatchResult>>();

        //比分记录
        private static volatile Dictionary<string, List<A_MatchScoreRecord>> Temp_A_MatchScoreRecord_Dict = new Dictionary<string, List<A_MatchScoreRecord>>();
        //private static volatile Dictionary<string, ConcurrentBag<A_MatchScoreRecord>> Temp_A_MatchScoreRecord_Dict = new Dictionary<string, ConcurrentBag<A_MatchScoreRecord>>();

        //赔率        
        private static volatile Dictionary<string, Dictionary<string, List<O_Odds>>> Temp_O_Odds_Dict = new Dictionary<string, Dictionary<string, List<O_Odds>>>();
        //private static volatile Dictionary<string, Dictionary<string, ConcurrentBag<O_Odds>>> Temp_O_Odds_Dict = new Dictionary<string, Dictionary<string, ConcurrentBag<O_Odds>>>();
        //玩法码表
        private Dictionary<string, Dictionary<string, string>> Dict_S_BetCode = new Dictionary<string, Dictionary<string, string>>();
        //体育类型
        private Dictionary<int, string> Dict_SportsType = new Dictionary<int, string>();
        //场次类型
        private string[] _Arr_HF = new string[] { "", "", "上半场", "下半场", "第一节", "第二节", "第三节", "第四节", "加时" };

        //初始化数据
        private void InitTempData()
        {
            try
            {
                var stdict = EnumExtension.GetEnumNameDict<SportsTypeEnum>();

                A_LeagueMatchBll lmbll = new A_LeagueMatchBll();
                A_TeamBll tbll = new A_TeamBll();
                A_MatchBll mbll = new A_MatchBll();
                A_MatchScoreRecordBll msrbll = new A_MatchScoreRecordBll();
                A_MatchResultBll mrbll = new A_MatchResultBll();
                O_OddsBll obll = new O_OddsBll();

                DateTime now = lmbll.GetServerDateTime();

                var lmlist = lmbll.FindAll(SourcePlatformEnum.SB.ToString(), "");
                var tlist = tbll.FindAll(SourcePlatformEnum.SB.ToString(), "");
                var mlist = mbll.FindByDate(SourcePlatformEnum.SB.ToString(), "", now.Date.AddDays(-1));
                var mrlist = mrbll.FindByDate(SourcePlatformEnum.SB.ToString(), "", now.Date.AddDays(-1));
                var msrlist = msrbll.FindByDate(SourcePlatformEnum.SB.ToString(), "", now.Date.AddDays(-1));
                var oddslist = obll.FindByDate(SourcePlatformEnum.SB.ToString(), "", now.Date.AddDays(-1));
                Main.Temp_A_LeagueMatch_Dict.Clear();
                Main.Temp_A_Team_Dict.Clear();
                Main.Temp_A_Match_Dict.Clear();
                Main.Temp_A_MatchResult_Dict.Clear();
                Main.Temp_A_MatchScoreRecord_Dict.Clear();
                Main.Temp_O_Odds_Dict.Clear();
                foreach (string key in stdict.Keys)
                {
                    Main.Temp_A_LeagueMatch_Dict.Add(key, lmlist.Where(x => x.SportsType == key).ToList());
                    //Main.Temp_A_LeagueMatch_Dict.Add(key, new ConcurrentBag<A_LeagueMatch>(lmlist.Where(x => x.SportsType == key)));
                    Main.Temp_A_Team_Dict.Add(key, tlist.Where(x => x.SportsType == key).ToList());
                    //Main.Temp_A_Team_Dict.Add(key, new ConcurrentBag<A_Team>(tlist.Where(x => x.SportsType == key)));
                    Main.Temp_A_Match_Dict.Add(key, mlist.Where(x => x.SportsType == key).ToList());
                    //Main.Temp_A_Match_Dict.Add(key, new ConcurrentBag<A_Match>(mlist.Where(x => x.SportsType == key)));
                    Main.Temp_A_MatchResult_Dict.Add(key, mrlist.Where(x => x.SportsType == key).ToList());
                    //Main.Temp_A_MatchResult_Dict.Add(key, new ConcurrentBag<A_MatchResult>(mrlist.Where(x => x.SportsType == key)));
                    Main.Temp_A_MatchScoreRecord_Dict.Add(key, msrlist.Where(x => x.SportsType == key).ToList());
                    //Main.Temp_A_MatchScoreRecord_Dict.Add(key, new ConcurrentBag<A_MatchScoreRecord>(msrlist.Where(x => x.SportsType == key)));
                    var od = new Dictionary<string, List<O_Odds>>();
                    foreach (string code in this.Dict_S_BetCode[key].Values)
                    {
                        od.Add(code, oddslist.Where(x => x.SportsType == key && x.BetCode == code).ToList());
                    }
                    //var od = new Dictionary<string, ConcurrentBag<O_Odds>>();
                    //foreach (string code in this.Dict_S_BetCode[key].Values)
                    //{
                    //    od.Add(code, new ConcurrentBag<O_Odds>(oddslist.Where(x => x.SportsType == key && x.BetCode == code)));
                    //}
                    Main.Temp_O_Odds_Dict.Add(key, od);
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLog(ex.Message, ex);
                //InitTempData();
                if (!Main._IsRunning)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        //初始化配置选项
        private void InitOption()
        {
            string congfigFile = Application.StartupPath + "\\Option.config";
            XmlDocument document = new XmlDocument();
            document.Load(congfigFile);
            //平台
            XmlNode spnode = document.SelectSingleNode("/configuration/sp/add");
            if (string.IsNullOrEmpty(spnode.Attributes["value"].Value))
            {
                this.rdo_TYC.Checked = true;
            }
            else
            {
                foreach (Control item in this.pnl_SourcePlatform.Controls)
                {
                    if (item is RadioButton)
                    {
                        RadioButton rdo = item as RadioButton;
                        if (rdo.Tag.ToString() == spnode.Attributes["value"].Value)
                        {
                            rdo.Checked = true;
                            break;
                        }
                    }
                }
            }
            //浏览器
            XmlNode browsernode = document.SelectSingleNode("/configuration/browser/add");
            if (string.IsNullOrEmpty(browsernode.Attributes["value"].Value))
            {
                this.rdo_gc.Checked = true;
            }
            else
            {
                foreach (Control item in this.pnl_Browser.Controls)
                {
                    if (item is RadioButton)
                    {
                        RadioButton rdo = item as RadioButton;
                        if (rdo.Tag.ToString() == browsernode.Attributes["value"].Value)
                        {
                            rdo.Checked = true;
                            break;
                        }
                    }
                }
            }
            if (this.rdo_gc.Checked)
            {
                this.chk_IPproxy.Checked = false;
                this.chk_IPproxy.Enabled = false;
            }
            //设置         
            XmlNodeList setnodes = document.SelectNodes("/configuration/set/add");
            foreach (XmlNode item in setnodes)
            {
                bool b = false;
                bool.TryParse(item.Attributes["value"].Value, out b);
                string key = item.Attributes["key"].Value;
                if (key == this.chk_Logined.Name)
                {
                    this.chk_Logined.Checked = b;
                }
                else if (key == this.chk_IPproxy.Name)
                {
                    this.chk_IPproxy.Checked = b;
                }
                else if (key == this.chk_VisibleBrowser.Name)
                {
                    this.chk_VisibleBrowser.Checked = b;
                }
                else if (key == this.chk_ZP_AllDay.Name)
                {
                    this.chk_ZP_AllDay.Checked = b;
                }
            }
            //抓取间隔
            XmlNodeList intervalnodes = document.SelectNodes("/configuration/interval/add");
            foreach (XmlNode item in intervalnodes)
            {
                string key = item.Attributes["key"].Value;
                string value = item.Attributes["value"].Value;
                if (key == this.txt_ZP_MS.Name)
                {
                    this.txt_ZP_MS.Text = value;
                }
                else if (key == this.txt_JR_MS.Name)
                {
                    this.txt_JR_MS.Text = value;
                }
                else if (key == this.txt_GQ_MS.Name)
                {
                    this.txt_GQ_MS.Text = value;
                }
                else if (key == this.txt_SG_MS.Name)
                {
                    this.txt_SG_MS.Text = value;
                }
            }
            //抓取内容
            XmlNodeList optionnodes = document.SelectNodes("/configuration/option/add");
            foreach (XmlNode item in optionnodes)
            {
                string key = item.Attributes["key"].Value;
                string value = item.Attributes["value"].Value;
                string[] arr = null;
                if (value.Contains(","))
                {
                    arr = value.Split(',');
                }
                else
                {
                    arr = new string[] { value };
                }
                switch (key)
                {
                    case "ZP":
                        foreach (TreeNode n in this.tvw_ZP.Nodes)
                        {
                            if (arr.Contains(n.Tag.ToString()))
                            {
                                n.Checked = true;
                                foreach (TreeNode n2 in n.Nodes)
                                {
                                    n2.Checked = true;
                                }
                            }
                        }
                        break;
                    case "JR":
                        foreach (TreeNode n in this.tvw_JR.Nodes)
                        {
                            if (arr.Contains(n.Tag.ToString()))
                            {
                                n.Checked = true;
                                foreach (TreeNode n2 in n.Nodes)
                                {
                                    n2.Checked = true;
                                }
                            }
                        }
                        break;
                    case "GQ":
                        foreach (TreeNode n in this.tvw_GQ.Nodes)
                        {
                            if (arr.Contains(n.Tag.ToString()))
                            {
                                n.Checked = true;
                                foreach (TreeNode n2 in n.Nodes)
                                {
                                    n2.Checked = true;
                                }
                            }
                        }
                        break;
                    case "SG":
                        foreach (Control c in this.grp_SG.Controls)
                        {
                            if (c is CheckBox)
                            {
                                CheckBox chk = c as CheckBox;
                                if (arr.Contains(chk.Tag.ToString().Replace("type-", "")))
                                {
                                    chk.Checked = true;
                                }
                            }
                        }
                        break;
                }
            }
        }
        //保存配置选项
        private void SaveOption()
        {
            string congfigFile = Application.StartupPath + "\\Option.config";
            XmlDocument document = new XmlDocument();
            document.Load(congfigFile);
            //平台
            string sp = string.Empty;
            foreach (Control item in this.pnl_SourcePlatform.Controls)
            {
                if (item is RadioButton)
                {
                    RadioButton rdo = item as RadioButton;
                    if (rdo.Checked)
                    {
                        sp = rdo.Tag.ToString();
                        break;
                    }
                }
            }
            XmlElement spnode = document.SelectSingleNode("/configuration/sp/add") as XmlElement;
            spnode.SetAttribute("value", sp);
            //浏览器
            string browser = string.Empty;
            foreach (Control item in this.pnl_Browser.Controls)
            {
                if (item is RadioButton)
                {
                    RadioButton rdo = item as RadioButton;
                    if (rdo.Checked)
                    {
                        browser = rdo.Tag.ToString();
                        break;
                    }
                }
            }
            XmlElement browsernode = document.SelectSingleNode("/configuration/browser/add") as XmlElement;
            browsernode.SetAttribute("value", browser);
            //设置         
            XmlNode setnode = document.SelectSingleNode("/configuration/set");
            XmlElement login = setnode.SelectSingleNode("//add[@key='" + this.chk_Logined.Name + "']") as XmlElement;
            login.SetAttribute("value", this.chk_Logined.Checked.ToString());
            XmlElement ip = setnode.SelectSingleNode("//add[@key='" + this.chk_IPproxy.Name + "']") as XmlElement;
            ip.SetAttribute("value", this.chk_IPproxy.Checked.ToString());
            XmlElement vb = setnode.SelectSingleNode("//add[@key='" + this.chk_VisibleBrowser.Name + "']") as XmlElement;
            vb.SetAttribute("value", this.chk_VisibleBrowser.Checked.ToString());
            XmlElement zpad = setnode.SelectSingleNode("//add[@key='" + this.chk_ZP_AllDay.Name + "']") as XmlElement;
            zpad.SetAttribute("value", this.chk_ZP_AllDay.Checked.ToString());
            //抓取间隔
            XmlNode intervalnode = document.SelectSingleNode("/configuration/interval");
            XmlElement zpms = intervalnode.SelectSingleNode("//add[@key='" + this.txt_ZP_MS.Name + "']") as XmlElement;
            zpms.SetAttribute("value", this.txt_ZP_MS.Text.Trim());
            XmlElement jrms = intervalnode.SelectSingleNode("//add[@key='" + this.txt_JR_MS.Name + "']") as XmlElement;
            jrms.SetAttribute("value", this.txt_JR_MS.Text.Trim());
            XmlElement gqms = intervalnode.SelectSingleNode("//add[@key='" + this.txt_GQ_MS.Name + "']") as XmlElement;
            gqms.SetAttribute("value", this.txt_GQ_MS.Text.Trim());
            XmlElement sgms = intervalnode.SelectSingleNode("//add[@key='" + this.txt_SG_MS.Name + "']") as XmlElement;
            sgms.SetAttribute("value", this.txt_SG_MS.Text.Trim());
            //抓取内容
            XmlNode optionnode = document.SelectSingleNode("/configuration/option");
            string zpt = string.Empty, jrt = string.Empty, gqt = string.Empty, sgt = string.Empty;
            foreach (TreeNode n in this.tvw_ZP.Nodes)
            {
                if (n.Checked)
                {
                    zpt += n.Tag.ToString() + ",";
                }
            }
            zpt = zpt.TrimEnd(',');
            foreach (TreeNode n in this.tvw_JR.Nodes)
            {
                if (n.Checked)
                {
                    jrt += n.Tag.ToString() + ",";
                }
            }
            jrt = jrt.TrimEnd(',');
            foreach (TreeNode n in this.tvw_GQ.Nodes)
            {
                if (n.Checked)
                {
                    gqt += n.Tag.ToString() + ",";
                }
            }
            gqt = gqt.TrimEnd(',');
            foreach (Control c in this.grp_SG.Controls)
            {
                if (c is CheckBox)
                {
                    CheckBox chk = c as CheckBox;
                    if (chk.Checked)
                    {
                        sgt += chk.Tag.ToString().Replace("type-", "") + ",";
                    }
                }
            }
            sgt = sgt.TrimEnd(',');
            XmlElement zp = intervalnode.SelectSingleNode("//add[@key='ZP']") as XmlElement;
            zp.SetAttribute("value", zpt);
            XmlElement jr = intervalnode.SelectSingleNode("//add[@key='JR']") as XmlElement;
            jr.SetAttribute("value", jrt);
            XmlElement gq = intervalnode.SelectSingleNode("//add[@key='GQ']") as XmlElement;
            gq.SetAttribute("value", gqt);
            XmlElement sg = intervalnode.SelectSingleNode("//add[@key='SG']") as XmlElement;
            sg.SetAttribute("value", sgt);

            document.Save(congfigFile);
        }
        public Main()
        {
            InitializeComponent();
            if (!Directory.Exists(_LogPath))
            {
                Directory.CreateDirectory(_LogPath);
            }
            string updateCache = ConfigurationManager.AppSettings["updateCache"];
            int ucm = 10;
            int.TryParse(updateCache, out ucm);
            this.timer2.Interval = 1000 * 60 * ucm;

            this._EnableParallelLeagueMatch = ConfigurationManager.AppSettings["enableParallelLeagueMatch"] == "1" ? true : false;
            this._EnableParallelMatch = ConfigurationManager.AppSettings["enableParallelMatch"] == "1" ? true : false;

            this._SaveRunningLog = ConfigurationManager.AppSettings["saveRunningLog"] == "1" ? true : false;

            //1：足球，2：篮球，3：网球，4：排球，5：棒球，6：羽毛球，7：乒乓球，8：美式足球
            this.Dict_SportsType.Add(1, SportsTypeEnum.Football.ToString());
            this.Dict_SportsType.Add(2, SportsTypeEnum.Basketball.ToString());
            this.Dict_SportsType.Add(3, SportsTypeEnum.Tennis.ToString());
            this.Dict_SportsType.Add(4, SportsTypeEnum.Volleyball.ToString());
            this.Dict_SportsType.Add(5, SportsTypeEnum.Baseball.ToString());
            this.Dict_SportsType.Add(6, SportsTypeEnum.Badminton.ToString());
            this.Dict_SportsType.Add(7, SportsTypeEnum.Pingpong.ToString());
            this.Dict_SportsType.Add(8, SportsTypeEnum.AmericanFootball.ToString());

            var stdict = EnumExtension.GetEnumNameDict<SportsTypeEnum>();
            var betcodelist = new S_BetCodeBll().FindList();
            foreach (string st in stdict.Keys)
            {
                var list = betcodelist.Where(x => x.SportsType == st).ToList();
                var dict = list.ToDictionary(key => key.CodeName, val => val.Code);
                this.Dict_S_BetCode.Add(st, dict);
            }
            InitOption();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key1">1：早盘，2：今日赛事，3：滚球</param>
        /// <param name="key2">1：足球，2：篮球、美式足球，3：网球，4：排球，5：棒球，6：羽毛球，7：乒乓球</param>
        /// <param name="key3">足球{1：独赢 & 让球 & 大小 & 单双，2：半场全场，3：波胆，5：总入球，5：冠军}</param>
        private void GetData(int key1, int key2, int key3, Log logwin)
        {
            bool islogin = false;
            bool isexception = false;
            int awaitms = 10000;
            IWebDriver driver = WebDriverHelper.CreateWebDriver(this._browser, chk_VisibleBrowser.Checked, false, this._ProxyIP);
            //if (key1 + "" + key2 + "" + key3 == "111")
            //{
            //    driver = WebDriverHelper.CreateWebDriver(true, false, this._ProxyIP);
            //}
            if (driver == null)
            {
                //MessageBox.Show(WebDriverHelper.ErrorMessage);
                LogHelper.WriteLog(WebDriverHelper.ErrorMessage);
                this.Invoke(new MethodInvoker(delegate
                {
                    logwin.txt_log.AppendText(WebDriverHelper.ErrorMessage + "\r\n");
                    if (this._SaveRunningLog)
                    {
                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", WebDriverHelper.ErrorMessage + "\r\n");
                    }
                }));                
                return;
            }
            _WebDriverList.Add(driver);
            string rqurl = string.Empty;
            this.Invoke(new MethodInvoker(delegate
            {
                Main._IsRunning = true;
                islogin = this.chk_Logined.Checked;
                if (islogin)
                {
                    rqurl = this._SourcePlatform.url;
                }
                else
                {
                    rqurl = this._SourcePlatform.nologinurl;
                }
                logwin.txt_log.AppendText("地址：" + rqurl + "\r\n");
                logwin.txt_log.AppendText("\r\n");
                logwin.txt_log.AppendText("" + key1 + key2 + key3 + "开始初次请求：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n");
                logwin.txt_log.AppendText("===========================================\r\n");
                if (this._SaveRunningLog)
                {
                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "地址：" + rqurl + "\r\n\r\n" + key1 + key2 + key3 + "开始初次请求：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n===========================================\r\n");
                }
            }));
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            if (islogin)
            {
                driver.Navigate().GoToUrl(this._SourcePlatform.url);
                driver.Manage().Cookies.DeleteAllCookies();
                foreach (var item in _CookieList)
                {
                    driver.Manage().Cookies.AddCookie(item);
                }
            }
            driver.Navigate().GoToUrl(rqurl);
            watch.Start();
            string html = string.Empty;
            int pagecount = 1;
            bool flag_loading = true;

            int count = 1;
            try
            {
                if (!Main._IsMaintenance)
                {
                    choosemenu(key1, key2, key3, logwin, driver, ref awaitms);
                    this.Invoke(new MethodInvoker(delegate
                    {
                        logwin.txt_log.AppendText("===========================================\r\n");
                        if (this._SaveRunningLog)
                        {
                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "===========================================\r\n");
                        }

                    }));
                    checkloading(key1, key2, key3, logwin, driver, ref count, ref awaitms);
                    this.Invoke(new MethodInvoker(delegate
                    {
                        logwin.txt_log.AppendText("loading结束\r\n");
                        if (this._SaveRunningLog)
                        {
                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "loading结束\r\n");
                        }

                    }));
                    //判断是否有数据
                    bool flag1 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#container .message-box"), _WebDriver_MS);
                    bool flag2 = WebDriverHelper.WaitForElementInvisible(driver, By.CssSelector("#container .pagination"), _WebDriver_MS);
                    if (flag1 && flag2)
                    {
                        int needupdatecount = 0, updatedcount = 0, deletedcount = 0;
                        string msgpush = string.Empty, delmsgpush = string.Empty;
                        Transformation(key1, key2, key3, new List<LeagueMatch>(), ref needupdatecount, ref updatedcount, ref deletedcount, ref msgpush, ref delmsgpush);
                        if (key2 == 2)
                        {
                            Transformation(key1, 8, key3, new List<LeagueMatch>(), ref needupdatecount, ref updatedcount, ref deletedcount, ref msgpush, ref delmsgpush);
                        }
                        this.Invoke(new MethodInvoker(delegate
                        {
                            //无数据
                            if (!string.IsNullOrEmpty(delmsgpush))
                            {
                                logwin.txt_log.AppendText("删除错误：\r\n" + delmsgpush);
                                if (this._SaveRunningLog)
                                {
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "删除错误：\r\n" + delmsgpush);
                                }
                            }
                            logwin.txt_log.AppendText("删除成功：" + deletedcount + "\r\n");
                            if (this._SaveRunningLog)
                            {
                                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "删除成功：" + deletedcount + "\r\n");
                            }

                            logwin.txt_log.AppendText("无赛事\r\n");
                            logwin.txt_log.AppendText("===========================================\r\n");
                            if (this._SaveRunningLog)
                            {
                                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "无赛事\r\n===========================================\r\n");
                            }
                        }));
                    }
                    else
                    {
                        flag_loading = false;
                        //第一页数据
                        html = driver.FindElement(By.CssSelector("#container .match-container")).GetAttribute("innerHTML");
                        this.Invoke(new MethodInvoker(delegate
                        {
                            logwin.txt_log.AppendText("第1页数据完\r\n");
                            if (this._SaveRunningLog)
                            {
                                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "第1页数据完\r\n");
                            }

                        }));
                        string pagecountstr = driver.FindElement(By.CssSelector("#container .page")).Text;
                        pagecountstr = pagecountstr.Split('/')[1];
                        pagecountstr = pagecountstr.Substring(0, pagecountstr.Length - 1);
                        int.TryParse(pagecountstr, out pagecount);
                        for (int i = 2; i <= pagecount; i++)
                        {
                            //翻页
                            driver.FindElement(By.CssSelector("#container .pagination .dropdown")).Click();
                            bool flag12 = false;
                            int dropcount = 0;
                            while (!flag12)
                            {
                                dropcount++;
                                if (dropcount > 20)
                                {
                                    dropcount = 0;
                                    driver.FindElement(By.CssSelector("#container .pagination .dropdown")).Click();
                                }
                                flag12 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#container .pagination .dropdown>ul"), _WebDriver_MS);
                            }
                            //WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#container .pagination .dropdown>ul"), _WebDriver_MS);
                            driver.FindElement(By.CssSelector("#container .pagination .dropdown>ul>li:nth-child(" + i + ")")).Click();
                            html += "$************************$" + driver.FindElement(By.CssSelector("#container .match-container")).GetAttribute("innerHTML");
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("第" + i + "页数据完\r\n");
                                if (this._SaveRunningLog)
                                {
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "第" + i + "页数据完\r\n");
                                }

                            }));
                        }
                        this.Invoke(new MethodInvoker(delegate
                        {
                            logwin.txt_log.AppendText("数据抓取完毕\r\n");
                            if (this._SaveRunningLog)
                            {
                                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "数据抓取完毕\r\n");
                            }
                        }));
                        if (key2 == 2)//拆开篮球和美式足球
                        {
                            html = html.Replace("in-play", "");
                            string htmlb = string.Empty, htmla = string.Empty;
                            string splitkey = "<div class=\"match-odds-title match-basketball \">";//美式足球
                            if (html.Contains(splitkey))//有美式足球
                            {
                                string[] htmlarr = html.Replace(splitkey, "mszq************mszq" + splitkey).Split(new string[] { "mszq************mszq" }, StringSplitOptions.None);
                                for (int i = 0; i < htmlarr.Length; i++)
                                {
                                    if (htmlarr[i].Contains(splitkey))
                                    {
                                        htmla += htmlarr[i];
                                    }
                                    else
                                    {
                                        htmlb += htmlarr[i];
                                    }
                                }
                                if (!string.IsNullOrEmpty(htmlb.Trim()))
                                {
                                    html = htmlb;
                                }
                                //解析数据美式足球
                                #region 解析数据
                                var almList = AnalysisHtml(htmla, key1, 8, key3);//美式足球
                                if (almList != null && almList.Count > 0)
                                {
                                    if (!System.Text.RegularExpressions.Regex.IsMatch(almList[0].Name, @"[\u4e00-\u9fa5]"))//不包含中文
                                    {
                                        this.Invoke(new MethodInvoker(delegate
                                        {
                                            logwin.txt_log.AppendText("-------------------------------------------\r\n");
                                            logwin.txt_log.AppendText("美式足球\r\n");
                                            logwin.txt_log.AppendText("数据异常\r\n");
                                            logwin.txt_log.AppendText("-------------------------------------------\r\n");
                                            if (this._SaveRunningLog)
                                            {
                                                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "-------------------------------------------\r\n美式足球\r\n数据异常\r\n-------------------------------------------\r\n");
                                            }
                                        }));
                                    }
                                    else
                                    {
                                        if (_IsRunning)
                                        {
                                            int needupdatecount = 0, updatedcount = 0, deletedcount = 0;
                                            string msgpush = string.Empty, delmsgpush = string.Empty;
                                            Transformation(key1, 8, key3, almList, ref needupdatecount, ref updatedcount, ref deletedcount, ref msgpush, ref delmsgpush);
                                            this.Invoke(new MethodInvoker(delegate
                                            {
                                                logwin.txt_log.AppendText("-------------------------------------------\r\n");
                                                logwin.txt_log.AppendText("美式足球\r\n");
                                                if (this._SaveRunningLog)
                                                {
                                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "-------------------------------------------\r\n美式足球\r\n");
                                                }
                                                if (!string.IsNullOrEmpty(msgpush))
                                                {
                                                    logwin.txt_log.AppendText("错误：\r\n" + msgpush);
                                                    if (this._SaveRunningLog)
                                                    {
                                                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "错误：\r\n" + msgpush);
                                                    }
                                                }
                                                if (!string.IsNullOrEmpty(delmsgpush))
                                                {
                                                    logwin.txt_log.AppendText("删除错误：\r\n" + delmsgpush);
                                                    if (this._SaveRunningLog)
                                                    {
                                                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "删除错误：\r\n" + delmsgpush);
                                                    }
                                                }
                                                logwin.txt_log.AppendText("赔率需更新：" + needupdatecount + "\r\n更新成功：" + updatedcount + "\r\n删除成功：" + deletedcount + "\r\n");
                                                logwin.txt_log.AppendText("-------------------------------------------\r\n");
                                                if (this._SaveRunningLog)
                                                {
                                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "赔率需更新：" + needupdatecount + "\r\n更新成功：" + updatedcount + "\r\n删除成功：" + deletedcount + "\r\n-------------------------------------------\r\n");
                                                }
                                            }));
                                            //if (needupdatecount != updatedcount)
                                            //{
                                            //    O_OddsBll obll = new O_OddsBll();
                                            //    DateTime now = obll.GetServerDateTime();
                                            //    string sportstype = this.Dict_SportsType[key2];
                                            //    var olist = obll.FindByDate(SourcePlatformEnum.SB.ToString(), sportstype, now.Date.AddDays(-1));
                                            //    lock (locklist)
                                            //    {
                                            //        foreach (string code in this.Dict_S_BetCode[sportstype].Values)
                                            //        {
                                            //            Main.Temp_O_Odds_Dict[sportstype][code] = olist.Where(x => x.BetCode == code).ToList();
                                            //        }
                                            //    }
                                            //    this.Invoke(new MethodInvoker(delegate
                                            //    {
                                            //        logwin.txt_log.AppendText("更新" + sportstype + "缓存\r\n");
                                            //        if (this._SaveRunningLog)
                                            //        {
                                            //            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "更新" + sportstype + "缓存\r\n");
                                            //        }

                                            //    }));
                                            //}
                                        }
                                    }
                                }
                                #endregion
                            }
                            else//没有美式足球
                            {
                                int needupdatecount = 0, updatedcount = 0, deletedcount = 0;
                                string msgpush = string.Empty, delmsgpush = string.Empty;
                                Transformation(key1, 8, key3, new List<LeagueMatch>(), ref needupdatecount, ref updatedcount, ref deletedcount, ref msgpush, ref delmsgpush);
                                this.Invoke(new MethodInvoker(delegate
                                {
                                    logwin.txt_log.AppendText("-------------------------------------------\r\n");
                                    logwin.txt_log.AppendText("美式足球\r\n");
                                    if (this._SaveRunningLog)
                                    {
                                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "-------------------------------------------\r\n美式足球\r\n");
                                    }
                                    //无数据
                                    if (!string.IsNullOrEmpty(delmsgpush))
                                    {
                                        logwin.txt_log.AppendText("删除错误：\r\n" + delmsgpush);
                                        if (this._SaveRunningLog)
                                        {
                                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "删除错误：\r\n" + delmsgpush);
                                        }
                                    }
                                    logwin.txt_log.AppendText("删除成功：" + deletedcount + "\r\n");
                                    if (this._SaveRunningLog)
                                    {
                                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "删除成功：" + deletedcount + "\r\n");
                                    }
                                    logwin.txt_log.AppendText("美式足球无赛事\r\n");
                                    logwin.txt_log.AppendText("-------------------------------------------\r\n");
                                    if (this._SaveRunningLog)
                                    {
                                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "美式足球无赛事\r\n-------------------------------------------\r\n");
                                    }
                                }));
                            }
                        }
                        //解析数据
                        var lmList = AnalysisHtml(html, key1, key2, key3);
                        if (lmList != null && lmList.Count > 0)
                        {
                            if (!System.Text.RegularExpressions.Regex.IsMatch(lmList[0].Name, @"[\u4e00-\u9fa5]"))//不包含中文
                            {
                                this.Invoke(new MethodInvoker(delegate
                                {
                                    logwin.txt_log.AppendText("数据异常\r\n");
                                    if (this._SaveRunningLog)
                                    {
                                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "数据异常\r\n");
                                    }
                                }));
                            }
                            else
                            {
                                if (_IsRunning)
                                {
                                    int needupdatecount = 0, updatedcount = 0, deletedcount = 0;
                                    string msgpush = string.Empty, delmsgpush = string.Empty;
                                    Transformation(key1, key2, key3, lmList, ref needupdatecount, ref updatedcount, ref deletedcount, ref msgpush, ref delmsgpush);
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        if (!string.IsNullOrEmpty(msgpush))
                                        {
                                            logwin.txt_log.AppendText("错误：\r\n" + msgpush);
                                            if (this._SaveRunningLog)
                                            {
                                                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "错误：\r\n" + msgpush);
                                            }
                                        }
                                        if (!string.IsNullOrEmpty(delmsgpush))
                                        {
                                            logwin.txt_log.AppendText("删除错误：\r\n" + delmsgpush);
                                            if (this._SaveRunningLog)
                                            {
                                                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "删除错误：\r\n" + delmsgpush);
                                            }
                                        }
                                        logwin.txt_log.AppendText("赔率需更新：" + needupdatecount + "\r\n更新成功：" + updatedcount + "\r\n删除成功：" + deletedcount + "\r\n");
                                        if (this._SaveRunningLog)
                                        {
                                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "赔率需更新：" + needupdatecount + "\r\n更新成功：" + updatedcount + "\r\n删除成功：" + deletedcount + "\r\n");
                                        }
                                    }));
                                    //if (needupdatecount != updatedcount)
                                    //{
                                    //    O_OddsBll obll = new O_OddsBll();
                                    //    DateTime now = obll.GetServerDateTime();
                                    //    string sportstype = this.Dict_SportsType[key2];
                                    //    var olist = obll.FindByDate(SourcePlatformEnum.SB.ToString(), sportstype, now.Date.AddDays(-1));
                                    //    lock (locklist)
                                    //    {
                                    //        foreach (string code in this.Dict_S_BetCode[sportstype].Values)
                                    //        {
                                    //            Main.Temp_O_Odds_Dict[sportstype][code] = olist.Where(x => x.BetCode == code).ToList();
                                    //        }
                                    //    }
                                    //    this.Invoke(new MethodInvoker(delegate
                                    //    {
                                    //        logwin.txt_log.AppendText("更新" + sportstype + "缓存\r\n");
                                    //        if (this._SaveRunningLog)
                                    //        {
                                    //            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "更新" + sportstype + "缓存\r\n");
                                    //        }

                                    //    }));
                                    //}
                                }
                            }
                        }
                        watch.Stop();
                        this.Invoke(new MethodInvoker(delegate
                        {
                            logwin.txt_log.AppendText("===========================================\r\n");
                            logwin.txt_log.AppendText("耗时：" + watch.Elapsed.TotalSeconds + "秒\r\n");
                            if (this._SaveRunningLog)
                            {
                                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "===========================================\r\n耗时：" + watch.Elapsed.TotalSeconds + "秒\r\n");
                            }
                        }));
                    }
                }
                else
                {
                    this.Invoke(new MethodInvoker(delegate
                    {
                        logwin.txt_log.AppendText("===========================================\r\n");
                        logwin.txt_log.AppendText("沙巴正在维护\r\n");
                        if (this._SaveRunningLog)
                        {
                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "===========================================\r\n沙巴正在维护\r\n");
                        }                        
                    }));
                    GetMaintenanceInfo(logwin);
                }
            }
            catch (Exception ex)
            {
                if (ex is AggregateException)
                {
                    AggregateException e = ex as AggregateException;
                    foreach (var item in e.InnerExceptions)
                    {
                        LogHelper.WriteLog($"异常类型：{item.GetType()}<br/>来自：{item.Source}<br/>异常内容：{item.Message}");
                    }
                }
                else if (ex is NoSuchElementException)
                {
                    isexception = true;
                }
                else if (ex is NoSuchFrameException)
                {
                    isexception = true;
                }
                else if (ex is WebDriverException)
                {
                    if (ex.Message.Contains("Cannot start the driver service") || ex.Message.Contains("The HTTP request to the remote WebDriver server for URL"))
                    {
                        driver.Quit();
                        driver.Dispose();
                        driver = null;
                        driver = WebDriverHelper.CreateWebDriver(this._browser, this.chk_VisibleBrowser.Checked, false, this._ProxyIP);
                        if (driver == null)
                        {
                            LogHelper.WriteLog(WebDriverHelper.ErrorMessage);
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText(WebDriverHelper.ErrorMessage + "\r\n");
                                if (this._SaveRunningLog)
                                {
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", WebDriverHelper.ErrorMessage + "\r\n");
                                }
                            }));
                        }
                        else
                        {
                            _WebDriverList.Add(driver);
                            if (this.chk_Logined.Checked)
                            {
                                driver.Navigate().GoToUrl(this._SourcePlatform.url);
                                driver.Manage().Cookies.DeleteAllCookies();
                                foreach (var item in _CookieList)
                                {
                                    driver.Manage().Cookies.AddCookie(item);
                                }
                                driver.Navigate().Refresh();
                            }
                            else
                            {
                                driver.Navigate().GoToUrl(this._SourcePlatform.nologinurl);
                            }
                        }
                    }
                    else
                    {
                        isexception = true;
                    }
                }
                LogHelper.ErrorLog(ex.Message, ex);
                //driver.Quit();
                this.Invoke(new MethodInvoker(delegate
                {
                    logwin.txt_log.AppendText("异常：" + ex.ToString() + ex.StackTrace + "\r\n");
                    if (this._SaveRunningLog)
                    {
                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "异常：" + ex.ToString() + ex.StackTrace + "\r\n");
                    }
                }));
                //return;
            }

            //int count = 1;
            Task.Run(async () =>
            {
                while (Main._IsRunning)
                {
                    await Task.Delay(awaitms);
                    try
                    {
                        count++;
                        if (WebDriverHelper.AlertExist(driver))
                        {
                            driver.SwitchTo().Alert().Accept();
                        }
                        if (isexception || Main._IsMaintenance)
                        {
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("异常或维护，刷新页面\r\n");
                                if (this._SaveRunningLog)
                                {
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "异常或维护，刷新页面\r\n");
                                }
                            }));
                            isexception = false;
                            flag_loading = true;
                            driver.SwitchTo().DefaultContent();
                            if (islogin)
                            {
                                driver.Navigate().GoToUrl(this._SourcePlatform.url);
                            }
                            else
                            {
                                driver.Navigate().GoToUrl(this._SourcePlatform.nologinurl);
                            }
                            if (islogin)
                            {
                                driver.Manage().Cookies.DeleteAllCookies();
                                foreach (var item in _CookieList)
                                {
                                    driver.Manage().Cookies.AddCookie(item);
                                }
                                driver.Navigate().GoToUrl(this._SourcePlatform.url);
                            }
                            choosemenu(key1, key2, key3, logwin, driver, ref awaitms);
                            checkloading(key1, key2, key3, logwin, driver, ref count, ref awaitms);
                        }
                        if (!Main._IsMaintenance)
                        {
                            if (!flag_loading)
                            {
                                //先切回第1页
                                driver.FindElement(By.CssSelector("#container .pagination .dropdown")).Click();
                                WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#container .pagination .dropdown>ul"), _WebDriver_MS);
                                driver.FindElement(By.CssSelector("#container .pagination .dropdown>ul>li:nth-child(1)")).Click();
                            }
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("\r\n" + key1 + key2 + key3 + "开始第" + count + "次请求：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n");
                                logwin.txt_log.AppendText("===========================================\r\n");
                                if (this._SaveRunningLog)
                                {
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "\r\n" + key1 + key2 + key3 + "开始第" + count + "次请求：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n===========================================\r\n");
                                }

                            }));
                            watch.Restart();
                            //通过Selenium驱动点击页面的刷新按钮
                            driver.FindElement(By.CssSelector("#container .btn-toolbar .icon-refresh")).Click();
                            await Task.Delay(200);
                            checkloading(key1, key2, key3, logwin, driver, ref count, ref awaitms);
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("loading结束\r\n");
                                if (this._SaveRunningLog)
                                {
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "loading结束\r\n");
                                }
                            }));
                            //判断是否有数据
                            bool flag1 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#container .message-box"), _WebDriver_MS);

                            bool flag2 = WebDriverHelper.WaitForElementInvisible(driver, By.CssSelector("#container .pagination"), _WebDriver_MS);

                            if (flag1 && flag2)
                            {
                                int needupdatecount = 0, updatedcount = 0, deletedcount = 0;
                                string msgpush = string.Empty, delmsgpush = string.Empty;
                                Transformation(key1, key2, key3, new List<LeagueMatch>(), ref needupdatecount, ref updatedcount, ref deletedcount, ref msgpush, ref delmsgpush);
                                if (key2 == 2)
                                {
                                    Transformation(key1, 8, key3, new List<LeagueMatch>(), ref needupdatecount, ref updatedcount, ref deletedcount, ref msgpush, ref delmsgpush);
                                }
                                this.Invoke(new MethodInvoker(delegate
                                {
                                    //无数据
                                    if (!string.IsNullOrEmpty(delmsgpush))
                                    {
                                        logwin.txt_log.AppendText("删除错误：\r\n" + delmsgpush);
                                        if (this._SaveRunningLog)
                                        {
                                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "删除错误：\r\n" + delmsgpush);
                                        }
                                    }
                                    logwin.txt_log.AppendText("删除成功：" + deletedcount + "\r\n");
                                    if (this._SaveRunningLog)
                                    {
                                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "删除成功：" + deletedcount + "\r\n");
                                    }
                                    logwin.txt_log.AppendText("无赛事\r\n");
                                    logwin.txt_log.AppendText("===========================================\r\n");
                                    if (this._SaveRunningLog)
                                    {
                                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "无赛事\r\n===========================================\r\n");
                                    }
                                }));
                                continue;
                            }
                            //第一页数据
                            html = driver.FindElement(By.CssSelector("#container .match-container")).GetAttribute("innerHTML");
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("第1页数据完\r\n");
                                if (this._SaveRunningLog)
                                {
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "第1页数据完\r\n");
                                }
                            }));
                            string pagecountstr = driver.FindElement(By.CssSelector("#container .page")).Text;
                            pagecountstr = pagecountstr.Split('/')[1];
                            pagecountstr = pagecountstr.Substring(0, pagecountstr.Length - 1);
                            int.TryParse(pagecountstr, out pagecount);
                            for (int i = 2; i <= pagecount; i++)
                            {
                                //翻页
                                driver.FindElement(By.CssSelector("#container .pagination .dropdown")).Click();
                                bool flag11 = false;
                                int dropcount = 0;
                                while (!flag11)
                                {
                                    dropcount++;
                                    if (dropcount > 20)
                                    {
                                        dropcount = 0;
                                        driver.FindElement(By.CssSelector("#container .pagination .dropdown")).Click();
                                    }
                                    flag11 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#container .pagination .dropdown>ul"), _WebDriver_MS);
                                }
                                //WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#container .pagination .dropdown>ul"), _WebDriver_MS);
                                driver.FindElement(By.CssSelector("#container .pagination .dropdown>ul>li:nth-child(" + i + ")")).Click();
                                html += "$************************$" + driver.FindElement(By.CssSelector("#container .match-container")).GetAttribute("innerHTML");
                                this.Invoke(new MethodInvoker(delegate
                                {
                                    logwin.txt_log.AppendText("第" + i + "页数据完\r\n");
                                    if (this._SaveRunningLog)
                                    {
                                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "第" + i + "页数据完\r\n");
                                    }
                                }));
                            }
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("数据抓取完毕\r\n");
                                if (this._SaveRunningLog)
                                {
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "数据抓取完毕\r\n");
                                }
                            }));
                            if (key2 == 2)//拆开篮球和美式足球
                            {
                                html = html.Replace("in-play", "");
                                string htmlb = string.Empty, htmla = string.Empty;
                                string splitkey = "<div class=\"match-odds-title match-basketball \">";//美式足球
                                if (html.Contains(splitkey))
                                {
                                    string[] htmlarr = html.Replace(splitkey, "mszq************mszq" + splitkey).Split(new string[] { "mszq************mszq" }, StringSplitOptions.None);
                                    for (int i = 0; i < htmlarr.Length; i++)
                                    {
                                        if (htmlarr[i].Contains(splitkey))
                                        {
                                            htmla += htmlarr[i];
                                        }
                                        else
                                        {
                                            htmlb += htmlarr[i];
                                        }
                                    }
                                    if (!string.IsNullOrEmpty(htmlb.Trim()))
                                    {
                                        html = htmlb;
                                    }
                                    //解析数据美式足球
                                    #region 解析数据
                                    var almList = AnalysisHtml(htmla, key1, 8, key3);//美式足球
                                    if (almList != null && almList.Count > 0)
                                    {
                                        if (!System.Text.RegularExpressions.Regex.IsMatch(almList[0].Name, @"[\u4e00-\u9fa5]"))//不包含中文
                                        {
                                            this.Invoke(new MethodInvoker(delegate
                                            {
                                                logwin.txt_log.AppendText("-------------------------------------------\r\n");
                                                logwin.txt_log.AppendText("美式足球\r\n");
                                                logwin.txt_log.AppendText("数据异常\r\n");
                                                logwin.txt_log.AppendText("-------------------------------------------\r\n");
                                                if (this._SaveRunningLog)
                                                {
                                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "-------------------------------------------\r\n美式足球\r\n数据异常\r\n-------------------------------------------\r\n");
                                                }

                                            }));
                                        }
                                        else
                                        {
                                            if (_IsRunning)
                                            {
                                                int needupdatecount = 0, updatedcount = 0, deletedcount = 0;
                                                string msgpush = string.Empty, delmsgpush = string.Empty;
                                                Transformation(key1, 8, key3, almList, ref needupdatecount, ref updatedcount, ref deletedcount, ref msgpush, ref delmsgpush);
                                                this.Invoke(new MethodInvoker(delegate
                                                {
                                                    logwin.txt_log.AppendText("-------------------------------------------\r\n");
                                                    logwin.txt_log.AppendText("美式足球\r\n");
                                                    if (this._SaveRunningLog)
                                                    {
                                                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "-------------------------------------------\r\n美式足球\r\n");
                                                    }
                                                    if (!string.IsNullOrEmpty(msgpush))
                                                    {
                                                        logwin.txt_log.AppendText("错误：\r\n" + msgpush);
                                                        if (this._SaveRunningLog)
                                                        {
                                                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "错误：\r\n" + msgpush);
                                                        }
                                                    }
                                                    if (!string.IsNullOrEmpty(delmsgpush))
                                                    {
                                                        logwin.txt_log.AppendText("删除错误：\r\n" + delmsgpush);
                                                        if (this._SaveRunningLog)
                                                        {
                                                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "删除错误：\r\n" + delmsgpush);
                                                        }
                                                    }
                                                    logwin.txt_log.AppendText("赔率需更新：" + needupdatecount + "\r\n更新成功：" + updatedcount + "\r\n删除成功：" + deletedcount + "\r\n");
                                                    logwin.txt_log.AppendText("-------------------------------------------\r\n");
                                                    if (this._SaveRunningLog)
                                                    {
                                                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "赔率需更新：" + needupdatecount + "\r\n更新成功：" + updatedcount + "\r\n删除成功：" + deletedcount + "\r\n-------------------------------------------\r\n");
                                                    }
                                                }));
                                                //if (needupdatecount != updatedcount)
                                                //{
                                                //    O_OddsBll obll = new O_OddsBll();
                                                //    DateTime now = obll.GetServerDateTime();
                                                //    string sportstype = this.Dict_SportsType[key2];
                                                //    var olist = obll.FindByDate(SourcePlatformEnum.SB.ToString(), sportstype, now.Date.AddDays(-1));
                                                //    lock (locklist)
                                                //    {
                                                //        foreach (string code in this.Dict_S_BetCode[sportstype].Values)
                                                //        {
                                                //            Main.Temp_O_Odds_Dict[sportstype][code] = olist.Where(x => x.BetCode == code).ToList();
                                                //        }
                                                //    }
                                                //    this.Invoke(new MethodInvoker(delegate
                                                //    {
                                                //        logwin.txt_log.AppendText("更新" + sportstype + "缓存\r\n");
                                                //        if (this._SaveRunningLog)
                                                //        {
                                                //            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "更新" + sportstype + "缓存\r\n");
                                                //        }
                                                //    }));
                                                //}
                                            }
                                        }
                                    }
                                    #endregion
                                }
                                else//没有美式足球
                                {
                                    int needupdatecount = 0, updatedcount = 0, deletedcount = 0;
                                    string msgpush = string.Empty, delmsgpush = string.Empty;
                                    Transformation(key1, 8, key3, new List<LeagueMatch>(), ref needupdatecount, ref updatedcount, ref deletedcount, ref msgpush, ref delmsgpush);
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("-------------------------------------------\r\n");
                                        logwin.txt_log.AppendText("美式足球\r\n");
                                        if (this._SaveRunningLog)
                                        {
                                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "-------------------------------------------\r\n美式足球\r\n");
                                        }
                                        //无数据
                                        if (!string.IsNullOrEmpty(delmsgpush))
                                        {
                                            logwin.txt_log.AppendText("删除错误：\r\n" + delmsgpush);
                                            if (this._SaveRunningLog)
                                            {
                                                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "删除错误：\r\n" + delmsgpush);
                                            }
                                        }
                                        logwin.txt_log.AppendText("删除成功：" + deletedcount + "\r\n");
                                        if (this._SaveRunningLog)
                                        {
                                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "删除成功：" + deletedcount + "\r\n");
                                        }
                                        logwin.txt_log.AppendText("美式足球无赛事\r\n");
                                        logwin.txt_log.AppendText("-------------------------------------------\r\n");
                                        if (this._SaveRunningLog)
                                        {
                                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "美式足球无赛事\r\n-------------------------------------------\r\n");
                                        }
                                    }));
                                }
                            }
                            //解析数据
                            var lmList = AnalysisHtml(html, key1, key2, key3);
                            if (lmList != null && lmList.Count > 0)
                            {
                                if (!System.Text.RegularExpressions.Regex.IsMatch(lmList[0].Name, @"[\u4e00-\u9fa5]") && !System.Text.RegularExpressions.Regex.IsMatch(lmList[0].MatchList[0].HomeTeam, @"[\u4e00-\u9fa5]") && !System.Text.RegularExpressions.Regex.IsMatch(lmList[0].MatchList[0].VisitingTeam, @"[\u4e00-\u9fa5]"))//不包含中文
                                {
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("数据异常，可能获取到英文版的数据，重刷页面。\r\n");
                                        if (this._SaveRunningLog)
                                        {
                                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "数据异常，可能获取到英文版的数据，重刷页面。\r\n");
                                        }
                                    }));
                                    flag_loading = true;
                                    driver.SwitchTo().DefaultContent();
                                    if (islogin)
                                    {
                                        driver.Navigate().GoToUrl(this._SourcePlatform.url);
                                    }
                                    else
                                    {
                                        driver.Navigate().GoToUrl(this._SourcePlatform.nologinurl);
                                    }
                                    choosemenu(key1, key2, key3, logwin, driver, ref awaitms);
                                    checkloading(key1, key2, key3, logwin, driver, ref count, ref awaitms);
                                    continue;
                                }
                                else
                                {
                                    if (_IsRunning)
                                    {
                                        int needupdatecount = 0, updatedcount = 0, deletedcount = 0;
                                        string msgpush = string.Empty, delmsgpush = string.Empty;
                                        Transformation(key1, key2, key3, lmList, ref needupdatecount, ref updatedcount, ref deletedcount, ref msgpush, ref delmsgpush);
                                        this.Invoke(new MethodInvoker(delegate
                                        {
                                            if (!string.IsNullOrEmpty(msgpush))
                                            {
                                                logwin.txt_log.AppendText("错误：\r\n" + msgpush);
                                                if (this._SaveRunningLog)
                                                {
                                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "错误：\r\n" + msgpush);
                                                }
                                            }
                                            if (!string.IsNullOrEmpty(delmsgpush))
                                            {
                                                logwin.txt_log.AppendText("删除错误：\r\n" + delmsgpush);
                                                if (this._SaveRunningLog)
                                                {
                                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "删除错误：\r\n" + delmsgpush);
                                                }
                                            }
                                            logwin.txt_log.AppendText("赔率需更新：" + needupdatecount + "\r\n更新成功：" + updatedcount + "\r\n删除成功：" + deletedcount + "\r\n");
                                            if (this._SaveRunningLog)
                                            {
                                                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "赔率需更新：" + needupdatecount + "\r\n更新成功：" + updatedcount + "\r\n删除成功：" + deletedcount + "\r\n");
                                            }

                                        }));
                                        //if (needupdatecount != updatedcount)
                                        //{
                                        //    O_OddsBll obll = new O_OddsBll();
                                        //    DateTime now = obll.GetServerDateTime();
                                        //    string sportstype = this.Dict_SportsType[key2];
                                        //    var olist = obll.FindByDate(SourcePlatformEnum.SB.ToString(), sportstype, now.Date.AddDays(-1));
                                        //    lock (locklist)
                                        //    {
                                        //        foreach (string code in this.Dict_S_BetCode[sportstype].Values)
                                        //        {
                                        //            Main.Temp_O_Odds_Dict[sportstype][code] = olist.Where(x => x.BetCode == code).ToList();
                                        //        }
                                        //    }
                                        //    this.Invoke(new MethodInvoker(delegate
                                        //    {
                                        //        logwin.txt_log.AppendText("更新" + sportstype + "缓存\r\n");
                                        //        if (this._SaveRunningLog)
                                        //        {
                                        //            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "更新" + sportstype + "缓存\r\n");
                                        //        }                                                
                                        //    }));
                                        //}
                                    }
                                }
                            }
                            watch.Stop();
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("===========================================\r\n");
                                logwin.txt_log.AppendText("第" + count + "次耗时：" + watch.Elapsed.TotalSeconds + "秒\r\n");
                                if (this._SaveRunningLog)
                                {
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "===========================================\r\n第" + count + "次耗时：" + watch.Elapsed.TotalSeconds + "秒\r\n");
                                }
                            }));
                        }
                        else
                        {
                            awaitms = 1000 * 60;
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("===========================================\r\n");
                                logwin.txt_log.AppendText("沙巴正在维护\r\n");
                                if (this._SaveRunningLog)
                                {
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "===========================================\r\n沙巴正在维护\r\n");
                                }
                            }));
                            GetMaintenanceInfo(logwin);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex is AggregateException)
                        {
                            AggregateException e = ex as AggregateException;
                            foreach (var item in e.InnerExceptions)
                            {
                                LogHelper.WriteLog($"异常类型：{item.GetType()}<br/>来自：{item.Source}<br/>异常内容：{item.Message}");
                            }
                        }
                        else if (ex is NoSuchElementException)
                        {
                            isexception = true;
                        }
                        else if (ex is NoSuchFrameException)
                        {
                            isexception = true;
                        }
                        else if (ex is WebDriverException)
                        {
                            if (ex.Message.Contains("Cannot start the driver service") || ex.Message.Contains("The HTTP request to the remote WebDriver server for URL"))
                            {
                                driver.Quit();
                                driver.Dispose();
                                driver = null;
                                driver = WebDriverHelper.CreateWebDriver(this._browser, this.chk_VisibleBrowser.Checked, false, this._ProxyIP);
                                if (driver == null)
                                {
                                    LogHelper.WriteLog(WebDriverHelper.ErrorMessage);
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText(WebDriverHelper.ErrorMessage + "\r\n");
                                        if (this._SaveRunningLog)
                                        {
                                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", WebDriverHelper.ErrorMessage + "\r\n");
                                        }
                                    }));
                                }
                                else
                                {
                                    _WebDriverList.Add(driver);
                                    if (this.chk_Logined.Checked)
                                    {
                                        driver.Navigate().GoToUrl(this._SourcePlatform.url);
                                        driver.Manage().Cookies.DeleteAllCookies();
                                        foreach (var item in _CookieList)
                                        {
                                            driver.Manage().Cookies.AddCookie(item);
                                        }
                                        driver.Navigate().Refresh();
                                    }
                                    else
                                    {
                                        driver.Navigate().GoToUrl(this._SourcePlatform.nologinurl);
                                    }
                                }
                            }
                            else
                            {
                                isexception = true;
                            }
                        }
                        LogHelper.ErrorLog(ex.Message, ex);
                        this.Invoke(new MethodInvoker(delegate
                        {
                            logwin.txt_log.AppendText("===========================================\r\n");
                            logwin.txt_log.AppendText("第" + count + "次异常：" + ex.Message + ex.StackTrace + "\r\n");
                            if (this._SaveRunningLog)
                            {
                                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "===========================================\r\n第" + count + "次异常：" + ex.Message + ex.StackTrace + "\r\n");
                            }
                        }));
                        continue;
                    }
                }
            });
        }
        private void choosemenu(int key1, int key2, int key3, Log logwin, IWebDriver driver, ref int awaitms)
        {
            if (this._SourcePlatform.key == "TYC")
            {
                bool iswh = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#sportsFrame"), _WebDriver_MS);
                if (!iswh && !Main._IsMaintenance)
                {
                    this.Invoke(new MethodInvoker(delegate
                    {
                        Main._IsMaintenance = true;
                        logwin.txt_log.AppendText("异常，沙巴可能在维护。\r\n");
                        if (this._SaveRunningLog)
                        {
                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "异常，沙巴可能在维护。\r\n");
                        }
                    }));
                    GetMaintenanceInfo(logwin);
                    return;
                }
                if (Main._IsMaintenance)
                {
                    return;
                }
                driver.SwitchTo().Frame("sportsFrame");
                iswh = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#sbContainer"), _WebDriver_MS);
                if (!iswh)
                {
                    this.Invoke(new MethodInvoker(delegate
                    {
                        Main._IsMaintenance = true;
                        logwin.txt_log.AppendText("异常，沙巴可能在维护。\r\n");
                        if (this._SaveRunningLog)
                        {
                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "异常，沙巴可能在维护。\r\n");
                        }
                    }));
                    GetMaintenanceInfo(logwin);
                    return;
                }
                Main._IsMaintenance = false;
                //driver.SwitchTo().Frame("sportsFrame");
            }
            if (this._SourcePlatform.key == "TYC")
            {
                driver.SwitchTo().DefaultContent();
                driver.SwitchTo().Frame("sportsFrame");
            }
            switch (key1)
            {
                case 1:
                    awaitms = _ZP_MS;
                    //进入早盘
                    #region 进入早盘
                    this.Invoke(new MethodInvoker(delegate
                    {
                        logwin.txt_log.AppendText("进入早盘\r\n");
                        if (this._SaveRunningLog)
                        {
                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "进入早盘\r\n");
                        }
                    }));
                    driver.FindElement(By.CssSelector("#sb-header>.header-tab>ul>li:nth-child(3)")).Click();
                    bool flag01 = false;
                    do
                    {
                        flag01 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#category .category-sports"), _WebDriver_MS);
                    } while (!flag01);
                    Thread.Sleep(2000);
                    if (_ZP_AllDay)//选择所有日期
                    {
                        this.Invoke(new MethodInvoker(delegate
                        {
                            logwin.txt_log.AppendText("选择所有日期\r\n");
                            if (this._SaveRunningLog)
                            {
                                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择所有日期\r\n");
                            }
                        }));
                        driver.FindElement(By.CssSelector("#container .filter-date>ul>li:last-child>a")).Click();
                    }
                    switch (key2)
                    {
                        case 1://选择足球                            
                            #region 选择足球
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("选择足球\r\n");
                                if (this._SaveRunningLog)
                                {
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择足球\r\n");
                                }
                            }));
                            driver.FindElement(By.CssSelector("#category .category-sports>ul>li:nth-child(1)")).Click();
                            bool flag102 = false;
                            do
                            {
                                flag102 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#category .category-bettype"), _WebDriver_MS);
                            } while (!flag102);
                            Thread.Sleep(2000);
                            switch (key3)
                            {
                                case 1:
                                    //独赢 & 让球 & 大小 & 单/双
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择独赢 & 让球 & 大小 & 单/双\r\n");
                                        if (this._SaveRunningLog)
                                        {
                                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择独赢 & 让球 & 大小 & 单/双\r\n");
                                        }
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(1)")).Click();
                                    break;
                                case 2:
                                    //半场 / 全场
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择半场 / 全场\r\n");
                                        if (this._SaveRunningLog)
                                        {
                                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择半场 / 全场\r\n");
                                        }
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(4)")).Click();
                                    break;
                                case 3:
                                    //波胆
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择波胆\r\n");
                                        if (this._SaveRunningLog)
                                        {
                                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择波胆\r\n");
                                        }
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(2)")).Click();
                                    break;
                                case 4:
                                    //总入球
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择总入球\r\n");
                                        if (this._SaveRunningLog)
                                        {
                                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择总入球\r\n");
                                        }
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(3)")).Click();
                                    break;
                                case 5:
                                    //冠军
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择冠军\r\n");
                                        if (this._SaveRunningLog)
                                        {
                                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择冠军\r\n");
                                        }
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(6)")).Click();
                                    //选择联赛
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择联赛\r\n");
                                        if (this._SaveRunningLog)
                                        {
                                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择联赛\r\n");
                                        }
                                    }));
                                    bool flag103 = false;
                                    do
                                    {
                                        flag103 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#container .match-toolbar .button-wrap>li .highlight"), _WebDriver_MS);
                                    } while (!flag103);
                                    driver.FindElement(By.CssSelector("#container .match-toolbar .button-wrap>li .highlight")).Click();
                                    bool flag104 = false;
                                    do
                                    {
                                        flag104 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector(".modal-dialog"), _WebDriver_MS);
                                    } while (!flag104);
                                    driver.FindElement(By.CssSelector(".modal-dialog .modal-body .league-list>div>input")).Click();
                                    var lmtd = driver.FindElements(By.CssSelector(".modal-dialog .modal-body .league-list table td"));
                                    foreach (var item in lmtd)
                                    {
                                        if (item.Text.Contains("- 冠军"))
                                        {
                                            item.FindElement(By.CssSelector("input")).Click();
                                        }
                                    }
                                    driver.FindElement(By.CssSelector(".modal-dialog .modal-footer .accent")).Click();
                                    break;
                            }
                            #endregion
                            break;
                        case 2://篮球、美式足球
                            #region 篮球、美式足球
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("选择篮球、美式足球\r\n");
                                if (this._SaveRunningLog)
                                {
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择篮球、美式足球\r\n");
                                }
                            }));
                            driver.FindElement(By.CssSelector("#category .category-sports>ul>li:nth-child(2)")).Click();
                            bool flag202 = false;
                            do
                            {
                                flag202 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#category .category-bettype"), _WebDriver_MS);
                            } while (!flag202);
                            Thread.Sleep(2000);
                            switch (key3)
                            {
                                case 1:
                                    //胜负盘 & 让球 & 大小 & 单/双
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择胜负盘 & 让球 & 大小 & 单/双\r\n");
                                        if (this._SaveRunningLog)
                                        {
                                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择胜负盘 & 让球 & 大小 & 单/双\r\n");
                                        }
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(1)")).Click();
                                    break;
                            }
                            #endregion
                            break;
                        case 3://网球
                            #region 网球
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("选择网球\r\n");
                                if (this._SaveRunningLog)
                                {
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择网球\r\n");
                                }
                            }));
                            driver.FindElement(By.CssSelector("#category .category-sports>ul>li:nth-child(3)")).Click();
                            bool flag302 = false;
                            do
                            {
                                flag302 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#category .category-bettype"), _WebDriver_MS);
                            } while (!flag302);
                            Thread.Sleep(2000);
                            switch (key3)
                            {
                                case 1:
                                    //胜负盘 & 让盘 & 大小 & 单/双
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择胜负盘 & 让盘 & 大小 & 单/双\r\n");
                                        if (this._SaveRunningLog)
                                        {
                                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择胜负盘 & 让盘 & 大小 & 单/双\r\n");
                                        }
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(1)")).Click();
                                    break;
                            }
                            #endregion
                            break;
                        case 4://排球
                            #region 排球
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("选择排球\r\n");
                                if (this._SaveRunningLog)
                                {
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择排球\r\n");
                                }
                            }));
                            driver.FindElement(By.CssSelector("#category .category-sports>ul>li:nth-child(4)")).Click();
                            bool flag402 = false;
                            do
                            {
                                flag402 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#category .category-bettype"), _WebDriver_MS);
                            } while (!flag402);
                            Thread.Sleep(2000);
                            switch (key3)
                            {
                                case 1:
                                    //胜负盘 & 让分 & 单/双 & 大小
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择胜负盘 & 让分 & 单/双 & 大小\r\n");
                                        if (this._SaveRunningLog)
                                        {
                                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择胜负盘 & 让分 & 单/双 & 大小\r\n");
                                        }
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(1)")).Click();
                                    break;
                            }
                            #endregion
                            break;
                        case 5://棒球
                            #region 棒球
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("选择棒球\r\n");
                                if (this._SaveRunningLog)
                                {
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择棒球\r\n");
                                }
                            }));
                            driver.FindElement(By.CssSelector("#category .category-sports>ul>li:nth-child(5)")).Click();
                            bool flag502 = false;
                            do
                            {
                                flag502 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#category .category-bettype"), _WebDriver_MS);
                            } while (!flag502);
                            Thread.Sleep(2000);
                            switch (key3)
                            {
                                case 1:
                                    //胜负盘 & 让分 & 大小
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择胜负盘 & 让分 & 大小\r\n");
                                        if (this._SaveRunningLog)
                                        {
                                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择胜负盘 & 让分 & 大小\r\n");
                                        }
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(1)")).Click();
                                    break;
                            }
                            #endregion
                            break;
                        case 6://羽毛球
                            #region 羽毛球
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("选择羽毛球\r\n");
                                if (this._SaveRunningLog)
                                {
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择羽毛球\r\n");
                                }
                            }));
                            driver.FindElement(By.CssSelector("#category .category-sports>ul>li:nth-child(6)")).Click();
                            bool flag602 = false;
                            do
                            {
                                flag602 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#category .category-bettype"), _WebDriver_MS);
                            } while (!flag602);
                            Thread.Sleep(2000);
                            switch (key3)
                            {
                                case 1:
                                    //胜负盘 & 让局 & 单/双 & 大小
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择胜负盘 & 让局 & 单/双 & 大小\r\n");
                                        if (this._SaveRunningLog)
                                        {
                                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择胜负盘 & 让局 & 单/双 & 大小\r\n");
                                        }
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(1)")).Click();
                                    break;
                            }
                            #endregion
                            break;
                        case 7://乒乓球
                            #region 乒乓球
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("选择乒乓球\r\n");
                                if (this._SaveRunningLog)
                                {
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择乒乓球\r\n");
                                }
                            }));
                            driver.FindElement(By.CssSelector("#category .category-sports>ul>li:nth-child(7)")).Click();
                            bool flag702 = false;
                            do
                            {
                                flag702 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#category .category-bettype"), _WebDriver_MS);
                            } while (!flag702);
                            Thread.Sleep(2000);
                            switch (key3)
                            {
                                case 1:
                                    //胜负盘 & 让局 & 单/双 & 大小
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择胜负盘 & 让局 & 单/双 & 大小\r\n");
                                        if (this._SaveRunningLog)
                                        {
                                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择胜负盘 & 让局 & 单/双 & 大小\r\n");
                                        }
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(1)")).Click();
                                    break;
                            }
                            #endregion
                            break;
                    }
                    #endregion
                    break;
                case 2:
                    awaitms = _JR_MS;
                    //进入今日赛事
                    #region 进入今日赛事
                    this.Invoke(new MethodInvoker(delegate
                    {
                        logwin.txt_log.AppendText("进入今日赛事\r\n");
                        if (this._SaveRunningLog)
                        {
                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "进入今日赛事\r\n");
                        }
                    }));
                    driver.FindElement(By.CssSelector("#sb-header>.header-tab>ul>li:nth-child(2)")).Click();
                    bool flag05 = false;
                    do
                    {
                        flag05 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#category .category-sports"), _WebDriver_MS);
                    } while (!flag05);
                    Thread.Sleep(2000);
                    switch (key2)
                    {
                        case 1://选择足球                            
                            #region 选择足球
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("选择足球\r\n");
                                if (this._SaveRunningLog)
                                {
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择足球\r\n");
                                }
                            }));
                            driver.FindElement(By.CssSelector("#category .category-sports>ul>li:nth-child(1)")).Click();
                            bool flag06 = false;
                            do
                            {
                                flag06 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#category .category-bettype"), _WebDriver_MS);
                            } while (!flag06);
                            Thread.Sleep(2000);
                            switch (key3)
                            {
                                case 1:
                                    //独赢 & 让球 & 大小 & 单/双
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择独赢 & 让球 & 大小 & 单/双\r\n");
                                        if (this._SaveRunningLog)
                                        {
                                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择独赢 & 让球 & 大小 & 单/双\r\n");
                                        }
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(1)")).Click();
                                    break;
                                case 2:
                                    //半场 / 全场
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择半场 / 全场\r\n");
                                        if (this._SaveRunningLog)
                                        {
                                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择半场 / 全场\r\n");
                                        }
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(4)")).Click();
                                    break;
                                case 3:
                                    //波胆
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择波胆\r\n");
                                        if (this._SaveRunningLog)
                                        {
                                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择波胆\r\n");
                                        }
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(2)")).Click();
                                    break;
                                case 4:
                                    //总入球
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择总入球\r\n");
                                        if (this._SaveRunningLog)
                                        {
                                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择总入球\r\n");
                                        }
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(3)")).Click();
                                    break;
                            }
                            #endregion
                            break;
                        case 2://篮球、美式足球
                            #region 篮球、美式足球
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("选择篮球、美式足球\r\n");
                                if (this._SaveRunningLog)
                                {
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择篮球、美式足球\r\n");
                                }
                            }));
                            driver.FindElement(By.CssSelector("#category .category-sports>ul>li:nth-child(2)")).Click();
                            bool flag202 = false;
                            do
                            {
                                flag202 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#category .category-bettype"), _WebDriver_MS);
                            } while (!flag202);
                            Thread.Sleep(2000);
                            switch (key3)
                            {
                                case 1:
                                    //胜负盘 & 让球 & 大小 & 单/双
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择胜负盘 & 让球 & 大小 & 单/双\r\n");
                                        if (this._SaveRunningLog)
                                        {
                                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择胜负盘 & 让球 & 大小 & 单/双\r\n");
                                        }
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(1)")).Click();
                                    break;
                            }
                            #endregion
                            break;
                        case 3://网球
                            #region 网球
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("选择网球\r\n");
                                if (this._SaveRunningLog)
                                {
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择网球\r\n");
                                }
                            }));
                            driver.FindElement(By.CssSelector("#category .category-sports>ul>li:nth-child(3)")).Click();
                            bool flag302 = false;
                            do
                            {
                                flag302 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#category .category-bettype"), _WebDriver_MS);
                            } while (!flag302);
                            Thread.Sleep(2000);
                            switch (key3)
                            {
                                case 1:
                                    //胜负盘 & 让盘 & 大小 & 单/双
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择胜负盘 & 让盘 & 大小 & 单/双\r\n");
                                        if (this._SaveRunningLog)
                                        {
                                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择胜负盘 & 让盘 & 大小 & 单/双\r\n");
                                        }
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(1)")).Click();
                                    break;
                            }
                            #endregion
                            break;
                        case 4://排球
                            #region 排球
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("选择排球\r\n");
                                if (this._SaveRunningLog)
                                {
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择排球\r\n");
                                }
                            }));
                            driver.FindElement(By.CssSelector("#category .category-sports>ul>li:nth-child(4)")).Click();
                            bool flag402 = false;
                            do
                            {
                                flag402 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#category .category-bettype"), _WebDriver_MS);
                            } while (!flag402);
                            Thread.Sleep(2000);
                            switch (key3)
                            {
                                case 1:
                                    //胜负盘 & 让分 & 单/双 & 大小
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择胜负盘 & 让分 & 单/双 & 大小\r\n");
                                        if (this._SaveRunningLog)
                                        {
                                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择胜负盘 & 让分 & 单/双 & 大小\r\n");
                                        }
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(1)")).Click();
                                    break;
                            }
                            #endregion
                            break;
                        case 5://棒球
                            #region 棒球
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("选择棒球\r\n");
                                if (this._SaveRunningLog)
                                {
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择棒球\r\n");
                                }
                            }));
                            driver.FindElement(By.CssSelector("#category .category-sports>ul>li:nth-child(5)")).Click();
                            bool flag502 = false;
                            do
                            {
                                flag502 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#category .category-bettype"), _WebDriver_MS);
                            } while (!flag502);
                            Thread.Sleep(2000);
                            switch (key3)
                            {
                                case 1:
                                    //胜负盘 & 让分 & 大小
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择胜负盘 & 让分 & 大小\r\n");
                                        if (this._SaveRunningLog)
                                        {
                                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择胜负盘 & 让分 & 大小\r\n");
                                        }
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(1)")).Click();
                                    break;
                            }
                            #endregion
                            break;
                        case 6://羽毛球
                            #region 羽毛球
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("选择羽毛球\r\n");
                                if (this._SaveRunningLog)
                                {
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择羽毛球\r\n");
                                }
                            }));
                            driver.FindElement(By.CssSelector("#category .category-sports>ul>li:nth-child(6)")).Click();
                            bool flag602 = false;
                            do
                            {
                                flag602 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#category .category-bettype"), _WebDriver_MS);
                            } while (!flag602);
                            Thread.Sleep(2000);
                            switch (key3)
                            {
                                case 1:
                                    //胜负盘 & 让局 & 单/双 & 大小
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择胜负盘 & 让局 & 单/双 & 大小\r\n");
                                        if (this._SaveRunningLog)
                                        {
                                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择胜负盘 & 让局 & 单/双 & 大小\r\n");
                                        }
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(1)")).Click();
                                    break;
                            }
                            #endregion
                            break;
                        case 7://乒乓球
                            #region 乒乓球
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("选择乒乓球\r\n");
                                if (this._SaveRunningLog)
                                {
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择乒乓球\r\n");
                                }
                            }));
                            driver.FindElement(By.CssSelector("#category .category-sports>ul>li:nth-child(7)")).Click();
                            bool flag702 = false;
                            do
                            {
                                flag702 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#category .category-bettype"), _WebDriver_MS);
                            } while (!flag702);
                            Thread.Sleep(2000);
                            switch (key3)
                            {
                                case 1:
                                    //胜负盘 & 让局 & 单/双 & 大小
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择胜负盘 & 让局 & 单/双 & 大小\r\n");
                                        if (this._SaveRunningLog)
                                        {
                                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择胜负盘 & 让局 & 单/双 & 大小\r\n");
                                        }
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(1)")).Click();
                                    break;
                            }
                            #endregion
                            break;
                    }
                    #endregion
                    break;
                case 3:
                    awaitms = _GQ_MS;
                    //进入滚球
                    #region 进入滚球
                    this.Invoke(new MethodInvoker(delegate
                    {
                        logwin.txt_log.AppendText("进入滚球\r\n");
                        if (this._SaveRunningLog)
                        {
                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "进入滚球\r\n");
                        }
                    }));
                    driver.FindElement(By.CssSelector("#sb-header>.header-tab>ul>li:nth-child(1)")).Click();
                    bool flag07 = false;
                    do
                    {
                        flag07 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#category .category-sports"), _WebDriver_MS);
                    } while (!flag07);
                    Thread.Sleep(2000);
                    switch (key2)
                    {
                        case 1://选择足球                            
                            #region 选择足球
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("选择足球\r\n");
                                if (this._SaveRunningLog)
                                {
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择足球\r\n");
                                }
                            }));
                            driver.FindElement(By.CssSelector("#category .category-sports>ul>li:nth-child(1)")).Click();
                            bool flag08 = false;
                            do
                            {
                                flag08 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#category .category-bettype"), _WebDriver_MS);
                            } while (!flag08);
                            Thread.Sleep(2000);
                            switch (key3)
                            {
                                case 1:
                                    //独赢 & 让球 & 大小 & 单/双
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择独赢 & 让球 & 大小 & 单/双\r\n");
                                        if (this._SaveRunningLog)
                                        {
                                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择独赢 & 让球 & 大小 & 单/双\r\n");
                                        }
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(1)")).Click();
                                    break;
                                case 3:
                                    //波胆
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择波胆\r\n");
                                        if (this._SaveRunningLog)
                                        {
                                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择波胆\r\n");
                                        }
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(2)")).Click();
                                    break;
                                case 4:
                                    //总入球
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择总入球\r\n");
                                        if (this._SaveRunningLog)
                                        {
                                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择总入球\r\n");
                                        }
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(3)")).Click();
                                    break;
                            }
                            #endregion
                            break;
                        case 2://篮球、美式足球
                            #region 篮球、美式足球
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("选择篮球、美式足球\r\n");
                                if (this._SaveRunningLog)
                                {
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择篮球、美式足球\r\n");
                                }
                            }));
                            driver.FindElement(By.CssSelector("#category .category-sports>ul>li:nth-child(2)")).Click();
                            bool flag202 = false;
                            do
                            {
                                flag202 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#category .category-bettype"), _WebDriver_MS);
                            } while (!flag202);
                            Thread.Sleep(2000);
                            switch (key3)
                            {
                                case 1:
                                    //胜负盘 & 让球 & 大小 & 单/双
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择胜负盘 & 让球 & 大小 & 单/双\r\n");
                                        if (this._SaveRunningLog)
                                        {
                                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择胜负盘 & 让球 & 大小 & 单/双\r\n");
                                        }
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(1)")).Click();
                                    break;
                            }
                            #endregion
                            break;
                        case 3://网球
                            #region 网球
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("选择网球\r\n");
                                if (this._SaveRunningLog)
                                {
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择网球\r\n");
                                }
                            }));
                            driver.FindElement(By.CssSelector("#category .category-sports>ul>li:nth-child(3)")).Click();
                            bool flag302 = false;
                            do
                            {
                                flag302 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#category .category-bettype"), _WebDriver_MS);
                            } while (!flag302);
                            Thread.Sleep(2000);
                            switch (key3)
                            {
                                case 1:
                                    //胜负盘 & 让盘 & 大小 & 单/双
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择胜负盘 & 让盘 & 大小 & 单/双\r\n");
                                        if (this._SaveRunningLog)
                                        {
                                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择胜负盘 & 让盘 & 大小 & 单/双\r\n");
                                        }
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(1)")).Click();
                                    break;
                            }
                            #endregion
                            break;
                        case 4://排球
                            #region 排球
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("选择排球\r\n");
                                if (this._SaveRunningLog)
                                {
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择排球\r\n");
                                }
                            }));
                            driver.FindElement(By.CssSelector("#category .category-sports>ul>li:nth-child(4)")).Click();
                            bool flag402 = false;
                            do
                            {
                                flag402 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#category .category-bettype"), _WebDriver_MS);
                            } while (!flag402);
                            Thread.Sleep(2000);
                            switch (key3)
                            {
                                case 1:
                                    //胜负盘 & 让分 & 单/双 & 大小
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择胜负盘 & 让分 & 单/双 & 大小\r\n");
                                        if (this._SaveRunningLog)
                                        {
                                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择胜负盘 & 让分 & 单/双 & 大小\r\n");
                                        }
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(1)")).Click();
                                    break;
                            }
                            #endregion
                            break;
                        case 5://棒球
                            #region 棒球
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("选择棒球\r\n");
                                if (this._SaveRunningLog)
                                {
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择棒球\r\n");
                                }
                            }));
                            driver.FindElement(By.CssSelector("#category .category-sports>ul>li:nth-child(5)")).Click();
                            bool flag502 = false;
                            do
                            {
                                flag502 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#category .category-bettype"), _WebDriver_MS);
                            } while (!flag502);
                            Thread.Sleep(2000);
                            switch (key3)
                            {
                                case 1:
                                    //胜负盘 & 让分 & 大小
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择胜负盘 & 让分 & 大小\r\n");
                                        if (this._SaveRunningLog)
                                        {
                                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择胜负盘 & 让分 & 大小\r\n");
                                        }
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(1)")).Click();
                                    break;
                            }
                            #endregion
                            break;
                        case 6://羽毛球
                            #region 羽毛球
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("选择羽毛球\r\n");
                                if (this._SaveRunningLog)
                                {
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择羽毛球\r\n");
                                }
                            }));
                            driver.FindElement(By.CssSelector("#category .category-sports>ul>li:nth-child(6)")).Click();
                            bool flag602 = false;
                            do
                            {
                                flag602 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#category .category-bettype"), _WebDriver_MS);
                            } while (!flag602);
                            Thread.Sleep(2000);
                            switch (key3)
                            {
                                case 1:
                                    //胜负盘 & 让局 & 单/双 & 大小
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择胜负盘 & 让局 & 单/双 & 大小\r\n");
                                        if (this._SaveRunningLog)
                                        {
                                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择胜负盘 & 让局 & 单/双 & 大小\r\n");
                                        }
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(1)")).Click();
                                    break;
                            }
                            #endregion
                            break;
                        case 7://乒乓球
                            #region 乒乓球
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("选择乒乓球\r\n");
                                if (this._SaveRunningLog)
                                {
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择乒乓球\r\n");
                                }
                            }));
                            driver.FindElement(By.CssSelector("#category .category-sports>ul>li:nth-child(7)")).Click();
                            bool flag702 = false;
                            do
                            {
                                flag702 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#category .category-bettype"), _WebDriver_MS);
                            } while (!flag702);
                            Thread.Sleep(2000);
                            switch (key3)
                            {
                                case 1:
                                    //胜负盘 & 让局 & 单/双 & 大小
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择胜负盘 & 让局 & 单/双 & 大小\r\n");
                                        if (this._SaveRunningLog)
                                        {
                                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择胜负盘 & 让局 & 单/双 & 大小\r\n");
                                        }
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(1)")).Click();
                                    break;
                            }
                            #endregion
                            break;
                    }
                    #endregion
                    break;
            }
        }
        private void checkloading(int key1, int key2, int key3, Log logwin, IWebDriver driver, ref int count, ref int awaitms)
        {
            if (Main._IsMaintenance)
            {
                return;
            }
            string matchtitle = driver.FindElement(By.CssSelector("#container .match-title")).GetAttribute("innerHTML");
            this.Invoke(new MethodInvoker(delegate
            {
                logwin.txt_log.AppendText("当前：" + matchtitle + "\r\n");
                if (this._SaveRunningLog)
                {
                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "当前：" + matchtitle + "\r\n");
                }
            }));
            if (!(matchtitle.Contains(menuarr[0, 0, key1 - 1].Replace("赛事", "")) && matchtitle.Contains(menuarr[1, 0, key2 - 1].Replace("、", " / "))))
            {
                choosemenu(key1, key2, key3, logwin, driver, ref awaitms);
                return;
            }
            int loadingcount = 0;
            bool flag09 = false;
            while (!flag09)
            {
                flag09 = WebDriverHelper.WaitForElementInvisible(driver, By.CssSelector("#container .loading"), _WebDriver_MS);
                this.Invoke(new MethodInvoker(delegate
                {
                    logwin.txt_log.AppendText("等待loading：" + flag09 + "\r\n");
                    if (this._SaveRunningLog)
                    {
                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "等待loading：" + flag09 + "\r\n");
                    }
                }));
                loadingcount++;
                bool islogin = false;
                if (loadingcount > 500)
                {
                    flag09 = true;
                    this.Invoke(new MethodInvoker(delegate
                    {
                        islogin = this.chk_Logined.Checked;
                        logwin.txt_log.AppendText("loading超时：" + loadingcount + "，页面重刷\r\n");
                        if (this._SaveRunningLog)
                        {
                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "loading超时：" + loadingcount + "，页面重刷\r\n");
                        }
                    }));
                    if (islogin)
                    {
                        driver.Navigate().GoToUrl(this._SourcePlatform.url);
                    }
                    else
                    {
                        driver.Navigate().GoToUrl(this._SourcePlatform.nologinurl);
                    }
                    choosemenu(key1, key2, key3, logwin, driver, ref awaitms);
                    checkloading(key1, key2, key3, logwin, driver, ref count, ref awaitms);
                }
                if ((key1 == 1 && count % 30 == 0) || (key1 == 2 && count % 60 == 0) || (key1 == 3 && count % 100 == 0))
                {
                    int c = count;
                    count++;
                    this.Invoke(new MethodInvoker(delegate
                    {
                        islogin = this.chk_Logined.Checked;
                        logwin.txt_log.AppendText("第" + c + "次，页面重刷\r\n");
                        if (this._SaveRunningLog)
                        {
                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "第" + c + "次，页面重刷\r\n");
                        }
                    }));
                    if (islogin)
                    {
                        driver.Navigate().GoToUrl(this._SourcePlatform.url);
                    }
                    else
                    {
                        driver.Navigate().GoToUrl(this._SourcePlatform.nologinurl);
                    }
                    choosemenu(key1, key2, key3, logwin, driver, ref awaitms);
                    checkloading(key1, key2, key3, logwin, driver, ref count, ref awaitms);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key1">1：早盘，2：今日赛事，3：滚球</param>
        /// <param name="key2">1：足球，2：篮球，3：网球，4：排球，5：棒球，6：羽毛球，7：乒乓球，8：美式足球</param>
        /// <param name="key3">足球{1：独赢 & 让球 & 大小 & 单双，2：半场全场，3：波胆，5：总入球，5：冠军}</param>
        private List<LeagueMatch> AnalysisHtml(string html, int key1, int key2, int key3)
        {
            List<LeagueMatch> lmList = new List<LeagueMatch>();
            if (html.Contains("$************************$"))
            {
                string[] htmlarr = html.Split(new string[] { "$************************$" }, StringSplitOptions.None);
                foreach (var item in htmlarr)
                {
                    switch (key1)
                    {
                        case 1://早盘
                            #region 早盘
                            switch (key2)
                            {
                                case 1://足球
                                    switch (key3)
                                    {
                                        case 1:
                                            //独赢 & 让球 & 大小 & 单/双
                                            lmList.AddRange(AnalysisHtmlHelper.SB_Football_ZP_DRDD_List(item));
                                            break;
                                        case 2:
                                            //半场 / 全场
                                            lmList.AddRange(AnalysisHtmlHelper.SB_Football_ZP_BCQC_List(item));
                                            break;
                                        case 3:
                                            //波胆
                                            lmList.AddRange(AnalysisHtmlHelper.SB_Football_ZP_BD_List(item));
                                            break;
                                        case 4:
                                            //总入球
                                            lmList.AddRange(AnalysisHtmlHelper.SB_Football_ZP_ZRQ_List(item));
                                            break;
                                        case 5:
                                            //冠军
                                            lmList.AddRange(AnalysisHtmlHelper.SB_Football_ZP_GJ_List(item));
                                            break;
                                    }
                                    break;
                                case 2://篮球
                                    switch (key3)
                                    {
                                        case 1:
                                            lmList.AddRange(AnalysisHtmlHelper.SB_Basketball_ZP_DRDD_List(item));
                                            break;
                                    }
                                    break;
                                case 3://网球
                                    switch (key3)
                                    {
                                        case 1:
                                            lmList.AddRange(AnalysisHtmlHelper.SB_Tennis_ZP_DRDD_List(item));
                                            break;
                                    }
                                    break;
                                case 4://排球
                                    switch (key3)
                                    {
                                        case 1:
                                            lmList.AddRange(AnalysisHtmlHelper.SB_Volleyball_ZP_DRDD_List(item));
                                            break;
                                    }
                                    break;
                                case 5://棒球
                                    switch (key3)
                                    {
                                        case 1:
                                            //lmList.AddRange(AnalysisHtmlHelper.SB_Baseball_ZP_DRDD_List(item));
                                            break;
                                    }
                                    break;
                                case 6://羽毛球
                                    switch (key3)
                                    {
                                        case 1:
                                            lmList.AddRange(AnalysisHtmlHelper.SB_Badminton_ZP_DRDD_List(item));
                                            break;
                                    }
                                    break;
                                case 7://乒乓球
                                    switch (key3)
                                    {
                                        case 1:
                                            lmList.AddRange(AnalysisHtmlHelper.SB_Pingpong_ZP_DRDD_List(item));
                                            break;
                                    }
                                    break;
                                case 8://美式足球
                                    switch (key3)
                                    {
                                        case 1:
                                            lmList.AddRange(AnalysisHtmlHelper.SB_AmericanFootball_ZP_DRDD_List(item));
                                            break;
                                    }
                                    break;
                            }
                            #endregion
                            break;
                        case 2://今日赛事
                            #region 今日赛事
                            switch (key2)
                            {
                                case 1://足球
                                    switch (key3)
                                    {
                                        case 1:
                                            //独赢 & 让球 & 大小 & 单/双
                                            lmList.AddRange(AnalysisHtmlHelper.SB_Football_ZP_DRDD_List(item));
                                            break;
                                        case 2:
                                            //半场 / 全场
                                            lmList.AddRange(AnalysisHtmlHelper.SB_Football_ZP_BCQC_List(item));
                                            break;
                                        case 3:
                                            //波胆
                                            lmList.AddRange(AnalysisHtmlHelper.SB_Football_ZP_BD_List(item));
                                            break;
                                        case 4:
                                            //总入球
                                            lmList.AddRange(AnalysisHtmlHelper.SB_Football_ZP_ZRQ_List(item));
                                            break;
                                    }
                                    break;
                                case 2://篮球
                                    switch (key3)
                                    {
                                        case 1:
                                            lmList.AddRange(AnalysisHtmlHelper.SB_Basketball_ZP_DRDD_List(item));
                                            break;
                                    }
                                    break;
                                case 3://网球
                                    switch (key3)
                                    {
                                        case 1:
                                            lmList.AddRange(AnalysisHtmlHelper.SB_Tennis_ZP_DRDD_List(item));
                                            break;
                                    }
                                    break;
                                case 4://排球
                                    switch (key3)
                                    {
                                        case 1:
                                            lmList.AddRange(AnalysisHtmlHelper.SB_Volleyball_ZP_DRDD_List(item));
                                            break;
                                    }
                                    break;
                                case 5://棒球
                                    switch (key3)
                                    {
                                        case 1:
                                            //lmList.AddRange(AnalysisHtmlHelper.SB_Baseball_ZP_DRDD_List(item));
                                            break;
                                    }
                                    break;
                                case 6://羽毛球
                                    switch (key3)
                                    {
                                        case 1:
                                            lmList.AddRange(AnalysisHtmlHelper.SB_Badminton_ZP_DRDD_List(item));
                                            break;
                                    }
                                    break;
                                case 7://乒乓球
                                    switch (key3)
                                    {
                                        case 1:
                                            lmList.AddRange(AnalysisHtmlHelper.SB_Pingpong_ZP_DRDD_List(item));
                                            break;
                                    }
                                    break;
                                case 8://美式足球
                                    switch (key3)
                                    {
                                        case 1:
                                            lmList.AddRange(AnalysisHtmlHelper.SB_AmericanFootball_ZP_DRDD_List(item));
                                            break;
                                    }
                                    break;
                            }
                            #endregion
                            break;
                        case 3://滚球
                            #region 滚球
                            switch (key2)
                            {
                                case 1://足球
                                    switch (key3)
                                    {
                                        case 1:
                                            //独赢 & 让球 & 大小 & 单/双
                                            lmList.AddRange(AnalysisHtmlHelper.SB_Football_GQ_DRDD_List(item));
                                            break;
                                        case 3:
                                            //波胆
                                            lmList.AddRange(AnalysisHtmlHelper.SB_Football_GQ_BD_List(item));
                                            break;
                                        case 4:
                                            //总入球
                                            lmList.AddRange(AnalysisHtmlHelper.SB_Football_GQ_ZRQ_List(item));
                                            break;
                                    }
                                    break;
                                case 2://篮球
                                    switch (key3)
                                    {
                                        case 1:
                                            lmList.AddRange(AnalysisHtmlHelper.SB_Basketball_GQ_DRDD_List(item));
                                            break;
                                    }
                                    break;
                                case 3://网球
                                    switch (key3)
                                    {
                                        case 1:
                                            lmList.AddRange(AnalysisHtmlHelper.SB_Tennis_GQ_DRDD_List(item));
                                            break;
                                    }
                                    break;
                                case 4://排球
                                    switch (key3)
                                    {
                                        case 1:
                                            lmList.AddRange(AnalysisHtmlHelper.SB_Volleyball_GQ_DRDD_List(item));
                                            break;
                                    }
                                    break;
                                case 5://棒球
                                    switch (key3)
                                    {
                                        case 1:
                                            //lmList.AddRange(AnalysisHtmlHelper.SB_Baseball_GQ_DRDD_List(item));
                                            break;
                                    }
                                    break;
                                case 6://羽毛球
                                    switch (key3)
                                    {
                                        case 1:
                                            lmList.AddRange(AnalysisHtmlHelper.SB_Badminton_GQ_DRDD_List(item));
                                            break;
                                    }
                                    break;
                                case 7://乒乓球
                                    switch (key3)
                                    {
                                        case 1:
                                            lmList.AddRange(AnalysisHtmlHelper.SB_Pingpong_GQ_DRDD_List(item));
                                            break;
                                    }
                                    break;
                                case 8://美式足球
                                    switch (key3)
                                    {
                                        case 1:
                                            lmList.AddRange(AnalysisHtmlHelper.SB_AmericanFootball_GQ_DRDD_List(item));
                                            break;
                                    }
                                    break;
                            }
                            #endregion
                            break;
                    }
                }
            }
            else
            {
                switch (key1)
                {
                    case 1://早盘
                        #region 早盘
                        switch (key2)
                        {
                            case 1://足球
                                switch (key3)
                                {
                                    case 1:
                                        //独赢 & 让球 & 大小 & 单/双
                                        lmList.AddRange(AnalysisHtmlHelper.SB_Football_ZP_DRDD_List(html));
                                        break;
                                    case 2:
                                        //半场 / 全场
                                        lmList.AddRange(AnalysisHtmlHelper.SB_Football_ZP_BCQC_List(html));
                                        break;
                                    case 3:
                                        //波胆
                                        lmList.AddRange(AnalysisHtmlHelper.SB_Football_ZP_BD_List(html));
                                        break;
                                    case 4:
                                        //总入球
                                        lmList.AddRange(AnalysisHtmlHelper.SB_Football_ZP_ZRQ_List(html));
                                        break;
                                    case 5:
                                        //冠军
                                        lmList.AddRange(AnalysisHtmlHelper.SB_Football_ZP_GJ_List(html));
                                        break;
                                }
                                break;
                            case 2://篮球
                                switch (key3)
                                {
                                    case 1:
                                        lmList.AddRange(AnalysisHtmlHelper.SB_Basketball_ZP_DRDD_List(html));
                                        break;
                                }
                                break;
                            case 3://网球
                                switch (key3)
                                {
                                    case 1:
                                        lmList.AddRange(AnalysisHtmlHelper.SB_Tennis_ZP_DRDD_List(html));
                                        break;
                                }
                                break;
                            case 4://排球
                                switch (key3)
                                {
                                    case 1:
                                        lmList.AddRange(AnalysisHtmlHelper.SB_Volleyball_ZP_DRDD_List(html));
                                        break;
                                }
                                break;
                            case 5://棒球
                                switch (key3)
                                {
                                    case 1:
                                        //lmList.AddRange(AnalysisHtmlHelper.SB_Baseball_ZP_DRDD_List(html));
                                        break;
                                }
                                break;
                            case 6://羽毛球
                                switch (key3)
                                {
                                    case 1:
                                        lmList.AddRange(AnalysisHtmlHelper.SB_Badminton_ZP_DRDD_List(html));
                                        break;
                                }
                                break;
                            case 7://乒乓球
                                switch (key3)
                                {
                                    case 1:
                                        lmList.AddRange(AnalysisHtmlHelper.SB_Pingpong_ZP_DRDD_List(html));
                                        break;
                                }
                                break;
                            case 8://美式足球
                                switch (key3)
                                {
                                    case 1:
                                        lmList.AddRange(AnalysisHtmlHelper.SB_AmericanFootball_ZP_DRDD_List(html));
                                        break;
                                }
                                break;
                        }
                        #endregion
                        break;
                    case 2://今日赛事
                        #region 今日赛事
                        switch (key2)
                        {
                            case 1://足球
                                switch (key3)
                                {
                                    case 1:
                                        //独赢 & 让球 & 大小 & 单/双
                                        lmList.AddRange(AnalysisHtmlHelper.SB_Football_ZP_DRDD_List(html));
                                        break;
                                    case 2:
                                        //半场 / 全场
                                        lmList.AddRange(AnalysisHtmlHelper.SB_Football_ZP_BCQC_List(html));
                                        break;
                                    case 3:
                                        //波胆
                                        lmList.AddRange(AnalysisHtmlHelper.SB_Football_ZP_BD_List(html));
                                        break;
                                    case 4:
                                        //总入球
                                        lmList.AddRange(AnalysisHtmlHelper.SB_Football_ZP_ZRQ_List(html));
                                        break;
                                }
                                break;
                            case 2://篮球
                                switch (key3)
                                {
                                    case 1:
                                        lmList.AddRange(AnalysisHtmlHelper.SB_Basketball_ZP_DRDD_List(html));
                                        break;
                                }
                                break;
                            case 3://网球
                                switch (key3)
                                {
                                    case 1:
                                        lmList.AddRange(AnalysisHtmlHelper.SB_Tennis_ZP_DRDD_List(html));
                                        break;
                                }
                                break;
                            case 4://排球
                                switch (key3)
                                {
                                    case 1:
                                        lmList.AddRange(AnalysisHtmlHelper.SB_Volleyball_ZP_DRDD_List(html));
                                        break;
                                }
                                break;
                            case 5://棒球
                                switch (key3)
                                {
                                    case 1:
                                        //lmList.AddRange(AnalysisHtmlHelper.SB_Baseball_ZP_DRDD_List(html));
                                        break;
                                }
                                break;
                            case 6://羽毛球
                                switch (key3)
                                {
                                    case 1:
                                        lmList.AddRange(AnalysisHtmlHelper.SB_Badminton_ZP_DRDD_List(html));
                                        break;
                                }
                                break;
                            case 7://乒乓球
                                switch (key3)
                                {
                                    case 1:
                                        lmList.AddRange(AnalysisHtmlHelper.SB_Pingpong_ZP_DRDD_List(html));
                                        break;
                                }
                                break;
                            case 8://美式足球
                                switch (key3)
                                {
                                    case 1:
                                        lmList.AddRange(AnalysisHtmlHelper.SB_AmericanFootball_ZP_DRDD_List(html));
                                        break;
                                }
                                break;
                        }
                        #endregion
                        break;
                    case 3://滚球
                        #region 滚球
                        switch (key2)
                        {
                            case 1://足球
                                switch (key3)
                                {
                                    case 1:
                                        //独赢 & 让球 & 大小 & 单/双
                                        lmList.AddRange(AnalysisHtmlHelper.SB_Football_GQ_DRDD_List(html));
                                        break;
                                    case 3:
                                        //波胆
                                        lmList.AddRange(AnalysisHtmlHelper.SB_Football_GQ_BD_List(html));
                                        break;
                                    case 4:
                                        //总入球
                                        lmList.AddRange(AnalysisHtmlHelper.SB_Football_GQ_ZRQ_List(html));
                                        break;
                                }
                                break;
                            case 2://篮球
                                switch (key3)
                                {
                                    case 1:
                                        lmList.AddRange(AnalysisHtmlHelper.SB_Basketball_GQ_DRDD_List(html));
                                        break;
                                }
                                break;
                            case 3://网球
                                switch (key3)
                                {
                                    case 1:
                                        lmList.AddRange(AnalysisHtmlHelper.SB_Tennis_GQ_DRDD_List(html));
                                        break;
                                }
                                break;
                            case 4://排球
                                switch (key3)
                                {
                                    case 1:
                                        lmList.AddRange(AnalysisHtmlHelper.SB_Volleyball_GQ_DRDD_List(html));
                                        break;
                                }
                                break;
                            case 5://棒球
                                switch (key3)
                                {
                                    case 1:
                                        //lmList.AddRange(AnalysisHtmlHelper.SB_Baseball_GQ_DRDD_List(html));
                                        break;
                                }
                                break;
                            case 6://羽毛球
                                switch (key3)
                                {
                                    case 1:
                                        lmList.AddRange(AnalysisHtmlHelper.SB_Badminton_GQ_DRDD_List(html));
                                        break;
                                }
                                break;
                            case 7://乒乓球
                                switch (key3)
                                {
                                    case 1:
                                        lmList.AddRange(AnalysisHtmlHelper.SB_Pingpong_GQ_DRDD_List(html));
                                        break;
                                }
                                break;
                            case 8://美式足球
                                switch (key3)
                                {
                                    case 1:
                                        lmList.AddRange(AnalysisHtmlHelper.SB_AmericanFootball_GQ_DRDD_List(html));
                                        break;
                                }
                                break;
                        }
                        #endregion
                        break;
                }
            }
            return lmList;
            //if (_IsRunning)
            //{
            //    Transformation(lmList);
            //}
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key1">1：早盘，2：今日赛事，3：滚球</param>
        /// <param name="key2">1：足球，2：篮球，3：网球，4：排球，5：棒球，6：羽毛球，7：乒乓球，8：美式足球</param>
        /// <param name="key3">足球{1：独赢 & 让球 & 大小 & 单双，2：半场全场，3：波胆，5：总入球，5：冠军}</param>
        private void Transformation(int key1, int key2, int key3, List<LeagueMatch> lmList, ref int needupdatecount, ref int updatedcount, ref int deletedcount, ref string msgpush, ref string delmsgpush)
        {
            //string msg211 = "初始：" + deletedcount + "；";
            A_LeagueMatchBll lmbll = new A_LeagueMatchBll();
            A_TeamBll tbll = new A_TeamBll();
            A_MatchBll mbll = new A_MatchBll();
            A_MatchScoreRecordBll msrbll = new A_MatchScoreRecordBll();

            List<string> _ZPNoLockMID = new List<string>();//早盘
            List<string> _JRNoLockMID = new List<string>();//今日
            List<string> _GQNoLockMID = new List<string>();//滚球

            if (lmList.Count > 1 && this._EnableParallelLeagueMatch)
            {
                int nuc = 0, uc = 0, dc = 0;
                string mp = string.Empty, dmp = string.Empty;
                Parallel.ForEach(lmList, lm =>
                {
                    LoopLM(key1, key2, key3, lm, ref nuc, ref uc, ref dc, ref mp, ref dmp, ref _ZPNoLockMID, ref _JRNoLockMID, ref _GQNoLockMID);
                });
                needupdatecount += nuc;
                updatedcount += uc;
                deletedcount += dc;
                msgpush += mp;
                delmsgpush += dmp;
            }
            else
            {
                foreach (LeagueMatch lm in lmList)
                {
                    LoopLM(key1, key2, key3, lm, ref needupdatecount, ref updatedcount, ref deletedcount, ref msgpush, ref delmsgpush, ref _ZPNoLockMID, ref _JRNoLockMID, ref _GQNoLockMID);
                }
            }
            //msg211 += "循环联赛：" + deletedcount + "；";
            List<string> _NoLockMID = new List<string>();
            List<string> _LockMID = new List<string>();
            switch (key1)
            {
                case 1:
                    _NoLockMID = _ZPNoLockMID;
                    break;
                case 2:
                    _NoLockMID = _JRNoLockMID;
                    break;
                case 3:
                    _NoLockMID = _GQNoLockMID;
                    break;
            }
            if (_NoLockMID.Count > 0)
            {
                lock (locklist)
                {
                    Main.Temp_A_Match_Dict[this.Dict_SportsType[key2]].Where(x =>
                    x != null &&
                    x.LastMenuType == key1.ToString() &&
                    //x.IsStart == null &&
                    x.IsLock == "0" &&
                    x.SourcePlatform == SourcePlatformEnum.SB.ToString() &&
                    (!_NoLockMID.Contains(x.ID))).ToList().ForEach(x =>
                    {
                        _LockMID.Add(x.ID);
                    });
                }
                //_LockMID.AddRange(
                //    Main.Temp_A_Match_Dict[this.Dict_SportsType[key2]].Where(x =>
                //    x != null &&
                //    x.LastMenuType == key1.ToString() &&
                //    //x.IsStart == null &&
                //    x.IsLock == "0" &&
                //    x.SourcePlatform == SourcePlatformEnum.SB.ToString() &&
                //    (!_NoLockMID.Contains(x.ID))).Select(x => x.ID).ToList()
                //);
            }
            else
            {
                lock (locklist)
                {
                    Main.Temp_A_Match_Dict[this.Dict_SportsType[key2]].Where(x =>
                    x != null &&
                    x.LastMenuType == key1.ToString() &&
                    //x.IsStart == null &&
                    x.IsLock == "0" &&
                    x.SourcePlatform == SourcePlatformEnum.SB.ToString()).ToList().ForEach(x =>
                    {
                        _LockMID.Add(x.ID);
                    });
                }
                //_LockMID.AddRange(
                //    Main.Temp_A_Match_Dict[this.Dict_SportsType[key2]].Where(x =>
                //    x != null &&
                //    x.LastMenuType == key1.ToString() &&
                //    //x.IsStart == null &&
                //    x.IsLock == "0" &&
                //    x.SourcePlatform == SourcePlatformEnum.SB.ToString()).Select(x => x.ID).ToList()
                //);
            }

            if (key3 == 1)//独赢 & 让球 & 大小 & 单双
            {
                #region 锁定赛事
                DateTime now2 = lmbll.GetServerDateTime();
                StringBuilder ids = new StringBuilder();
                string IsLive = string.Empty;
                int updatecount = 0, delcount = 0;
                string updatemsg = string.Empty, delmsg = string.Empty;
                List<string> lockedmid = new List<string>();
                string sql = string.Empty;
                switch (key1)
                {
                    case 1:
                        //锁定早盘消失赛事
                        IsLive = "0";
                        foreach (string item in _ZPNoLockMID)
                        {
                            ids.Append($"'{item}',");
                        }
                        sql = $"update A_Match set IsLock='1',ModifyTime='{now2.ToString("yyyy-MM-dd HH:mm:ss")}' where LastMenuType='1' and IsStart is NULL and IsLock='0' and SourcePlatform='{SourcePlatformEnum.SB.ToString()}' and SportsType='{this.Dict_SportsType[key2]}'";
                        if (_ZPNoLockMID.Count > 0)
                        {
                            sql += $" and ID not in({ids.ToString().TrimEnd(',')})";
                        }
                        updatemsg = mbll.ExecuteBySQL(sql, out updatecount);
                        if (string.IsNullOrEmpty(updatemsg))
                        {
                            lock (locklist)
                            {                                
                                var ll = Main.Temp_A_Match_Dict[this.Dict_SportsType[key2]].Where(x =>
                                  x != null &&
                                  x.LastMenuType == "1" &&
                                  x.IsStart == null &&
                                  x.IsLock == "0" &&
                                  x.SourcePlatform == SourcePlatformEnum.SB.ToString() &&
                                  (!_ZPNoLockMID.Contains(x.ID))).ToList();
                                for (int i = 0; i < ll.Count; i++)
                                {
                                    A_Match old= (A_Match)Utility.DeepCopy(ll[i]);
                                    ll[i].IsLock = "1";
                                    ll[i].ModifyTime = now2;
                                    lockedmid.Add(ll[i].ID);
                                    Main.Temp_A_Match_Dict[this.Dict_SportsType[key2]].Remove(old);
                                    Main.Temp_A_Match_Dict[this.Dict_SportsType[key2]].Add(ll[i]);
                                }                                
                            }
                        }
                        break;
                    case 2:
                        //锁定今日赛事消失赛事
                        IsLive = "0";
                        foreach (string item in _JRNoLockMID)
                        {
                            ids.Append($"'{item}',");
                        }
                        sql = $"update A_Match set IsLock='1',ModifyTime='{now2.ToString("yyyy-MM-dd HH:mm:ss")}' where LastMenuType='2' and IsStart is NULL and IsLock='0' and SourcePlatform='{SourcePlatformEnum.SB.ToString()}' and SportsType='{this.Dict_SportsType[key2]}'";
                        if (_JRNoLockMID.Count > 0)
                        {
                            sql += $" and ID not in({ids.ToString().TrimEnd(',')})";
                        }
                        updatemsg = mbll.ExecuteBySQL(sql, out updatecount);
                        if (string.IsNullOrEmpty(updatemsg))
                        {
                            lock (locklist)
                            {                                
                                var ll = Main.Temp_A_Match_Dict[this.Dict_SportsType[key2]].Where(x =>
                                x != null &&
                                x.LastMenuType == "2" &&
                                x.IsStart == null &&
                                x.IsLock == "0" &&
                                x.SourcePlatform == SourcePlatformEnum.SB.ToString() &&
                                (!_JRNoLockMID.Contains(x.ID))).ToList();
                                for (int i = 0; i < ll.Count; i++)
                                {
                                    A_Match old = (A_Match)Utility.DeepCopy(ll[i]);
                                    ll[i].IsLock = "1";
                                    ll[i].ModifyTime = now2;
                                    lockedmid.Add(ll[i].ID);
                                    Main.Temp_A_Match_Dict[this.Dict_SportsType[key2]].Remove(old);
                                    Main.Temp_A_Match_Dict[this.Dict_SportsType[key2]].Add(ll[i]);
                                }
                            }
                        }
                        break;
                    case 3:
                        //锁定滚球消失赛事
                        IsLive = "1";
                        foreach (string item in _GQNoLockMID)
                        {
                            ids.Append($"'{item}',");
                        }
                        sql = $"update A_Match set IsLock='1',ModifyTime='{now2.ToString("yyyy-MM-dd HH:mm:ss")}' where LastMenuType='3' and IsStart is not NULL and IsLock='0' and SourcePlatform='{SourcePlatformEnum.SB.ToString()}' and SportsType='{this.Dict_SportsType[key2]}'";
                        if (_GQNoLockMID.Count > 0)
                        {
                            sql += $" and ID not in({ids.ToString().TrimEnd(',')})";
                        }
                        updatemsg = mbll.ExecuteBySQL(sql, out updatecount);
                        if (string.IsNullOrEmpty(updatemsg))
                        {
                            lock (locklist)
                            {
                                var ll = Main.Temp_A_Match_Dict[this.Dict_SportsType[key2]].Where(x =>
                                x != null &&
                                x.LastMenuType == "3" &&
                                x.IsStart != null &&
                                x.IsLock == "0" &&
                                x.SourcePlatform == SourcePlatformEnum.SB.ToString() &&
                                (!_GQNoLockMID.Contains(x.ID))).ToList();
                                for (int i = 0; i < ll.Count; i++)
                                {
                                    A_Match old = (A_Match)Utility.DeepCopy(ll[i]);
                                    ll[i].IsLock = "1";
                                    ll[i].ModifyTime = now2;
                                    lockedmid.Add(ll[i].ID);
                                    Main.Temp_A_Match_Dict[this.Dict_SportsType[key2]].Remove(old);
                                    Main.Temp_A_Match_Dict[this.Dict_SportsType[key2]].Add(ll[i]);
                                }
                            }
                        }
                        break;
                }
                if (lockedmid.Count > 0)
                {
                    StringBuilder mids = new StringBuilder();
                    foreach (string item in lockedmid)
                    {
                        mids.Append($"'{item}',");
                    }
                    //IsLive='{IsLive}' and 
                    sql = $"delete O_Odds where SourcePlatform='{SourcePlatformEnum.SB.ToString()}' and MatchID in({mids.ToString().TrimEnd(',')})";
                    delmsg = mbll.ExecuteBySQL(sql, out delcount);
                    if (string.IsNullOrEmpty(delmsg))
                    {
                        deletedcount += delcount;
                        foreach (var key in Temp_O_Odds_Dict[this.Dict_SportsType[key2]].Keys)
                        {
                            if (key == "cp")
                            {
                                continue;
                            }
                            lock (locklist)
                            {
                                Main.Temp_O_Odds_Dict[this.Dict_SportsType[key2]][key].RemoveAll(x =>
                                //x.IsLive == IsLive &&
                                lockedmid.Contains(x.MatchID));
                            }

                            //Main.Temp_O_Odds_Dict[this.Dict_SportsType[key2]][key] = new ConcurrentBag<O_Odds>(Main.Temp_O_Odds_Dict[this.Dict_SportsType[key2]][key].Where(x => !lockedmid.Contains(x.MatchID)));
                        }
                    }
                    else
                    {
                        delmsgpush += delmsg + "\r\n" + sql + "\r\n";
                    }
                }
                #endregion
            }
            else if (key3 == 5)//冠军
            {

            }
            else
            {
                string betcode = string.Empty;
                if (key3 == 2)//半场全场
                {
                    betcode = "hf";
                }
                else if (key3 == 3)//波胆
                {
                    betcode = "cs";
                }
                else if (key3 == 4)//总入球
                {
                    betcode = "tg";
                }
                if (_LockMID.Count > 0)
                {
                    StringBuilder mids = new StringBuilder();
                    foreach (string item in _LockMID)
                    {
                        mids.Append($"'{item}',");
                    }
                    int delcount = 0;
                    //IsLive='{IsLive}' and 
                    string sql = $"delete O_Odds where BetCode='{betcode}' and SourcePlatform='{SourcePlatformEnum.SB.ToString()}' and MatchID in({mids.ToString().TrimEnd(',')})";
                    string delmsg = mbll.ExecuteBySQL(sql, out delcount);
                    if (string.IsNullOrEmpty(delmsg))
                    {
                        deletedcount += delcount;
                        lock (locklist)
                        {
                            Main.Temp_O_Odds_Dict[this.Dict_SportsType[key2]][betcode].RemoveAll(x =>
                            //x.IsLive == IsLive &&
                            _LockMID.Contains(x.MatchID));
                        }

                        //Main.Temp_O_Odds_Dict[this.Dict_SportsType[key2]][betcode] = new ConcurrentBag<O_Odds>(Main.Temp_O_Odds_Dict[this.Dict_SportsType[key2]][betcode].Where(x => !_LockMID.Contains(x.MatchID)));
                    }
                    else
                    {
                        delmsgpush += delmsg + "\r\n" + sql + "\r\n";
                    }
                }
            }
            //msg211 += "锁定赛事：" + deletedcount + "；";
            //if (key1 == 2 && key2 == 1 && key3 == 1)
            //{
            //    LogHelper.WriteLog(msg211);
            //}
        }
        //循环联赛
        private void LoopLM(int key1, int key2, int key3, LeagueMatch lm, ref int needupdatecount, ref int updatedcount, ref int deletedcount, ref string msgpush, ref string delmsgpush, ref List<string> _ZPNoLockMID, ref List<string> _JRNoLockMID, ref List<string> _GQNoLockMID)
        {
            if (!string.IsNullOrEmpty(lm.Name.Trim()))
            {
                A_LeagueMatchBll lmbll = new A_LeagueMatchBll();
                A_TeamBll tbll = new A_TeamBll();

                DateTime now = lmbll.GetServerDateTime();
                string msg = string.Empty;
                if (lm.TeamList.Count > 0)//冠军
                {
                    #region 折叠一下
                    lm.Name = lm.Name.Trim().Trim('*');
                    string sj = string.Empty;
                    if (lm.Name.Contains(" "))
                    {
                        string[] mnarr = lm.Name.Split(' ');
                        if (mnarr.Length == 2)
                        {
                            sj = mnarr[0];
                            lm.Name = mnarr[1];
                        }
                    }
                    //A_LeagueMatch objlm = lmbll.GetByName(lm.Name, SourcePlatformEnum.SB.ToString());
                    A_LeagueMatch objlm = null;
                    lock (locklist)
                    {
                        objlm = Main.Temp_A_LeagueMatch_Dict[this.Dict_SportsType[key2]].Where(x => x != null && x.Name == lm.Name).OrderByDescending(x => x.ModifyTime).FirstOrDefault();
                    }
                    if (objlm == null)
                    {
                        objlm = new A_LeagueMatch()
                        {
                            ID = Guid.NewGuid().ToString("N"),
                            Name = lm.Name.Trim(),
                            Season = sj,
                            ModifyTime = now,
                            SourcePlatform = SourcePlatformEnum.SB.ToString(),
                            SportsType = this.Dict_SportsType[key2]
                        };
                        msg = lmbll.Create(objlm);
                        if (string.IsNullOrEmpty(msg))
                        {
                            lock (locklist)
                            {
                                Main.Temp_A_LeagueMatch_Dict[this.Dict_SportsType[key2]].Add(objlm);
                            }
                        }
                        else
                        {
                            lock (locklist)
                            {
                                Main.Temp_A_LeagueMatch_Dict[this.Dict_SportsType[key2]] = lmbll.FindAll(SourcePlatformEnum.SB.ToString(), this.Dict_SportsType[key2]);
                            }
                            //Main.Temp_A_LeagueMatch_Dict[this.Dict_SportsType[key2]] = new ConcurrentBag<A_LeagueMatch>(lmbll.FindAll(SourcePlatformEnum.SB.ToString(), this.Dict_SportsType[key2]));
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(objlm.Season) && !string.IsNullOrEmpty(sj) && objlm.Season != sj)
                        {
                            var newlm = (A_LeagueMatch)Utility.DeepCopy(objlm);
                            newlm.Season = sj;
                            newlm.ModifyTime = now;
                            msg = lmbll.Update(newlm);
                            if (string.IsNullOrEmpty(msg))
                            {
                                lock (locklist)
                                {
                                    Main.Temp_A_LeagueMatch_Dict[this.Dict_SportsType[key2]].Remove(objlm);
                                    Main.Temp_A_LeagueMatch_Dict[this.Dict_SportsType[key2]].Add(newlm);
                                }
                                //Main.Temp_A_LeagueMatch_Dict[this.Dict_SportsType[key2]] = new ConcurrentBag<A_LeagueMatch>(Main.Temp_A_LeagueMatch_Dict[this.Dict_SportsType[key2]].Where(x => x != null && x.ID != objlm.ID));
                                //Main.Temp_A_LeagueMatch_Dict[this.Dict_SportsType[key2]].Add(newlm);
                            }
                            else
                            {
                                lock (locklist)
                                {
                                    Main.Temp_A_LeagueMatch_Dict[this.Dict_SportsType[key2]] = lmbll.FindAll(SourcePlatformEnum.SB.ToString(), this.Dict_SportsType[key2]);
                                }
                                //Main.Temp_A_LeagueMatch_Dict[this.Dict_SportsType[key2]] = new ConcurrentBag<A_LeagueMatch>(lmbll.FindAll(SourcePlatformEnum.SB.ToString(), this.Dict_SportsType[key2]));
                            }
                        }
                    }
                    if (string.IsNullOrEmpty(msg))
                    {
                        #region 冠军
                        List<string> IDlist = new List<string>();
                        foreach (Team t in lm.TeamList)
                        {
                            if (string.IsNullOrEmpty(t.Name.Trim()))
                            {
                                continue;
                            }
                            //A_Team objt = tbll.GetByLMIDName(objlm.ID, t.Name.Trim());
                            A_Team objt = null;
                            lock (locklist)
                            {
                                objt = Main.Temp_A_Team_Dict[this.Dict_SportsType[key2]].Where(x => x != null && x.LeagueMatchID == objlm.ID && x.Name == t.Name.Trim()).OrderByDescending(x => x.ModifyTime).FirstOrDefault();
                            }
                            if (objt == null)
                            {
                                objt = new A_Team()
                                {
                                    ID = Guid.NewGuid().ToString("N"),
                                    LeagueMatchID = objlm.ID,
                                    Name = t.Name.Trim(),
                                    ModifyTime = now,
                                    SourcePlatform = SourcePlatformEnum.SB.ToString(),
                                    SportsType = this.Dict_SportsType[key2]
                                };
                                msg = tbll.Create(objt);
                                if (string.IsNullOrEmpty(msg))
                                {
                                    lock (locklist)
                                    {
                                        Main.Temp_A_Team_Dict[this.Dict_SportsType[key2]].Add(objt);
                                    }
                                }
                                else
                                {
                                    lock (locklist)
                                    {
                                        Main.Temp_A_Team_Dict[this.Dict_SportsType[key2]] = tbll.FindAll(SourcePlatformEnum.SB.ToString(), this.Dict_SportsType[key2]);
                                    }
                                    //Main.Temp_A_Team_Dict[this.Dict_SportsType[key2]] = new ConcurrentBag<A_Team>(tbll.FindAll(SourcePlatformEnum.SB.ToString(), this.Dict_SportsType[key2]));
                                }
                            }
                            if (string.IsNullOrEmpty(msg))
                            {
                                O_OddsBll obll = new O_OddsBll();
                                O_OddsRecordBll orbll = new O_OddsRecordBll();
                                O_Odds odds_cp = new O_Odds()
                                {
                                    ID = Guid.NewGuid().ToString("N"),
                                    LeagueMatchID = objlm.ID,
                                    //MatchID = "",
                                    SportsType = this.Dict_SportsType[key2],
                                    BetCode = Dict_S_BetCode[this.Dict_SportsType[key2]]["冠军"],
                                    BetExplain = objt.ID,
                                    OddsSort = "",
                                    MainSort = 1,
                                    Odds = Utility.ObjConvertToDecimal(t.OutrightOdds),
                                    IsLive = "0",
                                    CreateTime = now,
                                    ModifyTime = now,
                                    SourcePlatform = SourcePlatformEnum.SB.ToString()
                                };
                                string ID = string.Empty;
                                update_odds(key2, odds_cp, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out ID);
                                IDlist.Add(ID);
                            }
                        }
                        //冠军                        
                        StringBuilder ids = new StringBuilder();
                        foreach (string item in IDlist)
                        {
                            ids.Append($"'{item}',");
                        }
                        int delcount = 0;
                        string sql = $"delete O_Odds where LeagueMatchID='{objlm.ID}' and MatchID is NULL and BetCode='{Dict_S_BetCode[this.Dict_SportsType[key2]]["冠军"]}' and SourcePlatform='{SourcePlatformEnum.SB.ToString()}' and SportsType='{this.Dict_SportsType[key2]}' and ID not in({ids.ToString().TrimEnd(',')})";
                        string delmsg = new O_OddsBll().ExecuteBySQL(sql, out delcount);
                        if (string.IsNullOrEmpty(delmsg))
                        {
                            deletedcount += delcount;
                            lock (locklist)
                            {
                                Main.Temp_O_Odds_Dict[this.Dict_SportsType[key2]][Dict_S_BetCode[this.Dict_SportsType[key2]]["冠军"]].RemoveAll(x =>
                                x.LeagueMatchID == objlm.ID &&
                                x.MatchID == null &&
                                x.BetCode == Dict_S_BetCode[this.Dict_SportsType[key2]]["冠军"] &&
                                (!IDlist.Contains(x.ID)));
                            }

                            //Main.Temp_O_Odds_Dict[this.Dict_SportsType[key2]][Dict_S_BetCode[this.Dict_SportsType[key2]]["冠军"]] = new ConcurrentBag<O_Odds>(Main.Temp_O_Odds_Dict[this.Dict_SportsType[key2]][Dict_S_BetCode[this.Dict_SportsType[key2]]["冠军"]].Where(x =>
                            //x.LeagueMatchID == objlm.ID &&
                            //x.MatchID == null &&
                            //x.BetCode == Dict_S_BetCode[this.Dict_SportsType[key2]]["冠军"] &&
                            //IDlist.Contains(x.ID)));
                        }
                        else
                        {
                            delmsgpush += delmsg + "\r\n" + sql + "\r\n";
                        }
                        #endregion
                    }
                    #endregion
                }
                else
                {
                    lm.Name = lm.Name.Trim().Trim('*');
                    if (key2 == 1)//足球
                    {
                        if (lm.Name.Contains(" - "))
                        {
                            string[] mnarr = lm.Name.Split(new string[] { " - " }, StringSplitOptions.None);
                            if (mnarr.Length == 2)
                            {
                                lm.Name = mnarr[0];
                            }
                        }
                    }
                    //A_LeagueMatch objlm = lmbll.GetByName(lm.Name, SourcePlatformEnum.SB.ToString());
                    A_LeagueMatch objlm = null;
                    lock (locklist)
                    {
                        objlm = Main.Temp_A_LeagueMatch_Dict[this.Dict_SportsType[key2]].Where(x => x != null && x.Name == lm.Name).OrderByDescending(x => x.ModifyTime).FirstOrDefault();
                    }
                    if (objlm == null)
                    {
                        objlm = new A_LeagueMatch()
                        {
                            ID = Guid.NewGuid().ToString("N"),
                            Name = lm.Name.Trim(),
                            ModifyTime = now,
                            SourcePlatform = SourcePlatformEnum.SB.ToString(),
                            SportsType = this.Dict_SportsType[key2]
                        };
                        msg = lmbll.Create(objlm);
                        if (string.IsNullOrEmpty(msg))
                        {
                            lock (locklist)
                            {
                                Main.Temp_A_LeagueMatch_Dict[this.Dict_SportsType[key2]].Add(objlm);
                            }
                        }
                        else
                        {
                            lock (locklist)
                            {
                                Main.Temp_A_LeagueMatch_Dict[this.Dict_SportsType[key2]] = lmbll.FindAll(SourcePlatformEnum.SB.ToString(), this.Dict_SportsType[key2]);
                            }
                            //Main.Temp_A_LeagueMatch_Dict[this.Dict_SportsType[key2]] = new ConcurrentBag<A_LeagueMatch>(lmbll.FindAll(SourcePlatformEnum.SB.ToString(), this.Dict_SportsType[key2]));
                        }
                    }
                    if (string.IsNullOrEmpty(msg))
                    {
                        if (lm.MatchList.Count > 0)//独赢 & 让球 & 大小 & 单/双 & 半场/全场 & 波胆 & 总入球
                        {
                            if (lm.MatchList.Count > 1 && this._EnableParallelMatch)
                            {
                                int nuc = 0, uc = 0, dc = 0;
                                string mp = string.Empty, dmp = string.Empty;
                                List<string> zpnlmid = new List<string>();//早盘
                                List<string> jrnlmid = new List<string>();//今日
                                List<string> gqnlmid = new List<string>();//滚球
                                Parallel.ForEach(lm.MatchList, m =>
                                {
                                    LoopMatch(now, key1, key2, key3, objlm, m, ref nuc, ref uc, ref dc, ref mp, ref dmp, ref zpnlmid, ref jrnlmid, ref gqnlmid);
                                });
                                needupdatecount += nuc;
                                updatedcount += uc;
                                deletedcount += dc;
                                msgpush += mp;
                                delmsgpush += dmp;
                                _ZPNoLockMID.AddRange(zpnlmid);
                                _JRNoLockMID.AddRange(jrnlmid);
                                _GQNoLockMID.AddRange(gqnlmid);
                            }
                            else
                            {
                                foreach (Match m in lm.MatchList)
                                {
                                    LoopMatch(now, key1, key2, key3, objlm, m, ref needupdatecount, ref updatedcount, ref deletedcount, ref msgpush, ref delmsgpush, ref _ZPNoLockMID, ref _JRNoLockMID, ref _GQNoLockMID);
                                }
                            }
                        }
                    }
                }
            }
        }
        //循环比赛
        private void LoopMatch(DateTime now, int key1, int key2, int key3, A_LeagueMatch objlm, Match m, ref int needupdatecount, ref int updatedcount, ref int deletedcount, ref string msgpush, ref string delmsgpush, ref List<string> _ZPNoLockMID, ref List<string> _JRNoLockMID, ref List<string> _GQNoLockMID)
        {
            if (!string.IsNullOrEmpty(m.HomeTeam.Trim()) && !string.IsNullOrEmpty(m.VisitingTeam.Trim()))
            {
                A_TeamBll tbll = new A_TeamBll();
                A_MatchBll mbll = new A_MatchBll();
                A_MatchScoreRecordBll msrbll = new A_MatchScoreRecordBll();
                string msg = string.Empty;
                A_Team objtH = null;
                lock (locklist)
                {
                    objtH = Main.Temp_A_Team_Dict[this.Dict_SportsType[key2]].Where(x => x != null && x.LeagueMatchID == objlm.ID && x.Name == m.HomeTeam.Trim()).OrderByDescending(x => x.ModifyTime).FirstOrDefault();
                }
                string msg1 = string.Empty;
                if (objtH == null)
                {
                    objtH = new A_Team()
                    {
                        ID = Guid.NewGuid().ToString("N"),
                        LeagueMatchID = objlm.ID,
                        Name = m.HomeTeam.Trim(),
                        ModifyTime = now,
                        SourcePlatform = SourcePlatformEnum.SB.ToString(),
                        SportsType = this.Dict_SportsType[key2]
                    };
                    msg1 = tbll.Create(objtH);
                    if (string.IsNullOrEmpty(msg1))
                    {
                        lock (locklist)
                        {
                            Main.Temp_A_Team_Dict[this.Dict_SportsType[key2]].Add(objtH);
                        }
                    }
                    else
                    {
                        lock (locklist)
                        {
                            Main.Temp_A_Team_Dict[this.Dict_SportsType[key2]] = tbll.FindAll(SourcePlatformEnum.SB.ToString(), this.Dict_SportsType[key2]);
                        }
                        //Main.Temp_A_Team_Dict[this.Dict_SportsType[key2]] = new ConcurrentBag<A_Team>(tbll.FindAll(SourcePlatformEnum.SB.ToString(), this.Dict_SportsType[key2]));
                    }
                }
                A_Team objtV = null;
                lock (locklist)
                {
                    objtV = Main.Temp_A_Team_Dict[this.Dict_SportsType[key2]].Where(x => x != null && x.LeagueMatchID == objlm.ID && x.Name == m.VisitingTeam.Trim()).OrderByDescending(x => x.ModifyTime).FirstOrDefault();
                }
                string msg2 = string.Empty;
                if (objtV == null)
                {
                    objtV = new A_Team()
                    {
                        ID = Guid.NewGuid().ToString("N"),
                        LeagueMatchID = objlm.ID,
                        Name = m.VisitingTeam.Trim(),
                        ModifyTime = now,
                        SourcePlatform = SourcePlatformEnum.SB.ToString(),
                        SportsType = this.Dict_SportsType[key2]
                    };
                    msg2 = tbll.Create(objtV);
                    if (string.IsNullOrEmpty(msg2))
                    {
                        lock (locklist)
                        {
                            Main.Temp_A_Team_Dict[this.Dict_SportsType[key2]].Add(objtV);
                        }
                    }
                    else
                    {
                        lock (locklist)
                        {
                            Main.Temp_A_Team_Dict[this.Dict_SportsType[key2]] = tbll.FindAll(SourcePlatformEnum.SB.ToString(), this.Dict_SportsType[key2]);
                        }
                        //Main.Temp_A_Team_Dict[this.Dict_SportsType[key2]] = new ConcurrentBag<A_Team>(tbll.FindAll(SourcePlatformEnum.SB.ToString(), this.Dict_SportsType[key2]));
                    }
                }
                if (string.IsNullOrEmpty(msg1) && string.IsNullOrEmpty(msg2))
                {
                    if (key1 == 3)//滚球，根据当天日期来找比赛
                    {
                        #region 滚球
                        try
                        {
                            DateTime bsdate = now.Date;
                            A_Match objm = null;
                            lock (locklist)
                            {
                                objm = Main.Temp_A_Match_Dict[this.Dict_SportsType[key2]].Where(x => x != null && x.HomeTeamID == objtH.ID && x.VisitingTeamID == objtV.ID && x.SP_GameStartTime.Value.Date == bsdate).OrderByDescending(x => x.ModifyTime).FirstOrDefault();
                            }
                            int ts = -1;
                            int.TryParse(m.timing, out ts);
                            if (objm == null)
                            {
                                objm = new A_Match()
                                {
                                    ID = Guid.NewGuid().ToString("N"),
                                    LeagueMatchID = objlm.ID,
                                    SportsType = this.Dict_SportsType[key2],
                                    HomeTeamID = objtH.ID,
                                    VisitingTeamID = objtV.ID,
                                    SourcePlatform = SourcePlatformEnum.SB.ToString(),
                                    SP_GameStartTime = now.Date,
                                    HomeTeamScore = m.HomeTeamScore,
                                    VisitingTeamScore = m.VisitingTeamScore,
                                    ExistLive = "1",
                                    IsStart = ts < 0 ? "0" : "1",
                                    Timing = ts * 60,
                                    StatusText = m.statustext,
                                    IsEnd = "0",
                                    ModifyTime = now,
                                    CreateTime = now,
                                    LastMenuType = key1.ToString(),
                                    IsLock = "0"
                                };
                                if (key2 == 1)//足球
                                {
                                    if (m.halftype == "1H")
                                    {
                                        objm.MatchType = MatchTypeEnum.Firsthalf.ToString();
                                    }
                                    else if (m.halftype == "2H")
                                    {
                                        objm.MatchType = MatchTypeEnum.Secondhalf.ToString();
                                    }
                                    if (m.halftype == "1H" && ts == 1)
                                    {
                                        objm.GameStartTime = Utility.ObjConvertToDateTime(now.ToString("yyyy-MM-dd HH:mm:00"));
                                    }
                                }
                                else if (key2 == 2)//篮球
                                {
                                    if (m.halftype == "1H")
                                    {
                                        objm.MatchType = MatchTypeEnum.Firsthalf.ToString();
                                    }
                                    else if (m.halftype == "2H")
                                    {
                                        objm.MatchType = MatchTypeEnum.Secondhalf.ToString();
                                    }
                                    else if (m.halftype == "1Q")
                                    {
                                        objm.MatchType = MatchTypeEnum.FirstQuarter.ToString();
                                    }
                                    else if (m.halftype == "2Q")
                                    {
                                        objm.MatchType = MatchTypeEnum.SecondQuarter.ToString();
                                    }
                                    else if (m.halftype == "3Q")
                                    {
                                        objm.MatchType = MatchTypeEnum.ThirdQuarter.ToString();
                                    }
                                    else if (m.halftype == "4Q")
                                    {
                                        objm.MatchType = MatchTypeEnum.FourthQuarter.ToString();
                                    }
                                    else if (m.halftype == "OT")
                                    {
                                        objm.MatchType = MatchTypeEnum.ExtraTime.ToString();
                                    }
                                    if (m.halftype == "1H" && ts == 20)
                                    {
                                        objm.GameStartTime = Utility.ObjConvertToDateTime(now.ToString("yyyy-MM-dd HH:mm:00"));
                                    }
                                    else if (m.halftype == "1Q" && ts == 12)
                                    {
                                        objm.GameStartTime = Utility.ObjConvertToDateTime(now.ToString("yyyy-MM-dd HH:mm:00"));
                                    }
                                }
                                else if (key2 == 8)//美式足球
                                {
                                    if (m.halftype == "1Q")
                                    {
                                        objm.MatchType = MatchTypeEnum.FirstQuarter.ToString();
                                    }
                                    else if (m.halftype == "2Q")
                                    {
                                        objm.MatchType = MatchTypeEnum.SecondQuarter.ToString();
                                    }
                                    else if (m.halftype == "3Q")
                                    {
                                        objm.MatchType = MatchTypeEnum.ThirdQuarter.ToString();
                                    }
                                    else if (m.halftype == "4Q")
                                    {
                                        objm.MatchType = MatchTypeEnum.FourthQuarter.ToString();
                                    }
                                    else if (m.halftype == "OT")
                                    {
                                        objm.MatchType = MatchTypeEnum.ExtraTime.ToString();
                                    }
                                    if (m.halftype == "1Q" && ts == 15)
                                    {
                                        objm.GameStartTime = Utility.ObjConvertToDateTime(now.ToString("yyyy-MM-dd HH:mm:00"));
                                    }
                                }
                                else if (key2 == 3)//网球
                                {
                                    objm.MatchType = MatchTypeEnum.Full.ToString();
                                    objm.HomeTeamSet = m.HomeTeamSet;
                                    objm.HomeTeamInning = m.HomeTeamInning;
                                    objm.VisitingTeamSet = m.VisitingTeamSet;
                                    objm.VisitingTeamInning = m.VisitingTeamInning;
                                }
                                else if (key2 == 4)//排球
                                {
                                    objm.MatchType = MatchTypeEnum.Full.ToString();
                                }
                                else if (key2 == 5)//棒球
                                {

                                }
                                else if (key2 == 6)//羽毛球
                                {
                                    objm.MatchType = MatchTypeEnum.Full.ToString();
                                }
                                else if (key2 == 7)//乒乓球
                                {
                                    objm.MatchType = MatchTypeEnum.Full.ToString();
                                }
                                msg = mbll.Create(objm);
                                if (string.IsNullOrEmpty(msg))
                                {
                                    _GQNoLockMID.Add(objm.ID);
                                    lock (locklist)
                                    {
                                        Main.Temp_A_Match_Dict[this.Dict_SportsType[key2]].Add(objm);
                                    }
                                }
                                else
                                {
                                    lock (locklist)
                                    {
                                        Main.Temp_A_Match_Dict[this.Dict_SportsType[key2]] = mbll.FindByDate(SourcePlatformEnum.SB.ToString(), this.Dict_SportsType[key2], now.Date.AddDays(-1));
                                    }
                                    //Main.Temp_A_Match_Dict[this.Dict_SportsType[key2]] = new ConcurrentBag<A_Match>(mbll.FindByDate(SourcePlatformEnum.SB.ToString(), this.Dict_SportsType[key2], now.Date.AddDays(-1)));
                                }
                            }
                            else//更新
                            {
                                _GQNoLockMID.Add(objm.ID);
                                var newm = (A_Match)Utility.DeepCopy(objm);
                                newm.LastMenuType = key1.ToString();
                                newm.HomeTeamScore = m.HomeTeamScore;
                                newm.VisitingTeamScore = m.VisitingTeamScore;
                                newm.IsStart = ts < 0 ? "0" : "1";
                                newm.Timing = ts * 60;
                                newm.StatusText = m.statustext;
                                newm.IsEnd = "0";
                                newm.IsLock = "0";
                                newm.ModifyTime = now;
                                if (key2 == 1)//足球
                                {
                                    if (m.halftype == "1H")
                                    {
                                        newm.MatchType = MatchTypeEnum.Firsthalf.ToString();
                                    }
                                    else if (m.halftype == "2H")
                                    {
                                        newm.MatchType = MatchTypeEnum.Secondhalf.ToString();
                                    }
                                    if (m.halftype == "1H" && ts == 1)
                                    {
                                        newm.GameStartTime = Utility.ObjConvertToDateTime(now.ToString("yyyy-MM-dd HH:mm:00"));
                                    }
                                }
                                else if (key2 == 2)//篮球
                                {
                                    if (m.halftype == "1H")
                                    {
                                        newm.MatchType = MatchTypeEnum.Firsthalf.ToString();
                                    }
                                    else if (m.halftype == "2H")
                                    {
                                        newm.MatchType = MatchTypeEnum.Secondhalf.ToString();
                                    }
                                    else if (m.halftype == "1Q")
                                    {
                                        newm.MatchType = MatchTypeEnum.FirstQuarter.ToString();
                                    }
                                    else if (m.halftype == "2Q")
                                    {
                                        newm.MatchType = MatchTypeEnum.SecondQuarter.ToString();
                                    }
                                    else if (m.halftype == "3Q")
                                    {
                                        newm.MatchType = MatchTypeEnum.ThirdQuarter.ToString();
                                    }
                                    else if (m.halftype == "4Q")
                                    {
                                        newm.MatchType = MatchTypeEnum.FourthQuarter.ToString();
                                    }
                                    if (m.halftype == "1H" && ts == 20)
                                    {
                                        newm.GameStartTime = Utility.ObjConvertToDateTime(now.ToString("yyyy-MM-dd HH:mm:00"));
                                    }
                                    else if (m.halftype == "1Q" && ts == 12)
                                    {
                                        newm.GameStartTime = Utility.ObjConvertToDateTime(now.ToString("yyyy-MM-dd HH:mm:00"));
                                    }
                                }
                                else if (key2 == 8)//美式足球
                                {
                                    if (m.halftype == "1Q")
                                    {
                                        newm.MatchType = MatchTypeEnum.FirstQuarter.ToString();
                                    }
                                    else if (m.halftype == "2Q")
                                    {
                                        newm.MatchType = MatchTypeEnum.SecondQuarter.ToString();
                                    }
                                    else if (m.halftype == "3Q")
                                    {
                                        newm.MatchType = MatchTypeEnum.ThirdQuarter.ToString();
                                    }
                                    else if (m.halftype == "4Q")
                                    {
                                        newm.MatchType = MatchTypeEnum.FourthQuarter.ToString();
                                    }
                                    if (m.halftype == "1Q" && ts == 15)
                                    {
                                        newm.GameStartTime = Utility.ObjConvertToDateTime(now.ToString("yyyy-MM-dd HH:mm:00"));
                                    }
                                }
                                else if (key2 == 3)//网球
                                {
                                    newm.MatchType = MatchTypeEnum.Full.ToString();
                                    newm.HomeTeamSet = m.HomeTeamSet;
                                    newm.HomeTeamInning = m.HomeTeamInning;
                                    newm.VisitingTeamSet = m.VisitingTeamSet;
                                    newm.VisitingTeamInning = m.VisitingTeamInning;
                                }
                                else if (key2 == 4)//排球
                                {
                                    newm.MatchType = MatchTypeEnum.Full.ToString();
                                }
                                else if (key2 == 5)//棒球
                                {

                                }
                                else if (key2 == 6)//羽毛球
                                {
                                    newm.MatchType = MatchTypeEnum.Full.ToString();
                                }
                                else if (key2 == 7)//乒乓球
                                {
                                    newm.MatchType = MatchTypeEnum.Full.ToString();
                                }
                                msg = mbll.Update(newm);
                                if (string.IsNullOrEmpty(msg))
                                {
                                    lock (locklist)
                                    {
                                        Main.Temp_A_Match_Dict[this.Dict_SportsType[key2]].Remove(objm);
                                        Main.Temp_A_Match_Dict[this.Dict_SportsType[key2]].Add(newm);
                                    }
                                    //Main.Temp_A_Match_Dict[this.Dict_SportsType[key2]] = new ConcurrentBag<A_Match>(Main.Temp_A_Match_Dict[this.Dict_SportsType[key2]].Where(x=>x!=null&&x.ID!= objm.ID));
                                    //Main.Temp_A_Match_Dict[this.Dict_SportsType[key2]].Add(newm);
                                    objm.MatchType = newm.MatchType;
                                }
                                else
                                {
                                    lock (locklist)
                                    {
                                        Main.Temp_A_Match_Dict[this.Dict_SportsType[key2]] = mbll.FindByDate(SourcePlatformEnum.SB.ToString(), this.Dict_SportsType[key2], now.Date.AddDays(-1));
                                    }
                                    //Main.Temp_A_Match_Dict[this.Dict_SportsType[key2]] = new ConcurrentBag<A_Match>(mbll.FindByDate(SourcePlatformEnum.SB.ToString(), this.Dict_SportsType[key2], now.Date.AddDays(-1)));
                                }
                            }
                            if (objm != null && string.IsNullOrEmpty(msg))
                            {
                                #region 比分记录
                                A_MatchScoreRecord objmsr = new A_MatchScoreRecord()
                                {
                                    ID = Guid.NewGuid().ToString("N"),
                                    MatchID = objm.ID,
                                    MatchType = objm.MatchType,
                                    HomeTeamScore = m.HomeTeamScore,
                                    VisitingTeamScore = m.VisitingTeamScore,
                                    Timing = ts,
                                    SportsType = this.Dict_SportsType[key2],
                                    SourcePlatform = SourcePlatformEnum.SB.ToString(),
                                    CreateTime = now
                                };
                                if (key2 == 3)//网球
                                {
                                    objmsr.MatchType = MatchTypeEnum.Full.ToString();
                                    objmsr.HomeTeamSet = m.HomeTeamSet;
                                    objmsr.HomeTeamInning = m.HomeTeamInning;
                                    objmsr.VisitingTeamSet = m.VisitingTeamSet;
                                    objmsr.VisitingTeamInning = m.VisitingTeamInning;
                                }
                                //A_MatchScoreRecord lastmsr = msrbll.GetByMID(objm.ID);
                                A_MatchScoreRecord lastmsr = null;
                                lock (locklist)
                                {
                                    lastmsr = Main.Temp_A_MatchScoreRecord_Dict[this.Dict_SportsType[key2]].Where(x => x != null && x.MatchID == objm.ID).OrderByDescending(x => x.CreateTime).FirstOrDefault();
                                }
                                if (lastmsr != null)//比较比分
                                {
                                    if (key2 == 3)//网球
                                    {
                                        if (lastmsr.HomeTeamScore != objmsr.HomeTeamScore ||
                                            lastmsr.HomeTeamInning != objmsr.HomeTeamInning ||
                                            lastmsr.HomeTeamSet != objmsr.HomeTeamSet ||
                                            lastmsr.VisitingTeamScore != objmsr.VisitingTeamScore ||
                                            lastmsr.VisitingTeamInning != objmsr.VisitingTeamInning ||
                                            lastmsr.VisitingTeamSet != objmsr.VisitingTeamSet
                                            )
                                        {
                                            msg = msrbll.Create(objmsr);
                                            if (string.IsNullOrEmpty(msg))
                                            {
                                                lock (locklist)
                                                {
                                                    Main.Temp_A_MatchScoreRecord_Dict[this.Dict_SportsType[key2]].Add(objmsr);
                                                }
                                            }
                                            else
                                            {
                                                lock (locklist)
                                                {
                                                    Main.Temp_A_MatchScoreRecord_Dict[this.Dict_SportsType[key2]] = msrbll.FindByDate(SourcePlatformEnum.SB.ToString(), this.Dict_SportsType[key2], now.Date.AddDays(-1));
                                                }
                                                //Main.Temp_A_MatchScoreRecord_Dict[this.Dict_SportsType[key2]] = new ConcurrentBag<A_MatchScoreRecord>(msrbll.FindByDate(SourcePlatformEnum.SB.ToString(), this.Dict_SportsType[key2], now.Date.AddDays(-1)));
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (lastmsr.HomeTeamScore != objmsr.HomeTeamScore || lastmsr.VisitingTeamScore != objmsr.VisitingTeamScore)
                                        {
                                            msg = msrbll.Create(objmsr);
                                            if (string.IsNullOrEmpty(msg))
                                            {
                                                lock (locklist)
                                                {
                                                    Main.Temp_A_MatchScoreRecord_Dict[this.Dict_SportsType[key2]].Add(objmsr);
                                                }
                                            }
                                            else
                                            {
                                                lock (locklist)
                                                {
                                                    Main.Temp_A_MatchScoreRecord_Dict[this.Dict_SportsType[key2]] = msrbll.FindByDate(SourcePlatformEnum.SB.ToString(), this.Dict_SportsType[key2], now.Date.AddDays(-1));
                                                }
                                                //Main.Temp_A_MatchScoreRecord_Dict[this.Dict_SportsType[key2]] = new ConcurrentBag<A_MatchScoreRecord>(msrbll.FindByDate(SourcePlatformEnum.SB.ToString(), this.Dict_SportsType[key2], now.Date.AddDays(-1)));
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    msg = msrbll.Create(objmsr);
                                    if (string.IsNullOrEmpty(msg))
                                    {
                                        lock (locklist)
                                        {
                                            Main.Temp_A_MatchScoreRecord_Dict[this.Dict_SportsType[key2]].Add(objmsr);
                                        }
                                    }
                                    else
                                    {
                                        lock (locklist)
                                        {
                                            Main.Temp_A_MatchScoreRecord_Dict[this.Dict_SportsType[key2]] = msrbll.FindByDate(SourcePlatformEnum.SB.ToString(), this.Dict_SportsType[key2], now.Date.AddDays(-1));
                                        }
                                        //Main.Temp_A_MatchScoreRecord_Dict[this.Dict_SportsType[key2]] = new ConcurrentBag<A_MatchScoreRecord>(msrbll.FindByDate(SourcePlatformEnum.SB.ToString(), this.Dict_SportsType[key2], now.Date.AddDays(-1)));
                                    }
                                }
                                #endregion

                                O_OddsBll obll = new O_OddsBll();
                                O_OddsRecordBll orbll = new O_OddsRecordBll();
                                #region 全场赔率
                                if (m.FullCourtList.Count > 0)
                                {
                                    FillData_dyrqdxds(key2, m.FullCourtList, objlm.ID, objm.ID, now, "1", "0", ref needupdatecount, ref updatedcount, ref deletedcount, ref msgpush, ref delmsgpush);
                                }
                                #endregion
                                #region 半场赔率
                                //if (m.HalfCourtList.Count > 0)
                                //{
                                FillData_dyrqdxds(key2, m.HalfCourtList, objlm.ID, objm.ID, now, "1", "1", ref needupdatecount, ref updatedcount, ref deletedcount, ref msgpush, ref delmsgpush);
                                //}
                                #endregion
                                #region 波胆
                                if (m.CorrectScoreList.Count > 0)
                                {
                                    var fullcs = m.CorrectScoreList.Where(x => x.type == 1).ToList();
                                    var fhcs = m.CorrectScoreList.Where(x => x.type == 2).ToList();
                                    if (fullcs.Count > 0)
                                    {
                                        FillData_bd(key2, fullcs, objlm.ID, objm.ID, now, "1", "", ref needupdatecount, ref updatedcount, ref deletedcount, ref msgpush, ref delmsgpush);
                                    }
                                    if (fhcs.Count > 0)
                                    {
                                        FillData_bd(key2, fhcs, objlm.ID, objm.ID, now, "1", "-上半场", ref needupdatecount, ref updatedcount, ref deletedcount, ref msgpush, ref delmsgpush);
                                    }
                                }
                                #endregion
                                #region 总入球
                                if (m.TotalGoalList.Count > 0)
                                {
                                    var fulltg = m.TotalGoalList.Where(x => x.type == 1).ToList();
                                    var fhtg = m.TotalGoalList.Where(x => x.type == 2).ToList();
                                    if (fulltg.Count > 0)
                                    {
                                        FillData_zrq(key2, fulltg, objlm.ID, objm.ID, now, "1", "", ref needupdatecount, ref updatedcount, ref deletedcount, ref msgpush, ref delmsgpush);
                                    }
                                    if (fhtg.Count > 0)
                                    {
                                        FillData_zrq(key2, fhtg, objlm.ID, objm.ID, now, "1", "-上半场", ref needupdatecount, ref updatedcount, ref deletedcount, ref msgpush, ref delmsgpush);
                                    }
                                }
                                #endregion
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.ErrorLog(ex.Message, ex);
                            msgpush += ex.Message + "\r\n";
                        }
                        #endregion
                    }
                    else
                    {
                        #region 早盘和今日
                        try
                        {
                            if (!string.IsNullOrEmpty(m.time))
                            {
                                string st = now.Year + "-" + m.time;
                                DateTime bstime = now;
                                bool timeisok = false;
                                try
                                {
                                    bstime = Convert.ToDateTime(st);
                                    if (bstime.Date < now.Date)
                                    {
                                        bstime = bstime.AddYears(1);
                                    }
                                    timeisok = true;
                                }
                                catch (Exception ex)
                                {
                                    LogHelper.ErrorLog(st + "：" + ex.Message, ex);
                                    msgpush += ex.Message + "\r\n";
                                }
                                if (timeisok)
                                {
                                    //A_Match objm = mbll.GetByHVTime(objtH.ID, objtV.ID, bstime);
                                    A_Match objm = null;
                                    lock (locklist)
                                    {
                                        objm = Main.Temp_A_Match_Dict[this.Dict_SportsType[key2]].Where(x => x != null && x.HomeTeamID == objtH.ID && x.VisitingTeamID == objtV.ID && x.SP_GameStartTime.Value.Date == bstime.Date).OrderByDescending(x => x.ModifyTime).FirstOrDefault();
                                    }
                                    int hs = 0, vs = 0;
                                    int.TryParse(m.HomeTeamScore, out hs);
                                    int.TryParse(m.VisitingTeamScore, out vs);
                                    if (objm == null)
                                    {
                                        objm = new A_Match()
                                        {
                                            ID = Guid.NewGuid().ToString("N"),
                                            LeagueMatchID = objlm.ID,
                                            SportsType = this.Dict_SportsType[key2],
                                            HomeTeamID = objtH.ID,
                                            VisitingTeamID = objtV.ID,
                                            SourcePlatform = SourcePlatformEnum.SB.ToString(),
                                            LastMenuType = key1.ToString(),
                                            SP_GameStartTime = bstime,
                                            ExistLive = string.IsNullOrEmpty(m.GQ) ? "0" : "1",
                                            IsLock = "0",
                                            StatusText = "",
                                            ModifyTime = now,
                                            CreateTime = now
                                        };
                                        msg = mbll.Create(objm);
                                        if (string.IsNullOrEmpty(msg))
                                        {
                                            if (key1 == 1)
                                            {
                                                _ZPNoLockMID.Add(objm.ID);
                                            }
                                            else if (key1 == 2)
                                            {
                                                _JRNoLockMID.Add(objm.ID);
                                            }
                                            lock (locklist)
                                            {
                                                Main.Temp_A_Match_Dict[this.Dict_SportsType[key2]].Add(objm);
                                            }
                                        }
                                        else
                                        {
                                            lock (locklist)
                                            {
                                                Main.Temp_A_Match_Dict[this.Dict_SportsType[key2]] = mbll.FindByDate(SourcePlatformEnum.SB.ToString(), this.Dict_SportsType[key2], now.Date.AddDays(-1));
                                            }
                                            //Main.Temp_A_Match_Dict[this.Dict_SportsType[key2]] = new ConcurrentBag<A_Match>(mbll.FindByDate(SourcePlatformEnum.SB.ToString(), this.Dict_SportsType[key2], now.Date.AddDays(-1)));
                                        }
                                    }
                                    else//更新
                                    {
                                        if (key1 == 1)
                                        {
                                            _ZPNoLockMID.Add(objm.ID);
                                        }
                                        else if (key1 == 2)
                                        {
                                            _JRNoLockMID.Add(objm.ID);
                                        }
                                        var newm = (A_Match)Utility.DeepCopy(objm);
                                        newm.LastMenuType = key1.ToString();
                                        newm.IsLock = "0";
                                        newm.ModifyTime = now;
                                        newm.SP_GameStartTime = bstime;
                                        newm.ExistLive = string.IsNullOrEmpty(m.GQ) ? "0" : "1";
                                        msg = mbll.Update(newm);
                                        if (string.IsNullOrEmpty(msg))
                                        {
                                            lock (locklist)
                                            {
                                                Main.Temp_A_Match_Dict[this.Dict_SportsType[key2]].Remove(objm);
                                                Main.Temp_A_Match_Dict[this.Dict_SportsType[key2]].Add(newm);
                                            }
                                            //Main.Temp_A_Match_Dict[this.Dict_SportsType[key2]] = new ConcurrentBag<A_Match>(Main.Temp_A_Match_Dict[this.Dict_SportsType[key2]].Where(x => x != null && x.ID != objm.ID));
                                            //Main.Temp_A_Match_Dict[this.Dict_SportsType[key2]].Add(newm);
                                        }
                                        else
                                        {
                                            lock (locklist)
                                            {
                                                Main.Temp_A_Match_Dict[this.Dict_SportsType[key2]] = mbll.FindByDate(SourcePlatformEnum.SB.ToString(), this.Dict_SportsType[key2], now.Date.AddDays(-1));
                                            }
                                            //Main.Temp_A_Match_Dict[this.Dict_SportsType[key2]] = new ConcurrentBag<A_Match>(mbll.FindByDate(SourcePlatformEnum.SB.ToString(), this.Dict_SportsType[key2], now.Date.AddDays(-1)));
                                        }
                                    }
                                    if (string.IsNullOrEmpty(msg))
                                    {
                                        O_OddsBll obll = new O_OddsBll();
                                        O_OddsRecordBll orbll = new O_OddsRecordBll();
                                        #region 全场赔率
                                        if (m.FullCourtList.Count > 0)
                                        {
                                            FillData_dyrqdxds(key2, m.FullCourtList, objlm.ID, objm.ID, now, "0", "0", ref needupdatecount, ref updatedcount, ref deletedcount, ref msgpush, ref delmsgpush);
                                        }
                                        #endregion
                                        #region 上半场赔率
                                        //if (m.HalfCourtList.Count > 0)
                                        //{
                                        FillData_dyrqdxds(key2, m.HalfCourtList, objlm.ID, objm.ID, now, "0", "1", ref needupdatecount, ref updatedcount, ref deletedcount, ref msgpush, ref delmsgpush);
                                        //}
                                        #endregion
                                        #region 半场 / 全场
                                        if (m.DoubleResult != null)
                                        {
                                            FillData_hf(key2, m.DoubleResult, objlm.ID, objm.ID, now, "0", ref needupdatecount, ref updatedcount, ref deletedcount, ref msgpush, ref delmsgpush);
                                        }
                                        #endregion
                                        #region 波胆
                                        if (m.CorrectScoreList.Count > 0)
                                        {
                                            var fullcs = m.CorrectScoreList.Where(x => x.type == 1).ToList();
                                            var fhcs = m.CorrectScoreList.Where(x => x.type == 2).ToList();
                                            if (fullcs.Count > 0)
                                            {
                                                FillData_bd(key2, fullcs, objlm.ID, objm.ID, now, "0", "", ref needupdatecount, ref updatedcount, ref deletedcount, ref msgpush, ref delmsgpush);
                                            }
                                            if (fhcs.Count > 0)
                                            {
                                                FillData_bd(key2, fhcs, objlm.ID, objm.ID, now, "0", "-上半场", ref needupdatecount, ref updatedcount, ref deletedcount, ref msgpush, ref delmsgpush);
                                            }
                                        }
                                        #endregion
                                        #region 总入球
                                        if (m.TotalGoalList.Count > 0)
                                        {
                                            var fulltg = m.TotalGoalList.Where(x => x.type == 1).ToList();
                                            var fhtg = m.TotalGoalList.Where(x => x.type == 2).ToList();
                                            if (fulltg.Count > 0)
                                            {
                                                FillData_zrq(key2, fulltg, objlm.ID, objm.ID, now, "0", "", ref needupdatecount, ref updatedcount, ref deletedcount, ref msgpush, ref delmsgpush);
                                            }
                                            if (fhtg.Count > 0)
                                            {
                                                FillData_zrq(key2, fhtg, objlm.ID, objm.ID, now, "0", "-上半场", ref needupdatecount, ref updatedcount, ref deletedcount, ref msgpush, ref delmsgpush);
                                            }
                                        }
                                        #endregion
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.ErrorLog(ex.Message, ex);
                            msgpush += ex.Message + "\r\n";
                        }
                        #endregion
                    }
                }
            }
        }
        /// <summary>
        /// 填充数据：独赢、让球、大小、单双
        /// </summary>
        /// <param name="ishf">0：全场，1：半场或小节</param>
        private void FillData_dyrqdxds(int key2, List<Odds> oddsList, string lmid, string mid, DateTime now, string IsLive, string ishf, ref int needupdatecount, ref int updatedcount, ref int deletedcount, ref string msgpush, ref string delmsgpush)
        {
            O_OddsBll obll = new O_OddsBll();
            O_OddsRecordBll orbll = new O_OddsRecordBll();
            int main = 0;
            List<string> IDlist_1x2 = new List<string>();
            List<string> IDlist_ah = new List<string>();
            List<string> IDlist_ou = new List<string>();
            List<string> IDlist_oe = new List<string>();
            List<string> IDlist_sou = new List<string>();
            List<string> IDlist_ahi = new List<string>();
            List<int> hflist = new List<int>();
            foreach (Odds item in oddsList)
            {
                main++;
                if (!hflist.Contains(item.type))
                {
                    hflist.Add(item.type);
                }
                string hf = _Arr_HF[item.type];
                if (ishf == "1")
                {
                    hf = "-" + hf;
                }
                if (key2 != 8)
                {
                    #region 独赢
                    O_Odds odds_1 = new O_Odds()
                    {
                        ID = Guid.NewGuid().ToString("N"),
                        LeagueMatchID = lmid,
                        MatchID = mid,
                        SportsType = this.Dict_SportsType[key2],
                        BetCode = Dict_S_BetCode[this.Dict_SportsType[key2]]["独赢" + hf],
                        BetExplain = "",
                        OddsSort = "1",
                        MainSort = main,
                        ReadSort = 1,
                        Odds = Utility.ObjConvertToDecimal(item.Odds_ZY),
                        IsLive = IsLive,
                        CreateTime = now,
                        ModifyTime = now,
                        SourcePlatform = SourcePlatformEnum.SB.ToString()
                    };
                    O_Odds odds_x = new O_Odds()
                    {
                        ID = Guid.NewGuid().ToString("N"),
                        LeagueMatchID = lmid,
                        MatchID = mid,
                        SportsType = this.Dict_SportsType[key2],
                        BetCode = Dict_S_BetCode[this.Dict_SportsType[key2]]["独赢" + hf],
                        BetExplain = "",
                        OddsSort = "x",
                        MainSort = main,
                        ReadSort = 2,
                        Odds = Utility.ObjConvertToDecimal(item.Odds_HJ),
                        IsLive = IsLive,
                        CreateTime = now,
                        ModifyTime = now,
                        SourcePlatform = SourcePlatformEnum.SB.ToString()
                    };
                    O_Odds odds_2 = new O_Odds()
                    {
                        ID = Guid.NewGuid().ToString("N"),
                        LeagueMatchID = lmid,
                        MatchID = mid,
                        SportsType = this.Dict_SportsType[key2],
                        BetCode = Dict_S_BetCode[this.Dict_SportsType[key2]]["独赢" + hf],
                        BetExplain = "",
                        OddsSort = "2",
                        MainSort = main,
                        ReadSort = 3,
                        Odds = Utility.ObjConvertToDecimal(item.Odds_KY),
                        IsLive = IsLive,
                        CreateTime = now,
                        ModifyTime = now,
                        SourcePlatform = SourcePlatformEnum.SB.ToString()
                    };
                    string ID1 = string.Empty, IDx = string.Empty, ID2 = string.Empty;
                    update_odds(key2, odds_1, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out ID1);
                    update_odds(key2, odds_x, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDx);
                    update_odds(key2, odds_2, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out ID2);
                    IDlist_1x2.Add(ID1);
                    IDlist_1x2.Add(IDx);
                    IDlist_1x2.Add(ID2);
                    #endregion
                }

                #region 让球
                O_Odds odds_ah1 = new O_Odds()
                {
                    ID = Guid.NewGuid().ToString("N"),
                    LeagueMatchID = lmid,
                    MatchID = mid,
                    SportsType = this.Dict_SportsType[key2],
                    BetCode = Dict_S_BetCode[this.Dict_SportsType[key2]]["让球" + hf],
                    BetExplain = "",
                    OddsSort = "1",
                    MainSort = main,
                    ReadSort = 1,
                    Odds = Utility.ObjConvertToDecimal(item.Odds_RQZY),
                    IsLive = IsLive,
                    CreateTime = now,
                    ModifyTime = now,
                    SourcePlatform = SourcePlatformEnum.SB.ToString()
                };
                O_Odds odds_ah2 = new O_Odds()
                {
                    ID = Guid.NewGuid().ToString("N"),
                    LeagueMatchID = lmid,
                    MatchID = mid,
                    SportsType = this.Dict_SportsType[key2],
                    BetCode = Dict_S_BetCode[this.Dict_SportsType[key2]]["让球" + hf],
                    BetExplain = "",
                    OddsSort = "2",
                    MainSort = main,
                    ReadSort = 2,
                    Odds = Utility.ObjConvertToDecimal(item.Odds_RQKY),
                    IsLive = IsLive,
                    CreateTime = now,
                    ModifyTime = now,
                    SourcePlatform = SourcePlatformEnum.SB.ToString()
                };

                if (!string.IsNullOrEmpty(item.Text_ZRKQ))//主让客
                {
                    string rq = item.Text_ZRKQ.Trim();
                    if (rq.Contains("/"))
                    {
                        string[] arr = rq.Split('/');
                        rq = arr[0].Trim() + "/" + arr[1].Trim();
                    }
                    odds_ah1.BetExplain = "-" + rq;
                    odds_ah2.BetExplain = "+" + rq;
                }
                else if (!string.IsNullOrEmpty(item.Text_KRZQ))//客让主
                {
                    string rq = item.Text_KRZQ.Trim();
                    if (rq.Contains("/"))
                    {
                        string[] arr = rq.Split('/');
                        rq = arr[0].Trim() + "/" + arr[1].Trim();
                    }
                    odds_ah1.BetExplain = "+" + rq;
                    odds_ah2.BetExplain = "-" + rq;
                }
                string IDah1 = string.Empty, IDah2 = string.Empty;
                update_odds(key2, odds_ah1, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDah1);
                update_odds(key2, odds_ah2, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDah2);
                IDlist_ah.Add(IDah1);
                IDlist_ah.Add(IDah2);
                #endregion

                #region 大小球
                string dxq = string.Empty;
                if (!string.IsNullOrEmpty(item.Text_DQ))
                {
                    dxq = item.Text_DQ.Replace("大", "").Replace("小", "").Trim();
                    if (dxq.Contains("/"))
                    {
                        string[] arr = dxq.Split('/');
                        dxq = arr[0].Trim() + "/" + arr[1].Trim();
                    }
                }
                O_Odds odds_ou_o = new O_Odds()
                {
                    ID = Guid.NewGuid().ToString("N"),
                    LeagueMatchID = lmid,
                    MatchID = mid,
                    SportsType = this.Dict_SportsType[key2],
                    BetCode = Dict_S_BetCode[this.Dict_SportsType[key2]]["大小" + hf],
                    BetExplain = dxq,
                    OddsSort = "o",
                    MainSort = main,
                    ReadSort = 1,
                    Odds = Utility.ObjConvertToDecimal(item.Odds_DQ),
                    IsLive = IsLive,
                    CreateTime = now,
                    ModifyTime = now,
                    SourcePlatform = SourcePlatformEnum.SB.ToString()
                };
                O_Odds odds_ou_u = new O_Odds()
                {
                    ID = Guid.NewGuid().ToString("N"),
                    LeagueMatchID = lmid,
                    MatchID = mid,
                    SportsType = this.Dict_SportsType[key2],
                    BetCode = Dict_S_BetCode[this.Dict_SportsType[key2]]["大小" + hf],
                    BetExplain = dxq,
                    OddsSort = "u",
                    MainSort = main,
                    ReadSort = 2,
                    Odds = Utility.ObjConvertToDecimal(item.Odds_XQ),
                    IsLive = IsLive,
                    CreateTime = now,
                    ModifyTime = now,
                    SourcePlatform = SourcePlatformEnum.SB.ToString()
                };
                string IDouo = string.Empty, IDouu = string.Empty;
                update_odds(key2, odds_ou_o, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDouo);
                update_odds(key2, odds_ou_u, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDouu);
                IDlist_ou.Add(IDouo);
                IDlist_ou.Add(IDouu);
                #endregion

                #region 单/双
                O_Odds odds_oe_o = new O_Odds()
                {
                    ID = Guid.NewGuid().ToString("N"),
                    LeagueMatchID = lmid,
                    MatchID = mid,
                    SportsType = this.Dict_SportsType[key2],
                    BetCode = Dict_S_BetCode[this.Dict_SportsType[key2]]["单/双" + hf],
                    BetExplain = "o",
                    OddsSort = "",
                    MainSort = main,
                    ReadSort = 1,
                    Odds = Utility.ObjConvertToDecimal(item.Odds_D),
                    IsLive = IsLive,
                    CreateTime = now,
                    ModifyTime = now,
                    SourcePlatform = SourcePlatformEnum.SB.ToString()
                };
                O_Odds odds_oe_e = new O_Odds()
                {
                    ID = Guid.NewGuid().ToString("N"),
                    LeagueMatchID = lmid,
                    MatchID = mid,
                    SportsType = this.Dict_SportsType[key2],
                    BetCode = Dict_S_BetCode[this.Dict_SportsType[key2]]["单/双" + hf],
                    BetExplain = "e",
                    OddsSort = "",
                    MainSort = main,
                    ReadSort = 2,
                    Odds = Utility.ObjConvertToDecimal(item.Odds_S),
                    IsLive = IsLive,
                    CreateTime = now,
                    ModifyTime = now,
                    SourcePlatform = SourcePlatformEnum.SB.ToString()
                };
                string IDoeo = string.Empty, IDoee = string.Empty;
                update_odds(key2, odds_oe_o, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDoeo);
                update_odds(key2, odds_oe_e, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDoee);
                IDlist_oe.Add(IDoeo);
                IDlist_oe.Add(IDoee);
                #endregion

                if (key2 == 2)//篮球-球队得分大小
                {
                    #region 球队得分大小
                    string dfdx1 = string.Empty;
                    if (!string.IsNullOrEmpty(item.Text_DQZ))
                    {
                        dfdx1 = item.Text_DQZ.Replace("大", "").Replace("小", "").Trim();
                        if (dfdx1.Contains("/"))
                        {
                            string[] arr = dfdx1.Split('/');
                            dfdx1 = arr[0].Trim() + "/" + arr[1].Trim();
                        }
                    }
                    string dfdx2 = string.Empty;
                    if (!string.IsNullOrEmpty(item.Text_DQK))
                    {
                        dfdx2 = item.Text_DQK.Replace("大", "").Replace("小", "").Trim();
                        if (dfdx2.Contains("/"))
                        {
                            string[] arr = dfdx2.Split('/');
                            dfdx2 = arr[0].Trim() + "/" + arr[1].Trim();
                        }
                    }
                    O_Odds odds_sou_1o = new O_Odds()
                    {
                        ID = Guid.NewGuid().ToString("N"),
                        LeagueMatchID = lmid,
                        MatchID = mid,
                        SportsType = this.Dict_SportsType[key2],
                        BetCode = Dict_S_BetCode[this.Dict_SportsType[key2]]["得分大小" + hf],
                        BetExplain = dfdx1,
                        OddsSort = "1o",
                        MainSort = main,
                        ReadSort = 1,
                        Odds = Utility.ObjConvertToDecimal(item.Odds_DQ),
                        IsLive = IsLive,
                        CreateTime = now,
                        ModifyTime = now,
                        SourcePlatform = SourcePlatformEnum.SB.ToString()
                    };
                    O_Odds odds_sou_1u = new O_Odds()
                    {
                        ID = Guid.NewGuid().ToString("N"),
                        LeagueMatchID = lmid,
                        MatchID = mid,
                        SportsType = this.Dict_SportsType[key2],
                        BetCode = Dict_S_BetCode[this.Dict_SportsType[key2]]["得分大小" + hf],
                        BetExplain = dfdx1,
                        OddsSort = "1u",
                        MainSort = main,
                        ReadSort = 2,
                        Odds = Utility.ObjConvertToDecimal(item.Odds_XQ),
                        IsLive = IsLive,
                        CreateTime = now,
                        ModifyTime = now,
                        SourcePlatform = SourcePlatformEnum.SB.ToString()
                    };
                    O_Odds odds_sou_2o = new O_Odds()
                    {
                        ID = Guid.NewGuid().ToString("N"),
                        LeagueMatchID = lmid,
                        MatchID = mid,
                        SportsType = this.Dict_SportsType[key2],
                        BetCode = Dict_S_BetCode[this.Dict_SportsType[key2]]["得分大小" + hf],
                        BetExplain = dfdx2,
                        OddsSort = "2o",
                        MainSort = main,
                        ReadSort = 3,
                        Odds = Utility.ObjConvertToDecimal(item.Odds_DQ),
                        IsLive = IsLive,
                        CreateTime = now,
                        ModifyTime = now,
                        SourcePlatform = SourcePlatformEnum.SB.ToString()
                    };
                    O_Odds odds_sou_2u = new O_Odds()
                    {
                        ID = Guid.NewGuid().ToString("N"),
                        LeagueMatchID = lmid,
                        MatchID = mid,
                        SportsType = this.Dict_SportsType[key2],
                        BetCode = Dict_S_BetCode[this.Dict_SportsType[key2]]["得分大小" + hf],
                        BetExplain = dfdx2,
                        OddsSort = "2u",
                        MainSort = main,
                        ReadSort = 4,
                        Odds = Utility.ObjConvertToDecimal(item.Odds_XQ),
                        IsLive = IsLive,
                        CreateTime = now,
                        ModifyTime = now,
                        SourcePlatform = SourcePlatformEnum.SB.ToString()
                    };
                    string IDsou1o = string.Empty, IDsou1u = string.Empty, IDsou2o = string.Empty, IDsou2u = string.Empty;
                    update_odds(key2, odds_sou_1o, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDsou1o);
                    update_odds(key2, odds_sou_1u, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDsou1u);
                    update_odds(key2, odds_sou_2o, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDsou2o);
                    update_odds(key2, odds_sou_2u, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDsou2u);
                    IDlist_sou.Add(IDsou1o);
                    IDlist_sou.Add(IDsou1u);
                    IDlist_sou.Add(IDsou2o);
                    IDlist_sou.Add(IDsou2u);
                    #endregion
                }
                if (key2 == 3)//网球-让局
                {
                    #region 让局
                    O_Odds odds_ahi1 = new O_Odds()
                    {
                        ID = Guid.NewGuid().ToString("N"),
                        LeagueMatchID = lmid,
                        MatchID = mid,
                        SportsType = this.Dict_SportsType[key2],
                        BetCode = Dict_S_BetCode[this.Dict_SportsType[key2]]["让局"],
                        BetExplain = "",
                        OddsSort = "1",
                        MainSort = main,
                        ReadSort = 1,
                        Odds = Utility.ObjConvertToDecimal(item.Odds_RJZY),
                        IsLive = IsLive,
                        CreateTime = now,
                        ModifyTime = now,
                        SourcePlatform = SourcePlatformEnum.SB.ToString()
                    };
                    O_Odds odds_ahi2 = new O_Odds()
                    {
                        ID = Guid.NewGuid().ToString("N"),
                        LeagueMatchID = lmid,
                        MatchID = mid,
                        SportsType = this.Dict_SportsType[key2],
                        BetCode = Dict_S_BetCode[this.Dict_SportsType[key2]]["让局"],
                        BetExplain = "",
                        OddsSort = "2",
                        MainSort = main,
                        ReadSort = 2,
                        Odds = Utility.ObjConvertToDecimal(item.Odds_RJKY),
                        IsLive = IsLive,
                        CreateTime = now,
                        ModifyTime = now,
                        SourcePlatform = SourcePlatformEnum.SB.ToString()
                    };

                    if (!string.IsNullOrEmpty(item.Text_ZRKJ))//主让客
                    {
                        string rq = item.Text_ZRKJ.Trim();
                        if (rq.Contains("/"))
                        {
                            string[] arr = rq.Split('/');
                            rq = arr[0].Trim() + "/" + arr[1].Trim();
                        }
                        odds_ahi1.BetExplain = "-" + rq;
                        odds_ahi2.BetExplain = "+" + rq;
                    }
                    else if (!string.IsNullOrEmpty(item.Text_KRZJ))//客让主
                    {
                        string rq = item.Text_KRZJ.Trim();
                        if (rq.Contains("/"))
                        {
                            string[] arr = rq.Split('/');
                            rq = arr[0].Trim() + "/" + arr[1].Trim();
                        }
                        odds_ahi1.BetExplain = "+" + rq;
                        odds_ahi2.BetExplain = "-" + rq;
                    }
                    string IDahi1 = string.Empty, IDahi2 = string.Empty;
                    update_odds(key2, odds_ahi1, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDahi1);
                    update_odds(key2, odds_ahi2, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDahi2);
                    IDlist_ahi.Add(IDahi1);
                    IDlist_ahi.Add(IDahi2);
                    #endregion
                }
            }
            string where = "'xxxx1st','xxxx2nd','xxxx1q','xxxx2q','xxxx3q','xxxx4q'";
            string sql = string.Empty;
            string delmsg = string.Empty;
            int delcount = 0;
            if (key2 != 8)
            {
                //独赢
                StringBuilder ids1x2 = new StringBuilder();
                ids1x2.Append("'',");
                foreach (string item in IDlist_1x2)
                {
                    ids1x2.Append($"'{item}',");
                }
                if (ishf == "1")//非全场
                {
                    sql = $"delete O_Odds where LeagueMatchID='{lmid}' and MatchID='{mid}' and BetCode in({where.Replace("xxxx", Dict_S_BetCode[this.Dict_SportsType[key2]]["独赢"])}) and IsLive='{IsLive}' and SourcePlatform='{SourcePlatformEnum.SB.ToString()}' and SportsType='{this.Dict_SportsType[key2]}' and ID not in({ids1x2.ToString().TrimEnd(',')})";
                }
                else
                {
                    sql = $"delete O_Odds where LeagueMatchID='{lmid}' and MatchID='{mid}' and BetCode='{Dict_S_BetCode[this.Dict_SportsType[key2]]["独赢"]}' and IsLive='{IsLive}' and SourcePlatform='{SourcePlatformEnum.SB.ToString()}' and SportsType='{this.Dict_SportsType[key2]}' and ID not in({ids1x2.ToString().TrimEnd(',')})";
                }
                delmsg = new O_OddsBll().ExecuteBySQL(sql, out delcount);
                if (string.IsNullOrEmpty(delmsg))
                {
                    deletedcount += delcount;
                    foreach (int item in hflist)
                    {
                        string hf = _Arr_HF[item];
                        if (ishf == "1")
                        {
                            hf = "-" + hf;
                        }
                        lock (locklist)
                        {
                            Main.Temp_O_Odds_Dict[this.Dict_SportsType[key2]][Dict_S_BetCode[this.Dict_SportsType[key2]]["独赢" + hf]].RemoveAll(x =>
                            x.LeagueMatchID == lmid &&
                            x.MatchID == mid &&
                            x.BetCode == Dict_S_BetCode[this.Dict_SportsType[key2]]["独赢" + hf] &&
                            x.IsLive == IsLive &&
                            (!IDlist_1x2.Contains(x.ID)));
                        }

                        //Main.Temp_O_Odds_Dict[this.Dict_SportsType[key2]][Dict_S_BetCode[this.Dict_SportsType[key2]]["独赢" + hf]] = new ConcurrentBag<O_Odds>(Main.Temp_O_Odds_Dict[this.Dict_SportsType[key2]][Dict_S_BetCode[this.Dict_SportsType[key2]]["独赢" + hf]].Where(x =>
                        //x.LeagueMatchID == lmid &&
                        //x.MatchID == mid &&
                        //x.BetCode == Dict_S_BetCode[this.Dict_SportsType[key2]]["独赢" + hf] &&
                        //x.IsLive == IsLive &&
                        //IDlist_1x2.Contains(x.ID)));
                    }
                }
                else
                {
                    delmsgpush += delmsg + "\r\n" + sql + "\r\n";
                }
            }
            //让球
            StringBuilder idsah = new StringBuilder();
            idsah.Append("'',");
            foreach (string item in IDlist_ah)
            {
                idsah.Append($"'{item}',");
            }
            if (ishf == "1")//非全场
            {
                sql = $"delete O_Odds where LeagueMatchID='{lmid}' and MatchID='{mid}' and BetCode in({where.Replace("xxxx", Dict_S_BetCode[this.Dict_SportsType[key2]]["让球"])}) and IsLive='{IsLive}' and SourcePlatform='{SourcePlatformEnum.SB.ToString()}' and SportsType='{this.Dict_SportsType[key2]}' and ID not in({idsah.ToString().TrimEnd(',')})";
            }
            else
            {
                sql = $"delete O_Odds where LeagueMatchID='{lmid}' and MatchID='{mid}' and BetCode='{Dict_S_BetCode[this.Dict_SportsType[key2]]["让球"]}' and IsLive='{IsLive}' and SourcePlatform='{SourcePlatformEnum.SB.ToString()}' and SportsType='{this.Dict_SportsType[key2]}' and ID not in({idsah.ToString().TrimEnd(',')})";
            }
            delmsg = new O_OddsBll().ExecuteBySQL(sql, out delcount);
            if (string.IsNullOrEmpty(delmsg))
            {
                deletedcount += delcount;
                foreach (int item in hflist)
                {
                    string hf = _Arr_HF[item];
                    if (ishf == "1")
                    {
                        hf = "-" + hf;
                    }
                    lock (locklist)
                    {
                        Main.Temp_O_Odds_Dict[this.Dict_SportsType[key2]][Dict_S_BetCode[this.Dict_SportsType[key2]]["让球" + hf]].RemoveAll(x =>
                        x.LeagueMatchID == lmid &&
                        x.MatchID == mid &&
                        x.BetCode == Dict_S_BetCode[this.Dict_SportsType[key2]]["让球" + hf] &&
                        x.IsLive == IsLive &&
                        (!IDlist_ah.Contains(x.ID)));
                    }

                    //Main.Temp_O_Odds_Dict[this.Dict_SportsType[key2]][Dict_S_BetCode[this.Dict_SportsType[key2]]["让球" + hf]] = new ConcurrentBag<O_Odds>(Main.Temp_O_Odds_Dict[this.Dict_SportsType[key2]][Dict_S_BetCode[this.Dict_SportsType[key2]]["让球" + hf]].Where(x =>
                    //x.LeagueMatchID == lmid &&
                    //x.MatchID == mid &&
                    //x.BetCode == Dict_S_BetCode[this.Dict_SportsType[key2]]["让球" + hf] &&
                    //x.IsLive == IsLive &&
                    //IDlist_ah.Contains(x.ID)));
                }
            }
            else
            {
                delmsgpush += delmsg + "\r\n" + sql + "\r\n";
            }
            //大小
            StringBuilder idsou = new StringBuilder();
            idsou.Append("'',");
            foreach (string item in IDlist_ou)
            {
                idsou.Append($"'{item}',");
            }
            if (ishf == "1")//非全场
            {
                sql = $"delete O_Odds where LeagueMatchID='{lmid}' and MatchID='{mid}' and BetCode in({where.Replace("xxxx", Dict_S_BetCode[this.Dict_SportsType[key2]]["大小"])}) and IsLive='{IsLive}' and SourcePlatform='{SourcePlatformEnum.SB.ToString()}' and SportsType='{this.Dict_SportsType[key2]}' and ID not in({idsou.ToString().TrimEnd(',')})";
            }
            else
            {
                sql = $"delete O_Odds where LeagueMatchID='{lmid}' and MatchID='{mid}' and BetCode='{Dict_S_BetCode[this.Dict_SportsType[key2]]["大小"]}' and IsLive='{IsLive}' and SourcePlatform='{SourcePlatformEnum.SB.ToString()}' and SportsType='{this.Dict_SportsType[key2]}' and ID not in({idsou.ToString().TrimEnd(',')})";
            }
            delmsg = new O_OddsBll().ExecuteBySQL(sql, out delcount);
            if (string.IsNullOrEmpty(delmsg))
            {
                deletedcount += delcount;
                foreach (int item in hflist)
                {
                    string hf = _Arr_HF[item];
                    if (ishf == "1")
                    {
                        hf = "-" + hf;
                    }
                    lock (locklist)
                    {
                        Main.Temp_O_Odds_Dict[this.Dict_SportsType[key2]][Dict_S_BetCode[this.Dict_SportsType[key2]]["大小" + hf]].RemoveAll(x =>
                        x.LeagueMatchID == lmid &&
                        x.MatchID == mid &&
                        x.BetCode == Dict_S_BetCode[this.Dict_SportsType[key2]]["大小" + hf] &&
                        x.IsLive == IsLive &&
                        (!IDlist_ou.Contains(x.ID)));
                    }

                    //Main.Temp_O_Odds_Dict[this.Dict_SportsType[key2]][Dict_S_BetCode[this.Dict_SportsType[key2]]["大小" + hf]] = new ConcurrentBag<O_Odds>(Main.Temp_O_Odds_Dict[this.Dict_SportsType[key2]][Dict_S_BetCode[this.Dict_SportsType[key2]]["大小" + hf]].Where(x =>
                    //x.LeagueMatchID == lmid &&
                    //x.MatchID == mid &&
                    //x.BetCode == Dict_S_BetCode[this.Dict_SportsType[key2]]["大小" + hf] &&
                    //x.IsLive == IsLive &&
                    //IDlist_ou.Contains(x.ID)));
                }
            }
            else
            {
                delmsgpush += delmsg + "\r\n" + sql + "\r\n";
            }
            //单/双
            StringBuilder idsoe = new StringBuilder();
            idsoe.Append("'',");
            foreach (string item in IDlist_oe)
            {
                idsoe.Append($"'{item}',");
            }
            if (ishf == "1")//非全场
            {
                sql = $"delete O_Odds where LeagueMatchID='{lmid}' and MatchID='{mid}' and BetCode in({where.Replace("xxxx", Dict_S_BetCode[this.Dict_SportsType[key2]]["单/双"])}) and IsLive='{IsLive}' and SourcePlatform='{SourcePlatformEnum.SB.ToString()}' and SportsType='{this.Dict_SportsType[key2]}' and ID not in({idsoe.ToString().TrimEnd(',')})";
            }
            else
            {
                sql = $"delete O_Odds where LeagueMatchID='{lmid}' and MatchID='{mid}' and BetCode='{Dict_S_BetCode[this.Dict_SportsType[key2]]["单/双"]}' and IsLive='{IsLive}' and SourcePlatform='{SourcePlatformEnum.SB.ToString()}' and SportsType='{this.Dict_SportsType[key2]}' and ID not in({idsoe.ToString().TrimEnd(',')})";
            }
            delmsg = new O_OddsBll().ExecuteBySQL(sql, out delcount);
            if (string.IsNullOrEmpty(delmsg))
            {
                deletedcount += delcount;
                foreach (int item in hflist)
                {
                    string hf = _Arr_HF[item];
                    if (ishf == "1")
                    {
                        hf = "-" + hf;
                    }
                    lock (locklist)
                    {
                        Main.Temp_O_Odds_Dict[this.Dict_SportsType[key2]][Dict_S_BetCode[this.Dict_SportsType[key2]]["单/双" + hf]].RemoveAll(x =>
                        x.LeagueMatchID == lmid &&
                        x.MatchID == mid &&
                        x.BetCode == Dict_S_BetCode[this.Dict_SportsType[key2]]["单/双" + hf] &&
                        x.IsLive == IsLive &&
                        (!IDlist_oe.Contains(x.ID)));
                    }

                    //Main.Temp_O_Odds_Dict[this.Dict_SportsType[key2]][Dict_S_BetCode[this.Dict_SportsType[key2]]["单/双" + hf]] = new ConcurrentBag<O_Odds>(Main.Temp_O_Odds_Dict[this.Dict_SportsType[key2]][Dict_S_BetCode[this.Dict_SportsType[key2]]["单/双" + hf]].Where(x =>
                    //x.LeagueMatchID == lmid &&
                    //x.MatchID == mid &&
                    //x.BetCode == Dict_S_BetCode[this.Dict_SportsType[key2]]["单/双" + hf] &&
                    //x.IsLive == IsLive &&
                    //IDlist_oe.Contains(x.ID)));
                }
            }
            else
            {
                delmsgpush += delmsg + "\r\n" + sql + "\r\n";
            }
            if (key2 == 2)
            {
                //得分大小
                StringBuilder idssou = new StringBuilder();
                idssou.Append("'',");
                foreach (string item in IDlist_sou)
                {
                    idssou.Append($"'{item}',");
                }
                if (ishf == "1")//非全场
                {
                    sql = $"delete O_Odds where LeagueMatchID='{lmid}' and MatchID='{mid}' and BetCode in({where.Replace("xxxx", Dict_S_BetCode[this.Dict_SportsType[key2]]["得分大小"])}) and IsLive='{IsLive}' and SourcePlatform='{SourcePlatformEnum.SB.ToString()}' and SportsType='{this.Dict_SportsType[key2]}' and ID not in({idssou.ToString().TrimEnd(',')})";
                }
                else
                {
                    sql = $"delete O_Odds where LeagueMatchID='{lmid}' and MatchID='{mid}' and BetCode='{Dict_S_BetCode[this.Dict_SportsType[key2]]["得分大小"]}' and IsLive='{IsLive}' and SourcePlatform='{SourcePlatformEnum.SB.ToString()}' and SportsType='{this.Dict_SportsType[key2]}' and ID not in({idssou.ToString().TrimEnd(',')})";
                }
                delmsg = new O_OddsBll().ExecuteBySQL(sql, out delcount);
                if (string.IsNullOrEmpty(delmsg))
                {
                    deletedcount += delcount;
                    foreach (int item in hflist)
                    {
                        string hf = _Arr_HF[item];
                        if (ishf == "1")
                        {
                            hf = "-" + hf;
                        }
                        lock (locklist)
                        {
                            Main.Temp_O_Odds_Dict[this.Dict_SportsType[key2]][Dict_S_BetCode[this.Dict_SportsType[key2]]["得分大小" + hf]].RemoveAll(x =>
                            x.LeagueMatchID == lmid &&
                            x.MatchID == mid &&
                            x.BetCode == Dict_S_BetCode[this.Dict_SportsType[key2]]["得分大小" + hf] &&
                            x.IsLive == IsLive &&
                            (!IDlist_sou.Contains(x.ID)));
                        }

                        //Main.Temp_O_Odds_Dict[this.Dict_SportsType[key2]][Dict_S_BetCode[this.Dict_SportsType[key2]]["得分大小" + hf]] = new ConcurrentBag<O_Odds>(Main.Temp_O_Odds_Dict[this.Dict_SportsType[key2]][Dict_S_BetCode[this.Dict_SportsType[key2]]["得分大小" + hf]].Where(x =>
                        //x.LeagueMatchID == lmid &&
                        //x.MatchID == mid &&
                        //x.BetCode == Dict_S_BetCode[this.Dict_SportsType[key2]]["得分大小" + hf] &&
                        //x.IsLive == IsLive &&
                        //IDlist_sou.Contains(x.ID)));
                    }
                }
                else
                {
                    delmsgpush += delmsg + "\r\n" + sql + "\r\n";
                }
            }
            if (key2 == 3)
            {
                //让局
                StringBuilder idsahi = new StringBuilder();
                idsahi.Append("'',");
                foreach (string item in IDlist_ahi)
                {
                    idsahi.Append($"'{item}',");
                }
                sql = $"delete O_Odds where LeagueMatchID='{lmid}' and MatchID='{mid}' and BetCode='{Dict_S_BetCode[this.Dict_SportsType[key2]]["让局"]}' and IsLive='{IsLive}' and SourcePlatform='{SourcePlatformEnum.SB.ToString()}' and SportsType='{this.Dict_SportsType[key2]}' and ID not in({idsahi.ToString().TrimEnd(',')})";
                delmsg = new O_OddsBll().ExecuteBySQL(sql, out delcount);
                if (string.IsNullOrEmpty(delmsg))
                {
                    deletedcount += delcount;
                    lock (locklist)
                    {
                        Main.Temp_O_Odds_Dict[this.Dict_SportsType[key2]][Dict_S_BetCode[this.Dict_SportsType[key2]]["让局"]].RemoveAll(x =>
                        x.LeagueMatchID == lmid &&
                        x.MatchID == mid &&
                        x.BetCode == Dict_S_BetCode[this.Dict_SportsType[key2]]["让局"] &&
                        x.IsLive == IsLive &&
                        (!IDlist_sou.Contains(x.ID)));
                    }

                    //Main.Temp_O_Odds_Dict[this.Dict_SportsType[key2]][Dict_S_BetCode[this.Dict_SportsType[key2]]["让局"]] = new ConcurrentBag<O_Odds>(Main.Temp_O_Odds_Dict[this.Dict_SportsType[key2]][Dict_S_BetCode[this.Dict_SportsType[key2]]["让局"]].Where(x =>
                    //x.LeagueMatchID == lmid &&
                    //x.MatchID == mid &&
                    //x.BetCode == Dict_S_BetCode[this.Dict_SportsType[key2]]["让局"] &&
                    //x.IsLive == IsLive &&
                    //IDlist_sou.Contains(x.ID)));
                }
                else
                {
                    delmsgpush += delmsg + "\r\n" + sql + "\r\n";
                }
            }
        }
        /// <summary>
        /// 填充数据：半场/全场
        /// </summary>
        private void FillData_hf(int key2, OddsBCQC doubleresult, string lmid, string mid, DateTime now, string IsLive, ref int needupdatecount, ref int updatedcount, ref int deletedcount, ref string msgpush, ref string delmsgpush)
        {
            O_OddsBll obll = new O_OddsBll();
            O_OddsRecordBll orbll = new O_OddsRecordBll();
            List<string> IDlist_hf = new List<string>();
            O_Odds odds_hf_11 = new O_Odds()
            {
                ID = Guid.NewGuid().ToString("N"),
                LeagueMatchID = lmid,
                MatchID = mid,
                SportsType = this.Dict_SportsType[key2],
                BetCode = Dict_S_BetCode[this.Dict_SportsType[key2]]["半场/全场"],
                BetExplain = "11",
                OddsSort = "",
                MainSort = 1,
                Odds = Utility.ObjConvertToDecimal(doubleresult.Odds_HH),
                IsLive = IsLive,
                CreateTime = now,
                ModifyTime = now,
                SourcePlatform = SourcePlatformEnum.SB.ToString()
            };
            O_Odds odds_hf_1x = new O_Odds()
            {
                ID = Guid.NewGuid().ToString("N"),
                LeagueMatchID = lmid,
                MatchID = mid,
                SportsType = this.Dict_SportsType[key2],
                BetCode = Dict_S_BetCode[this.Dict_SportsType[key2]]["半场/全场"],
                BetExplain = "1x",
                OddsSort = "",
                MainSort = 1,
                Odds = Utility.ObjConvertToDecimal(doubleresult.Odds_HD),
                IsLive = IsLive,
                CreateTime = now,
                ModifyTime = now,
                SourcePlatform = SourcePlatformEnum.SB.ToString()
            };
            O_Odds odds_hf_12 = new O_Odds()
            {
                ID = Guid.NewGuid().ToString("N"),
                LeagueMatchID = lmid,
                MatchID = mid,
                SportsType = this.Dict_SportsType[key2],
                BetCode = Dict_S_BetCode[this.Dict_SportsType[key2]]["半场/全场"],
                BetExplain = "12",
                OddsSort = "",
                MainSort = 1,
                Odds = Utility.ObjConvertToDecimal(doubleresult.Odds_HV),
                IsLive = IsLive,
                CreateTime = now,
                ModifyTime = now,
                SourcePlatform = SourcePlatformEnum.SB.ToString()
            };
            O_Odds odds_hf_x1 = new O_Odds()
            {
                ID = Guid.NewGuid().ToString("N"),
                LeagueMatchID = lmid,
                MatchID = mid,
                SportsType = this.Dict_SportsType[key2],
                BetCode = Dict_S_BetCode[this.Dict_SportsType[key2]]["半场/全场"],
                BetExplain = "x1",
                OddsSort = "",
                MainSort = 1,
                Odds = Utility.ObjConvertToDecimal(doubleresult.Odds_DH),
                IsLive = IsLive,
                CreateTime = now,
                ModifyTime = now,
                SourcePlatform = SourcePlatformEnum.SB.ToString()
            };
            O_Odds odds_hf_xx = new O_Odds()
            {
                ID = Guid.NewGuid().ToString("N"),
                LeagueMatchID = lmid,
                MatchID = mid,
                SportsType = this.Dict_SportsType[key2],
                BetCode = Dict_S_BetCode[this.Dict_SportsType[key2]]["半场/全场"],
                BetExplain = "xx",
                OddsSort = "",
                MainSort = 1,
                Odds = Utility.ObjConvertToDecimal(doubleresult.Odds_DD),
                IsLive = IsLive,
                CreateTime = now,
                ModifyTime = now,
                SourcePlatform = SourcePlatformEnum.SB.ToString()
            };
            O_Odds odds_hf_x2 = new O_Odds()
            {
                ID = Guid.NewGuid().ToString("N"),
                LeagueMatchID = lmid,
                MatchID = mid,
                SportsType = this.Dict_SportsType[key2],
                BetCode = Dict_S_BetCode[this.Dict_SportsType[key2]]["半场/全场"],
                BetExplain = "x2",
                OddsSort = "",
                MainSort = 1,
                Odds = Utility.ObjConvertToDecimal(doubleresult.Odds_DV),
                IsLive = IsLive,
                CreateTime = now,
                ModifyTime = now,
                SourcePlatform = SourcePlatformEnum.SB.ToString()
            };
            O_Odds odds_hf_21 = new O_Odds()
            {
                ID = Guid.NewGuid().ToString("N"),
                LeagueMatchID = lmid,
                MatchID = mid,
                SportsType = this.Dict_SportsType[key2],
                BetCode = Dict_S_BetCode[this.Dict_SportsType[key2]]["半场/全场"],
                BetExplain = "21",
                OddsSort = "",
                MainSort = 1,
                Odds = Utility.ObjConvertToDecimal(doubleresult.Odds_VH),
                IsLive = IsLive,
                CreateTime = now,
                ModifyTime = now,
                SourcePlatform = SourcePlatformEnum.SB.ToString()
            };
            O_Odds odds_hf_2x = new O_Odds()
            {
                ID = Guid.NewGuid().ToString("N"),
                LeagueMatchID = lmid,
                MatchID = mid,
                SportsType = this.Dict_SportsType[key2],
                BetCode = Dict_S_BetCode[this.Dict_SportsType[key2]]["半场/全场"],
                BetExplain = "2x",
                OddsSort = "",
                MainSort = 1,
                Odds = Utility.ObjConvertToDecimal(doubleresult.Odds_VD),
                IsLive = IsLive,
                CreateTime = now,
                ModifyTime = now,
                SourcePlatform = SourcePlatformEnum.SB.ToString()
            };
            O_Odds odds_hf_22 = new O_Odds()
            {
                ID = Guid.NewGuid().ToString("N"),
                LeagueMatchID = lmid,
                MatchID = mid,
                SportsType = this.Dict_SportsType[key2],
                BetCode = Dict_S_BetCode[this.Dict_SportsType[key2]]["半场/全场"],
                BetExplain = "22",
                OddsSort = "",
                MainSort = 1,
                Odds = Utility.ObjConvertToDecimal(doubleresult.Odds_VV),
                IsLive = IsLive,
                CreateTime = now,
                ModifyTime = now,
                SourcePlatform = SourcePlatformEnum.SB.ToString()
            };
            string ID11 = string.Empty, ID1x = string.Empty, ID12 = string.Empty, IDx1 = string.Empty, IDxx = string.Empty, IDx2 = string.Empty, ID21 = string.Empty, ID2x = string.Empty, ID22 = string.Empty;
            update_odds(key2, odds_hf_11, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out ID11);
            update_odds(key2, odds_hf_1x, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out ID1x);
            update_odds(key2, odds_hf_12, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out ID12);
            update_odds(key2, odds_hf_x1, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDx1);
            update_odds(key2, odds_hf_xx, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDxx);
            update_odds(key2, odds_hf_x2, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDx2);
            update_odds(key2, odds_hf_21, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out ID21);
            update_odds(key2, odds_hf_2x, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out ID2x);
            update_odds(key2, odds_hf_22, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out ID22);
            IDlist_hf.Add(ID11);
            IDlist_hf.Add(ID1x);
            IDlist_hf.Add(ID12);
            IDlist_hf.Add(IDx1);
            IDlist_hf.Add(IDxx);
            IDlist_hf.Add(IDx2);
            IDlist_hf.Add(ID21);
            IDlist_hf.Add(ID2x);
            IDlist_hf.Add(ID22);
            //半场/全场
            StringBuilder idshf = new StringBuilder();
            foreach (string item in IDlist_hf)
            {
                idshf.Append($"'{item}',");
            }
            int delcount = 0;
            string sql = $"delete O_Odds where LeagueMatchID='{lmid}' and MatchID='{mid}' and BetCode='{Dict_S_BetCode[this.Dict_SportsType[key2]]["半场/全场"]}' and IsLive='{IsLive}' and SourcePlatform='{SourcePlatformEnum.SB.ToString()}' and SportsType='{this.Dict_SportsType[key2]}' and ID not in({idshf.ToString().TrimEnd(',')})";
            string delmsg = new O_OddsBll().ExecuteBySQL(sql, out delcount);
            if (string.IsNullOrEmpty(delmsg))
            {
                deletedcount += delcount;
                lock (locklist)
                {
                    Main.Temp_O_Odds_Dict[this.Dict_SportsType[key2]][Dict_S_BetCode[this.Dict_SportsType[key2]]["半场/全场"]].RemoveAll(x =>
                    x.LeagueMatchID == lmid &&
                    x.MatchID == mid &&
                    x.BetCode == Dict_S_BetCode[this.Dict_SportsType[key2]]["半场/全场"] &&
                    x.IsLive == IsLive &&
                    (!IDlist_hf.Contains(x.ID)));
                }

                //Main.Temp_O_Odds_Dict[this.Dict_SportsType[key2]][Dict_S_BetCode[this.Dict_SportsType[key2]]["半场/全场"]] = new ConcurrentBag<O_Odds>(Main.Temp_O_Odds_Dict[this.Dict_SportsType[key2]][Dict_S_BetCode[this.Dict_SportsType[key2]]["半场/全场"]].Where(x =>
                //x.LeagueMatchID == lmid &&
                //x.MatchID == mid &&
                //x.BetCode == Dict_S_BetCode[this.Dict_SportsType[key2]]["半场/全场"] &&
                //x.IsLive == IsLive &&
                //IDlist_hf.Contains(x.ID)));
            }
            else
            {
                delmsgpush += delmsg + "\r\n" + sql + "\r\n";
            }
        }
        /// <summary>
        /// 填充数据：波胆
        /// </summary>
        private void FillData_bd(int key2, List<OddsBD> oddsList, string lmid, string mid, DateTime now, string IsLive, string hf, ref int needupdatecount, ref int updatedcount, ref int deletedcount, ref string msgpush, ref string delmsgpush)
        {
            O_OddsBll obll = new O_OddsBll();
            O_OddsRecordBll orbll = new O_OddsRecordBll();
            List<string> IDlist_cs = new List<string>();
            foreach (OddsBD item in oddsList)
            {
                O_Odds odds_cs = new O_Odds()
                {
                    ID = Guid.NewGuid().ToString("N"),
                    LeagueMatchID = lmid,
                    MatchID = mid,
                    SportsType = this.Dict_SportsType[key2],
                    BetCode = Dict_S_BetCode[this.Dict_SportsType[key2]]["波胆" + hf],
                    BetExplain = item.Text_H + "-" + item.Text_V,
                    OddsSort = "",
                    MainSort = 1,
                    Odds = Utility.ObjConvertToDecimal(item.Odds_BD),
                    IsLive = IsLive,
                    CreateTime = now,
                    ModifyTime = now,
                    SourcePlatform = SourcePlatformEnum.SB.ToString()
                };
                string IDcs = string.Empty;
                update_odds(key2, odds_cs, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDcs);
                IDlist_cs.Add(IDcs);
            }
            //波胆
            StringBuilder idscs = new StringBuilder();
            foreach (string item in IDlist_cs)
            {
                idscs.Append($"'{item}',");
            }
            int delcount = 0;
            string sql = $"delete O_Odds where LeagueMatchID='{lmid}' and MatchID='{mid}' and BetCode='{Dict_S_BetCode[this.Dict_SportsType[key2]]["波胆" + hf]}' and IsLive='{IsLive}' and SourcePlatform='{SourcePlatformEnum.SB.ToString()}' and SportsType='{this.Dict_SportsType[key2]}' and ID not in({idscs.ToString().TrimEnd(',')})";
            string delmsg = new O_OddsBll().ExecuteBySQL(sql, out delcount);
            if (string.IsNullOrEmpty(delmsg))
            {
                deletedcount += delcount;
                lock (locklist)
                {
                    Main.Temp_O_Odds_Dict[this.Dict_SportsType[key2]][Dict_S_BetCode[this.Dict_SportsType[key2]]["波胆" + hf]].RemoveAll(x =>
                    x.LeagueMatchID == lmid &&
                    x.MatchID == mid &&
                    x.BetCode == Dict_S_BetCode[this.Dict_SportsType[key2]]["波胆" + hf] &&
                    x.IsLive == IsLive &&
                    (!IDlist_cs.Contains(x.ID)));
                }

                //Main.Temp_O_Odds_Dict[this.Dict_SportsType[key2]][Dict_S_BetCode[this.Dict_SportsType[key2]]["波胆" + hf]] = new ConcurrentBag<O_Odds>(Main.Temp_O_Odds_Dict[this.Dict_SportsType[key2]][Dict_S_BetCode[this.Dict_SportsType[key2]]["波胆" + hf]].Where(x =>
                //x.LeagueMatchID == lmid &&
                //x.MatchID == mid &&
                //x.BetCode == Dict_S_BetCode[this.Dict_SportsType[key2]]["波胆" + hf] &&
                //x.IsLive == IsLive &&
                //IDlist_cs.Contains(x.ID)));
            }
            else
            {
                delmsgpush += delmsg + "\r\n" + sql + "\r\n";
            }

        }
        /// <summary>
        /// 填充数据：总入球
        /// </summary>
        private void FillData_zrq(int key2, List<OddsZRQ> oddsList, string lmid, string mid, DateTime now, string IsLive, string hf, ref int needupdatecount, ref int updatedcount, ref int deletedcount, ref string msgpush, ref string delmsgpush)
        {
            O_OddsBll obll = new O_OddsBll();
            O_OddsRecordBll orbll = new O_OddsRecordBll();
            List<string> IDlist_tg = new List<string>();
            foreach (OddsZRQ item in oddsList)
            {
                O_Odds odds_tg = new O_Odds()
                {
                    ID = Guid.NewGuid().ToString("N"),
                    LeagueMatchID = lmid,
                    MatchID = mid,
                    SportsType = this.Dict_SportsType[key2],
                    BetCode = Dict_S_BetCode[this.Dict_SportsType[key2]]["总进球数" + hf],
                    BetExplain = item.Text_Goals,
                    OddsSort = "",
                    MainSort = 1,
                    Odds = Utility.ObjConvertToDecimal(item.Odds_ZRQ),
                    IsLive = IsLive,
                    CreateTime = now,
                    ModifyTime = now,
                    SourcePlatform = SourcePlatformEnum.SB.ToString()
                };
                string IDtg = string.Empty;
                update_odds(key2, odds_tg, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDtg);
                IDlist_tg.Add(IDtg);
            }
            //总进球数
            StringBuilder idstg = new StringBuilder();
            foreach (string item in IDlist_tg)
            {
                idstg.Append($"'{item}',");
            }
            int delcount = 0;
            string sql = $"delete O_Odds where LeagueMatchID='{lmid}' and MatchID='{mid}' and BetCode='{Dict_S_BetCode[this.Dict_SportsType[key2]]["总进球数" + hf]}' and IsLive='{IsLive}' and SourcePlatform='{SourcePlatformEnum.SB.ToString()}' and SportsType='{this.Dict_SportsType[key2]}' and ID not in({idstg.ToString().TrimEnd(',')})";
            string delmsg = new O_OddsBll().ExecuteBySQL(sql, out delcount);
            if (string.IsNullOrEmpty(delmsg))
            {
                deletedcount += delcount;
                lock (locklist)
                {
                    Main.Temp_O_Odds_Dict[this.Dict_SportsType[key2]][Dict_S_BetCode[this.Dict_SportsType[key2]]["总进球数" + hf]].RemoveAll(x =>
                    x.LeagueMatchID == lmid &&
                    x.MatchID == mid &&
                    x.BetCode == Dict_S_BetCode[this.Dict_SportsType[key2]]["总进球数" + hf] &&
                    x.IsLive == IsLive &&
                    (!IDlist_tg.Contains(x.ID)));
                }

                //Main.Temp_O_Odds_Dict[this.Dict_SportsType[key2]][Dict_S_BetCode[this.Dict_SportsType[key2]]["总进球数" + hf]] = new ConcurrentBag<O_Odds>(Main.Temp_O_Odds_Dict[this.Dict_SportsType[key2]][Dict_S_BetCode[this.Dict_SportsType[key2]]["总进球数" + hf]].Where(x =>
                //x.LeagueMatchID == lmid &&
                //x.MatchID == mid &&
                //x.BetCode == Dict_S_BetCode[this.Dict_SportsType[key2]]["总进球数" + hf] &&
                //x.IsLive == IsLive &&
                //IDlist_tg.Contains(x.ID)));
            }
            else
            {
                delmsgpush += delmsg + "\r\n" + sql + "\r\n";
            }


        }
        /// <summary>
        /// 
        /// </summary>
        private void update_odds(int key2, O_Odds odds, DateTime now, O_OddsBll obll, O_OddsRecordBll orbll, ref int needupdatecount, ref int updatedcount, ref string msgpush, out string ID)
        {
            ID = "";
            string msg = string.Empty;
            O_Odds lastodds = null;
            lock (locklist)
            {
                lastodds = Main.Temp_O_Odds_Dict[this.Dict_SportsType[key2]][odds.BetCode].Where(x => x != null && x.LeagueMatchID == odds.LeagueMatchID && x.MatchID == odds.MatchID && x.BetCode == odds.BetCode && x.BetExplain == odds.BetExplain && x.OddsSort == odds.OddsSort && x.MainSort == odds.MainSort).OrderByDescending(x => x.ModifyTime).FirstOrDefault();
            }
            O_OddsRecord oddsrecord = new O_OddsRecord()
            {
                ID = Guid.NewGuid().ToString("N"),
                LeagueMatchID = odds.LeagueMatchID,
                MatchID = odds.MatchID,
                SportsType = this.Dict_SportsType[key2],
                BetCode = odds.BetCode,
                BetExplain = odds.BetExplain,
                OddsSort = odds.OddsSort,
                MainSort = odds.MainSort,
                Odds = odds.Odds,
                IsLive = odds.IsLive,
                CreateTime = odds.CreateTime,
                SourcePlatform = SourcePlatformEnum.SB.ToString()
            };
            if (lastodds == null)
            {
                needupdatecount++;
                odds.LastOdds = odds.Odds;
                msg = obll.Create(odds);
                if (string.IsNullOrEmpty(msg))
                {
                    ID = odds.ID;
                    updatedcount++;
                    lock (locklist)
                    {
                        Main.Temp_O_Odds_Dict[this.Dict_SportsType[key2]][odds.BetCode].Add(odds);
                    }
                }
                else
                {
                    msgpush += msg + "\r\n";
                }
                msg = orbll.Create(oddsrecord);
                if (!string.IsNullOrEmpty(msg))
                {
                    msgpush += msg + "\r\n";
                }
            }
            else
            {
                ID = lastodds.ID;
                if (lastodds.Odds != odds.Odds)
                {
                    needupdatecount++;
                    var newo = (O_Odds)Utility.DeepCopy(lastodds);
                    newo.LastOdds = newo.Odds;
                    newo.Odds = odds.Odds;
                    newo.IsLive = odds.IsLive;
                    newo.ModifyTime = now;
                    msg = obll.Update(newo);
                    if (string.IsNullOrEmpty(msg))
                    {
                        updatedcount++;
                        lock (locklist)
                        {                            
                            Main.Temp_O_Odds_Dict[this.Dict_SportsType[key2]][odds.BetCode].Remove(lastodds);
                            Main.Temp_O_Odds_Dict[this.Dict_SportsType[key2]][odds.BetCode].Add(newo);
                        }
                        msg = orbll.Create(oddsrecord);
                        if (!string.IsNullOrEmpty(msg))
                        {
                            msgpush += msg + "\r\n";
                        }
                    }
                    else
                    {
                        msgpush += msg + "\r\n";
                        if (msg.Contains("Store update, insert, or delete") )//"Store update, insert, or delete statement affected an unexpected number of rows (0). Entities may have been modified or deleted since entities were loaded. See http://go.microsoft.com/fwlink/?LinkId=472540 for information on understanding and handling optimistic concurrency exceptions."
                        {
                            //obll.DeleteById(lastodds.ID);
                            lock (locklist)
                            {
                                Main.Temp_O_Odds_Dict[this.Dict_SportsType[key2]][odds.BetCode].Remove(lastodds);
                            }

                            //Main.Temp_O_Odds_Dict[this.Dict_SportsType[key2]][odds.BetCode] = new ConcurrentBag<O_Odds>(Main.Temp_O_Odds_Dict[this.Dict_SportsType[key2]][odds.BetCode].Where(x => x.ID != lastodds.ID));
                        }
                    }
                }
            }
        }

        private void btn_Start_Click(object sender, EventArgs e)
        {
            if (this._SourcePlatform == null)
            {
                foreach (Control item in this.pnl_SourcePlatform.Controls)
                {
                    if (item is RadioButton)
                    {
                        RadioButton rdo = item as RadioButton;
                        if (rdo.Checked)
                        {
                            this._SourcePlatform = new SourcePlatform(rdo.Tag.ToString());
                            break;
                        }
                    }
                }
            }
            if (this._SourcePlatform == null)
            {
                MessageBox.Show("平台信息异常");
                return;
            }
            foreach (Control item in this.pnl_Browser.Controls)
            {
                if (item is RadioButton)
                {
                    RadioButton rdo = item as RadioButton;
                    if (rdo.Checked)
                    {
                        this._browser = rdo.Tag.ToString();
                        break;
                    }
                }
            }
            SaveOption();
            this._ProxyIP = string.Empty;
            if (this.chk_IPproxy.Checked)
            {
                this._ProxyIP = ConfigurationManager.AppSettings["proxyIp"];
            }
            InitTempData();
            _ZP_MS = 1000;
            _JR_MS = 1000;
            _GQ_MS = 1000;
            _SG_MS = 1000;
            _IsRunning = true;
            this.btn_Start.Text = "运行中";
            this.btn_Start.Enabled = false;
            this.btn_Stop.Enabled = true;
            this.txt_ZP_MS.Enabled = false;
            this.txt_JR_MS.Enabled = false;
            this.txt_GQ_MS.Enabled = false;

            int zpms = 0, jrms = 0, gqms = 0, sgms = 0;
            int.TryParse(this.txt_ZP_MS.Text.Trim(), out zpms);
            int.TryParse(this.txt_JR_MS.Text.Trim(), out jrms);
            int.TryParse(this.txt_GQ_MS.Text.Trim(), out gqms);
            int.TryParse(this.txt_SG_MS.Text.Trim(), out sgms);
            if (zpms * 1000 >= _ZP_MS)
            {
                _ZP_MS = zpms * 1000;
            }
            else
            {
                this.txt_ZP_MS.Text = (_ZP_MS / 1000).ToString();
            }
            if (jrms * 1000 >= _JR_MS)
            {
                _JR_MS = jrms * 1000;
            }
            else
            {
                this.txt_JR_MS.Text = (_JR_MS / 1000).ToString();
            }
            if (gqms * 1000 >= _GQ_MS)
            {
                _GQ_MS = gqms * 1000;
            }
            else
            {
                this.txt_GQ_MS.Text = (_GQ_MS / 1000).ToString();
            }
            if (sgms * 1000 >= _SG_MS)
            {
                _SG_MS = sgms * 1000;
            }
            else
            {
                this.txt_SG_MS.Text = (_SG_MS / 1000).ToString();
            }
            _ZP_AllDay = this.chk_ZP_AllDay.Checked;
            foreach (Log item in _LogWinList)
            {
                item.Close();
            }
            foreach (IWebDriver item in _WebDriverList)
            {
                if (item != null)
                {
                    item.Quit();
                }
            }
            if (this.chk_Logined.Checked)
            {
                LoginingSB(true);
            }
            int count = 0;
            foreach (Control g1 in this.pnl_BetArea.Controls)
            {
                if (g1 is GroupBox)
                {
                    foreach (Control g2 in g1.Controls)
                    {
                        if (g2 is GroupBox)
                        {
                            foreach (Control c in g2.Controls)
                            {
                                if (c is CheckBox)
                                {
                                    CheckBox chk = c as CheckBox;
                                    if (chk.Checked)
                                    {
                                        count++;
                                        if (chk.Tag.ToString().Contains("type"))
                                        {
                                            string[] tags = chk.Tag.ToString().Split('-');
                                            Log log = new Log();
                                            if (tags[1] == "2")
                                            {
                                                log.Text = "赛果，篮球";
                                            }
                                            else if (tags[1] == "8")
                                            {
                                                log.Text = "赛果，美式足球";
                                            }
                                            else
                                            {
                                                log.Text = "赛果，" + menuarr[1, 0, int.Parse(tags[1]) - 1];
                                            }
                                            log.Show();
                                            chk.ImageKey = chk.Tag.ToString();
                                            chk.Tag = log;
                                            _LogWinList.Add(log);
                                            Task.Run(async () =>
                                            {
                                                await Task.Delay(100);
                                                GetResultData(Convert.ToInt32(tags[1]), log);
                                            });
                                        }
                                        else
                                        {
                                            //string[] tags = chk.Tag.ToString().Split('-');
                                            //Log log = new Log();
                                            //log.Text = menuarr[0, 0, int.Parse(tags[0]) - 1] + "，"
                                            //    + menuarr[1, 0, int.Parse(tags[1]) - 1] + "，"
                                            //    + menuarr[2, 0, int.Parse(tags[2]) - 1];
                                            //log.Show();
                                            //chk.ImageKey = chk.Tag.ToString();
                                            //chk.Tag = log;
                                            //_LogWinList.Add(log);
                                            //Task.Run(async () =>
                                            //{
                                            //    await Task.Delay(100);
                                            //    GetData(Convert.ToInt32(tags[0]), Convert.ToInt32(tags[1]), Convert.ToInt32(tags[2]), log);
                                            //});
                                        }
                                    }
                                }
                            }
                        }
                        if (g2 is TreeView)
                        {
                            TreeView tvw = g2 as TreeView;
                            foreach (TreeNode n1 in tvw.Nodes)
                            {
                                if (n1.Checked)
                                {
                                    foreach (TreeNode n2 in n1.Nodes)
                                    {
                                        if (n2.Checked)
                                        {
                                            count++;
                                            string[] tags = n2.Tag.ToString().Split('-');
                                            Log log = new Log();
                                            log.Text = menuarr[0, 0, int.Parse(tags[0]) - 1] + "，"
                                                + menuarr[1, 0, int.Parse(tags[1]) - 1] + "，"
                                                + menuarr[2, 0, int.Parse(tags[2]) - 1];
                                            log.Show();
                                            n2.ImageKey = n2.Tag.ToString();
                                            n2.Tag = log;
                                            _LogWinList.Add(log);
                                            Task.Run(async () =>
                                            {
                                                await Task.Delay(100);
                                                GetData(Convert.ToInt32(tags[0]), Convert.ToInt32(tags[1]), Convert.ToInt32(tags[2]), log);
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

            }
            if (count < 1)
            {
                _IsRunning = false;
                this.btn_Start.Text = "开始";
                this.btn_Start.Enabled = true;
                this.btn_Stop.Enabled = false;
                this.txt_ZP_MS.Enabled = true;
                this.txt_JR_MS.Enabled = true;
                this.txt_GQ_MS.Enabled = true;
                this.timer1.Stop();
                this.timer2.Stop();
                this.lbl_runtime.Text = "-";
                MessageBox.Show("请选择要抓的数据");
            }
            else
            {
                _runstarttime = DateTime.Now;
                this.timer1.Start();
                this.timer2.Start();
            }
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("确认要关闭吗？", "提示", MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
            {
                Dispose();
                foreach (IWebDriver item in _WebDriverList)
                {
                    item.Quit();
                }
                Application.Exit();
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void chk_Click(object sender, EventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            if (_IsRunning)
            {
                if (chk.Checked)
                {
                    if (chk.Tag is Log)
                    {
                        Log l = chk.Tag as Log;
                        l.Show();
                    }
                }
            }
            else
            {
                chk.Checked = !chk.Checked;
            }
        }
        private void chk_Result_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            if (chk.Checked)
            {
                if (!this.chk_Logined.Checked)
                {
                    this.chk_Logined.Checked = true;
                }
            }
        }

        private void btn_Init_Click(object sender, EventArgs e)
        {
            LoginingSB(true);
        }
        //登录平台
        private void LoginingSB(bool main)
        {
            if (Main._IsLogining)
            {
                return;
            }
            try
            {
                Main._IsLogining = true;
                if (this._SourcePlatform == null)
                {
                    foreach (Control item in this.pnl_SourcePlatform.Controls)
                    {
                        if (item is RadioButton)
                        {
                            RadioButton rdo = item as RadioButton;
                            if (rdo.Checked)
                            {
                                this._SourcePlatform = new SourcePlatform(rdo.Tag.ToString());
                                break;
                            }
                        }
                    }
                }
                if (this._SourcePlatform == null)
                {
                    MessageBox.Show("平台信息异常");
                    return;
                }
                //用的session保存登录状态
                //IWebDriver driver = WebDriverHelper.CreateChromeDriver(true, true, "47.99.111.233:3808");//47.99.111.233:3808
                IWebDriver driver = WebDriverHelper.CreateWebDriver(this._browser, true, true, this._ProxyIP);
                if (driver == null)
                {
                    //MessageBox.Show(WebDriverHelper.ErrorMessage);
                    LogHelper.WriteLog(WebDriverHelper.ErrorMessage);
                    //尝试重启程序

                    return;
                }
                _WebDriverList.Add(driver);
                driver.Navigate().GoToUrl(this._SourcePlatform.url);
                if (WebDriverHelper.AlertExist(driver))
                {
                    driver.SwitchTo().Alert().Accept();
                }
                //if (this._CookieList.Count>0)
                //{
                //    driver.Manage().Cookies.DeleteAllCookies();
                //    foreach (var item in _CookieList)
                //    {
                //        driver.Manage().Cookies.AddCookie(item);
                //    }
                //    driver.Navigate().Refresh();
                //    if (WebDriverHelper.AlertExist(driver))
                //    {
                //        driver.SwitchTo().Alert().Accept();
                //    }
                //}
                //IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                //js.ExecuteScript("window.open('about:blank','_blank');");
                //var wname = driver.WindowHandles;
                //driver.SwitchTo().Frame("sportsbook").SwitchTo().Frame("sportsFrame");
                if (this._SourcePlatform.key == "TYC")
                {
                    bool islogined = true;
                    #region 太阳城
                    bool iswh = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#sportsFrame"), _WebDriver_MS);
                    if (driver.Url.Contains("DepositLogin"))//未登录
                    {
                        islogined = false;
                    }
                    else
                    {
                        if (!iswh)
                        {
                            Main._IsMaintenance = true;
                            GetMaintenanceInfo(null);
                            driver.Quit();
                            if (main)
                            {
                                MessageBox.Show("异常，沙巴可能在维护。");
                            }
                            return;
                        }
                        driver.SwitchTo().Frame("sportsFrame");
                        iswh = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#sbContainer"), _WebDriver_MS);
                        if (!iswh)
                        {
                            Main._IsMaintenance = true;
                            GetMaintenanceInfo(null);
                            driver.Quit();
                            if (main)
                            {
                                MessageBox.Show("异常，沙巴可能在维护。");
                            }
                            return;
                        }
                        string username = driver.FindElement(By.CssSelector("#sbContainer #sb-header #n1")).GetAttribute("innerHTML");
                        if (!username.Contains(","))
                        {
                            islogined = false;
                        }
                    }

                    if (!islogined)//未登录
                    {
                        driver.SwitchTo().DefaultContent();
                        driver.Navigate().GoToUrl(this._SourcePlatform.loginurl);
                        bool tycwh = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#authentication"), _WebDriver_MS);
                        if (!tycwh)
                        {
                            driver.Quit();
                            if (main)
                            {
                                MessageBox.Show("异常，沙巴可能在维护。");
                            }
                            return;
                        }
                        driver.FindElement(By.CssSelector("#authentication form input[name='username']")).SendKeys(this._SourcePlatform.loginname);
                        driver.FindElement(By.CssSelector("#authentication form input[name='password']")).SendKeys(this._SourcePlatform.loginpassword);
                        Thread.Sleep(1000);
                        bool flag = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#authentication form input[type='submit']"), 50);
                        if (flag)
                        {
                            driver.FindElement(By.CssSelector("#authentication form input[type='submit']")).Click();
                        }
                        Thread.Sleep(10000);
                        //driver.SwitchTo().Frame("sportsbook").SwitchTo().Frame("sportsFrame");
                        bool flag01 = false;
                        while (!flag01)
                        {
                            flag01 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#sportsbook"), _WebDriver_MS);
                        }
                        driver.SwitchTo().Frame("sportsbook");
                        int lcount = 0;
                        bool flag02 = false;
                        while (!flag02)
                        {
                            lcount++;
                            flag02 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#sportsFrame"), _WebDriver_MS);
                            if (lcount>60)
                            {
                                flag02 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#mainframe"), _WebDriver_MS);
                                if (flag02)//查看是否维护中
                                {
                                    driver.Navigate().GoToUrl(this._SourcePlatform.nologinurl);
                                    break;
                                }
                            }
                        }
                        string newurl = this._SourcePlatform.url;
                        if (lcount <= 60)
                        {
                            driver.SwitchTo().DefaultContent();
                            string loginingurl = driver.FindElement(By.CssSelector("#sportsbook")).GetAttribute("src");
                            do
                            {
                                lcount++;
                                Thread.Sleep(1000);
                                loginingurl = driver.FindElement(By.CssSelector("#sportsbook")).GetAttribute("src");
                            } while (!loginingurl.Contains("Deposit_ProcessLogin"));
                            //LogHelper.WriteLog(loginingurl);
                            driver.Navigate().GoToUrl(loginingurl);
                            newurl = driver.Url;
                        }                          
                        //driver.SwitchTo().Frame("sportsbook");
                        bool flag03 = false;
                        //while (!flag02)
                        //{
                        flag03 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#sportsFrame"), _WebDriver_MS);
                        //}
                        if (!flag03)
                        {
                            Main._IsMaintenance = true;
                            GetMaintenanceInfo(null);
                            driver.Quit();
                            if (main)
                            {
                                MessageBox.Show("异常，沙巴可能在维护。");
                            }
                            return;
                        }
                        driver.SwitchTo().Frame("sportsFrame");
                        bool flag04 = false;
                        while (!flag04)
                        {
                            flag04 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#header #betList"), _WebDriver_MS);
                        }
                        //Thread.Sleep(10000);
                        //MessageBox.Show(driver.Url);
                        if (newurl.Contains("NewIndex"))
                        {
                            newurl = newurl.Substring(0, newurl.IndexOf("?"));
                            newurl += "?webskintype=2&lang=cs";
                            this._SourcePlatform.url = newurl;
                        }
                        //MessageBox.Show(newurl);
                        //string ccc = string.Empty;
                        //foreach (var item in driver.Manage().Cookies.AllCookies)
                        //{
                        //    ccc += item.Domain + "-" + item.Name + ":" + item.Value + "\r\n";
                        //}
                        //MessageBox.Show(ccc);
                        //return;
                        //driver.Navigate().GoToUrl(this._SourcePlatform.url);
                        string strcookie = string.Empty;
                        _CookieList.Clear();
                        foreach (var item in driver.Manage().Cookies.AllCookies)
                        {
                            strcookie += item.Name + ":" + item.Value + "\r\n";
                            _CookieList.Add(item);
                        }
                        //赛果
                        driver.SwitchTo().DefaultContent();
                        driver.Navigate().GoToUrl(this._SourcePlatform.resulturl);
                        _ResultCookieList.Clear();
                        foreach (var item in driver.Manage().Cookies.AllCookies)
                        {
                            _ResultCookieList.Add(item);
                        }
                        driver.Quit();
                    }
                    else
                    {
                        //MessageBox.Show("已登录");
                        string strcookie = string.Empty;
                        _CookieList.Clear();
                        foreach (var item in driver.Manage().Cookies.AllCookies)
                        {
                            strcookie += item.Name + ":" + item.Value + "\r\n";
                            _CookieList.Add(item);
                        }
                        //赛果
                        driver.SwitchTo().DefaultContent();
                        driver.Navigate().GoToUrl(this._SourcePlatform.resulturl);
                        _ResultCookieList.Clear();
                        foreach (var item in driver.Manage().Cookies.AllCookies)
                        {
                            _ResultCookieList.Add(item);
                        }
                        driver.Quit();
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                //尝试重启程序

                throw ex;
            }
            finally
            {
                Main._IsLogining = false;
            }
        }
        private void btn_Stop_Click(object sender, EventArgs e)
        {
            _IsRunning = false;
            this.btn_Start.Text = "开始";
            this.btn_Start.Enabled = true;
            this.btn_Stop.Enabled = false;
            this.txt_ZP_MS.Enabled = true;
            this.txt_JR_MS.Enabled = true;
            this.txt_GQ_MS.Enabled = true;
            this.timer1.Stop();
            this.timer2.Stop();
            this.lbl_runtime.Text = "-";
            foreach (Control g1 in this.pnl_BetArea.Controls)
            {
                if (g1 is GroupBox)
                {
                    foreach (Control g2 in g1.Controls)
                    {
                        if (g2 is GroupBox)
                        {
                            foreach (Control c in g2.Controls)
                            {
                                if (c is CheckBox)
                                {
                                    CheckBox chk = c as CheckBox;
                                    if (chk.Checked)
                                    {
                                        chk.Tag = chk.ImageKey;
                                    }
                                }
                            }
                        }
                        if (g2 is TreeView)
                        {
                            TreeView tvw = g2 as TreeView;
                            foreach (TreeNode n1 in tvw.Nodes)
                            {
                                if (n1.Checked)
                                {
                                    foreach (TreeNode n2 in n1.Nodes)
                                    {
                                        if (n2.Checked)
                                        {
                                            n2.Tag = n2.ImageKey;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            TimeSpan ts = DateTime.Now - _runstarttime;
            this.lbl_runtime.Text = ts.Days + "天" + ts.Hours + "小时" + ts.Minutes + "分" + ts.Seconds + "秒";
        }

        //赛果
        private void GetResultData(int key2, Log logwin)
        {
            int awaitms = 1000;
            IWebDriver driver = WebDriverHelper.CreateWebDriver(this._browser, chk_VisibleBrowser.Checked, false, this._ProxyIP);
            //if (key2 == 1)
            //{
            //    driver = WebDriverHelper.CreateWebDriver(true, false, this._ProxyIP);
            //}
            if (driver == null)
            {
                //MessageBox.Show(WebDriverHelper.ErrorMessage);
                LogHelper.WriteLog(WebDriverHelper.ErrorMessage);
                logwin.txt_log.AppendText(WebDriverHelper.ErrorMessage + "\r\n");
                if (this._SaveRunningLog)
                {
                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", WebDriverHelper.ErrorMessage + "\r\n");
                }
                return;
            }
            _WebDriverList.Add(driver);
            this.Invoke(new MethodInvoker(delegate
            {
                Main._IsRunning = true;
                logwin.txt_log.AppendText("地址：" + this._SourcePlatform.resulturl + "\r\n");
                logwin.txt_log.AppendText("\r\n");
                logwin.txt_log.AppendText("开始初次请求：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n");
                logwin.txt_log.AppendText("===========================================\r\n");
                if (this._SaveRunningLog)
                {
                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "地址：" + this._SourcePlatform.resulturl + "\r\n\r\n开始初次请求：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n===========================================\r\n");
                }
            }));
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            driver.Navigate().GoToUrl(this._SourcePlatform.resulturl);
            driver.Manage().Cookies.DeleteAllCookies();
            foreach (var item in _ResultCookieList)
            {
                driver.Manage().Cookies.AddCookie(item);
            }
            driver.Navigate().Refresh();
            watch.Start();
            if (this._SourcePlatform.key == "TYC")
            {
                bool iswh = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#rusultContainer"), _WebDriver_MS);
                if (!iswh)
                {
                    this.Invoke(new MethodInvoker(delegate
                    {
                        logwin.txt_log.AppendText("异常，没有登录或者沙巴可能在维护。\r\n");
                        if (this._SaveRunningLog)
                        {
                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "异常，沙巴可能在维护。\r\n");
                        }
                    }));
                    GetMaintenanceInfo(logwin);
                    //return;
                }
                int count = 0;
                Task.Run(async () =>
                {
                    layout: while (_IsRunning)
                    {
                        await Task.Delay(awaitms);
                        count++;
                        if (count != 1)
                        {
                            awaitms = this._SG_MS;
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("开始第" + count + "次请求：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n");
                                logwin.txt_log.AppendText("===========================================\r\n");
                                if (this._SaveRunningLog)
                                {
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "开始第" + count + "次请求：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n===========================================\r\n");
                                }
                            }));
                        }
                        string html = string.Empty;
                        try
                        {
                            #region 折叠
                            int flag01count = 0;
                            bool flag01 = false;
                            while (!flag01)
                            {
                                flag01 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#f_trigger_a"), _WebDriver_MS);
                                flag01count++;
                                if (flag01count > 200)
                                {
                                    //判断登录状态是否还有效
                                    string bodyhtml = driver.FindElement(By.CssSelector("body")).GetAttribute("innerHTML");
                                    if (string.IsNullOrEmpty(bodyhtml))
                                    {
                                        this.Invoke(new MethodInvoker(delegate
                                        {
                                            logwin.txt_log.AppendText("登录失效，重新登录\r\n");
                                            if (this._SaveRunningLog)
                                            {
                                                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "登录失效，重新登录\r\n");
                                            }
                                        }));
                                        //重新登录
                                        if (!Main._IsLogining && !Main._IsMaintenance)
                                        {
                                            LoginingSB(false);
                                        }
                                        while (Main._IsLogining)
                                        {
                                            await Task.Delay(5000);
                                        }
                                        driver.Navigate().GoToUrl(this._SourcePlatform.resulturl);
                                        driver.Manage().Cookies.DeleteAllCookies();
                                        foreach (var item in _ResultCookieList)
                                        {
                                            driver.Manage().Cookies.AddCookie(item);
                                        }
                                        driver.Navigate().Refresh();
                                        goto layout;
                                    }
                                    else
                                    {
                                        this.Invoke(new MethodInvoker(delegate
                                        {
                                            logwin.txt_log.AppendText("请求超时，重刷页面\r\n");
                                            if (this._SaveRunningLog)
                                            {
                                                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "请求超时，重刷页面\r\n");
                                            }
                                        }));
                                        try
                                        {
                                            driver.Manage().Cookies.DeleteAllCookies();
                                            foreach (var item in _ResultCookieList)
                                            {
                                                driver.Manage().Cookies.AddCookie(item);
                                            }
                                            flag01 = true;
                                            driver.Navigate().Refresh();
                                            goto layout;
                                        }
                                        catch (Exception)
                                        {
                                            driver.Navigate().GoToUrl(this._SourcePlatform.resulturl);
                                            goto layout;
                                        }
                                    }
                                }
                            }
                            //选择今日
                            bool flagcheckresult = true;
                            driver.FindElement(By.CssSelector(".filterBlock>.filterRow>button:nth-child(2)")).Click();
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("选择今日\r\n");
                                if (this._SaveRunningLog)
                                {
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择今日\r\n");
                                }
                            }));
                            Thread.Sleep(500);
                            bool flag02 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#ddSport"), 500);
                            if (flag02)
                            {
                                switch (key2)
                                {
                                    case 1://选择足球                                        
                                        #region 选择足球
                                        driver.FindElement(By.CssSelector("#ddSport")).Click();
                                        Thread.Sleep(500);
                                        //判断是否有足球
                                        bool flag1111 = WebDriverHelper.WaitForElementVisible(driver, By.XPath("//div[@id='ddSport']/div[@class='dropdownPanel']/div[text()='足球']"), 500);
                                        if (flag1111)
                                        {
                                            driver.FindElement(By.XPath("//div[@id='ddSport']/div[@class='dropdownPanel']/div[text()='足球']")).Click();
                                            this.Invoke(new MethodInvoker(delegate
                                            {
                                                logwin.txt_log.AppendText("选择足球\r\n");
                                                if (this._SaveRunningLog)
                                                {
                                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择足球\r\n");
                                                }
                                            }));
                                        }
                                        else
                                        {
                                            this.Invoke(new MethodInvoker(delegate
                                            {
                                                logwin.txt_log.AppendText("今日无足球\r\n");
                                                if (this._SaveRunningLog)
                                                {
                                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "今日无足球\r\n");
                                                }
                                            }));
                                            flagcheckresult = false;
                                        }
                                        #endregion
                                        break;
                                    case 2://选择篮球                                        
                                        #region 选择篮球
                                        driver.FindElement(By.CssSelector("#ddSport")).Click();
                                        Thread.Sleep(500);
                                        //判断是否有足球
                                        bool flag2222 = WebDriverHelper.WaitForElementVisible(driver, By.XPath("//div[@id='ddSport']/div[@class='dropdownPanel']/div[text()='篮球']"), 500);
                                        if (flag2222)
                                        {
                                            driver.FindElement(By.XPath("//div[@id='ddSport']/div[@class='dropdownPanel']/div[text()='篮球']")).Click();
                                            this.Invoke(new MethodInvoker(delegate
                                            {
                                                logwin.txt_log.AppendText("选择篮球\r\n");
                                                if (this._SaveRunningLog)
                                                {
                                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择篮球\r\n");
                                                }
                                            }));
                                        }
                                        else
                                        {
                                            this.Invoke(new MethodInvoker(delegate
                                            {
                                                logwin.txt_log.AppendText("今日无篮球\r\n");
                                                if (this._SaveRunningLog)
                                                {
                                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "今日无篮球\r\n");
                                                }
                                            }));
                                            flagcheckresult = false;
                                        }
                                        #endregion
                                        break;
                                    case 3://选择网球                                        
                                        #region 选择网球
                                        driver.FindElement(By.CssSelector("#ddSport")).Click();
                                        Thread.Sleep(500);
                                        //判断是否有网球
                                        bool flag3333 = WebDriverHelper.WaitForElementVisible(driver, By.XPath("//div[@id='ddSport']/div[@class='dropdownPanel']/div[text()='网球']"), 500);
                                        if (flag3333)
                                        {
                                            driver.FindElement(By.XPath("//div[@id='ddSport']/div[@class='dropdownPanel']/div[text()='网球']")).Click();
                                            this.Invoke(new MethodInvoker(delegate
                                            {
                                                logwin.txt_log.AppendText("选择网球\r\n");
                                                if (this._SaveRunningLog)
                                                {
                                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择网球\r\n");
                                                }
                                            }));
                                        }
                                        else
                                        {
                                            this.Invoke(new MethodInvoker(delegate
                                            {
                                                logwin.txt_log.AppendText("今日无网球\r\n");
                                                if (this._SaveRunningLog)
                                                {
                                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "今日无网球\r\n");
                                                }
                                            }));
                                            flagcheckresult = false;
                                        }
                                        #endregion
                                        break;
                                    case 4://选择排球                                        
                                        #region 选择排球
                                        driver.FindElement(By.CssSelector("#ddSport")).Click();
                                        Thread.Sleep(500);
                                        //判断是否有排球
                                        bool flag4444 = WebDriverHelper.WaitForElementVisible(driver, By.XPath("//div[@id='ddSport']/div[@class='dropdownPanel']/div[text()='排球']"), 500);
                                        if (flag4444)
                                        {
                                            driver.FindElement(By.XPath("//div[@id='ddSport']/div[@class='dropdownPanel']/div[text()='排球']")).Click();
                                            this.Invoke(new MethodInvoker(delegate
                                            {
                                                logwin.txt_log.AppendText("选择排球\r\n");
                                                if (this._SaveRunningLog)
                                                {
                                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择排球\r\n");
                                                }
                                            }));
                                        }
                                        else
                                        {
                                            this.Invoke(new MethodInvoker(delegate
                                            {
                                                logwin.txt_log.AppendText("今日无排球\r\n");
                                                if (this._SaveRunningLog)
                                                {
                                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "今日无排球\r\n");
                                                }
                                            }));
                                            flagcheckresult = false;
                                        }
                                        #endregion
                                        break;
                                    case 5://选择棒球                                        
                                        #region 选择棒球
                                        driver.FindElement(By.CssSelector("#ddSport")).Click();
                                        Thread.Sleep(500);
                                        //判断是否有棒球
                                        bool flag5555 = WebDriverHelper.WaitForElementVisible(driver, By.XPath("//div[@id='ddSport']/div[@class='dropdownPanel']/div[text()='棒球']"), 500);
                                        if (flag5555)
                                        {
                                            driver.FindElement(By.XPath("//div[@id='ddSport']/div[@class='dropdownPanel']/div[text()='棒球']")).Click();
                                            this.Invoke(new MethodInvoker(delegate
                                            {
                                                logwin.txt_log.AppendText("选择棒球\r\n");
                                                if (this._SaveRunningLog)
                                                {
                                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择棒球\r\n");
                                                }
                                            }));
                                        }
                                        else
                                        {
                                            this.Invoke(new MethodInvoker(delegate
                                            {
                                                logwin.txt_log.AppendText("今日无棒球\r\n");
                                                if (this._SaveRunningLog)
                                                {
                                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "今日无棒球\r\n");
                                                }
                                            }));
                                            flagcheckresult = false;
                                        }
                                        #endregion
                                        break;
                                    case 6://选择羽毛球                                       
                                        #region 选择羽毛球
                                        driver.FindElement(By.CssSelector("#ddSport")).Click();
                                        Thread.Sleep(500);
                                        //判断是否有羽毛球
                                        bool flag6666 = WebDriverHelper.WaitForElementVisible(driver, By.XPath("//div[@id='ddSport']/div[@class='dropdownPanel']/div[text()='羽毛球']"), 500);
                                        if (flag6666)
                                        {
                                            driver.FindElement(By.XPath("//div[@id='ddSport']/div[@class='dropdownPanel']/div[text()='羽毛球']")).Click();
                                            this.Invoke(new MethodInvoker(delegate
                                            {
                                                logwin.txt_log.AppendText("选择羽毛球\r\n");
                                                if (this._SaveRunningLog)
                                                {
                                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择羽毛球\r\n");
                                                }
                                            }));
                                        }
                                        else
                                        {
                                            this.Invoke(new MethodInvoker(delegate
                                            {
                                                logwin.txt_log.AppendText("今日无羽毛球\r\n");
                                                if (this._SaveRunningLog)
                                                {
                                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "今日无羽毛球\r\n");
                                                }
                                            }));
                                            flagcheckresult = false;
                                        }
                                        #endregion
                                        break;
                                    case 7://选择乒乓球                                        
                                        #region 选择乒乓球
                                        driver.FindElement(By.CssSelector("#ddSport")).Click();
                                        Thread.Sleep(500);
                                        //判断是否有乒乓球
                                        bool flag7777 = WebDriverHelper.WaitForElementVisible(driver, By.XPath("//div[@id='ddSport']/div[@class='dropdownPanel']/div[text()='乒乓球']"), 500);
                                        if (flag7777)
                                        {
                                            driver.FindElement(By.XPath("//div[@id='ddSport']/div[@class='dropdownPanel']/div[text()='乒乓球']")).Click();
                                            this.Invoke(new MethodInvoker(delegate
                                            {
                                                logwin.txt_log.AppendText("选择乒乓球\r\n");
                                                if (this._SaveRunningLog)
                                                {
                                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择乒乓球\r\n");
                                                }
                                            }));
                                        }
                                        else
                                        {
                                            this.Invoke(new MethodInvoker(delegate
                                            {
                                                logwin.txt_log.AppendText("今日无乒乓球\r\n");
                                                if (this._SaveRunningLog)
                                                {
                                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "今日无乒乓球\r\n");
                                                }
                                            }));
                                            flagcheckresult = false;
                                        }
                                        #endregion
                                        break;
                                    case 8://选择美式足球                                        
                                        #region 选择美式足球
                                        driver.FindElement(By.CssSelector("#ddSport")).Click();
                                        Thread.Sleep(500);
                                        //判断是否有美式足球
                                        bool flag8888 = WebDriverHelper.WaitForElementVisible(driver, By.XPath("//div[@id='ddSport']/div[@class='dropdownPanel']/div[text()='美式足球']"), 500);
                                        if (flag8888)
                                        {
                                            driver.FindElement(By.XPath("//div[@id='ddSport']/div[@class='dropdownPanel']/div[text()='美式足球']")).Click();
                                            this.Invoke(new MethodInvoker(delegate
                                            {
                                                logwin.txt_log.AppendText("选择美式足球\r\n");
                                                if (this._SaveRunningLog)
                                                {
                                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择美式足球\r\n");
                                                }
                                            }));
                                        }
                                        else
                                        {
                                            this.Invoke(new MethodInvoker(delegate
                                            {
                                                logwin.txt_log.AppendText("今日无美式足球\r\n");
                                                if (this._SaveRunningLog)
                                                {
                                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "今日无美式足球\r\n");
                                                }
                                            }));
                                            flagcheckresult = false;
                                        }
                                        #endregion
                                        break;
                                }
                            }
                            else
                            {
                                this.Invoke(new MethodInvoker(delegate
                                {
                                    logwin.txt_log.AppendText("今日无任何数据\r\n");
                                    if (this._SaveRunningLog)
                                    {
                                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "今日无任何数据\r\n");
                                    }
                                }));
                                flagcheckresult = false;
                            }
                            bool flag1 = true, flag2 = true;
                            if (flagcheckresult)
                            {
                                //判断是否有数据
                                flag1 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#rusultContainer .noInfo"), _WebDriver_MS);
                                flag2 = WebDriverHelper.WaitForElementInvisible(driver, By.CssSelector("#rusultContainer .tableBody"), _WebDriver_MS);
                                if (flag1 && flag2)
                                {
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("今日暂无赛果\r\n");
                                        logwin.txt_log.AppendText("===========================================\r\n");
                                        if (this._SaveRunningLog)
                                        {
                                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "今日暂无赛果\r\n===========================================\r\n");
                                        }
                                    }));
                                }
                                else
                                {
                                    if (key2 == 1)
                                    {
                                        ////展开详情
                                        //foreach (var item in driver.FindElements(By.CssSelector("#rusultContainer .tableBody .tableRow")))
                                        //{
                                        //    bool flag = WebDriverHelper.ElementExist(item, By.CssSelector(".other .smallBtn"));
                                        //    if (flag)
                                        //    {
                                        //        item.FindElement(By.CssSelector(".other .smallBtn")).Click();
                                        //        await Task.Delay(50);
                                        //        if (!item.FindElement(By.CssSelector(".other .smallBtn")).GetAttribute("class").Contains("specialC"))
                                        //        {
                                        //            item.FindElement(By.CssSelector(".other .smallBtn")).Click();
                                        //        }
                                        //    }
                                        //}
                                        //this.Invoke(new MethodInvoker(delegate
                                        //{
                                        //    logwin.txt_log.AppendText("展开\r\n");
                                        //}));
                                    }
                                    IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                                    js.ExecuteScript("$('#rusultContainer .tableBody>span.leagueName').addClass('xxxxxxxxxxxx');");
                                    Thread.Sleep(1000);

                                    var tableBodys = driver.FindElements(By.CssSelector("#rusultContainer .tableBody"));
                                    foreach (var item in tableBodys)
                                    {
                                        html += item.GetAttribute("innerHTML");
                                    }
                                }
                            }
                            bool flagcheckresult2 = true;
                            //选择昨日
                            driver.FindElement(By.CssSelector(".filterBlock>.filterRow>button:nth-child(3)")).Click();
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("选择昨日\r\n");
                                if (this._SaveRunningLog)
                                {
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择昨日\r\n");
                                }
                            }));
                            Thread.Sleep(500);
                            bool flag03 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#ddSport"), 500);
                            if (flag03)
                            {
                                switch (key2)
                                {
                                    case 1://选择足球                                        
                                        #region 选择足球
                                        driver.FindElement(By.CssSelector("#ddSport")).Click();
                                        Thread.Sleep(500);
                                        //判断是否有足球
                                        bool flag1111 = WebDriverHelper.WaitForElementVisible(driver, By.XPath("//div[@id='ddSport']/div[@class='dropdownPanel']/div[text()='足球']"), 500);
                                        if (flag1111)
                                        {
                                            driver.FindElement(By.XPath("//div[@id='ddSport']/div[@class='dropdownPanel']/div[text()='足球']")).Click();
                                            this.Invoke(new MethodInvoker(delegate
                                            {
                                                logwin.txt_log.AppendText("选择足球\r\n");
                                                if (this._SaveRunningLog)
                                                {
                                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择足球\r\n");
                                                }
                                            }));
                                        }
                                        else
                                        {
                                            this.Invoke(new MethodInvoker(delegate
                                            {
                                                logwin.txt_log.AppendText("昨日无足球\r\n");
                                                if (this._SaveRunningLog)
                                                {
                                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "昨日无足球\r\n");
                                                }
                                            }));
                                            flagcheckresult2 = false;
                                        }
                                        #endregion
                                        break;
                                    case 2://选择篮球                                        
                                        #region 选择篮球
                                        driver.FindElement(By.CssSelector("#ddSport")).Click();
                                        Thread.Sleep(500);
                                        //判断是否有足球
                                        bool flag2222 = WebDriverHelper.WaitForElementVisible(driver, By.XPath("//div[@id='ddSport']/div[@class='dropdownPanel']/div[text()='篮球']"), 500);
                                        if (flag2222)
                                        {
                                            driver.FindElement(By.XPath("//div[@id='ddSport']/div[@class='dropdownPanel']/div[text()='篮球']")).Click();
                                            this.Invoke(new MethodInvoker(delegate
                                            {
                                                logwin.txt_log.AppendText("选择篮球\r\n");
                                                if (this._SaveRunningLog)
                                                {
                                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择篮球\r\n");
                                                }
                                            }));
                                        }
                                        else
                                        {
                                            this.Invoke(new MethodInvoker(delegate
                                            {
                                                logwin.txt_log.AppendText("今日无篮球\r\n");
                                                if (this._SaveRunningLog)
                                                {
                                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "今日无篮球\r\n");
                                                }
                                            }));
                                            flagcheckresult2 = false;
                                        }
                                        #endregion
                                        break;
                                    case 3://选择网球                                        
                                        #region 选择网球
                                        driver.FindElement(By.CssSelector("#ddSport")).Click();
                                        Thread.Sleep(500);
                                        //判断是否有网球
                                        bool flag3333 = WebDriverHelper.WaitForElementVisible(driver, By.XPath("//div[@id='ddSport']/div[@class='dropdownPanel']/div[text()='网球']"), 500);
                                        if (flag3333)
                                        {
                                            driver.FindElement(By.XPath("//div[@id='ddSport']/div[@class='dropdownPanel']/div[text()='网球']")).Click();
                                            this.Invoke(new MethodInvoker(delegate
                                            {
                                                logwin.txt_log.AppendText("选择网球\r\n");
                                                if (this._SaveRunningLog)
                                                {
                                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择网球\r\n");
                                                }
                                            }));
                                        }
                                        else
                                        {
                                            this.Invoke(new MethodInvoker(delegate
                                            {
                                                logwin.txt_log.AppendText("今日无网球\r\n");
                                                if (this._SaveRunningLog)
                                                {
                                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "今日无网球\r\n");
                                                }
                                            }));
                                            flagcheckresult2 = false;
                                        }
                                        #endregion
                                        break;
                                    case 4://选择排球                                        
                                        #region 选择排球
                                        driver.FindElement(By.CssSelector("#ddSport")).Click();
                                        Thread.Sleep(500);
                                        //判断是否有排球
                                        bool flag4444 = WebDriverHelper.WaitForElementVisible(driver, By.XPath("//div[@id='ddSport']/div[@class='dropdownPanel']/div[text()='排球']"), 500);
                                        if (flag4444)
                                        {
                                            driver.FindElement(By.XPath("//div[@id='ddSport']/div[@class='dropdownPanel']/div[text()='排球']")).Click();
                                            this.Invoke(new MethodInvoker(delegate
                                            {
                                                logwin.txt_log.AppendText("选择排球\r\n");
                                                if (this._SaveRunningLog)
                                                {
                                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择排球\r\n");
                                                }
                                            }));
                                        }
                                        else
                                        {
                                            this.Invoke(new MethodInvoker(delegate
                                            {
                                                logwin.txt_log.AppendText("今日无排球\r\n");
                                                if (this._SaveRunningLog)
                                                {
                                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "今日无排球\r\n");
                                                }
                                            }));
                                            flagcheckresult2 = false;
                                        }
                                        #endregion
                                        break;
                                    case 5://选择棒球                                        
                                        #region 选择棒球
                                        driver.FindElement(By.CssSelector("#ddSport")).Click();
                                        Thread.Sleep(500);
                                        //判断是否有棒球
                                        bool flag5555 = WebDriverHelper.WaitForElementVisible(driver, By.XPath("//div[@id='ddSport']/div[@class='dropdownPanel']/div[text()='棒球']"), 500);
                                        if (flag5555)
                                        {
                                            driver.FindElement(By.XPath("//div[@id='ddSport']/div[@class='dropdownPanel']/div[text()='棒球']")).Click();
                                            this.Invoke(new MethodInvoker(delegate
                                            {
                                                logwin.txt_log.AppendText("选择棒球\r\n");
                                                if (this._SaveRunningLog)
                                                {
                                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择棒球\r\n");
                                                }
                                            }));
                                        }
                                        else
                                        {
                                            this.Invoke(new MethodInvoker(delegate
                                            {
                                                logwin.txt_log.AppendText("今日无棒球\r\n");
                                                if (this._SaveRunningLog)
                                                {
                                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "今日无棒球\r\n");
                                                }
                                            }));
                                            flagcheckresult2 = false;
                                        }
                                        #endregion
                                        break;
                                    case 6://选择羽毛球                                       
                                        #region 选择羽毛球
                                        driver.FindElement(By.CssSelector("#ddSport")).Click();
                                        Thread.Sleep(500);
                                        //判断是否有羽毛球
                                        bool flag6666 = WebDriverHelper.WaitForElementVisible(driver, By.XPath("//div[@id='ddSport']/div[@class='dropdownPanel']/div[text()='羽毛球']"), 500);
                                        if (flag6666)
                                        {
                                            driver.FindElement(By.XPath("//div[@id='ddSport']/div[@class='dropdownPanel']/div[text()='羽毛球']")).Click();
                                            this.Invoke(new MethodInvoker(delegate
                                            {
                                                logwin.txt_log.AppendText("选择羽毛球\r\n");
                                                if (this._SaveRunningLog)
                                                {
                                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择羽毛球\r\n");
                                                }
                                            }));
                                        }
                                        else
                                        {
                                            this.Invoke(new MethodInvoker(delegate
                                            {
                                                logwin.txt_log.AppendText("今日无羽毛球\r\n");
                                                if (this._SaveRunningLog)
                                                {
                                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "今日无羽毛球\r\n");
                                                }
                                            }));
                                            flagcheckresult2 = false;
                                        }
                                        #endregion
                                        break;
                                    case 7://选择乒乓球                                        
                                        #region 选择乒乓球
                                        driver.FindElement(By.CssSelector("#ddSport")).Click();
                                        Thread.Sleep(500);
                                        //判断是否有乒乓球
                                        bool flag7777 = WebDriverHelper.WaitForElementVisible(driver, By.XPath("//div[@id='ddSport']/div[@class='dropdownPanel']/div[text()='乒乓球']"), 500);
                                        if (flag7777)
                                        {
                                            driver.FindElement(By.XPath("//div[@id='ddSport']/div[@class='dropdownPanel']/div[text()='乒乓球']")).Click();
                                            this.Invoke(new MethodInvoker(delegate
                                            {
                                                logwin.txt_log.AppendText("选择乒乓球\r\n");
                                                if (this._SaveRunningLog)
                                                {
                                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择乒乓球\r\n");
                                                }
                                            }));
                                        }
                                        else
                                        {
                                            this.Invoke(new MethodInvoker(delegate
                                            {
                                                logwin.txt_log.AppendText("今日无乒乓球\r\n");
                                                if (this._SaveRunningLog)
                                                {
                                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "今日无乒乓球\r\n");
                                                }
                                            }));
                                            flagcheckresult2 = false;
                                        }
                                        #endregion
                                        break;
                                    case 8://选择美式足球
                                        #region 选择美式足球
                                        driver.FindElement(By.CssSelector("#ddSport")).Click();
                                        Thread.Sleep(500);
                                        //判断是否有美式足球
                                        bool flag8888 = WebDriverHelper.WaitForElementVisible(driver, By.XPath("//div[@id='ddSport']/div[@class='dropdownPanel']/div[text()='美式足球']"), 500);
                                        if (flag8888)
                                        {
                                            driver.FindElement(By.XPath("//div[@id='ddSport']/div[@class='dropdownPanel']/div[text()='美式足球']")).Click();
                                            this.Invoke(new MethodInvoker(delegate
                                            {
                                                logwin.txt_log.AppendText("选择美式足球\r\n");
                                                if (this._SaveRunningLog)
                                                {
                                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择美式足球\r\n");
                                                }
                                            }));
                                        }
                                        else
                                        {
                                            this.Invoke(new MethodInvoker(delegate
                                            {
                                                logwin.txt_log.AppendText("今日无美式足球\r\n");
                                                if (this._SaveRunningLog)
                                                {
                                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "今日无美式足球\r\n");
                                                }
                                            }));
                                            flagcheckresult2 = false;
                                        }
                                        #endregion
                                        break;
                                }
                            }
                            else
                            {
                                this.Invoke(new MethodInvoker(delegate
                                {
                                    logwin.txt_log.AppendText("昨日无任何数据\r\n");
                                    if (this._SaveRunningLog)
                                    {
                                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "昨日无任何数据\r\n");
                                    }
                                }));
                                flagcheckresult2 = false;
                            }

                            bool flag11 = true, flag22 = true;
                            if (flagcheckresult2)
                            {
                                //判断是否有数据
                                flag11 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#rusultContainer .noInfo"), _WebDriver_MS);
                                flag22 = WebDriverHelper.WaitForElementInvisible(driver, By.CssSelector("#rusultContainer .tableBody"), _WebDriver_MS);
                                if (flag11 && flag22)
                                {
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("昨日暂无赛果\r\n");
                                        logwin.txt_log.AppendText("===========================================\r\n");
                                        if (this._SaveRunningLog)
                                        {
                                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "昨日暂无赛果\r\n===========================================\r\n");
                                        }
                                    }));
                                }
                                else
                                {
                                    if (key2 == 1)
                                    {
                                        ////展开详情
                                        //foreach (var item in driver.FindElements(By.CssSelector("#rusultContainer .tableBody .tableRow")))
                                        //{
                                        //    bool flag = WebDriverHelper.ElementExist(item, By.CssSelector(".other .smallBtn"));
                                        //    if (flag)
                                        //    {
                                        //        item.FindElement(By.CssSelector(".other .smallBtn")).Click();
                                        //        await Task.Delay(50);
                                        //        if (!item.FindElement(By.CssSelector(".other .smallBtn")).GetAttribute("class").Contains("specialC"))
                                        //        {
                                        //            item.FindElement(By.CssSelector(".other .smallBtn")).Click();
                                        //        }
                                        //    }
                                        //}
                                        //this.Invoke(new MethodInvoker(delegate
                                        //{
                                        //    logwin.txt_log.AppendText("展开\r\n");
                                        //}));
                                    }

                                    IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                                    js.ExecuteScript("$('#rusultContainer .tableBody>span.leagueName').addClass('xxxxxxxxxxxx');");
                                    Thread.Sleep(1000);

                                    var tableBodys = driver.FindElements(By.CssSelector("#rusultContainer .tableBody"));
                                    foreach (var item in tableBodys)
                                    {
                                        html += item.GetAttribute("innerHTML");
                                    }
                                }
                            }
                            if (flag1 && flag2 && flag11 && flag22)
                            {
                                this.Invoke(new MethodInvoker(delegate
                                {
                                    logwin.txt_log.AppendText("无数据\r\n");
                                    logwin.txt_log.AppendText("===========================================\r\n");
                                    if (this._SaveRunningLog)
                                    {
                                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "无数据\r\n===========================================\r\n");
                                    }
                                }));
                            }
                            else
                            {
                                this.Invoke(new MethodInvoker(delegate
                                {
                                    logwin.txt_log.AppendText("===========================================\r\n");
                                    logwin.txt_log.AppendText("数据抓取完毕\r\n");
                                    if (this._SaveRunningLog)
                                    {
                                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "===========================================\r\n数据抓取完毕\r\n");
                                    }
                                }));
                                int updatecount = 0;
                                string msgpush = string.Empty;
                                //解析数据
                                AnalysisResultHtml_TYC(html, key2, ref updatecount, ref msgpush);
                                this.Invoke(new MethodInvoker(delegate
                                {
                                    if (!string.IsNullOrEmpty(msgpush))
                                    {
                                        logwin.txt_log.AppendText("错误：" + msgpush);
                                        if (this._SaveRunningLog)
                                        {
                                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "错误：" + msgpush);
                                        }
                                    }
                                    logwin.txt_log.AppendText("更新成功：" + updatecount + "\r\n");
                                    if (this._SaveRunningLog)
                                    {
                                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "更新成功：" + updatecount + "\r\n");
                                    }
                                }));
                                watch.Stop();
                                this.Invoke(new MethodInvoker(delegate
                                {
                                    //logwin.txt_log.AppendText(html);
                                    logwin.txt_log.AppendText("===========================================\r\n");
                                    logwin.txt_log.AppendText("耗时：" + watch.Elapsed.TotalSeconds + "秒\r\n\r\n");
                                    if (this._SaveRunningLog)
                                    {
                                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "===========================================\r\n耗时：" + watch.Elapsed.TotalSeconds + "秒\r\n\r\n");
                                    }
                                }));
                            }
                            #endregion

                        }
                        catch (Exception ex)
                        {
                            if (ex is AggregateException)
                            {
                                AggregateException e = ex as AggregateException;
                                foreach (var item in e.InnerExceptions)
                                {
                                    LogHelper.WriteLog($"异常类型：{item.GetType()}<br/>来自：{item.Source}<br/>异常内容：{item.Message}");
                                }
                            }
                            else if (ex is WebDriverException)
                            {
                                if (ex.Message.Contains("Cannot start the driver service") || ex.Message.Contains("The HTTP request to the remote WebDriver server for URL"))
                                {
                                    driver.Quit();
                                    driver.Dispose();
                                    driver = null;
                                    driver = WebDriverHelper.CreateWebDriver(this._browser, this.chk_VisibleBrowser.Checked, false, this._ProxyIP);
                                    if (driver == null)
                                    {
                                        LogHelper.WriteLog(WebDriverHelper.ErrorMessage);
                                        this.Invoke(new MethodInvoker(delegate
                                        {
                                            logwin.txt_log.AppendText(WebDriverHelper.ErrorMessage + "\r\n");
                                            if (this._SaveRunningLog)
                                            {
                                                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", WebDriverHelper.ErrorMessage + "\r\n");
                                            }
                                        }));
                                    }
                                    else
                                    {
                                        _WebDriverList.Add(driver);
                                        driver.Navigate().GoToUrl(this._SourcePlatform.resulturl);
                                        driver.Manage().Cookies.DeleteAllCookies();
                                        foreach (var item in _ResultCookieList)
                                        {
                                            driver.Manage().Cookies.AddCookie(item);
                                        }
                                        driver.Navigate().Refresh();
                                    }
                                }
                            }
                            LogHelper.ErrorLog(ex.Message, ex);
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("===========================================\r\n");
                                logwin.txt_log.AppendText("第" + count + "次异常：" + ex.Message + ex.StackTrace + "\r\n\r\n");
                                if (this._SaveRunningLog)
                                {
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "===========================================\r\n第" + count + "次异常：" + ex.Message + ex.StackTrace + "\r\n\r\n");
                                }
                            }));
                            continue;
                        }
                    }
                });
            }
        }

        private void AnalysisResultHtml_TYC(string html, int key2, ref int updatecount, ref string msgpush)
        {
            if (_IsRunning)
            {
                html = html.Replace("&nbsp;", "").Replace("\r\n", "").Trim();
                html = html.Replace("<span class=\"leagueName xxxxxxxxxxxx\">", "$************************$<span class=\"leagueName xxxxxxxxxxxx\">");
                string[] htmlarr = html.Split(new string[] { "$************************$" }, StringSplitOptions.None);

                if (htmlarr.Length > 1 && this._EnableParallelLeagueMatch)//要并行
                {
                    int uc = 0;
                    string mp = string.Empty;
                    Parallel.ForEach(htmlarr, item =>
                    {
                        LoopResultHtml(item, key2, ref uc, ref mp);
                    });
                    updatecount += uc;
                    msgpush += mp;
                }
                else
                {
                    foreach (var item in htmlarr)
                    {
                        LoopResultHtml(item, key2, ref updatecount, ref msgpush);
                    }
                }

            }
        }
        //循环赛果
        private void LoopResultHtml(string item, int key2, ref int updatecount, ref string msgpush)
        {
            if (!string.IsNullOrEmpty(item))
            {
                bool isNCAA = false;//篮球专用
                string lnhtml = "<root>" + item + "</root>";
                var doc = new System.Xml.XmlDocument();
                doc.LoadXml(lnhtml);
                string LeagueMatchName = doc.DocumentElement.SelectSingleNode("span[1]").InnerText.Trim();
                if (!AnalysisHtmlHelper.FilterLeagueMatch(key2, LeagueMatchName))
                {
                    string msg = string.Empty;
                    A_LeagueMatchBll lmbll = new A_LeagueMatchBll();
                    A_TeamBll tbll = new A_TeamBll();
                    A_MatchBll mbll = new A_MatchBll();
                    A_MatchResultBll mrbll = new A_MatchResultBll();
                    DateTime now = lmbll.GetServerDateTime();
                    if (LeagueMatchName.Contains("NCAA"))
                    {
                        isNCAA = true;
                    }
                    //联赛
                    A_LeagueMatch objlm = null;
                    lock (locklist)
                    {
                        objlm = Main.Temp_A_LeagueMatch_Dict[this.Dict_SportsType[key2]].Where(x => x != null && x.Name == LeagueMatchName).OrderByDescending(x => x.ModifyTime).FirstOrDefault();
                    }
                    if (objlm == null)
                    {
                        objlm = new A_LeagueMatch()
                        {
                            ID = Guid.NewGuid().ToString("N"),
                            Name = LeagueMatchName,
                            ModifyTime = now,
                            SourcePlatform = SourcePlatformEnum.SB.ToString(),
                            SportsType = this.Dict_SportsType[key2]
                        };
                        msg = lmbll.Create(objlm);
                        if (string.IsNullOrEmpty(msg))
                        {
                            lock (locklist)
                            {
                                Main.Temp_A_LeagueMatch_Dict[this.Dict_SportsType[key2]].Add(objlm);
                            }
                        }
                        else
                        {
                            lock (locklist)
                            {
                                Main.Temp_A_LeagueMatch_Dict[this.Dict_SportsType[key2]] = lmbll.FindAll(SourcePlatformEnum.SB.ToString(), this.Dict_SportsType[key2]);
                            }
                            //Main.Temp_A_LeagueMatch_Dict[this.Dict_SportsType[key2]] = new ConcurrentBag<A_LeagueMatch>(lmbll.FindAll(SourcePlatformEnum.SB.ToString(), this.Dict_SportsType[key2]).AsEnumerable());
                        }
                    }
                    foreach (System.Xml.XmlNode tableRow in doc.DocumentElement.SelectNodes("div"))
                    {
                        if (tableRow.Attributes["class"].Value.Contains("tableRow"))
                        {
                            string status = string.Empty;
                            if (key2 == 1)//足球
                            {
                                status = tableRow.ChildNodes[5].FirstChild.InnerText.Trim();
                            }
                            else
                            {
                                status = tableRow.ChildNodes[3].FirstChild.InnerText.Trim();
                            }
                            if (status != "进行中" && status != "等待中")
                            {
                                //DateTime starttime = DateTime.ParseExact(tableRow.FirstChild.InnerText.Trim(), "MM/dd/yyyy hh:mm tt", System.Globalization.CultureInfo.CurrentCulture);
                                DateTime starttime = Convert.ToDateTime(tableRow.FirstChild.InnerText.Trim());//MM/dd/yyyy hh:mm tt
                                string htname = string.Empty, vtname = string.Empty;
                                var namedivs = tableRow.ChildNodes[1].SelectNodes("div[contains(@class,'name')]");
                                var htdiv = tableRow.ChildNodes[1];
                                var vtdiv = tableRow.ChildNodes[2];
                                if (key2 == 1)
                                {
                                    htname = namedivs[0].InnerText.Trim();
                                    vtname = namedivs[1].InnerText.Trim();
                                }
                                else
                                {
                                    htname = htdiv.FirstChild.FirstChild.InnerText.Trim();
                                    vtname = vtdiv.FirstChild.FirstChild.InnerText.Trim();
                                }

                                if (!string.IsNullOrEmpty(htname) && !string.IsNullOrEmpty(vtname))
                                {
                                    if (!htname.Contains("加时") && !htname.Contains("点球") && !vtname.Contains("加时") && !vtname.Contains("点球"))
                                    {
                                        //主队
                                        A_Team objtH = null;
                                        lock (locklist)
                                        {
                                            objtH = Main.Temp_A_Team_Dict[this.Dict_SportsType[key2]].Where(x => x != null && x.LeagueMatchID == objlm.ID && x.Name == htname).OrderByDescending(x => x.ModifyTime).FirstOrDefault();
                                        }
                                        string msg1 = string.Empty;
                                        if (objtH == null)
                                        {
                                            objtH = new A_Team()
                                            {
                                                ID = Guid.NewGuid().ToString("N"),
                                                LeagueMatchID = objlm.ID,
                                                Name = htname,
                                                ModifyTime = now,
                                                SourcePlatform = SourcePlatformEnum.SB.ToString(),
                                                SportsType = this.Dict_SportsType[key2]
                                            };
                                            msg1 = tbll.Create(objtH);
                                            if (string.IsNullOrEmpty(msg1))
                                            {
                                                lock (locklist)
                                                {
                                                    Main.Temp_A_Team_Dict[this.Dict_SportsType[key2]].Add(objtH);
                                                }
                                            }
                                            else
                                            {
                                                lock (locklist)
                                                {
                                                    Main.Temp_A_Team_Dict[this.Dict_SportsType[key2]] = tbll.FindAll(SourcePlatformEnum.SB.ToString(), this.Dict_SportsType[key2]);
                                                }
                                                //Main.Temp_A_Team_Dict[this.Dict_SportsType[key2]] = new ConcurrentBag<A_Team>(tbll.FindAll(SourcePlatformEnum.SB.ToString(), this.Dict_SportsType[key2]));
                                            }
                                        }
                                        //客队
                                        A_Team objtV = null;
                                        lock (locklist)
                                        {
                                            objtV = Main.Temp_A_Team_Dict[this.Dict_SportsType[key2]].Where(x => x != null && x.LeagueMatchID == objlm.ID && x.Name == vtname).OrderByDescending(x => x.ModifyTime).FirstOrDefault();
                                        }
                                        string msg2 = string.Empty;
                                        if (objtV == null)
                                        {
                                            objtV = new A_Team()
                                            {
                                                ID = Guid.NewGuid().ToString("N"),
                                                LeagueMatchID = objlm.ID,
                                                Name = vtname,
                                                ModifyTime = now,
                                                SourcePlatform = SourcePlatformEnum.SB.ToString(),
                                                SportsType = this.Dict_SportsType[key2]
                                            };
                                            msg2 = tbll.Create(objtV);
                                            if (string.IsNullOrEmpty(msg2))
                                            {
                                                lock (locklist)
                                                {
                                                    Main.Temp_A_Team_Dict[this.Dict_SportsType[key2]].Add(objtV);
                                                }
                                            }
                                            else
                                            {
                                                lock (locklist)
                                                {
                                                    Main.Temp_A_Team_Dict[this.Dict_SportsType[key2]] = tbll.FindAll(SourcePlatformEnum.SB.ToString(), this.Dict_SportsType[key2]);
                                                }
                                                //Main.Temp_A_Team_Dict[this.Dict_SportsType[key2]] = new ConcurrentBag<A_Team>(tbll.FindAll(SourcePlatformEnum.SB.ToString(), this.Dict_SportsType[key2]));
                                            }
                                        }
                                        if (string.IsNullOrEmpty(msg1) && string.IsNullOrEmpty(msg2))
                                        {
                                            string mid = null;
                                            A_Match objm = null;
                                            lock (locklist)
                                            {
                                                objm = Main.Temp_A_Match_Dict[this.Dict_SportsType[key2]].Where(x => x != null && x.HomeTeamID == objtH.ID && x.VisitingTeamID == objtV.ID && x.SP_GameStartTime.Value.Date == starttime.Date).OrderByDescending(x => x.ModifyTime).FirstOrDefault();
                                            }
                                            if (objm != null)
                                            {
                                                mid = objm.ID;
                                                objm.IsEnd = "1";
                                                objm.ModifyTime = now;
                                                //objm.GameEndTime = now;
                                                mbll.Update(objm);
                                            }
                                            A_MatchResult objmr = null;
                                            lock (locklist)
                                            {
                                                objmr = Main.Temp_A_MatchResult_Dict[this.Dict_SportsType[key2]].Where(x => x != null && x.LeagueMatchID == objlm.ID && x.HomeTeamID == objtH.ID && x.VisitingTeamID == objtV.ID && x.GameStartTime == starttime).OrderByDescending(x => x.CreateTime).FirstOrDefault();
                                            }
                                            if (objmr == null)
                                            {
                                                string hfull = string.Empty, vfull = string.Empty;
                                                string hh1 = string.Empty, vh1 = string.Empty;
                                                string hq1 = string.Empty,
                                                    hq2 = string.Empty,
                                                    hq3 = string.Empty,
                                                    hq4 = string.Empty,
                                                    hq5 = string.Empty,
                                                    hq6 = string.Empty,
                                                    hq7 = string.Empty,
                                                    hq8 = string.Empty,
                                                    hq9 = string.Empty,
                                                    hqex = string.Empty;
                                                string vq1 = string.Empty,
                                                    vq2 = string.Empty,
                                                    vq3 = string.Empty,
                                                    vq4 = string.Empty,
                                                    vq5 = string.Empty,
                                                    vq6 = string.Empty,
                                                    vq7 = string.Empty,
                                                    vq8 = string.Empty,
                                                    vq9 = string.Empty,
                                                    vqex = string.Empty;
                                                switch (key2)
                                                {
                                                    case 1://足球
                                                        #region 足球
                                                        int? hfhg = null, vfhg = null, hfullg = null, vfullg = null;
                                                        string fhscore = tableRow.ChildNodes[2].InnerText.Trim();
                                                        string fullscore = tableRow.ChildNodes[3].InnerText.Trim();
                                                        if (fhscore != "-" && !fhscore.Contains("退款"))
                                                        {
                                                            string[] fhs = fhscore.Split('-');
                                                            hfhg = Utility.ObjConvertToInt(fhs[0]);
                                                            vfhg = Utility.ObjConvertToInt(fhs[1]);
                                                        }
                                                        if (fullscore != "-" && !fullscore.Contains("退款"))
                                                        {
                                                            string[] fulls = fullscore.Split('-');
                                                            hfullg = Utility.ObjConvertToInt(fulls[0]);
                                                            vfullg = Utility.ObjConvertToInt(fulls[1]);
                                                        }

                                                        //进球顺序
                                                        string jqsx = string.Empty;
                                                        //if (tableRow.ChildNodes[4].ChildNodes.Count != 0)
                                                        //{
                                                        //    string id = tableRow.ChildNodes[4].FirstChild.Attributes["onclick"].Value;
                                                        //    id = id.Replace("OpenSoccerDetail(this,", "").Replace(");", "").Trim(); //OpenSoccerDetail(this, 27256330);
                                                        //    var expandArea = doc.DocumentElement.SelectSingleNode("//span[@id='Detail_" + id + "']");
                                                        //    if (expandArea != null)
                                                        //    {
                                                        //        //if (expandArea.LastChild.Attributes["class"].Value.Contains("col1"))
                                                        //        //{
                                                        //        //    jqsx = expandArea.LastChild.LastChild.FirstChild.FirstChild.InnerText.Trim();
                                                        //        //}
                                                        //        foreach (System.Xml.XmlNode col1 in expandArea.SelectNodes("div[contains(@class,'col1')]"))
                                                        //        {
                                                        //            if (col1.FirstChild.FirstChild.InnerText.Trim() == "进球顺序")
                                                        //            {
                                                        //                jqsx = col1.LastChild.FirstChild.FirstChild.InnerText.Trim();
                                                        //                break;
                                                        //            }
                                                        //        }
                                                        //    }
                                                        //}
                                                        objmr = new A_MatchResult()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            LeagueMatchID = objlm.ID,
                                                            MatchID = mid,
                                                            SportsType = this.Dict_SportsType[key2],
                                                            HomeTeamID = objtH.ID,
                                                            VisitingTeamID = objtV.ID,
                                                            SourcePlatform = SourcePlatformEnum.SB.ToString(),
                                                            GameStartTime = starttime,
                                                            HomeTeamScore1H = hfhg,
                                                            VisitingTeamScore1H = vfhg,
                                                            HomeTeamScore = hfullg,
                                                            VisitingTeamScore = vfullg,
                                                            GoalOrder = jqsx,
                                                            Status = status,
                                                            CreateTime = now
                                                        };
                                                        #endregion
                                                        break;
                                                    case 2://篮球
                                                        #region 篮球
                                                        objmr = new A_MatchResult()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            LeagueMatchID = objlm.ID,
                                                            MatchID = mid,
                                                            SportsType = this.Dict_SportsType[key2],
                                                            HomeTeamID = objtH.ID,
                                                            VisitingTeamID = objtV.ID,
                                                            SourcePlatform = SourcePlatformEnum.SB.ToString(),
                                                            GameStartTime = starttime,
                                                            Status = status,
                                                            CreateTime = now
                                                        };
                                                        if (isNCAA)//NCAA只分上下半场
                                                        {
                                                            #region 折叠
                                                            hq1 = htdiv.ChildNodes[1].InnerText.Trim();
                                                            hq2 = htdiv.ChildNodes[2].InnerText.Trim();
                                                            hqex = htdiv.ChildNodes[3].InnerText.Trim();
                                                            hh1 = htdiv.ChildNodes[4].InnerText.Trim();
                                                            hfull = htdiv.ChildNodes[5].InnerText.Trim();

                                                            vq1 = vtdiv.ChildNodes[1].InnerText.Trim();
                                                            vq2 = vtdiv.ChildNodes[2].InnerText.Trim();
                                                            vqex = vtdiv.ChildNodes[3].InnerText.Trim();
                                                            vh1 = vtdiv.ChildNodes[4].InnerText.Trim();
                                                            vfull = vtdiv.ChildNodes[5].InnerText.Trim();

                                                            if (hq1 != "-" && !hq1.Contains("退款"))
                                                            {
                                                                objmr.HomeTeamScore1Q = Convert.ToInt32(hq1); ;
                                                            }
                                                            if (hq2 != "-" && !hq2.Contains("退款"))
                                                            {
                                                                objmr.HomeTeamScore2Q = Convert.ToInt32(hq2); ;
                                                            }
                                                            if (hqex != "-" && !hqex.Contains("退款"))
                                                            {
                                                                objmr.HomeTeamScoreEX = Convert.ToInt32(hqex); ;
                                                            }
                                                            if (hh1 != "-" && !hh1.Contains("退款"))
                                                            {
                                                                objmr.HomeTeamScore1H = Convert.ToInt32(hh1); ;
                                                            }
                                                            if (hfull != "-" && !hfull.Contains("退款"))
                                                            {
                                                                objmr.HomeTeamScore = Convert.ToInt32(hfull); ;
                                                            }

                                                            if (vq1 != "-" && !vq1.Contains("退款"))
                                                            {
                                                                objmr.VisitingTeamScore1Q = Convert.ToInt32(vq1); ;
                                                            }
                                                            if (vq2 != "-" && !vq2.Contains("退款"))
                                                            {
                                                                objmr.VisitingTeamScore2Q = Convert.ToInt32(vq2); ;
                                                            }
                                                            if (vqex != "-" && !vqex.Contains("退款"))
                                                            {
                                                                objmr.VisitingTeamScoreEX = Convert.ToInt32(vqex); ;
                                                            }
                                                            if (vh1 != "-" && !vh1.Contains("退款"))
                                                            {
                                                                objmr.VisitingTeamScore1H = Convert.ToInt32(vh1); ;
                                                            }
                                                            if (vfull != "-" && !vfull.Contains("退款"))
                                                            {
                                                                objmr.VisitingTeamScore = Convert.ToInt32(vfull); ;
                                                            }
                                                            #endregion
                                                        }
                                                        else
                                                        {
                                                            #region 折叠
                                                            hq1 = htdiv.ChildNodes[1].InnerText.Trim();
                                                            hq2 = htdiv.ChildNodes[2].InnerText.Trim();
                                                            hq3 = htdiv.ChildNodes[3].InnerText.Trim();
                                                            hq4 = htdiv.ChildNodes[4].InnerText.Trim();
                                                            hqex = htdiv.ChildNodes[5].InnerText.Trim();
                                                            hh1 = htdiv.ChildNodes[6].InnerText.Trim();
                                                            hfull = htdiv.ChildNodes[7].InnerText.Trim();

                                                            vq1 = vtdiv.ChildNodes[1].InnerText.Trim();
                                                            vq2 = vtdiv.ChildNodes[2].InnerText.Trim();
                                                            vq3 = vtdiv.ChildNodes[3].InnerText.Trim();
                                                            vq4 = vtdiv.ChildNodes[4].InnerText.Trim();
                                                            vqex = vtdiv.ChildNodes[5].InnerText.Trim();
                                                            vh1 = vtdiv.ChildNodes[6].InnerText.Trim();
                                                            vfull = vtdiv.ChildNodes[7].InnerText.Trim();

                                                            if (hq1 != "-" && !hq1.Contains("退款"))
                                                            {
                                                                objmr.HomeTeamScore1Q = Convert.ToInt32(hq1); ;
                                                            }
                                                            if (hq2 != "-" && !hq2.Contains("退款"))
                                                            {
                                                                objmr.HomeTeamScore2Q = Convert.ToInt32(hq2); ;
                                                            }
                                                            if (hq3 != "-" && !hq3.Contains("退款"))
                                                            {
                                                                objmr.HomeTeamScore3Q = Convert.ToInt32(hq3); ;
                                                            }
                                                            if (hq4 != "-" && !hq4.Contains("退款"))
                                                            {
                                                                objmr.HomeTeamScore4Q = Convert.ToInt32(hq4); ;
                                                            }
                                                            if (hqex != "-" && !hqex.Contains("退款"))
                                                            {
                                                                objmr.HomeTeamScoreEX = Convert.ToInt32(hqex); ;
                                                            }
                                                            if (hh1 != "-" && !hh1.Contains("退款"))
                                                            {
                                                                objmr.HomeTeamScore1H = Convert.ToInt32(hh1); ;
                                                            }
                                                            if (hfull != "-" && !hfull.Contains("退款"))
                                                            {
                                                                objmr.HomeTeamScore = Convert.ToInt32(hfull); ;
                                                            }

                                                            if (vq1 != "-" && !vq1.Contains("退款"))
                                                            {
                                                                objmr.VisitingTeamScore1Q = Convert.ToInt32(vq1); ;
                                                            }
                                                            if (vq2 != "-" && !vq2.Contains("退款"))
                                                            {
                                                                objmr.VisitingTeamScore2Q = Convert.ToInt32(vq2); ;
                                                            }
                                                            if (vq3 != "-" && !vq3.Contains("退款"))
                                                            {
                                                                objmr.VisitingTeamScore3Q = Convert.ToInt32(vq3); ;
                                                            }
                                                            if (vq4 != "-" && !vq4.Contains("退款"))
                                                            {
                                                                objmr.VisitingTeamScore4Q = Convert.ToInt32(vq4); ;
                                                            }
                                                            if (vqex != "-" && !vqex.Contains("退款"))
                                                            {
                                                                objmr.VisitingTeamScoreEX = Convert.ToInt32(vqex); ;
                                                            }
                                                            if (vh1 != "-" && !vh1.Contains("退款"))
                                                            {
                                                                objmr.VisitingTeamScore1H = Convert.ToInt32(vh1); ;
                                                            }
                                                            if (vfull != "-" && !vfull.Contains("退款"))
                                                            {
                                                                objmr.VisitingTeamScore = Convert.ToInt32(vfull); ;
                                                            }
                                                            #endregion
                                                        }
                                                        #endregion
                                                        break;
                                                    case 3://网球
                                                        #region 网球
                                                        objmr = new A_MatchResult()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            LeagueMatchID = objlm.ID,
                                                            MatchID = mid,
                                                            SportsType = this.Dict_SportsType[key2],
                                                            HomeTeamID = objtH.ID,
                                                            VisitingTeamID = objtV.ID,
                                                            SourcePlatform = SourcePlatformEnum.SB.ToString(),
                                                            GameStartTime = starttime,
                                                            Status = status,
                                                            CreateTime = now
                                                        };
                                                        hq1 = htdiv.ChildNodes[1].InnerText.Trim();
                                                        hq2 = htdiv.ChildNodes[2].InnerText.Trim();
                                                        hq3 = htdiv.ChildNodes[3].InnerText.Trim();
                                                        hh1 = htdiv.ChildNodes[4].InnerText.Trim();
                                                        hfull = htdiv.ChildNodes[5].InnerText.Trim();

                                                        vq1 = vtdiv.ChildNodes[1].InnerText.Trim();
                                                        vq2 = vtdiv.ChildNodes[2].InnerText.Trim();
                                                        vq3 = vtdiv.ChildNodes[3].InnerText.Trim();
                                                        vh1 = vtdiv.ChildNodes[4].InnerText.Trim();
                                                        vfull = vtdiv.ChildNodes[5].InnerText.Trim();

                                                        if (hq1 != "-" && !hq1.Contains("退款"))
                                                        {
                                                            objmr.HomeTeamScore1Q = Convert.ToInt32(hq1); ;
                                                        }
                                                        if (hq2 != "-" && !hq2.Contains("退款"))
                                                        {
                                                            objmr.HomeTeamScore2Q = Convert.ToInt32(hq2); ;
                                                        }
                                                        if (hq3 != "-" && !hq3.Contains("退款"))
                                                        {
                                                            objmr.HomeTeamScore3Q = Convert.ToInt32(hq3); ;
                                                        }
                                                        if (hh1 != "-" && !hh1.Contains("退款"))
                                                        {
                                                            objmr.HomeTeamScore1H = Convert.ToInt32(hh1); ;
                                                        }
                                                        if (hfull != "-" && !hfull.Contains("退款"))
                                                        {
                                                            objmr.HomeTeamScore = Convert.ToInt32(hfull); ;
                                                        }

                                                        if (vq1 != "-" && !vq1.Contains("退款"))
                                                        {
                                                            objmr.VisitingTeamScore1Q = Convert.ToInt32(vq1); ;
                                                        }
                                                        if (vq2 != "-" && !vq2.Contains("退款"))
                                                        {
                                                            objmr.VisitingTeamScore2Q = Convert.ToInt32(vq2); ;
                                                        }
                                                        if (vq3 != "-" && !vq3.Contains("退款"))
                                                        {
                                                            objmr.VisitingTeamScore3Q = Convert.ToInt32(vq3); ;
                                                        }
                                                        if (vh1 != "-" && !vh1.Contains("退款"))
                                                        {
                                                            objmr.VisitingTeamScore1H = Convert.ToInt32(vh1); ;
                                                        }
                                                        if (vfull != "-" && !vfull.Contains("退款"))
                                                        {
                                                            objmr.VisitingTeamScore = Convert.ToInt32(vfull); ;
                                                        }
                                                        #endregion
                                                        break;
                                                    case 4://排球
                                                        #region 排球
                                                        objmr = new A_MatchResult()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            LeagueMatchID = objlm.ID,
                                                            MatchID = mid,
                                                            SportsType = this.Dict_SportsType[key2],
                                                            HomeTeamID = objtH.ID,
                                                            VisitingTeamID = objtV.ID,
                                                            SourcePlatform = SourcePlatformEnum.SB.ToString(),
                                                            GameStartTime = starttime,
                                                            Status = status,
                                                            CreateTime = now
                                                        };
                                                        hq1 = htdiv.ChildNodes[1].InnerText.Trim();
                                                        hq2 = htdiv.ChildNodes[2].InnerText.Trim();
                                                        hq3 = htdiv.ChildNodes[3].InnerText.Trim();
                                                        hq4 = htdiv.ChildNodes[4].InnerText.Trim();
                                                        hq5 = htdiv.ChildNodes[5].InnerText.Trim();
                                                        hh1 = htdiv.ChildNodes[6].InnerText.Trim();
                                                        hfull = htdiv.ChildNodes[7].InnerText.Trim();

                                                        vq1 = vtdiv.ChildNodes[1].InnerText.Trim();
                                                        vq2 = vtdiv.ChildNodes[2].InnerText.Trim();
                                                        vq3 = vtdiv.ChildNodes[3].InnerText.Trim();
                                                        vq4 = vtdiv.ChildNodes[4].InnerText.Trim();
                                                        vq5 = vtdiv.ChildNodes[5].InnerText.Trim();
                                                        vh1 = vtdiv.ChildNodes[6].InnerText.Trim();
                                                        vfull = vtdiv.ChildNodes[7].InnerText.Trim();

                                                        if (hq1 != "-" && !hq1.Contains("退款"))
                                                        {
                                                            objmr.HomeTeamScore1Q = Convert.ToInt32(hq1); ;
                                                        }
                                                        if (hq2 != "-" && !hq2.Contains("退款"))
                                                        {
                                                            objmr.HomeTeamScore2Q = Convert.ToInt32(hq2); ;
                                                        }
                                                        if (hq3 != "-" && !hq3.Contains("退款"))
                                                        {
                                                            objmr.HomeTeamScore3Q = Convert.ToInt32(hq3); ;
                                                        }
                                                        if (hq4 != "-" && !hq4.Contains("退款"))
                                                        {
                                                            objmr.HomeTeamScore3Q = Convert.ToInt32(hq4); ;
                                                        }
                                                        if (hq5 != "-" && !hq5.Contains("退款"))
                                                        {
                                                            objmr.HomeTeamScore3Q = Convert.ToInt32(hq5); ;
                                                        }
                                                        if (hh1 != "-" && !hh1.Contains("退款"))
                                                        {
                                                            objmr.HomeTeamScore1H = Convert.ToInt32(hh1); ;
                                                        }
                                                        if (hfull != "-" && !hfull.Contains("退款"))
                                                        {
                                                            objmr.HomeTeamScore = Convert.ToInt32(hfull); ;
                                                        }

                                                        if (vq1 != "-" && !vq1.Contains("退款"))
                                                        {
                                                            objmr.VisitingTeamScore1Q = Convert.ToInt32(vq1); ;
                                                        }
                                                        if (vq2 != "-" && !vq2.Contains("退款"))
                                                        {
                                                            objmr.VisitingTeamScore2Q = Convert.ToInt32(vq2); ;
                                                        }
                                                        if (vq3 != "-" && !vq3.Contains("退款"))
                                                        {
                                                            objmr.VisitingTeamScore3Q = Convert.ToInt32(vq3); ;
                                                        }
                                                        if (vq4 != "-" && !vq4.Contains("退款"))
                                                        {
                                                            objmr.VisitingTeamScore3Q = Convert.ToInt32(vq4); ;
                                                        }
                                                        if (vq5 != "-" && !vq5.Contains("退款"))
                                                        {
                                                            objmr.VisitingTeamScore3Q = Convert.ToInt32(vq5); ;
                                                        }
                                                        if (vh1 != "-" && !vh1.Contains("退款"))
                                                        {
                                                            objmr.VisitingTeamScore1H = Convert.ToInt32(vh1); ;
                                                        }
                                                        if (vfull != "-" && !vfull.Contains("退款"))
                                                        {
                                                            objmr.VisitingTeamScore = Convert.ToInt32(vfull); ;
                                                        }
                                                        #endregion
                                                        break;
                                                    case 5://棒球
                                                        #region 棒球
                                                        objmr = new A_MatchResult()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            LeagueMatchID = objlm.ID,
                                                            MatchID = mid,
                                                            SportsType = this.Dict_SportsType[key2],
                                                            HomeTeamID = objtH.ID,
                                                            VisitingTeamID = objtV.ID,
                                                            SourcePlatform = SourcePlatformEnum.SB.ToString(),
                                                            GameStartTime = starttime,
                                                            Status = status,
                                                            CreateTime = now
                                                        };
                                                        hq1 = htdiv.ChildNodes[1].InnerText.Trim();
                                                        hq2 = htdiv.ChildNodes[2].InnerText.Trim();
                                                        hq3 = htdiv.ChildNodes[3].InnerText.Trim();
                                                        hq4 = htdiv.ChildNodes[4].InnerText.Trim();
                                                        hq5 = htdiv.ChildNodes[5].InnerText.Trim();
                                                        hq6 = htdiv.ChildNodes[6].InnerText.Trim();
                                                        hq7 = htdiv.ChildNodes[7].InnerText.Trim();
                                                        hq8 = htdiv.ChildNodes[8].InnerText.Trim();
                                                        hq9 = htdiv.ChildNodes[9].InnerText.Trim();
                                                        hqex = htdiv.ChildNodes[10].InnerText.Trim();
                                                        hfull = htdiv.ChildNodes[11].InnerText.Trim();

                                                        vq1 = vtdiv.ChildNodes[1].InnerText.Trim();
                                                        vq2 = vtdiv.ChildNodes[2].InnerText.Trim();
                                                        vq3 = vtdiv.ChildNodes[3].InnerText.Trim();
                                                        vq4 = vtdiv.ChildNodes[4].InnerText.Trim();
                                                        vq5 = vtdiv.ChildNodes[5].InnerText.Trim();
                                                        vq6 = vtdiv.ChildNodes[6].InnerText.Trim();
                                                        vq7 = vtdiv.ChildNodes[7].InnerText.Trim();
                                                        vq8 = vtdiv.ChildNodes[8].InnerText.Trim();
                                                        vq9 = vtdiv.ChildNodes[9].InnerText.Trim();
                                                        vqex = vtdiv.ChildNodes[10].InnerText.Trim();
                                                        vfull = vtdiv.ChildNodes[11].InnerText.Trim();

                                                        if (hq1 != "-" && !hq1.Contains("退款"))
                                                        {
                                                            objmr.HomeTeamScore1Q = Convert.ToInt32(hq1); ;
                                                        }
                                                        if (hq2 != "-" && !hq2.Contains("退款"))
                                                        {
                                                            objmr.HomeTeamScore2Q = Convert.ToInt32(hq2); ;
                                                        }
                                                        if (hq3 != "-" && !hq3.Contains("退款"))
                                                        {
                                                            objmr.HomeTeamScore3Q = Convert.ToInt32(hq3); ;
                                                        }
                                                        if (hq4 != "-" && !hq4.Contains("退款"))
                                                        {
                                                            objmr.HomeTeamScore4Q = Convert.ToInt32(hq4); ;
                                                        }
                                                        if (hq5 != "-" && !hq5.Contains("退款"))
                                                        {
                                                            objmr.HomeTeamScore4Q = Convert.ToInt32(hq5); ;
                                                        }
                                                        if (hq6 != "-" && !hq6.Contains("退款"))
                                                        {
                                                            objmr.HomeTeamScore4Q = Convert.ToInt32(hq6); ;
                                                        }
                                                        if (hq7 != "-" && !hq7.Contains("退款"))
                                                        {
                                                            objmr.HomeTeamScore4Q = Convert.ToInt32(hq7); ;
                                                        }
                                                        if (hq8 != "-" && !hq8.Contains("退款"))
                                                        {
                                                            objmr.HomeTeamScore4Q = Convert.ToInt32(hq8); ;
                                                        }
                                                        if (hq9 != "-" && !hq9.Contains("退款"))
                                                        {
                                                            objmr.HomeTeamScore4Q = Convert.ToInt32(hq9); ;
                                                        }
                                                        if (hqex != "-" && !hqex.Contains("退款"))
                                                        {
                                                            objmr.HomeTeamScoreEX = Convert.ToInt32(hqex); ;
                                                        }
                                                        if (hfull != "-" && !hfull.Contains("退款"))
                                                        {
                                                            objmr.HomeTeamScore = Convert.ToInt32(hfull); ;
                                                        }

                                                        if (vq1 != "-" && !vq1.Contains("退款"))
                                                        {
                                                            objmr.VisitingTeamScore1Q = Convert.ToInt32(vq1); ;
                                                        }
                                                        if (vq2 != "-" && !vq2.Contains("退款"))
                                                        {
                                                            objmr.VisitingTeamScore2Q = Convert.ToInt32(vq2); ;
                                                        }
                                                        if (vq3 != "-" && !vq3.Contains("退款"))
                                                        {
                                                            objmr.VisitingTeamScore3Q = Convert.ToInt32(vq3); ;
                                                        }
                                                        if (vq4 != "-" && !vq4.Contains("退款"))
                                                        {
                                                            objmr.VisitingTeamScore4Q = Convert.ToInt32(vq4); ;
                                                        }
                                                        if (vq5 != "-" && !vq5.Contains("退款"))
                                                        {
                                                            objmr.VisitingTeamScore4Q = Convert.ToInt32(vq5); ;
                                                        }
                                                        if (vq6 != "-" && !vq6.Contains("退款"))
                                                        {
                                                            objmr.VisitingTeamScore4Q = Convert.ToInt32(vq6); ;
                                                        }
                                                        if (vq7 != "-" && !vq7.Contains("退款"))
                                                        {
                                                            objmr.VisitingTeamScore4Q = Convert.ToInt32(vq7); ;
                                                        }
                                                        if (vq8 != "-" && !vq8.Contains("退款"))
                                                        {
                                                            objmr.VisitingTeamScore4Q = Convert.ToInt32(vq8); ;
                                                        }
                                                        if (vq9 != "-" && !vq9.Contains("退款"))
                                                        {
                                                            objmr.VisitingTeamScore4Q = Convert.ToInt32(vq9); ;
                                                        }
                                                        if (vqex != "-" && !vqex.Contains("退款"))
                                                        {
                                                            objmr.VisitingTeamScoreEX = Convert.ToInt32(vqex); ;
                                                        }
                                                        if (vfull != "-" && !vfull.Contains("退款"))
                                                        {
                                                            objmr.VisitingTeamScore = Convert.ToInt32(vfull); ;
                                                        }
                                                        #endregion
                                                        break;
                                                    case 6://羽毛球
                                                        #region 羽毛球
                                                        objmr = new A_MatchResult()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            LeagueMatchID = objlm.ID,
                                                            MatchID = mid,
                                                            SportsType = this.Dict_SportsType[key2],
                                                            HomeTeamID = objtH.ID,
                                                            VisitingTeamID = objtV.ID,
                                                            SourcePlatform = SourcePlatformEnum.SB.ToString(),
                                                            GameStartTime = starttime,
                                                            Status = status,
                                                            CreateTime = now
                                                        };
                                                        hq1 = htdiv.ChildNodes[1].InnerText.Trim();
                                                        hq2 = htdiv.ChildNodes[2].InnerText.Trim();
                                                        hq3 = htdiv.ChildNodes[3].InnerText.Trim();
                                                        hh1 = htdiv.ChildNodes[4].InnerText.Trim();
                                                        hfull = htdiv.ChildNodes[5].InnerText.Trim();

                                                        vq1 = vtdiv.ChildNodes[1].InnerText.Trim();
                                                        vq2 = vtdiv.ChildNodes[2].InnerText.Trim();
                                                        vq3 = vtdiv.ChildNodes[3].InnerText.Trim();
                                                        vh1 = vtdiv.ChildNodes[4].InnerText.Trim();
                                                        vfull = vtdiv.ChildNodes[5].InnerText.Trim();

                                                        if (hq1 != "-" && !hq1.Contains("退款"))
                                                        {
                                                            objmr.HomeTeamScore1Q = Convert.ToInt32(hq1); ;
                                                        }
                                                        if (hq2 != "-" && !hq2.Contains("退款"))
                                                        {
                                                            objmr.HomeTeamScore2Q = Convert.ToInt32(hq2); ;
                                                        }
                                                        if (hq3 != "-" && !hq3.Contains("退款"))
                                                        {
                                                            objmr.HomeTeamScore3Q = Convert.ToInt32(hq3); ;
                                                        }
                                                        if (hh1 != "-" && !hh1.Contains("退款"))
                                                        {
                                                            objmr.HomeTeamScore1H = Convert.ToInt32(hh1); ;
                                                        }
                                                        if (hfull != "-" && !hfull.Contains("退款"))
                                                        {
                                                            objmr.HomeTeamScore = Convert.ToInt32(hfull); ;
                                                        }

                                                        if (vq1 != "-" && !vq1.Contains("退款"))
                                                        {
                                                            objmr.VisitingTeamScore1Q = Convert.ToInt32(vq1); ;
                                                        }
                                                        if (vq2 != "-" && !vq2.Contains("退款"))
                                                        {
                                                            objmr.VisitingTeamScore2Q = Convert.ToInt32(vq2); ;
                                                        }
                                                        if (vq3 != "-" && !vq3.Contains("退款"))
                                                        {
                                                            objmr.VisitingTeamScore3Q = Convert.ToInt32(vq3); ;
                                                        }
                                                        if (vh1 != "-" && !vh1.Contains("退款"))
                                                        {
                                                            objmr.VisitingTeamScore1H = Convert.ToInt32(vh1); ;
                                                        }
                                                        if (vfull != "-" && !vfull.Contains("退款"))
                                                        {
                                                            objmr.VisitingTeamScore = Convert.ToInt32(vfull); ;
                                                        }
                                                        #endregion
                                                        break;
                                                    case 7://乒乓球
                                                        #region 乒乓球
                                                        objmr = new A_MatchResult()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            LeagueMatchID = objlm.ID,
                                                            MatchID = mid,
                                                            SportsType = this.Dict_SportsType[key2],
                                                            HomeTeamID = objtH.ID,
                                                            VisitingTeamID = objtV.ID,
                                                            SourcePlatform = SourcePlatformEnum.SB.ToString(),
                                                            GameStartTime = starttime,
                                                            Status = status,
                                                            CreateTime = now
                                                        };
                                                        hfull = htdiv.ChildNodes[1].InnerText.Trim();
                                                        vfull = vtdiv.ChildNodes[1].InnerText.Trim();

                                                        if (hfull != "-" && !hfull.Contains("退款"))
                                                        {
                                                            objmr.HomeTeamScore = Convert.ToInt32(hfull); ;
                                                        }
                                                        if (vfull != "-" && !vfull.Contains("退款"))
                                                        {
                                                            objmr.VisitingTeamScore = Convert.ToInt32(vfull); ;
                                                        }
                                                        #endregion
                                                        break;
                                                    case 8://美式足球
                                                        #region 美式足球
                                                        objmr = new A_MatchResult()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            LeagueMatchID = objlm.ID,
                                                            MatchID = mid,
                                                            SportsType = this.Dict_SportsType[key2],
                                                            HomeTeamID = objtH.ID,
                                                            VisitingTeamID = objtV.ID,
                                                            SourcePlatform = SourcePlatformEnum.SB.ToString(),
                                                            GameStartTime = starttime,
                                                            Status = status,
                                                            CreateTime = now
                                                        };
                                                        hq1 = htdiv.ChildNodes[1].InnerText.Trim();
                                                        hq2 = htdiv.ChildNodes[2].InnerText.Trim();
                                                        hq3 = htdiv.ChildNodes[3].InnerText.Trim();
                                                        hq4 = htdiv.ChildNodes[4].InnerText.Trim();
                                                        hqex = htdiv.ChildNodes[5].InnerText.Trim();
                                                        hh1 = htdiv.ChildNodes[6].InnerText.Trim();
                                                        hfull = htdiv.ChildNodes[7].InnerText.Trim();

                                                        vq1 = vtdiv.ChildNodes[1].InnerText.Trim();
                                                        vq2 = vtdiv.ChildNodes[2].InnerText.Trim();
                                                        vq3 = vtdiv.ChildNodes[3].InnerText.Trim();
                                                        vq4 = vtdiv.ChildNodes[4].InnerText.Trim();
                                                        vqex = vtdiv.ChildNodes[5].InnerText.Trim();
                                                        vh1 = vtdiv.ChildNodes[6].InnerText.Trim();
                                                        vfull = vtdiv.ChildNodes[7].InnerText.Trim();

                                                        if (hq1 != "-" && !hq1.Contains("退款"))
                                                        {
                                                            objmr.HomeTeamScore1Q = Convert.ToInt32(hq1); ;
                                                        }
                                                        if (hq2 != "-" && !hq2.Contains("退款"))
                                                        {
                                                            objmr.HomeTeamScore2Q = Convert.ToInt32(hq2); ;
                                                        }
                                                        if (hq3 != "-" && !hq3.Contains("退款"))
                                                        {
                                                            objmr.HomeTeamScore3Q = Convert.ToInt32(hq3); ;
                                                        }
                                                        if (hq4 != "-" && !hq4.Contains("退款"))
                                                        {
                                                            objmr.HomeTeamScore4Q = Convert.ToInt32(hq4); ;
                                                        }
                                                        if (hqex != "-" && !hqex.Contains("退款"))
                                                        {
                                                            objmr.HomeTeamScoreEX = Convert.ToInt32(hqex); ;
                                                        }
                                                        if (hh1 != "-" && !hh1.Contains("退款"))
                                                        {
                                                            objmr.HomeTeamScore1H = Convert.ToInt32(hh1); ;
                                                        }
                                                        if (hfull != "-" && !hfull.Contains("退款"))
                                                        {
                                                            objmr.HomeTeamScore = Convert.ToInt32(hfull); ;
                                                        }

                                                        if (vq1 != "-" && !vq1.Contains("退款"))
                                                        {
                                                            objmr.VisitingTeamScore1Q = Convert.ToInt32(vq1); ;
                                                        }
                                                        if (vq2 != "-" && !vq2.Contains("退款"))
                                                        {
                                                            objmr.VisitingTeamScore2Q = Convert.ToInt32(vq2); ;
                                                        }
                                                        if (vq3 != "-" && !vq3.Contains("退款"))
                                                        {
                                                            objmr.VisitingTeamScore3Q = Convert.ToInt32(vq3); ;
                                                        }
                                                        if (vq4 != "-" && !vq4.Contains("退款"))
                                                        {
                                                            objmr.VisitingTeamScore4Q = Convert.ToInt32(vq4); ;
                                                        }
                                                        if (vqex != "-" && !vqex.Contains("退款"))
                                                        {
                                                            objmr.VisitingTeamScoreEX = Convert.ToInt32(vqex); ;
                                                        }
                                                        if (vh1 != "-" && !vh1.Contains("退款"))
                                                        {
                                                            objmr.VisitingTeamScore1H = Convert.ToInt32(vh1); ;
                                                        }
                                                        if (vfull != "-" && !vfull.Contains("退款"))
                                                        {
                                                            objmr.VisitingTeamScore = Convert.ToInt32(vfull); ;
                                                        }
                                                        #endregion
                                                        break;
                                                }
                                                msg = mrbll.Create(objmr);
                                                if (string.IsNullOrEmpty(msg))
                                                {
                                                    updatecount++;
                                                    lock (locklist)
                                                    {
                                                        Main.Temp_A_MatchResult_Dict[this.Dict_SportsType[key2]].Add(objmr);
                                                    }
                                                }
                                                else
                                                {
                                                    msgpush += msg + "\r\n";
                                                    lock (locklist)
                                                    {
                                                        Main.Temp_A_MatchResult_Dict[this.Dict_SportsType[key2]] = mrbll.FindByDate(SourcePlatformEnum.SB.ToString(), this.Dict_SportsType[key2], now.Date.AddDays(-1));
                                                    }
                                                    //Main.Temp_A_MatchResult_Dict[this.Dict_SportsType[key2]] = new ConcurrentBag<A_MatchResult>(mrbll.FindByDate(SourcePlatformEnum.SB.ToString(), this.Dict_SportsType[key2], now.Date.AddDays(-1)));
                                                }
                                            }
                                        }

                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        private void timer2_Tick(object sender, EventArgs e)
        {
            lock (lockwin)
            {
                InitTempData();
            }
        }

        /// <summary>
        /// 获取维护信息
        /// </summary>
        /// <param name="logwin"></param>
        private void GetMaintenanceInfo(Log logwin)
        {
            IWebDriver driver = WebDriverHelper.CreateWebDriver(this._browser, false, false, this._ProxyIP);
            if (driver == null)
            {
                //MessageBox.Show(WebDriverHelper.ErrorMessage);
                LogHelper.WriteLog(WebDriverHelper.ErrorMessage);
                return;
            }
            driver.Navigate().GoToUrl(this._SourcePlatform.UMurl);
            int count = 0;
            bool flag01 = false;
            while (!flag01)
            {
                count++;
                flag01 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#mainframe"), _WebDriver_MS);
                if (count > 30)
                {
                    flag01 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#sportsFrame"), _WebDriver_MS);
                    if (flag01)//查看是否维护中
                    {
                        Main._IsMaintenance = false;
                        driver.Quit();
                        return;
                    }
                }
            }
            driver.SwitchTo().Frame("mainframe");
            var div_cs_noInfo = driver.FindElement(By.CssSelector(".bgcpe #div_cs_noInfo"));
            string title = div_cs_noInfo.FindElement(By.CssSelector("h1")).GetAttribute("innerText");
            string info = div_cs_noInfo.FindElement(By.CssSelector(".UdrDogTeamClass h3")).GetAttribute("innerHTML").Trim();
            info = info.Substring(0, info.IndexOf("<br>"));
            string time = div_cs_noInfo.FindElement(By.CssSelector(".FavOddsClass")).GetAttribute("innerText").Trim();
            this.Invoke(new MethodInvoker(delegate
            {
                if (logwin == null)
                {
                    MessageBox.Show(title + "\r\n" + info + "\r\n预计恢复时间：" + time + "\r\n");
                }
                else
                {
                    logwin.txt_log.AppendText(title + "\r\n" + info + "\r\n预计恢复时间：" + time + "\r\n");
                    if (this._SaveRunningLog)
                    {
                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", title + "\r\n" + info + "\r\n预计恢复时间：" + time + "\r\n");
                    }
                }
                //this.btn_Stop.PerformClick();
            }));
            driver.Quit();
        }

        private void tvw_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            TreeView tvw = sender as TreeView;
            foreach (TreeNode node in tvw.Nodes)
            {
                node.Collapse();
            }
        }

        private void tvw_MouseClick(object sender, MouseEventArgs e)
        {
            TreeView tvw = sender as TreeView;
            TreeNode node = tvw.GetNodeAt(new Point(e.X, e.Y));
            if (node != null)
            {
                if (_IsRunning)
                {
                    if (node.Checked)
                    {
                        if (node.Tag is Log)
                        {
                            Log l = node.Tag as Log;
                            l.Show();
                        }
                    }
                }
            }
        }

        private void tvw_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeView tvw = sender as TreeView;
            tvw.SelectedNode = null;
            tvw.Focus();
            tvw.Refresh();
        }

        private void tvw_BeforeCheck(object sender, TreeViewCancelEventArgs e)
        {
            if (_IsRunning)
            {
                e.Cancel = true;
            }
        }

        private void tvw_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (!_IsRunning)
            {
                if (e.Action != TreeViewAction.Unknown)
                {
                    if (e.Node != null && !Convert.IsDBNull(e.Node))
                    {
                        CheckParentNode(e.Node);
                        if (e.Node.Nodes.Count > 0)
                        {
                            CheckAllChildNodes(e.Node, e.Node.Checked);
                        }
                    }
                }
            }
        }
        #region 私有方法

        //改变所有子节点的状态
        private static void CheckAllChildNodes(TreeNode pn, bool IsChecked)
        {
            foreach (TreeNode tn in pn.Nodes)
            {
                tn.Checked = IsChecked;

                if (tn.Nodes.Count > 0)
                {
                    CheckAllChildNodes(tn, IsChecked);
                }
            }
        }

        //改变父节点的选中状态，此处为所有子节点不选中时才取消父节点选中，可以根据需要修改
        private static void CheckParentNode(TreeNode curNode)
        {
            bool bChecked = false;

            if (curNode.Parent != null)
            {
                foreach (TreeNode node in curNode.Parent.Nodes)
                {
                    if (node.Checked)
                    {
                        bChecked = true;
                        break;
                    }
                }

                if (bChecked)
                {
                    curNode.Parent.Checked = true;
                    CheckParentNode(curNode.Parent);
                }
                else
                {
                    curNode.Parent.Checked = false;
                    CheckParentNode(curNode.Parent);
                }
            }
        }


        #endregion

        private void rdo_ff_CheckedChanged(object sender, EventArgs e)
        {
            if (this.rdo_ff.Checked)
            {
                this.chk_IPproxy.Enabled = true;
            }
            else
            {
                this.chk_IPproxy.Checked = false;
                this.chk_IPproxy.Enabled = false;
            }
        }
    }
}
