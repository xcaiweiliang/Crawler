using BLL;
using Common;
using Model;
using OpenQA.Selenium;
using System;
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

namespace Control_SB
{
    public partial class Main : Form
    {
        object lockobj = new object();

        private DateTime _runstarttime = DateTime.Now;
        private SourcePlatform _SourcePlatform = null;

        private bool _IsRunning = false;//是否正在运行
        private bool _IsMaintenance = false;//是否正在维护

        private List<IWebDriver> _WebDriverList = new List<IWebDriver>();
        private List<Log> _LogWinList = new List<Log>();

        private bool _ZP_AllDay = false;//早盘，所有日期

        private int _ZP_MS = 1000;//早盘抓取间隔，毫秒
        private int _JR_MS = 1000;//今日赛事抓取间隔，毫秒
        private int _GQ_MS = 1000;//滚球抓取间隔，毫秒

        private int _WebDriver_MS = 50;//判断元素时需等待的时间，毫秒

        private string _LogPath = System.Windows.Forms.Application.StartupPath + "/log/";

        private List<Cookie> _CookieList = new List<Cookie>();
        private List<Cookie> _ResultCookieList = new List<Cookie>();

        string[,,] menuarr = new string[,,] 
        {
            {{"早盘","今日赛事","滚球","",""}},
            {{"足球","","","",""}},
            {{"独赢 & 让球 & 大小 & 单/双","半场 / 全场","波胆","总入球","冠军"}},
        };

        private List<A_LeagueMatch> Temp_A_LeagueMatch = new List<A_LeagueMatch>();
        private List<A_Team> Temp_A_Team = new List<A_Team>();
        private List<A_Match> Temp_A_Match = new List<A_Match>();
        private List<A_MatchScoreRecord> Temp_A_MatchScoreRecord = new List<A_MatchScoreRecord>();
        private List<A_MatchResult> Temp_A_MatchResult = new List<A_MatchResult>();

        //private List<O_Odds> Temp_O_Odds = new List<O_Odds>();
        private Dictionary<string, List<O_Odds>> Temp_O_Odds_Dict = new Dictionary<string, List<O_Odds>>();

        private Dictionary<string,string> Dict_S_BetCode = new Dictionary<string, string>();        

        private void InitTempData()
        {
            try
            {
                A_LeagueMatchBll lmbll = new A_LeagueMatchBll();
                A_TeamBll tbll = new A_TeamBll();
                A_MatchBll mbll = new A_MatchBll();
                A_MatchScoreRecordBll msrbll = new A_MatchScoreRecordBll();
                A_MatchResultBll mrbll = new A_MatchResultBll();

                DateTime now = lmbll.GetServerDateTime();

                this.Temp_A_Match = mbll.FindByDate(SourcePlatformEnum.SB.ToString(), now.Date.AddDays(-1));
                this.Temp_A_MatchScoreRecord = msrbll.FindByDate(SourcePlatformEnum.SB.ToString(), now.Date.AddDays(-1));
                this.Temp_A_MatchResult = mrbll.FindByDate(SourcePlatformEnum.SB.ToString(), now.Date.AddDays(-1));
                this.Temp_A_LeagueMatch = lmbll.FindAll(SourcePlatformEnum.SB.ToString());
                this.Temp_A_Team = tbll.FindAll(SourcePlatformEnum.SB.ToString());

                O_OddsBll obll = new O_OddsBll();                
                var oddslist = obll.FindByDate(SourcePlatformEnum.SB.ToString(), now.Date.AddHours(-2));
                this.Temp_O_Odds_Dict.Clear();
                foreach (string code in this.Dict_S_BetCode.Values)
                {
                    this.Temp_O_Odds_Dict.Add(code, oddslist.Where(x => x.BetCode == code).ToList());
                }
                //this.Temp_O_Odds = obll.FindByDate(now.Date.AddDays(-1));                
            }
            catch (Exception ex)
            {
                //InitTempData();
            }
        }
        public Main()
        {
            InitializeComponent();
            if (!Directory.Exists(_LogPath))
            {
                Directory.CreateDirectory(_LogPath);
            }
            this.timer2.Interval = 1000 * 60 * 7;
            this.Dict_S_BetCode = new S_BetCodeBll().FindList().ToDictionary(key => key.CodeName, val => val.Code);
            bool checkedall = ConfigurationManager.AppSettings["checkedall"] == "1";
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
                                    chk.Checked = checkedall;
                                }
                            }
                        }
                    }
                }

            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key1">1：早盘，2：滚球</param>
        /// <param name="key2">1：足球</param>
        /// <param name="key3">足球{1：独赢 & 让球 & 大小 & 单/双，2：半场 / 全场，3：波胆，4：冠军}</param>
        private void GetData(int key1, int key2, int key3, Log logwin)
        {
            bool islogin = false;
            bool isexception = false;
            int awaitms = 10000;
            IWebDriver driver = WebDriverHelper.CreateChromeDriver(chk_VisibleChrome.Checked);//new ChromeDriver();
            if (driver == null)
            {
                MessageBox.Show(WebDriverHelper.ErrorMessage);
                logwin.txt_log.AppendText(WebDriverHelper.ErrorMessage + "\r\n");
                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", WebDriverHelper.ErrorMessage + "\r\n");
                return;
            }
            _WebDriverList.Add(driver);
            this.Invoke(new MethodInvoker(delegate
            {
                _IsRunning = true;
                islogin = this.chk_Logined.Checked;
                logwin.txt_log.AppendText("地址：" + _SourcePlatform.url + "\r\n");
                logwin.txt_log.AppendText("\r\n");
                logwin.txt_log.AppendText(key1 + key2 + key3 + "开始初次请求：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n");
                logwin.txt_log.AppendText("===========================================\r\n");
                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "地址：" + _SourcePlatform.url + "\r\n\r\n" + key1 + key2 + key3 + "开始初次请求：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n===========================================\r\n");
            }));
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            driver.Navigate().GoToUrl(_SourcePlatform.url);
            if (islogin)
            {
                driver.Manage().Cookies.DeleteAllCookies();
                foreach (var item in _CookieList)
                {
                    driver.Manage().Cookies.AddCookie(item);
                }
                driver.Navigate().Refresh();
            }            
            watch.Start();
            string html = string.Empty;
            int pagecount = 1;
            bool flag_loading = true;

            int count = 1;
            try
            {
                if (!this._IsMaintenance)
                {
                    choosemenu(key1, key2, key3, logwin, driver, ref awaitms);
                    this.Invoke(new MethodInvoker(delegate
                    {
                        logwin.txt_log.AppendText("===========================================\r\n");
                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "===========================================\r\n");
                    }));
                    checkloading(key1, key2, key3, logwin, driver, ref count, ref awaitms);
                    this.Invoke(new MethodInvoker(delegate
                    {
                        logwin.txt_log.AppendText("loading结束\r\n");
                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "loading结束\r\n");
                    }));
                    //判断是否有数据
                    bool flag1 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#container .message-box"), _WebDriver_MS);
                    bool flag2 = WebDriverHelper.WaitForElementInvisible(driver, By.CssSelector("#container .pagination"), _WebDriver_MS);
                    if (flag1 && flag2)
                    {
                        int needupdatecount = 0, updatedcount = 0, deletedcount = 0;
                        string msgpush = string.Empty, delmsgpush = string.Empty;
                        Transformation(key1, key2, key3, new List<LeagueMatch>(), ref needupdatecount, ref updatedcount,ref deletedcount, ref msgpush, ref delmsgpush);
                        this.Invoke(new MethodInvoker(delegate
                        {
                            //无数据
                            if (!string.IsNullOrEmpty(delmsgpush))
                            {
                                logwin.txt_log.AppendText("删除错误：\r\n" + delmsgpush);
                                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "删除错误：\r\n" + delmsgpush);
                            }
                            logwin.txt_log.AppendText("删除成功：" + deletedcount + "\r\n");
                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "删除成功：" + deletedcount + "\r\n");
                            logwin.txt_log.AppendText("无赛事\r\n");                            
                            logwin.txt_log.AppendText("===========================================\r\n");
                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "无赛事\r\n===========================================\r\n");
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
                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "第1页数据完\r\n");
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
                                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "第" + i + "页数据完\r\n");
                            }));
                        }
                        this.Invoke(new MethodInvoker(delegate
                        {
                            logwin.txt_log.AppendText("数据抓取完毕\r\n");
                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "数据抓取完毕\r\n");
                        }));
                        //解析数据
                        var lmList = AnalysisHtml(html, key1, key2, key3);
                        if (lmList != null && lmList.Count > 0)
                        {
                            if (!System.Text.RegularExpressions.Regex.IsMatch(lmList[0].Name, @"[\u4e00-\u9fa5]"))//不包含中文
                            {
                                this.Invoke(new MethodInvoker(delegate
                                {
                                    logwin.txt_log.AppendText("数据异常\r\n");
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "数据异常\r\n");
                                }));
                            }
                            else
                            {
                                if (_IsRunning)
                                {
                                    int needupdatecount = 0, updatedcount = 0, deletedcount=0;
                                    string msgpush = string.Empty, delmsgpush = string.Empty;
                                    Transformation(key1, key2, key3, lmList, ref needupdatecount, ref updatedcount, ref deletedcount, ref msgpush, ref delmsgpush);
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        if (!string.IsNullOrEmpty(msgpush))
                                        {
                                            logwin.txt_log.AppendText("错误：\r\n" + msgpush);
                                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "错误：\r\n" + msgpush);
                                        }
                                        if (!string.IsNullOrEmpty(delmsgpush))
                                        {
                                            logwin.txt_log.AppendText("删除错误：\r\n" + delmsgpush);
                                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "删除错误：\r\n" + delmsgpush);
                                        }
                                        logwin.txt_log.AppendText("赔率需更新：" + needupdatecount + "\r\n更新成功：" + updatedcount + "\r\n删除成功：" + deletedcount + "\r\n");
                                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "赔率需更新：" + needupdatecount + "\r\n更新成功：" + updatedcount + "\r\n删除成功：" + deletedcount + "\r\n");
                                    }));
                                }
                            }
                        }
                        watch.Stop();
                        this.Invoke(new MethodInvoker(delegate
                        {
                            logwin.txt_log.AppendText("===========================================\r\n");
                            logwin.txt_log.AppendText("耗时：" + watch.Elapsed.TotalSeconds + "秒\r\n");
                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "===========================================\r\n耗时：" + watch.Elapsed.TotalSeconds + "秒\r\n");
                        }));
                    }
                }
                else
                {
                    this.Invoke(new MethodInvoker(delegate
                    {
                        logwin.txt_log.AppendText("===========================================\r\n");
                        logwin.txt_log.AppendText("沙巴正在维护\r\n");
                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "===========================================\r\n沙巴正在维护\r\n");
                        GetMaintenanceInfo(logwin);
                    }));
                }
            }
            catch (Exception ex)
            {
                isexception = true;
                //driver.Quit();
                this.Invoke(new MethodInvoker(delegate
                {
                    logwin.txt_log.AppendText("异常：" + ex.ToString() + ex.StackTrace + "\r\n");
                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "异常：" + ex.ToString() + ex.StackTrace + "\r\n");
                }));
                //return;
            }

            //int count = 1;
            Task.Run(async () =>
            {
                while (_IsRunning)
                {
                    await Task.Delay(awaitms);
                    try
                    {
                        count++;
                        if (WebDriverHelper.AlertExist(driver))
                        {
                            driver.SwitchTo().Alert().Accept();
                        }
                        if (isexception || this._IsMaintenance)
                        {
                            isexception = false;
                            flag_loading = true;
                            driver.SwitchTo().DefaultContent();
                            driver.Navigate().GoToUrl(_SourcePlatform.url);
                            choosemenu(key1, key2, key3, logwin, driver, ref awaitms);
                            checkloading(key1, key2, key3, logwin, driver, ref count, ref awaitms);
                        }
                        if (!this._IsMaintenance)
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
                                logwin.txt_log.AppendText("\r\n"+ key1 + key2 + key3 + "开始第" + count + "次请求：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n");
                                logwin.txt_log.AppendText("===========================================\r\n");
                                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "\r\n" + key1 + key2 + key3 + "开始第" + count + "次请求：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n===========================================\r\n");
                            }));
                            watch.Restart();
                            //通过Selenium驱动点击页面的刷新按钮
                            driver.FindElement(By.CssSelector("#container .btn-toolbar .icon-refresh")).Click();
                            await Task.Delay(200);
                            checkloading(key1, key2, key3, logwin, driver, ref count, ref awaitms);
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("loading结束\r\n");
                                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "loading结束\r\n");
                            }));
                            //判断是否有数据
                            bool flag1 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#container .message-box"), _WebDriver_MS);

                            bool flag2 = WebDriverHelper.WaitForElementInvisible(driver, By.CssSelector("#container .pagination"), _WebDriver_MS);

                            if (flag1 && flag2)
                            {
                                int needupdatecount = 0, updatedcount = 0, deletedcount = 0;
                                string msgpush = string.Empty, delmsgpush = string.Empty;
                                Transformation(key1, key2, key3, new List<LeagueMatch>(), ref needupdatecount, ref updatedcount, ref deletedcount, ref msgpush, ref delmsgpush);
                                this.Invoke(new MethodInvoker(delegate
                                {
                                    //无数据
                                    if (!string.IsNullOrEmpty(delmsgpush))
                                    {
                                        logwin.txt_log.AppendText("删除错误：\r\n" + delmsgpush);
                                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "删除错误：\r\n" + delmsgpush);
                                    }
                                    logwin.txt_log.AppendText("删除成功：" + deletedcount + "\r\n");
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "删除成功：" + deletedcount + "\r\n");
                                    logwin.txt_log.AppendText("无赛事\r\n");
                                    logwin.txt_log.AppendText("===========================================\r\n");
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "无赛事\r\n===========================================\r\n");
                                }));
                                continue;
                            }
                            //第一页数据
                            html = driver.FindElement(By.CssSelector("#container .match-container")).GetAttribute("innerHTML");
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("第1页数据完\r\n");
                                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "第1页数据完\r\n");
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
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "第" + i + "页数据完\r\n");
                                }));
                            }
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("数据抓取完毕\r\n");
                                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "数据抓取完毕\r\n");
                            }));
                            //解析数据
                            var lmList = AnalysisHtml(html, key1, key2, key3);
                            if (lmList != null && lmList.Count > 0)
                            {
                                if (!System.Text.RegularExpressions.Regex.IsMatch(lmList[0].Name, @"[\u4e00-\u9fa5]"))//不包含中文
                                {
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("数据异常，可能获取到英文版的数据，重刷页面。\r\n");
                                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "数据异常，可能获取到英文版的数据，重刷页面。\r\n");
                                    }));
                                    flag_loading = true;
                                    driver.SwitchTo().DefaultContent();
                                    driver.Navigate().Refresh();
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
                                                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "错误：\r\n" + msgpush);
                                            }
                                            if (!string.IsNullOrEmpty(delmsgpush))
                                            {
                                                logwin.txt_log.AppendText("删除错误：\r\n" + delmsgpush);
                                                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "删除错误：\r\n" + delmsgpush);
                                            }
                                            logwin.txt_log.AppendText("赔率需更新：" + needupdatecount + "\r\n更新成功：" + updatedcount + "\r\n删除成功：" + deletedcount + "\r\n");
                                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "赔率需更新：" + needupdatecount + "\r\n更新成功：" + updatedcount + "\r\n删除成功：" + deletedcount + "\r\n");
                                        }));
                                    }
                                }
                            }
                            watch.Stop();
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("===========================================\r\n");
                                logwin.txt_log.AppendText("第" + count + "次耗时：" + watch.Elapsed.TotalSeconds + "秒\r\n");
                                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "===========================================\r\n第" + count + "次耗时：" + watch.Elapsed.TotalSeconds + "秒\r\n");
                            }));
                        }
                        else
                        {
                            awaitms = 1000 * 60;
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("===========================================\r\n");
                                logwin.txt_log.AppendText("沙巴正在维护\r\n");
                                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "===========================================\r\n沙巴正在维护\r\n");
                                GetMaintenanceInfo(logwin);
                            }));
                        }
                    }
                    catch (Exception ex)
                    {
                        isexception = true;
                        this.Invoke(new MethodInvoker(delegate
                        {
                            logwin.txt_log.AppendText("===========================================\r\n");
                            logwin.txt_log.AppendText("第" + count + "次异常：" + ex.Message + ex.StackTrace + "\r\n");
                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "===========================================\r\n第" + count + "次异常：" + ex.Message + ex.StackTrace + "\r\n");
                        }));
                        continue;
                    }
                }
            });
        }
        private void choosemenu(int key1, int key2, int key3, Log logwin, IWebDriver driver,ref int awaitms)
        {
            if (_SourcePlatform.key == "TYC")
            {
                bool iswh = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#sportsFrame"), _WebDriver_MS);
                if (!iswh)
                {
                    this.Invoke(new MethodInvoker(delegate
                    {
                        this._IsMaintenance = true;
                        logwin.txt_log.AppendText("异常，沙巴可能在维护。\r\n");
                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "异常，沙巴可能在维护。\r\n");
                        GetMaintenanceInfo(logwin);
                        //this.btn_Stop.PerformClick();
                    }));
                    return;
                }
                driver.SwitchTo().Frame("sportsFrame");
                iswh = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#sbContainer"), _WebDriver_MS);
                if (!iswh)
                {
                    this.Invoke(new MethodInvoker(delegate
                    {
                        this._IsMaintenance = true;
                        logwin.txt_log.AppendText("异常，沙巴可能在维护。\r\n");
                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "异常，沙巴可能在维护。\r\n");
                        GetMaintenanceInfo(logwin);
                        //this.btn_Stop.PerformClick();
                    }));
                    return;
                }
                this.Invoke(new MethodInvoker(delegate
                {
                    this._IsMaintenance = false;
                }));
                //driver.SwitchTo().Frame("sportsFrame");
            }
            else if (_SourcePlatform.key == "MS")
            {
                bool iswh = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#sbContainer"), _WebDriver_MS);
                if (!iswh)
                {
                    this.Invoke(new MethodInvoker(delegate
                    {
                        this._IsMaintenance = true;
                        logwin.txt_log.AppendText("异常，沙巴可能在维护。\r\n");
                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "异常，沙巴可能在维护。\r\n");
                        GetMaintenanceInfo(logwin);
                        //this.btn_Stop.PerformClick();
                    }));
                    return;
                }
            }
            if (_SourcePlatform.key == "TYC")
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
                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "进入早盘\r\n");
                    }));
                    driver.FindElement(By.CssSelector("#sb-header>.header-tab>ul>li:nth-child(3)")).Click();
                    bool flag01 = false;
                    do
                    {
                        flag01 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#category .category-sports"), _WebDriver_MS);
                    } while (!flag01);
                    if (_ZP_AllDay)//选择所有日期
                    {
                        this.Invoke(new MethodInvoker(delegate
                        {
                            logwin.txt_log.AppendText("选择所有日期\r\n");
                            Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择所有日期\r\n");
                        }));
                        driver.FindElement(By.CssSelector("#container .filter-date>ul>li:last-child>a")).Click();
                    }
                    switch (key2)
                    {
                        case 1:
                            //选择足球
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("选择足球\r\n");
                                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择足球\r\n");
                            }));
                            driver.FindElement(By.CssSelector("#category .category-sports>ul>li:nth-child(1)")).Click();
                            bool flag02 = false;
                            do
                            {
                                flag02 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#category .category-bettype"), _WebDriver_MS);
                            } while (!flag02);
                            switch (key3)
                            {
                                case 1:
                                    //独赢 & 让球 & 大小 & 单/双
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择独赢 & 让球 & 大小 & 单/双\r\n");
                                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择独赢 & 让球 & 大小 & 单/双\r\n");
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(1)")).Click();
                                    break;
                                case 2:
                                    //半场 / 全场
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择半场 / 全场\r\n");
                                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择半场 / 全场\r\n");
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(4)")).Click();
                                    break;
                                case 3:
                                    //波胆
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择波胆\r\n");
                                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择波胆\r\n");
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(2)")).Click();
                                    break;
                                case 4:
                                    //总入球
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择总入球\r\n");
                                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择总入球\r\n");
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(3)")).Click();
                                    break;
                                case 5:
                                    //冠军
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择冠军\r\n");
                                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择冠军\r\n");
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(6)")).Click();
                                    //选择联赛
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择联赛\r\n");
                                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择联赛\r\n");
                                    }));
                                    bool flag03 = false;
                                    do
                                    {
                                        flag03 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#container .match-toolbar .button-wrap>li .highlight"), _WebDriver_MS);
                                    } while (!flag03);
                                    driver.FindElement(By.CssSelector("#container .match-toolbar .button-wrap>li .highlight")).Click();
                                    bool flag04 = false;
                                    do
                                    {
                                        flag04 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector(".modal-dialog"), _WebDriver_MS);
                                    } while (!flag04);
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
                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "进入今日赛事\r\n");
                    }));
                    driver.FindElement(By.CssSelector("#sb-header>.header-tab>ul>li:nth-child(2)")).Click();
                    bool flag05 = false;
                    do
                    {
                        flag05 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#category .category-sports"), _WebDriver_MS);
                    } while (!flag05);
                    switch (key2)
                    {
                        case 1:
                            //选择足球
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("选择足球\r\n");
                                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择足球\r\n");
                            }));
                            driver.FindElement(By.CssSelector("#category .category-sports>ul>li:nth-child(1)")).Click();
                            bool flag06 = false;
                            do
                            {
                                flag06 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#category .category-bettype"), _WebDriver_MS);
                            } while (!flag06);
                            switch (key3)
                            {
                                case 1:
                                    //独赢 & 让球 & 大小 & 单/双
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择独赢 & 让球 & 大小 & 单/双\r\n");
                                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择独赢 & 让球 & 大小 & 单/双\r\n");
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(1)")).Click();
                                    break;
                                case 2:
                                    //半场 / 全场
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择半场 / 全场\r\n");
                                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择半场 / 全场\r\n");
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(4)")).Click();
                                    break;
                                case 3:
                                    //波胆
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择波胆\r\n");
                                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择波胆\r\n");
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(2)")).Click();
                                    break;
                                case 4:
                                    //总入球
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择总入球\r\n");
                                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择总入球\r\n");
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(3)")).Click();
                                    break;
                            }
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
                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "进入滚球\r\n");
                    }));
                    driver.FindElement(By.CssSelector("#sb-header>.header-tab>ul>li:nth-child(1)")).Click();
                    bool flag07 = false;
                    do
                    {
                        flag07 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#category .category-sports"), _WebDriver_MS);
                    } while (!flag07);
                    switch (key2)
                    {
                        case 1:
                            //选择足球
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("选择足球\r\n");
                                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择足球\r\n");
                            }));
                            driver.FindElement(By.CssSelector("#category .category-sports>ul>li:nth-child(1)")).Click();
                            bool flag08 = false;
                            do
                            {
                                flag08 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#category .category-bettype"), _WebDriver_MS);
                            } while (!flag08);
                            switch (key3)
                            {
                                case 1:
                                    //独赢 & 让球 & 大小 & 单/双
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择独赢 & 让球 & 大小 & 单/双\r\n");
                                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择独赢 & 让球 & 大小 & 单/双\r\n");
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(1)")).Click();
                                    break;
                                case 3:
                                    //波胆
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择波胆\r\n");
                                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择波胆\r\n");
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(2)")).Click();
                                    break;
                                case 4:
                                    //总入球
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择总入球\r\n");
                                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择总入球\r\n");
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(3)")).Click();
                                    break;
                            }
                            break;
                    }
                    #endregion
                    break;
            }
        }
        private void checkloading(int key1, int key2, int key3, Log logwin, IWebDriver driver,ref int count, ref int awaitms)
        {            
            int loadingcount = 0;
            bool flag09 = false;
            while (!flag09)
            {
                flag09 = WebDriverHelper.WaitForElementInvisible(driver, By.CssSelector("#container .loading"), _WebDriver_MS);
                this.Invoke(new MethodInvoker(delegate
                {
                    logwin.txt_log.AppendText("等待loading："+ flag09 + "\r\n");
                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "等待loading：" + flag09 + "\r\n");
                }));
                loadingcount++;
                bool islogin = false;
                if (loadingcount > 500)
                {
                    flag09 = true;
                    this.Invoke(new MethodInvoker(delegate
                    {
                        islogin = this.chk_Logined.Checked;
                        logwin.txt_log.AppendText("loading超时："+ loadingcount + "，页面重刷\r\n");
                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "loading超时：" + loadingcount + "，页面重刷\r\n");
                    }));
                    driver.Navigate().Refresh();
                    choosemenu(key1, key2, key3, logwin, driver, ref awaitms);
                    checkloading(key1, key2, key3, logwin, driver, ref count, ref awaitms);
                }
                if ((key1 == 1 && count % 30 == 0)|| (key1 == 2 && count % 60 == 0)|| (key1 == 3 && count % 100 == 0))
                {
                    int c = count;
                    count++;
                    this.Invoke(new MethodInvoker(delegate
                    {
                        islogin = this.chk_Logined.Checked;
                        logwin.txt_log.AppendText("第" + c + "次，页面重刷\r\n");
                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "第" + c + "次，页面重刷\r\n");
                    }));
                    driver.Navigate().Refresh();
                    choosemenu(key1, key2, key3, logwin, driver, ref awaitms);
                    checkloading(key1, key2, key3, logwin, driver, ref count, ref awaitms);
                }
            }
        }
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
        private void Transformation(int key1, int key2, int key3, List<LeagueMatch> lmList, ref int needupdatecount, ref int updatedcount, ref int deletedcount, ref string msgpush, ref string delmsgpush)
        {
            A_LeagueMatchBll lmbll = new A_LeagueMatchBll();
            A_TeamBll tbll = new A_TeamBll();
            A_MatchBll mbll = new A_MatchBll();
            A_MatchScoreRecordBll msrbll = new A_MatchScoreRecordBll();

            List<string> _ZPNoLockMID = new List<string>();//早盘
            List<string> _JRNoLockMID = new List<string>();//今日
            List<string> _GQNoLockMID = new List<string>();//滚球

            foreach (LeagueMatch lm in lmList)
            {
                if (string.IsNullOrEmpty(lm.Name.Trim()))
                {
                    continue;
                }
                DateTime now = lmbll.GetServerDateTime();
                string msg = string.Empty;
                if (lm.TeamList.Count > 0)//冠军
                {
                    lm.Name = lm.Name.Trim().Trim('*');
                    string sj = string.Empty;
                    if (lm.Name.Contains(" "))
                    {
                        string[] mnarr = lm.Name.Split(' ');
                        if (mnarr.Length==2)
                        {
                            sj = mnarr[0];
                            lm.Name = mnarr[1];
                        }
                    }
                    //A_LeagueMatch objlm = lmbll.GetByName(lm.Name, SourcePlatformEnum.SB.ToString());
                    A_LeagueMatch objlm = this.Temp_A_LeagueMatch.Where(x => x != null && x.Name == lm.Name).FirstOrDefault();
                    if (objlm == null)
                    {
                        objlm = new A_LeagueMatch()
                        {
                            ID = Guid.NewGuid().ToString("N"),
                            Name = lm.Name.Trim(),
                            Season = sj,
                            ModifyTime = now,
                            SourcePlatform = SourcePlatformEnum.SB.ToString(),
                            SportsType = SportsTypeEnum.Football.ToString()
                        };                        
                        msg = lmbll.Create(objlm);
                        if (string.IsNullOrEmpty(msg))
                        {
                            this.Temp_A_LeagueMatch.Add(objlm);
                        }
                        else
                        {
                            this.Temp_A_LeagueMatch = lmbll.FindAll(SourcePlatformEnum.SB.ToString());
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
                                this.Temp_A_LeagueMatch.Remove(objlm);
                                this.Temp_A_LeagueMatch.Add(newlm);
                            }
                            else
                            {
                                this.Temp_A_LeagueMatch = lmbll.FindAll(SourcePlatformEnum.SB.ToString());
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
                            A_Team objt = this.Temp_A_Team.Where(x => x != null && x.LeagueMatchID == objlm.ID && x.Name == t.Name.Trim()).FirstOrDefault();
                            if (objt == null)
                            {
                                objt = new A_Team()
                                {
                                    ID = Guid.NewGuid().ToString("N"),
                                    LeagueMatchID = objlm.ID,
                                    Name = t.Name.Trim(),
                                    ModifyTime = now,
                                    SourcePlatform = SourcePlatformEnum.SB.ToString(),
                                    SportsType = SportsTypeEnum.Football.ToString()
                                };
                                msg = tbll.Create(objt);
                                if (string.IsNullOrEmpty(msg))
                                {
                                    this.Temp_A_Team.Add(objt);
                                }
                                else
                                {
                                    this.Temp_A_Team = tbll.FindAll(SourcePlatformEnum.SB.ToString());
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
                                    BetCode = Dict_S_BetCode["冠军"],
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
                                update_odds(odds_cp, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out ID);
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
                        string delmsg = new O_OddsBll().DeleteBySQL($"delete O_Odds where LeagueMatchID='{objlm.ID}' and MatchID is NULL and BetCode='{Dict_S_BetCode["冠军"]}' and SourcePlatform='{SourcePlatformEnum.SB.ToString()}' and ID not in({ids.ToString().TrimEnd(',')})", out delcount);
                        if (string.IsNullOrEmpty(delmsg))
                        {
                            deletedcount += delcount;
                            this.Temp_O_Odds_Dict[Dict_S_BetCode["冠军"]].RemoveAll(x =>
                            x.LeagueMatchID == objlm.ID &&
                            x.MatchID == null &&
                            x.BetCode == Dict_S_BetCode["冠军"] &&
                            (!IDlist.Contains(x.ID)) &&
                            x.SourcePlatform == SourcePlatformEnum.SB.ToString());
                        }
                        else
                        {
                            delmsgpush += delmsg + "\r\n";
                        }
                        #endregion
                    }
                }
                else
                {
                    lm.Name = lm.Name.Trim().Trim('*');
                    if (lm.Name.Contains(" - "))
                    {
                        string[] mnarr = lm.Name.Split(new string[] { " - " }, StringSplitOptions.None);
                        if (mnarr.Length == 2)
                        {
                            lm.Name = mnarr[0];
                        }
                    }
                    //A_LeagueMatch objlm = lmbll.GetByName(lm.Name, SourcePlatformEnum.SB.ToString());
                    A_LeagueMatch objlm = this.Temp_A_LeagueMatch.Where(x => x != null && x.Name == lm.Name).FirstOrDefault();
                    if (objlm == null)
                    {
                        objlm = new A_LeagueMatch()
                        {
                            ID = Guid.NewGuid().ToString("N"),
                            Name = lm.Name.Trim(),
                            ModifyTime = now,
                            SourcePlatform = SourcePlatformEnum.SB.ToString(),
                            SportsType = SportsTypeEnum.Football.ToString()
                        };
                        msg = lmbll.Create(objlm);
                        if (string.IsNullOrEmpty(msg))
                        {
                            this.Temp_A_LeagueMatch.Add(objlm);
                        }
                        else
                        {
                            this.Temp_A_LeagueMatch = lmbll.FindAll(SourcePlatformEnum.SB.ToString());
                        }
                    }
                    if (string.IsNullOrEmpty(msg))
                    {
                        if (lm.MatchList.Count > 0)//独赢 & 让球 & 大小 & 单/双 & 半场/全场 & 波胆 & 总入球
                        {
                            #region 独赢 & 让球 & 大小 & 单/双 & 半场/全场 & 波胆 & 总入球
                            foreach (Match m in lm.MatchList)
                            {
                                if (string.IsNullOrEmpty(m.HomeTeam.Trim()) || string.IsNullOrEmpty(m.VisitingTeam.Trim()))
                                {
                                    continue;
                                }
                                //A_Team objtH = tbll.GetByLMIDName(objlm.ID, m.HomeTeam.Trim());
                                A_Team objtH = this.Temp_A_Team.Where(x => x != null && x.LeagueMatchID == objlm.ID && x.Name == m.HomeTeam.Trim()).FirstOrDefault();
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
                                        SportsType = SportsTypeEnum.Football.ToString()
                                    };
                                    msg1 = tbll.Create(objtH);
                                    if (string.IsNullOrEmpty(msg1))
                                    {
                                        this.Temp_A_Team.Add(objtH);
                                    }
                                    else
                                    {
                                        this.Temp_A_Team = tbll.FindAll(SourcePlatformEnum.SB.ToString());
                                    }
                                }
                                //A_Team objtV = tbll.GetByLMIDName(objlm.ID, m.VisitingTeam.Trim());
                                A_Team objtV = this.Temp_A_Team.Where(x => x != null && x.LeagueMatchID == objlm.ID && x.Name == m.VisitingTeam.Trim()).FirstOrDefault();
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
                                        SportsType = SportsTypeEnum.Football.ToString()
                                    };
                                    msg2 = tbll.Create(objtV);
                                    if (string.IsNullOrEmpty(msg2))
                                    {
                                        this.Temp_A_Team.Add(objtV);
                                    }
                                    else
                                    {
                                        this.Temp_A_Team = tbll.FindAll(SourcePlatformEnum.SB.ToString());
                                    }
                                }
                                if (string.IsNullOrEmpty(msg1) && string.IsNullOrEmpty(msg2))
                                {
                                    if (!string.IsNullOrEmpty(m.timing))//滚球，根据当天日期来找比赛
                                    {
                                        #region 滚球
                                        try
                                        {
                                            DateTime bsdate = now.Date;
                                            //A_Match objm = mbll.GetByHVDate(objtH.ID, objtV.ID, bsdate);
                                            A_Match objm = this.Temp_A_Match.Where(x => x != null && x.HomeTeamID == objtH.ID && x.VisitingTeamID == objtV.ID && x.SP_GameStartTime.Value.Date== bsdate).FirstOrDefault();
                                            int hs = 0, vs = 0, ts = -1;
                                            int.TryParse(m.HomeTeamScore, out hs);
                                            int.TryParse(m.VisitingTeamScore, out vs);
                                            int.TryParse(m.timing, out ts);
                                            if (objm == null)
                                            {
                                                //objm = new A_Match()
                                                //{
                                                //    ID = Guid.NewGuid().ToString("N"),
                                                //    LeagueMatchID = objlm.ID,
                                                //    SportsType = SportsTypeEnum.Football.ToString(),
                                                //    HomeTeamID = objtH.ID,
                                                //    VisitingTeamID = objtV.ID,
                                                //    SourcePlatform = SourcePlatformEnum.SB.ToString(),
                                                //    SP_GameStartTime = now.AddMinutes(-ts),
                                                //    HomeTeamScore = hs,
                                                //    VisitingTeamScore = vs,
                                                //    IsStart = ts < 0 ? "0" : "1",
                                                //    Timing = ts * 60,
                                                //    IsEnd = "0",
                                                //    ExistLive = "1",
                                                //    ModifyTime = now,
                                                //    IsLock = "0"
                                                //};
                                                //if (m.halftype == "1H")
                                                //{
                                                //    objm.MatchType = MatchTypeEnum_Football.Firsthalf.ToString();
                                                //}
                                                //else if (m.halftype == "2H")
                                                //{
                                                //    objm.MatchType = MatchTypeEnum_Football.Secondhalf.ToString();
                                                //}
                                                //if (ts == 1)
                                                //{
                                                //    objm.GameStartTime = Utility.ObjConvertToDateTime(now.ToString("yyyy-MM-dd HH:mm:00"));
                                                //}
                                                //msg = mbll.Create(objm);
                                                //if (string.IsNullOrEmpty(msg))
                                                //{
                                                //    _NoLockMID.Add(objm.ID);
                                                //    this.Temp_A_Match.Add(objm);
                                                //    A_MatchScoreRecord objmsr = new A_MatchScoreRecord()
                                                //    {
                                                //        ID = Guid.NewGuid().ToString("N"),
                                                //        MatchID = objm.ID,
                                                //        MatchType = objm.MatchType,
                                                //        HomeTeamScore = hs,
                                                //        VisitingTeamScore = vs,
                                                //        Timing = ts,
                                                //        SourcePlatform = SourcePlatformEnum.SB.ToString(),
                                                //        CreateTime = now
                                                //    };
                                                //    string msgmsr = msrbll.Create(objmsr);
                                                //    if (string.IsNullOrEmpty(msgmsr))
                                                //    {
                                                //        this.Temp_A_MatchScoreRecord.Add(objmsr);
                                                //    }
                                                //    else
                                                //    {
                                                //        this.Temp_A_MatchScoreRecord = msrbll.FindByDate(now.Date);
                                                //    }
                                                //}
                                                //else
                                                //{
                                                //    this.Temp_A_Match = mbll.FindByDate(now.Date);
                                                //}
                                            }
                                            else//更新
                                            {
                                                _GQNoLockMID.Add(objm.ID);
                                                var newm = (A_Match)Utility.DeepCopy(objm);
                                                newm.LastMenuType = key1.ToString();
                                                newm.HomeTeamScore = hs;
                                                newm.VisitingTeamScore = vs;
                                                newm.IsStart = ts < 0 ? "0" : "1";
                                                newm.Timing = ts * 60;
                                                newm.StatusText = m.statustext;
                                                newm.IsEnd = "0";
                                                newm.IsLock = "0";
                                                newm.ModifyTime = now;
                                                if (m.halftype == "1H")
                                                {
                                                    newm.MatchType = MatchTypeEnum_Football.Firsthalf.ToString();
                                                }
                                                else if (m.halftype == "2H")
                                                {
                                                    newm.MatchType = MatchTypeEnum_Football.Secondhalf.ToString();
                                                }
                                                if (ts == 1)
                                                {
                                                    newm.GameStartTime = Utility.ObjConvertToDateTime(now.ToString("yyyy-MM-dd HH:mm:00"));
                                                }
                                                msg = mbll.Update(newm);
                                                if (string.IsNullOrEmpty(msg))
                                                {
                                                    this.Temp_A_Match.Remove(objm);
                                                    this.Temp_A_Match.Add(newm);
                                                    objm.MatchType = newm.MatchType;
                                                }
                                                else
                                                {
                                                    this.Temp_A_Match = mbll.FindByDate(SourcePlatformEnum.SB.ToString(), now.Date.AddDays(-1));
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
                                                    HomeTeamScore = hs,
                                                    VisitingTeamScore = vs,
                                                    Timing = ts,
                                                    SourcePlatform = SourcePlatformEnum.SB.ToString(),
                                                    CreateTime = now
                                                };
                                                //A_MatchScoreRecord lastmsr = msrbll.GetByMID(objm.ID);
                                                A_MatchScoreRecord lastmsr = this.Temp_A_MatchScoreRecord.Where(x => x != null && x.MatchID == objm.ID).OrderByDescending(x => x.CreateTime).FirstOrDefault();
                                                if (lastmsr != null)//比较比分
                                                {
                                                    if (lastmsr.HomeTeamScore != objmsr.HomeTeamScore || lastmsr.VisitingTeamScore != objmsr.VisitingTeamScore)
                                                    {
                                                        msg = msrbll.Create(objmsr);
                                                        if (string.IsNullOrEmpty(msg))
                                                        {
                                                            this.Temp_A_MatchScoreRecord.Add(objmsr);
                                                        }
                                                        else
                                                        {
                                                            this.Temp_A_MatchScoreRecord = msrbll.FindByDate(SourcePlatformEnum.SB.ToString(), now.Date.AddDays(-1));
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    msg = msrbll.Create(objmsr);
                                                    if (string.IsNullOrEmpty(msg))
                                                    {
                                                        this.Temp_A_MatchScoreRecord.Add(objmsr);
                                                    }
                                                    else
                                                    {
                                                        this.Temp_A_MatchScoreRecord = msrbll.FindByDate(SourcePlatformEnum.SB.ToString(), now.Date.AddDays(-1));
                                                    }
                                                }
                                                #endregion

                                                O_OddsBll obll = new O_OddsBll();
                                                O_OddsRecordBll orbll = new O_OddsRecordBll();
                                                #region 全场赔率
                                                if (m.FullCourtList.Count > 0)
                                                {
                                                    dyrqdxds(m.FullCourtList, objlm.ID, objm.ID, now, "1", "", ref needupdatecount, ref updatedcount, ref deletedcount, ref msgpush, ref delmsgpush);
                                                    int main = 0;
                                                    List<string> IDlist_1x2 = new List<string>();
                                                    List<string> IDlist_ah = new List<string>();
                                                    List<string> IDlist_ou = new List<string>();
                                                    List<string> IDlist_oe = new List<string>();
                                                    foreach (Odds item in m.FullCourtList)
                                                    {
                                                        main++;
                                                        #region 独赢
                                                        O_Odds odds_1 = new O_Odds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            LeagueMatchID = objlm.ID,
                                                            MatchID = objm.ID,
                                                            BetCode = Dict_S_BetCode["独赢"],
                                                            BetExplain = "",
                                                            OddsSort = "1",
                                                            MainSort = main,
                                                            Odds = Utility.ObjConvertToDecimal(item.Odds_ZY),
                                                            IsLive = "1",
                                                            CreateTime = now,
                                                            ModifyTime = now,
                                                            SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                        };
                                                        O_Odds odds_x = new O_Odds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            LeagueMatchID = objlm.ID,
                                                            MatchID = objm.ID,
                                                            BetCode = Dict_S_BetCode["独赢"],
                                                            BetExplain = "",
                                                            OddsSort = "x",
                                                            MainSort = main,
                                                            Odds = Utility.ObjConvertToDecimal(item.Odds_HJ),
                                                            IsLive = "1",
                                                            CreateTime = now,
                                                            ModifyTime = now,
                                                            SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                        };
                                                        O_Odds odds_2 = new O_Odds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            LeagueMatchID = objlm.ID,
                                                            MatchID = objm.ID,
                                                            BetCode = Dict_S_BetCode["独赢"],
                                                            BetExplain = "",
                                                            OddsSort = "2",
                                                            MainSort = main,
                                                            Odds = Utility.ObjConvertToDecimal(item.Odds_KY),
                                                            IsLive = "1",
                                                            CreateTime = now,
                                                            ModifyTime = now,
                                                            SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                        };
                                                        string ID1 = string.Empty, IDx = string.Empty, ID2 = string.Empty;
                                                        update_odds(odds_1, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out ID1);
                                                        update_odds(odds_x, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDx);
                                                        update_odds(odds_2, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out ID2);
                                                        IDlist_1x2.Add(ID1);
                                                        IDlist_1x2.Add(IDx);
                                                        IDlist_1x2.Add(ID2);
                                                        #endregion

                                                        #region 让球
                                                        O_Odds odds_ah1 = new O_Odds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            LeagueMatchID = objlm.ID,
                                                            MatchID = objm.ID,
                                                            BetCode = Dict_S_BetCode["让球"],
                                                            BetExplain = "",
                                                            OddsSort = "1",
                                                            MainSort = main,
                                                            Odds = Utility.ObjConvertToDecimal(item.Odds_RQZY),
                                                            IsLive = "1",
                                                            CreateTime = now,
                                                            ModifyTime = now,
                                                            SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                        };
                                                        O_Odds odds_ah2 = new O_Odds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            LeagueMatchID = objlm.ID,
                                                            MatchID = objm.ID,
                                                            BetCode = Dict_S_BetCode["让球"],
                                                            BetExplain = "",
                                                            OddsSort = "2",
                                                            MainSort = main,
                                                            Odds = Utility.ObjConvertToDecimal(item.Odds_RQKY),
                                                            IsLive = "1",
                                                            CreateTime = now,
                                                            ModifyTime = now,
                                                            SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                        };
                                                        
                                                        if (!string.IsNullOrEmpty(item.Text_ZRKQ.Trim()))//主让客
                                                        {
                                                            string rq = item.Text_ZRKQ.Trim();
                                                            if (rq.Contains("/"))
                                                            {
                                                                string[] arr = rq.Split('/');
                                                                rq = arr[0].Trim() + "/" + arr[1].Trim();
                                                            }
                                                            odds_ah1.BetExplain = "+"+ rq;
                                                            odds_ah2.BetExplain = "-" + rq;
                                                        }
                                                        else if (!string.IsNullOrEmpty(item.Text_KRZQ.Trim()))//客让主
                                                        {
                                                            string rq = item.Text_KRZQ.Trim();
                                                            if (rq.Contains("/"))
                                                            {
                                                                string[] arr = rq.Split('/');
                                                                rq = arr[0].Trim() + "/" + arr[1].Trim();
                                                            }
                                                            odds_ah1.BetExplain = "-" + rq;
                                                            odds_ah2.BetExplain = "+" + rq;
                                                        }
                                                        string IDah1 = string.Empty, IDah2 = string.Empty;
                                                        update_odds(odds_ah1, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDah1);
                                                        update_odds(odds_ah2, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDah2);
                                                        IDlist_ah.Add(IDah1);
                                                        IDlist_ah.Add(IDah2);
                                                        #endregion

                                                        #region 大小球
                                                        string dxq = string.Empty;
                                                        if (!string.IsNullOrEmpty(item.Text_DQ.Trim()))
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
                                                            LeagueMatchID = objlm.ID,
                                                            MatchID = objm.ID,
                                                            BetCode = Dict_S_BetCode["大小"],
                                                            BetExplain = dxq,
                                                            OddsSort = "o",
                                                            MainSort = main,
                                                            Odds = Utility.ObjConvertToDecimal(item.Odds_DQ),
                                                            IsLive = "1",
                                                            CreateTime = now,
                                                            ModifyTime = now,
                                                            SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                        };
                                                        O_Odds odds_ou_u = new O_Odds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            LeagueMatchID = objlm.ID,
                                                            MatchID = objm.ID,
                                                            BetCode = Dict_S_BetCode["大小"],
                                                            BetExplain = dxq,
                                                            OddsSort = "u",
                                                            MainSort = main,
                                                            Odds = Utility.ObjConvertToDecimal(item.Odds_XQ),
                                                            IsLive = "1",
                                                            CreateTime = now,                                                            
                                                            ModifyTime = now,
                                                            SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                        };
                                                        string IDouo = string.Empty, IDouu = string.Empty;
                                                        update_odds(odds_ou_o, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDouo);
                                                        update_odds(odds_ou_u, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDouu);
                                                        IDlist_ou.Add(IDouo);
                                                        IDlist_ou.Add(IDouu);
                                                        #endregion

                                                        #region 单双
                                                        O_Odds odds_oe_o = new O_Odds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            LeagueMatchID = objlm.ID,
                                                            MatchID = objm.ID,
                                                            BetCode = Dict_S_BetCode["单/双"],
                                                            BetExplain = "o",
                                                            OddsSort = "",
                                                            MainSort = main,
                                                            Odds = Utility.ObjConvertToDecimal(item.Odds_D),
                                                            IsLive = "1",
                                                            CreateTime = now,
                                                            ModifyTime = now,
                                                            SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                        };
                                                        O_Odds odds_oe_e = new O_Odds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            LeagueMatchID = objlm.ID,
                                                            MatchID = objm.ID,
                                                            BetCode = Dict_S_BetCode["单/双"],
                                                            BetExplain = "e",
                                                            OddsSort = "",
                                                            MainSort = main,
                                                            Odds = Utility.ObjConvertToDecimal(item.Odds_S),
                                                            IsLive = "1",
                                                            CreateTime = now,
                                                            ModifyTime = now,
                                                            SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                        };
                                                        string IDoeo = string.Empty, IDoee = string.Empty;
                                                        update_odds(odds_oe_o, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDoeo);
                                                        update_odds(odds_oe_e, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDoee);
                                                        IDlist_oe.Add(IDoeo);
                                                        IDlist_oe.Add(IDoee);
                                                        #endregion
                                                    }
                                                    
                                                }
                                                #endregion
                                                #region 上半场赔率
                                                if (m.HalfCourtList.Count > 0)
                                                {
                                                    int main = 0;
                                                    List<string> IDlist_1x2 = new List<string>();
                                                    List<string> IDlist_ah = new List<string>();
                                                    List<string> IDlist_ou = new List<string>();
                                                    List<string> IDlist_oe = new List<string>();
                                                    foreach (Odds item in m.HalfCourtList)
                                                    {
                                                        main++;
                                                        #region 独赢
                                                        O_Odds odds_1 = new O_Odds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            LeagueMatchID = objlm.ID,
                                                            MatchID = objm.ID,
                                                            BetCode = Dict_S_BetCode["独赢-上半场"],
                                                            BetExplain = "",
                                                            OddsSort = "1",
                                                            MainSort = main,
                                                            Odds = Utility.ObjConvertToDecimal(item.Odds_ZY),
                                                            IsLive = "1",
                                                            CreateTime = now,
                                                            ModifyTime = now,
                                                            SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                        };
                                                        O_Odds odds_x = new O_Odds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            LeagueMatchID = objlm.ID,
                                                            MatchID = objm.ID,
                                                            BetCode = Dict_S_BetCode["独赢-上半场"],
                                                            BetExplain = "",
                                                            OddsSort = "x",
                                                            MainSort = main,
                                                            Odds = Utility.ObjConvertToDecimal(item.Odds_HJ),
                                                            IsLive = "1",
                                                            CreateTime = now,
                                                            ModifyTime = now,
                                                            SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                        };
                                                        O_Odds odds_2 = new O_Odds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            LeagueMatchID = objlm.ID,
                                                            MatchID = objm.ID,
                                                            BetCode = Dict_S_BetCode["独赢-上半场"],
                                                            BetExplain = "",
                                                            OddsSort = "2",
                                                            MainSort = main,
                                                            Odds = Utility.ObjConvertToDecimal(item.Odds_KY),
                                                            IsLive = "1",
                                                            CreateTime = now,
                                                            ModifyTime = now,
                                                            SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                        };
                                                        string ID1 = string.Empty, IDx = string.Empty, ID2 = string.Empty;
                                                        update_odds(odds_1, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out ID1);
                                                        update_odds(odds_x, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDx);
                                                        update_odds(odds_2, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out ID2);
                                                        IDlist_1x2.Add(ID1);
                                                        IDlist_1x2.Add(IDx);
                                                        IDlist_1x2.Add(ID2);
                                                        #endregion

                                                        #region 让球
                                                        O_Odds odds_ah1 = new O_Odds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            LeagueMatchID = objlm.ID,
                                                            MatchID = objm.ID,
                                                            BetCode = Dict_S_BetCode["让球-上半场"],
                                                            BetExplain = "",
                                                            OddsSort = "1",
                                                            MainSort = main,
                                                            Odds = Utility.ObjConvertToDecimal(item.Odds_RQZY),
                                                            IsLive = "1",
                                                            CreateTime = now,
                                                            ModifyTime = now,
                                                            SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                        };
                                                        O_Odds odds_ah2 = new O_Odds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            LeagueMatchID = objlm.ID,
                                                            MatchID = objm.ID,
                                                            BetCode = Dict_S_BetCode["让球-上半场"],
                                                            BetExplain = "",
                                                            OddsSort = "2",
                                                            MainSort = main,
                                                            Odds = Utility.ObjConvertToDecimal(item.Odds_RQKY),
                                                            IsLive = "1",
                                                            CreateTime = now,
                                                            ModifyTime = now,
                                                            SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                        };

                                                        if (!string.IsNullOrEmpty(item.Text_ZRKQ.Trim()))//主让客
                                                        {
                                                            string rq = item.Text_ZRKQ.Trim();
                                                            if (rq.Contains("/"))
                                                            {
                                                                string[] arr = rq.Split('/');
                                                                rq = arr[0].Trim() + "/" + arr[1].Trim();
                                                            }
                                                            odds_ah1.BetExplain = "+" + rq;
                                                            odds_ah2.BetExplain = "-" + rq;
                                                        }
                                                        else if (!string.IsNullOrEmpty(item.Text_KRZQ.Trim()))//客让主
                                                        {
                                                            string rq = item.Text_KRZQ.Trim();
                                                            if (rq.Contains("/"))
                                                            {
                                                                string[] arr = rq.Split('/');
                                                                rq = arr[0].Trim() + "/" + arr[1].Trim();
                                                            }
                                                            odds_ah1.BetExplain = "-" + rq;
                                                            odds_ah2.BetExplain = "+" + rq;
                                                        }
                                                        string IDah1 = string.Empty, IDah2 = string.Empty;
                                                        update_odds(odds_ah1, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDah1);
                                                        update_odds(odds_ah2, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDah2);
                                                        IDlist_ah.Add(IDah1);
                                                        IDlist_ah.Add(IDah2);
                                                        #endregion

                                                        #region 大小球
                                                        string dxq = string.Empty;
                                                        if (!string.IsNullOrEmpty(item.Text_DQ.Trim()))
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
                                                            LeagueMatchID = objlm.ID,
                                                            MatchID = objm.ID,
                                                            BetCode = Dict_S_BetCode["大小-上半场"],
                                                            BetExplain = dxq,
                                                            OddsSort = "o",
                                                            MainSort = main,
                                                            Odds = Utility.ObjConvertToDecimal(item.Odds_DQ),
                                                            IsLive = "1",
                                                            CreateTime = now,
                                                            ModifyTime = now,
                                                            SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                        };
                                                        O_Odds odds_ou_u = new O_Odds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            LeagueMatchID = objlm.ID,
                                                            MatchID = objm.ID,
                                                            BetCode = Dict_S_BetCode["大小-上半场"],
                                                            BetExplain = dxq,
                                                            OddsSort = "u",
                                                            MainSort = main,
                                                            Odds = Utility.ObjConvertToDecimal(item.Odds_XQ),
                                                            IsLive = "1",
                                                            CreateTime = now,
                                                            ModifyTime = now,
                                                            SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                        };
                                                        string IDouo = string.Empty, IDouu = string.Empty;
                                                        update_odds(odds_ou_o, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDouo);
                                                        update_odds(odds_ou_u, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDouu);
                                                        IDlist_ou.Add(IDouo);
                                                        IDlist_ou.Add(IDouu);
                                                        #endregion

                                                        #region 单双
                                                        O_Odds odds_oe_o = new O_Odds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            LeagueMatchID = objlm.ID,
                                                            MatchID = objm.ID,
                                                            BetCode = Dict_S_BetCode["单/双-上半场"],
                                                            BetExplain = "o",
                                                            OddsSort = "",
                                                            MainSort = main,
                                                            Odds = Utility.ObjConvertToDecimal(item.Odds_D),
                                                            IsLive = "1",
                                                            CreateTime = now,
                                                            ModifyTime = now,
                                                            SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                        };
                                                        O_Odds odds_oe_e = new O_Odds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            LeagueMatchID = objlm.ID,
                                                            MatchID = objm.ID,
                                                            BetCode = Dict_S_BetCode["单/双-上半场"],
                                                            BetExplain = "e",
                                                            OddsSort = "",
                                                            MainSort = main,
                                                            Odds = Utility.ObjConvertToDecimal(item.Odds_S),
                                                            IsLive = "1",
                                                            CreateTime = now,
                                                            ModifyTime = now,
                                                            SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                        };
                                                        string IDoeo = string.Empty, IDoee = string.Empty;
                                                        update_odds(odds_oe_o, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDoeo);
                                                        update_odds(odds_oe_e, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDoee);
                                                        IDlist_oe.Add(IDoeo);
                                                        IDlist_oe.Add(IDoee);
                                                        #endregion
                                                    }
                                                    //独赢
                                                    this.Temp_O_Odds_Dict[Dict_S_BetCode["独赢-上半场"]].RemoveAll(x => x.LeagueMatchID == objlm.ID && x.MatchID == objm.ID && x.BetCode == Dict_S_BetCode["独赢-上半场"] && x.BetExplain == "" && x.IsLive == "1" && (!IDlist_1x2.Contains(x.ID)) && x.SourcePlatform == SourcePlatformEnum.SB.ToString());
                                                    //让球
                                                    this.Temp_O_Odds_Dict[Dict_S_BetCode["让球-上半场"]].RemoveAll(x => x.LeagueMatchID == objlm.ID && x.MatchID == objm.ID && x.BetCode == Dict_S_BetCode["让球-上半场"] && x.BetExplain == "" && x.IsLive == "1" && (!IDlist_1x2.Contains(x.ID)) && x.SourcePlatform == SourcePlatformEnum.SB.ToString());
                                                    //大小
                                                    this.Temp_O_Odds_Dict[Dict_S_BetCode["大小-上半场"]].RemoveAll(x => x.LeagueMatchID == objlm.ID && x.MatchID == objm.ID && x.BetCode == Dict_S_BetCode["大小-上半场"] && x.BetExplain == "" && x.IsLive == "1" && (!IDlist_1x2.Contains(x.ID)) && x.SourcePlatform == SourcePlatformEnum.SB.ToString());
                                                    //单/双
                                                    this.Temp_O_Odds_Dict[Dict_S_BetCode["单/双-上半场"]].RemoveAll(x => x.LeagueMatchID == objlm.ID && x.MatchID == objm.ID && x.BetCode == Dict_S_BetCode["单/双-上半场"] && x.BetExplain == "" && x.IsLive == "1" && (!IDlist_1x2.Contains(x.ID)) && x.SourcePlatform == SourcePlatformEnum.SB.ToString());
                                                }
                                                #endregion
                                                #region 波胆
                                                if (m.CorrectScoreList.Count > 0)
                                                {
                                                    List<string> IDlist_cs = new List<string>();
                                                    foreach (OddsBD item in m.CorrectScoreList)
                                                    {
                                                        O_Odds odds_cs = new O_Odds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            LeagueMatchID = objlm.ID,
                                                            MatchID = objm.ID,
                                                            BetCode = Dict_S_BetCode["波胆"],
                                                            BetExplain = item.Text_H+"*"+ item.Text_V,
                                                            OddsSort = "",
                                                            MainSort = 1,
                                                            Odds = Utility.ObjConvertToDecimal(item.Odds_BD),
                                                            IsLive = "1",
                                                            CreateTime = now,
                                                            ModifyTime = now,
                                                            SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                        };
                                                        if (item.type == 1)
                                                        {
                                                            odds_cs.BetCode = Dict_S_BetCode["波胆"];
                                                        }
                                                        else if (item.type == 2)
                                                        {
                                                            odds_cs.BetCode = Dict_S_BetCode["波胆-上半场"];
                                                        }
                                                        string IDcs = string.Empty;
                                                        update_odds(odds_cs, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDcs);
                                                        IDlist_cs.Add(IDcs);
                                                    }
                                                    //波胆
                                                    this.Temp_O_Odds_Dict[Dict_S_BetCode["波胆"]].RemoveAll(x => x.LeagueMatchID == objlm.ID && x.MatchID == objm.ID && x.BetCode == Dict_S_BetCode["波胆"] && x.BetExplain == "" && x.IsLive == "1" && (!IDlist_cs.Contains(x.ID)) && x.SourcePlatform == SourcePlatformEnum.SB.ToString());
                                                }
                                                #endregion
                                                #region 总入球
                                                if (m.TotalGoalList.Count > 0)
                                                {
                                                    List<string> IDlist_tg = new List<string>();
                                                    foreach (OddsZRQ item in m.TotalGoalList)
                                                    {
                                                        O_Odds odds_tg = new O_Odds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            LeagueMatchID = objlm.ID,
                                                            MatchID = objm.ID,
                                                            BetCode = Dict_S_BetCode["总进球数"],
                                                            BetExplain = item.Text_Goals,
                                                            OddsSort = "",
                                                            MainSort = 1,
                                                            Odds = Utility.ObjConvertToDecimal(item.Odds_ZRQ),
                                                            IsLive = "1",
                                                            CreateTime = now,
                                                            ModifyTime = now,
                                                            SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                        };
                                                        if (item.type == 1)
                                                        {
                                                            odds_tg.BetCode = Dict_S_BetCode["总进球数"];
                                                        }
                                                        else if (item.type == 2)
                                                        {
                                                            odds_tg.BetCode = Dict_S_BetCode["总进球数-上半场"];
                                                        }
                                                        string IDtg = string.Empty;
                                                        update_odds(odds_tg, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDtg);
                                                        IDlist_tg.Add(IDtg);
                                                    }
                                                    //总进球数
                                                    this.Temp_O_Odds_Dict[Dict_S_BetCode["总进球数"]].RemoveAll(x => x.LeagueMatchID == objlm.ID && x.MatchID == objm.ID && x.BetCode == Dict_S_BetCode["总进球数"] && x.BetExplain == "" && x.IsLive == "1" && (!IDlist_tg.Contains(x.ID)) && x.SourcePlatform == SourcePlatformEnum.SB.ToString());
                                                }
                                                #endregion
                                            }
                                        }
                                        catch (Exception ex)
                                        {

                                        }
                                        #endregion
                                    }
                                    else
                                    {
                                        #region 早盘和今日
                                        try
                                        {
                                            DateTime bstime = Convert.ToDateTime(now.Year + "-" + m.time);
                                            //A_Match objm = mbll.GetByHVTime(objtH.ID, objtV.ID, bstime);
                                            A_Match objm = this.Temp_A_Match.Where(x => x != null && x.HomeTeamID == objtH.ID && x.VisitingTeamID == objtV.ID && x.SP_GameStartTime.Value.Date == bstime.Date).FirstOrDefault();                                            
                                            int hs = 0, vs = 0;
                                            int.TryParse(m.HomeTeamScore, out hs);
                                            int.TryParse(m.VisitingTeamScore, out vs);
                                            if (objm == null)
                                            {
                                                objm = new A_Match()
                                                {
                                                    ID = Guid.NewGuid().ToString("N"),
                                                    LeagueMatchID = objlm.ID,
                                                    SportsType = SportsTypeEnum.Football.ToString(),
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
                                                    this.Temp_A_Match.Add(objm);
                                                }
                                                else
                                                {
                                                    this.Temp_A_Match = mbll.FindByDate(SourcePlatformEnum.SB.ToString(), now.Date.AddDays(-1));
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
                                                    this.Temp_A_Match.Remove(objm);
                                                    this.Temp_A_Match.Add(newm);
                                                }
                                                else
                                                {
                                                    this.Temp_A_Match = mbll.FindByDate(SourcePlatformEnum.SB.ToString(), now.Date.AddDays(-1));
                                                }
                                            }
                                            if (string.IsNullOrEmpty(msg))
                                            {
                                                O_OddsBll obll = new O_OddsBll();
                                                O_OddsRecordBll orbll = new O_OddsRecordBll();
                                                #region 全场赔率
                                                if (m.FullCourtList.Count > 0)
                                                {
                                                    int main = 0;
                                                    List<string> IDlist_1x2 = new List<string>();
                                                    List<string> IDlist_ah = new List<string>();
                                                    List<string> IDlist_ou = new List<string>();
                                                    List<string> IDlist_oe = new List<string>();
                                                    foreach (Odds item in m.FullCourtList)
                                                    {
                                                        main++;
                                                        #region 独赢
                                                        O_Odds odds_1 = new O_Odds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            LeagueMatchID = objlm.ID,
                                                            MatchID = objm.ID,
                                                            BetCode = Dict_S_BetCode["独赢"],
                                                            BetExplain = "",
                                                            OddsSort = "1",
                                                            MainSort = main,
                                                            Odds = Utility.ObjConvertToDecimal(item.Odds_ZY),
                                                            IsLive = "0",
                                                            CreateTime = now,
                                                            ModifyTime = now,
                                                            SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                        };
                                                        O_Odds odds_x = new O_Odds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            LeagueMatchID = objlm.ID,
                                                            MatchID = objm.ID,
                                                            BetCode = Dict_S_BetCode["独赢"],
                                                            BetExplain = "",
                                                            OddsSort = "x",
                                                            MainSort = main,
                                                            Odds = Utility.ObjConvertToDecimal(item.Odds_HJ),
                                                            IsLive = "0",
                                                            CreateTime = now,
                                                            ModifyTime = now,
                                                            SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                        };
                                                        O_Odds odds_2 = new O_Odds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            LeagueMatchID = objlm.ID,
                                                            MatchID = objm.ID,
                                                            BetCode = Dict_S_BetCode["独赢"],
                                                            BetExplain = "",
                                                            OddsSort = "2",
                                                            MainSort = main,
                                                            Odds = Utility.ObjConvertToDecimal(item.Odds_KY),
                                                            IsLive = "0",
                                                            CreateTime = now,
                                                            ModifyTime = now,
                                                            SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                        };
                                                        string ID1 = string.Empty, IDx = string.Empty, ID2 = string.Empty;
                                                        update_odds(odds_1, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out ID1);
                                                        update_odds(odds_x, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDx);
                                                        update_odds(odds_2, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out ID2);
                                                        IDlist_1x2.Add(ID1);
                                                        IDlist_1x2.Add(IDx);
                                                        IDlist_1x2.Add(ID2);
                                                        #endregion

                                                        #region 让球
                                                        O_Odds odds_ah1 = new O_Odds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            LeagueMatchID = objlm.ID,
                                                            MatchID = objm.ID,
                                                            BetCode = Dict_S_BetCode["让球"],
                                                            BetExplain = "",
                                                            OddsSort = "1",
                                                            MainSort = main,
                                                            Odds = Utility.ObjConvertToDecimal(item.Odds_RQZY),
                                                            IsLive = "0",
                                                            CreateTime = now,
                                                            ModifyTime = now,
                                                            SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                        };
                                                        O_Odds odds_ah2 = new O_Odds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            LeagueMatchID = objlm.ID,
                                                            MatchID = objm.ID,
                                                            BetCode = Dict_S_BetCode["让球"],
                                                            BetExplain = "",
                                                            OddsSort = "2",
                                                            MainSort = main,
                                                            Odds = Utility.ObjConvertToDecimal(item.Odds_RQKY),
                                                            IsLive = "0",
                                                            CreateTime = now,
                                                            ModifyTime = now,
                                                            SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                        };

                                                        if (!string.IsNullOrEmpty(item.Text_ZRKQ.Trim()))//主让客
                                                        {
                                                            string rq = item.Text_ZRKQ.Trim();
                                                            if (rq.Contains("/"))
                                                            {
                                                                string[] arr = rq.Split('/');
                                                                rq = arr[0].Trim() + "/" + arr[1].Trim();
                                                            }
                                                            odds_ah1.BetExplain = "+" + rq;
                                                            odds_ah2.BetExplain = "-" + rq;
                                                        }
                                                        else if (!string.IsNullOrEmpty(item.Text_KRZQ.Trim()))//客让主
                                                        {
                                                            string rq = item.Text_KRZQ.Trim();
                                                            if (rq.Contains("/"))
                                                            {
                                                                string[] arr = rq.Split('/');
                                                                rq = arr[0].Trim() + "/" + arr[1].Trim();
                                                            }
                                                            odds_ah1.BetExplain = "-" + rq;
                                                            odds_ah2.BetExplain = "+" + rq;
                                                        }
                                                        string IDah1 = string.Empty, IDah2 = string.Empty;
                                                        update_odds(odds_ah1, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDah1);
                                                        update_odds(odds_ah2, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDah2);
                                                        IDlist_ah.Add(IDah1);
                                                        IDlist_ah.Add(IDah2);
                                                        #endregion

                                                        #region 大小球
                                                        string dxq = string.Empty;
                                                        if (!string.IsNullOrEmpty(item.Text_DQ.Trim()))
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
                                                            LeagueMatchID = objlm.ID,
                                                            MatchID = objm.ID,
                                                            BetCode = Dict_S_BetCode["大小"],
                                                            BetExplain = dxq,
                                                            OddsSort = "o",
                                                            MainSort = main,
                                                            Odds = Utility.ObjConvertToDecimal(item.Odds_DQ),
                                                            IsLive = "0",
                                                            CreateTime = now,
                                                            ModifyTime = now,
                                                            SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                        };
                                                        O_Odds odds_ou_u = new O_Odds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            LeagueMatchID = objlm.ID,
                                                            MatchID = objm.ID,
                                                            BetCode = Dict_S_BetCode["大小"],
                                                            BetExplain = dxq,
                                                            OddsSort = "u",
                                                            MainSort = main,
                                                            Odds = Utility.ObjConvertToDecimal(item.Odds_XQ),
                                                            IsLive = "0",
                                                            CreateTime = now,
                                                            ModifyTime = now,
                                                            SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                        };
                                                        string IDouo = string.Empty, IDouu = string.Empty;
                                                        update_odds(odds_ou_o, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDouo);
                                                        update_odds(odds_ou_u, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDouu);
                                                        IDlist_ou.Add(IDouo);
                                                        IDlist_ou.Add(IDouu);
                                                        #endregion

                                                        #region 单双
                                                        O_Odds odds_oe_o = new O_Odds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            LeagueMatchID = objlm.ID,
                                                            MatchID = objm.ID,
                                                            BetCode = Dict_S_BetCode["单/双"],
                                                            BetExplain = "o",
                                                            OddsSort = "",
                                                            MainSort = main,
                                                            Odds = Utility.ObjConvertToDecimal(item.Odds_D),
                                                            IsLive = "0",
                                                            CreateTime = now,
                                                            ModifyTime = now,
                                                            SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                        };
                                                        O_Odds odds_oe_e = new O_Odds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            LeagueMatchID = objlm.ID,
                                                            MatchID = objm.ID,
                                                            BetCode = Dict_S_BetCode["单/双"],
                                                            BetExplain = "e",
                                                            OddsSort = "",
                                                            MainSort = main,
                                                            Odds = Utility.ObjConvertToDecimal(item.Odds_S),
                                                            IsLive = "0",
                                                            CreateTime = now,
                                                            ModifyTime = now,
                                                            SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                        };
                                                        string IDoeo = string.Empty, IDoee = string.Empty;
                                                        update_odds(odds_oe_o, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDoeo);
                                                        update_odds(odds_oe_e, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDoee);
                                                        IDlist_oe.Add(IDoeo);
                                                        IDlist_oe.Add(IDoee);
                                                        #endregion
                                                    }
                                                    //独赢
                                                    this.Temp_O_Odds_Dict[Dict_S_BetCode["独赢"]].RemoveAll(x => x.LeagueMatchID == objlm.ID && x.MatchID == objm.ID && x.BetCode == Dict_S_BetCode["独赢"] && x.BetExplain == "" && x.IsLive == "0" && (!IDlist_1x2.Contains(x.ID)) && x.SourcePlatform == SourcePlatformEnum.SB.ToString());
                                                    //让球
                                                    this.Temp_O_Odds_Dict[Dict_S_BetCode["让球"]].RemoveAll(x => x.LeagueMatchID == objlm.ID && x.MatchID == objm.ID && x.BetCode == Dict_S_BetCode["让球"] && x.BetExplain == "" && x.IsLive == "0" && (!IDlist_1x2.Contains(x.ID)) && x.SourcePlatform == SourcePlatformEnum.SB.ToString());
                                                    //大小
                                                    this.Temp_O_Odds_Dict[Dict_S_BetCode["大小"]].RemoveAll(x => x.LeagueMatchID == objlm.ID && x.MatchID == objm.ID && x.BetCode == Dict_S_BetCode["大小"] && x.BetExplain == "" && x.IsLive == "0" && (!IDlist_1x2.Contains(x.ID)) && x.SourcePlatform == SourcePlatformEnum.SB.ToString());
                                                    //单/双
                                                    this.Temp_O_Odds_Dict[Dict_S_BetCode["单/双"]].RemoveAll(x => x.LeagueMatchID == objlm.ID && x.MatchID == objm.ID && x.BetCode == Dict_S_BetCode["单/双"] && x.BetExplain == "" && x.IsLive == "0" && (!IDlist_1x2.Contains(x.ID)) && x.SourcePlatform == SourcePlatformEnum.SB.ToString());
                                                }
                                                #endregion
                                                #region 上半场赔率
                                                if (m.HalfCourtList.Count > 0)
                                                {
                                                    int main = 0;
                                                    List<string> IDlist_1x2 = new List<string>();
                                                    List<string> IDlist_ah = new List<string>();
                                                    List<string> IDlist_ou = new List<string>();
                                                    List<string> IDlist_oe = new List<string>();
                                                    foreach (Odds item in m.HalfCourtList)
                                                    {
                                                        main++;
                                                        #region 独赢
                                                        O_Odds odds_1 = new O_Odds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            LeagueMatchID = objlm.ID,
                                                            MatchID = objm.ID,
                                                            BetCode = Dict_S_BetCode["独赢-上半场"],
                                                            BetExplain = "",
                                                            OddsSort = "1",
                                                            MainSort = main,
                                                            Odds = Utility.ObjConvertToDecimal(item.Odds_ZY),
                                                            IsLive = "0",
                                                            CreateTime = now,
                                                            ModifyTime = now,
                                                            SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                        };
                                                        O_Odds odds_x = new O_Odds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            LeagueMatchID = objlm.ID,
                                                            MatchID = objm.ID,
                                                            BetCode = Dict_S_BetCode["独赢-上半场"],
                                                            BetExplain = "",
                                                            OddsSort = "x",
                                                            MainSort = main,
                                                            Odds = Utility.ObjConvertToDecimal(item.Odds_HJ),
                                                            IsLive = "0",
                                                            CreateTime = now,
                                                            ModifyTime = now,
                                                            SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                        };
                                                        O_Odds odds_2 = new O_Odds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            LeagueMatchID = objlm.ID,
                                                            MatchID = objm.ID,
                                                            BetCode = Dict_S_BetCode["独赢-上半场"],
                                                            BetExplain = "",
                                                            OddsSort = "2",
                                                            MainSort = main,
                                                            Odds = Utility.ObjConvertToDecimal(item.Odds_KY),
                                                            IsLive = "0",
                                                            CreateTime = now,
                                                            ModifyTime = now,
                                                            SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                        };
                                                        string ID1 = string.Empty, IDx = string.Empty, ID2 = string.Empty;
                                                        update_odds(odds_1, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out ID1);
                                                        update_odds(odds_x, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDx);
                                                        update_odds(odds_2, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out ID2);
                                                        IDlist_1x2.Add(ID1);
                                                        IDlist_1x2.Add(IDx);
                                                        IDlist_1x2.Add(ID2);
                                                        #endregion

                                                        #region 让球
                                                        O_Odds odds_ah1 = new O_Odds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            LeagueMatchID = objlm.ID,
                                                            MatchID = objm.ID,
                                                            BetCode = Dict_S_BetCode["让球-上半场"],
                                                            BetExplain = "",
                                                            OddsSort = "1",
                                                            MainSort = main,
                                                            Odds = Utility.ObjConvertToDecimal(item.Odds_RQZY),
                                                            IsLive = "0",
                                                            CreateTime = now,
                                                            ModifyTime = now,
                                                            SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                        };
                                                        O_Odds odds_ah2 = new O_Odds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            LeagueMatchID = objlm.ID,
                                                            MatchID = objm.ID,
                                                            BetCode = Dict_S_BetCode["让球-上半场"],
                                                            BetExplain = "",
                                                            OddsSort = "2",
                                                            MainSort = main,
                                                            Odds = Utility.ObjConvertToDecimal(item.Odds_RQKY),
                                                            IsLive = "0",
                                                            CreateTime = now,
                                                            ModifyTime = now,
                                                            SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                        };

                                                        if (!string.IsNullOrEmpty(item.Text_ZRKQ.Trim()))//主让客
                                                        {
                                                            string rq = item.Text_ZRKQ.Trim();
                                                            if (rq.Contains("/"))
                                                            {
                                                                string[] arr = rq.Split('/');
                                                                rq = arr[0].Trim() + "/" + arr[1].Trim();
                                                            }
                                                            odds_ah1.BetExplain = "+" + rq;
                                                            odds_ah2.BetExplain = "-" + rq;
                                                        }
                                                        else if (!string.IsNullOrEmpty(item.Text_KRZQ.Trim()))//客让主
                                                        {
                                                            string rq = item.Text_KRZQ.Trim();
                                                            if (rq.Contains("/"))
                                                            {
                                                                string[] arr = rq.Split('/');
                                                                rq = arr[0].Trim() + "/" + arr[1].Trim();
                                                            }
                                                            odds_ah1.BetExplain = "-" + rq;
                                                            odds_ah2.BetExplain = "+" + rq;
                                                        }
                                                        string IDah1 = string.Empty, IDah2 = string.Empty;
                                                        update_odds(odds_ah1, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDah1);
                                                        update_odds(odds_ah2, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDah2);
                                                        IDlist_ah.Add(IDah1);
                                                        IDlist_ah.Add(IDah2);
                                                        #endregion

                                                        #region 大小球
                                                        string dxq = string.Empty;
                                                        if (!string.IsNullOrEmpty(item.Text_DQ.Trim()))
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
                                                            LeagueMatchID = objlm.ID,
                                                            MatchID = objm.ID,
                                                            BetCode = Dict_S_BetCode["大小-上半场"],
                                                            BetExplain = dxq,
                                                            OddsSort = "o",
                                                            MainSort = main,
                                                            Odds = Utility.ObjConvertToDecimal(item.Odds_DQ),
                                                            IsLive = "0",
                                                            CreateTime = now,
                                                            ModifyTime = now,
                                                            SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                        };
                                                        O_Odds odds_ou_u = new O_Odds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            LeagueMatchID = objlm.ID,
                                                            MatchID = objm.ID,
                                                            BetCode = Dict_S_BetCode["大小-上半场"],
                                                            BetExplain = dxq,
                                                            OddsSort = "u",
                                                            MainSort = main,
                                                            Odds = Utility.ObjConvertToDecimal(item.Odds_XQ),
                                                            IsLive = "0",
                                                            CreateTime = now,
                                                            ModifyTime = now,
                                                            SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                        };
                                                        string IDouo = string.Empty, IDouu = string.Empty;
                                                        update_odds(odds_ou_o, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDouo);
                                                        update_odds(odds_ou_u, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDouu);
                                                        IDlist_ou.Add(IDouo);
                                                        IDlist_ou.Add(IDouu);
                                                        #endregion

                                                        #region 单双
                                                        O_Odds odds_oe_o = new O_Odds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            LeagueMatchID = objlm.ID,
                                                            MatchID = objm.ID,
                                                            BetCode = Dict_S_BetCode["单/双-上半场"],
                                                            BetExplain = "o",
                                                            OddsSort = "",
                                                            MainSort = main,
                                                            Odds = Utility.ObjConvertToDecimal(item.Odds_D),
                                                            IsLive = "0",
                                                            CreateTime = now,
                                                            ModifyTime = now,
                                                            SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                        };
                                                        O_Odds odds_oe_e = new O_Odds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            LeagueMatchID = objlm.ID,
                                                            MatchID = objm.ID,
                                                            BetCode = Dict_S_BetCode["单/双-上半场"],
                                                            BetExplain = "e",
                                                            OddsSort = "",
                                                            MainSort = main,
                                                            Odds = Utility.ObjConvertToDecimal(item.Odds_S),
                                                            IsLive = "0",
                                                            CreateTime = now,
                                                            ModifyTime = now,
                                                            SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                        };
                                                        string IDoeo = string.Empty, IDoee = string.Empty;
                                                        update_odds(odds_oe_o, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDoeo);
                                                        update_odds(odds_oe_e, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDoee);
                                                        IDlist_oe.Add(IDoeo);
                                                        IDlist_oe.Add(IDoee);
                                                        #endregion
                                                    }
                                                    //独赢
                                                    this.Temp_O_Odds_Dict[Dict_S_BetCode["独赢-上半场"]].RemoveAll(x => x.LeagueMatchID == objlm.ID && x.MatchID == objm.ID && x.BetCode == Dict_S_BetCode["独赢-上半场"] && x.BetExplain == "" && x.IsLive == "0" && (!IDlist_1x2.Contains(x.ID)) && x.SourcePlatform == SourcePlatformEnum.SB.ToString());
                                                    //让球
                                                    this.Temp_O_Odds_Dict[Dict_S_BetCode["让球-上半场"]].RemoveAll(x => x.LeagueMatchID == objlm.ID && x.MatchID == objm.ID && x.BetCode == Dict_S_BetCode["让球-上半场"] && x.BetExplain == "" && x.IsLive == "0" && (!IDlist_1x2.Contains(x.ID)) && x.SourcePlatform == SourcePlatformEnum.SB.ToString());
                                                    //大小
                                                    this.Temp_O_Odds_Dict[Dict_S_BetCode["大小-上半场"]].RemoveAll(x => x.LeagueMatchID == objlm.ID && x.MatchID == objm.ID && x.BetCode == Dict_S_BetCode["大小-上半场"] && x.BetExplain == "" && x.IsLive == "0" && (!IDlist_1x2.Contains(x.ID)) && x.SourcePlatform == SourcePlatformEnum.SB.ToString());
                                                    //单/双
                                                    this.Temp_O_Odds_Dict[Dict_S_BetCode["单/双-上半场"]].RemoveAll(x => x.LeagueMatchID == objlm.ID && x.MatchID == objm.ID && x.BetCode == Dict_S_BetCode["单/双-上半场"] && x.BetExplain == "" && x.IsLive == "0" && (!IDlist_1x2.Contains(x.ID)) && x.SourcePlatform == SourcePlatformEnum.SB.ToString());
                                                }
                                                #endregion
                                                #region 半场 / 全场
                                                if (m.DoubleResult != null)
                                                {
                                                    List<string> IDlist_hf = new List<string>();
                                                    O_Odds odds_hf_11 = new O_Odds()
                                                    {
                                                        ID = Guid.NewGuid().ToString("N"),
                                                        LeagueMatchID = objlm.ID,
                                                        MatchID = objm.ID,
                                                        BetCode = Dict_S_BetCode["半场/全场"],
                                                        BetExplain = "11",
                                                        OddsSort = "",
                                                        MainSort = 1,
                                                        Odds = Utility.ObjConvertToDecimal(m.DoubleResult.Odds_HH),
                                                        IsLive = "0",
                                                        CreateTime = now,
                                                        ModifyTime = now,
                                                        SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                    };
                                                    O_Odds odds_hf_1x = new O_Odds()
                                                    {
                                                        ID = Guid.NewGuid().ToString("N"),
                                                        LeagueMatchID = objlm.ID,
                                                        MatchID = objm.ID,
                                                        BetCode = Dict_S_BetCode["半场/全场"],
                                                        BetExplain = "1x",
                                                        OddsSort = "",
                                                        MainSort = 1,
                                                        Odds = Utility.ObjConvertToDecimal(m.DoubleResult.Odds_HD),
                                                        IsLive = "0",
                                                        CreateTime = now,
                                                        ModifyTime = now,
                                                        SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                    };
                                                    O_Odds odds_hf_12 = new O_Odds()
                                                    {
                                                        ID = Guid.NewGuid().ToString("N"),
                                                        LeagueMatchID = objlm.ID,
                                                        MatchID = objm.ID,
                                                        BetCode = Dict_S_BetCode["半场/全场"],
                                                        BetExplain = "12",
                                                        OddsSort = "",
                                                        MainSort = 1,
                                                        Odds = Utility.ObjConvertToDecimal(m.DoubleResult.Odds_HV),
                                                        IsLive = "0",
                                                        CreateTime = now,
                                                        ModifyTime = now,
                                                        SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                    };
                                                    O_Odds odds_hf_x1 = new O_Odds()
                                                    {
                                                        ID = Guid.NewGuid().ToString("N"),
                                                        LeagueMatchID = objlm.ID,
                                                        MatchID = objm.ID,
                                                        BetCode = Dict_S_BetCode["半场/全场"],
                                                        BetExplain = "x1",
                                                        OddsSort = "",
                                                        MainSort = 1,
                                                        Odds = Utility.ObjConvertToDecimal(m.DoubleResult.Odds_DH),
                                                        IsLive = "0",
                                                        CreateTime = now,
                                                        ModifyTime = now,
                                                        SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                    };
                                                    O_Odds odds_hf_xx = new O_Odds()
                                                    {
                                                        ID = Guid.NewGuid().ToString("N"),
                                                        LeagueMatchID = objlm.ID,
                                                        MatchID = objm.ID,
                                                        BetCode = Dict_S_BetCode["半场/全场"],
                                                        BetExplain = "xx",
                                                        OddsSort = "",
                                                        MainSort = 1,
                                                        Odds = Utility.ObjConvertToDecimal(m.DoubleResult.Odds_DD),
                                                        IsLive = "0",
                                                        CreateTime = now,
                                                        ModifyTime = now,
                                                        SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                    };
                                                    O_Odds odds_hf_x2 = new O_Odds()
                                                    {
                                                        ID = Guid.NewGuid().ToString("N"),
                                                        LeagueMatchID = objlm.ID,
                                                        MatchID = objm.ID,
                                                        BetCode = Dict_S_BetCode["半场/全场"],
                                                        BetExplain = "x2",
                                                        OddsSort = "",
                                                        MainSort = 1,
                                                        Odds = Utility.ObjConvertToDecimal(m.DoubleResult.Odds_DV),
                                                        IsLive = "0",
                                                        CreateTime = now,
                                                        ModifyTime = now,
                                                        SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                    };
                                                    O_Odds odds_hf_21 = new O_Odds()
                                                    {
                                                        ID = Guid.NewGuid().ToString("N"),
                                                        LeagueMatchID = objlm.ID,
                                                        MatchID = objm.ID,
                                                        BetCode = Dict_S_BetCode["半场/全场"],
                                                        BetExplain = "21",
                                                        OddsSort = "",
                                                        MainSort = 1,
                                                        Odds = Utility.ObjConvertToDecimal(m.DoubleResult.Odds_VH),
                                                        IsLive = "0",
                                                        CreateTime = now,
                                                        ModifyTime = now,
                                                        SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                    };
                                                    O_Odds odds_hf_2x = new O_Odds()
                                                    {
                                                        ID = Guid.NewGuid().ToString("N"),
                                                        LeagueMatchID = objlm.ID,
                                                        MatchID = objm.ID,
                                                        BetCode = Dict_S_BetCode["半场/全场"],
                                                        BetExplain = "2x",
                                                        OddsSort = "",
                                                        MainSort = 1,
                                                        Odds = Utility.ObjConvertToDecimal(m.DoubleResult.Odds_VD),
                                                        IsLive = "0",
                                                        CreateTime = now,
                                                        ModifyTime = now,
                                                        SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                    };
                                                    O_Odds odds_hf_22 = new O_Odds()
                                                    {
                                                        ID = Guid.NewGuid().ToString("N"),
                                                        LeagueMatchID = objlm.ID,
                                                        MatchID = objm.ID,
                                                        BetCode = Dict_S_BetCode["半场/全场"],
                                                        BetExplain = "22",
                                                        OddsSort = "",
                                                        MainSort = 1,
                                                        Odds = Utility.ObjConvertToDecimal(m.DoubleResult.Odds_VV),
                                                        IsLive = "0",
                                                        CreateTime = now,
                                                        ModifyTime = now,
                                                        SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                    };
                                                    string ID11 = string.Empty, ID1x = string.Empty, ID12 = string.Empty, IDx1 = string.Empty, IDxx = string.Empty, IDx2 = string.Empty, ID21 = string.Empty, ID2x = string.Empty, ID22 = string.Empty;
                                                    update_odds(odds_hf_11, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out ID11);
                                                    update_odds(odds_hf_1x, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out ID1x);
                                                    update_odds(odds_hf_12, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out ID12);
                                                    update_odds(odds_hf_x1, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDx1);
                                                    update_odds(odds_hf_xx, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDxx);
                                                    update_odds(odds_hf_x2, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDx2);
                                                    update_odds(odds_hf_21, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out ID21);
                                                    update_odds(odds_hf_2x, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out ID2x);
                                                    update_odds(odds_hf_22, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out ID22);
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
                                                    this.Temp_O_Odds_Dict[Dict_S_BetCode["半场/全场"]].RemoveAll(x => x.LeagueMatchID == objlm.ID && x.MatchID == objm.ID && x.BetCode == Dict_S_BetCode["半场/全场"] && x.BetExplain == "" && x.IsLive == "0" && (!IDlist_hf.Contains(x.ID)) && x.SourcePlatform == SourcePlatformEnum.SB.ToString());
                                                }
                                                #endregion
                                                #region 波胆
                                                if (m.CorrectScoreList.Count > 0)
                                                {
                                                    List<string> IDlist_cs = new List<string>();
                                                    foreach (OddsBD item in m.CorrectScoreList)
                                                    {
                                                        O_Odds odds_cs = new O_Odds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            LeagueMatchID = objlm.ID,
                                                            MatchID = objm.ID,
                                                            BetCode = Dict_S_BetCode["波胆"],
                                                            BetExplain = item.Text_H + "*" + item.Text_V,
                                                            OddsSort = "",
                                                            MainSort = 1,
                                                            Odds = Utility.ObjConvertToDecimal(item.Odds_BD),
                                                            IsLive = "0",
                                                            CreateTime = now,
                                                            ModifyTime = now,
                                                            SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                        };
                                                        if (item.type == 1)
                                                        {
                                                            odds_cs.BetCode = Dict_S_BetCode["波胆"];
                                                        }
                                                        else if (item.type == 2)
                                                        {
                                                            odds_cs.BetCode = Dict_S_BetCode["波胆-上半场"];
                                                        }
                                                        string IDcs = string.Empty;
                                                        update_odds(odds_cs, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDcs);
                                                        IDlist_cs.Add(IDcs);
                                                    }
                                                    //波胆
                                                    this.Temp_O_Odds_Dict[Dict_S_BetCode["波胆"]].RemoveAll(x => x.LeagueMatchID == objlm.ID && x.MatchID == objm.ID && x.BetCode == Dict_S_BetCode["波胆"] && x.BetExplain == "" && x.IsLive == "0" && (!IDlist_cs.Contains(x.ID)) && x.SourcePlatform == SourcePlatformEnum.SB.ToString());
                                                }
                                                #endregion
                                                #region 总入球
                                                if (m.TotalGoalList.Count > 0)
                                                {
                                                    List<string> IDlist_tg = new List<string>();
                                                    foreach (OddsZRQ item in m.TotalGoalList)
                                                    {
                                                        O_Odds odds_tg = new O_Odds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            LeagueMatchID = objlm.ID,
                                                            MatchID = objm.ID,
                                                            BetCode = Dict_S_BetCode["总进球数"],
                                                            BetExplain = item.Text_Goals,
                                                            OddsSort = "",
                                                            MainSort = 1,
                                                            Odds = Utility.ObjConvertToDecimal(item.Odds_ZRQ),
                                                            IsLive = "0",
                                                            CreateTime = now,
                                                            ModifyTime = now,
                                                            SourcePlatform = SourcePlatformEnum.SB.ToString()
                                                        };
                                                        if (item.type == 1)
                                                        {
                                                            odds_tg.BetCode = Dict_S_BetCode["总进球数"];
                                                        }
                                                        else if (item.type == 2)
                                                        {
                                                            odds_tg.BetCode = Dict_S_BetCode["总进球数-上半场"];
                                                        }
                                                        string IDtg = string.Empty;
                                                        update_odds(odds_tg, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDtg);
                                                        IDlist_tg.Add(IDtg);
                                                    }
                                                    //总进球数
                                                    this.Temp_O_Odds_Dict[Dict_S_BetCode["总进球数"]].RemoveAll(x => x.LeagueMatchID == objlm.ID && x.MatchID == objm.ID && x.BetCode == Dict_S_BetCode["总进球数"] && x.BetExplain == "" && x.IsLive == "0" && (!IDlist_tg.Contains(x.ID)) && x.SourcePlatform == SourcePlatformEnum.SB.ToString());
                                                }
                                                #endregion
                                            }
                                        }
                                        catch (Exception)
                                        {

                                        }
                                        #endregion
                                    }
                                }
                            }                            
                            #endregion
                        }
                    }
                }
            }
            if (key3 == 1)
            {
                DateTime now2 = lmbll.GetServerDateTime();
                switch (key1)
                {
                    case 1:
                        //锁定早盘消失赛事
                        this.Temp_A_Match.Where(x => x != null && x.LastMenuType == "1" && x.IsStart == null && x.IsLock == "0" && (!_ZPNoLockMID.Contains(x.ID))).ToList().ForEach(x =>
                        {
                            x.IsLock = "1";
                            x.ModifyTime = now2;
                            mbll.Update(x);
                        });
                        break;
                    case 2:
                        //锁定今日赛事消失赛事
                        this.Temp_A_Match.Where(x => x != null && x.LastMenuType == "2" && x.IsStart == null && x.IsLock == "0" && (!_JRNoLockMID.Contains(x.ID))).ToList().ForEach(x =>
                        {
                            x.IsLock = "1";
                            x.ModifyTime = now2;
                            mbll.Update(x);
                        });
                        break;
                    case 3:
                        //锁定滚球消失赛事
                        this.Temp_A_Match.Where(x => x != null && x.LastMenuType == "3" && x.IsStart != null && x.IsLock == "0" && (!_GQNoLockMID.Contains(x.ID))).ToList().ForEach(x =>
                        {
                            x.IsLock = "1";
                            x.ModifyTime = now2;
                            mbll.Update(x);
                        });
                        break;
                }                
                
            }
        }
        /// <summary>
        /// 独赢、让球、大小、单双
        /// </summary>
        private void dyrqdxds(List<Odds> oddsList, string lmid, string mid, DateTime now, string IsLive, string hf, ref int needupdatecount, ref int updatedcount, ref int deletedcount, ref string msgpush, ref string delmsgpush)
        {
            O_OddsBll obll = new O_OddsBll();
            O_OddsRecordBll orbll = new O_OddsRecordBll();
            int main = 0;
            List<string> IDlist_1x2 = new List<string>();
            List<string> IDlist_ah = new List<string>();
            List<string> IDlist_ou = new List<string>();
            List<string> IDlist_oe = new List<string>();
            foreach (Odds item in oddsList)
            {
                main++;
                #region 独赢
                O_Odds odds_1 = new O_Odds()
                {
                    ID = Guid.NewGuid().ToString("N"),
                    LeagueMatchID = lmid,
                    MatchID = mid,
                    BetCode = Dict_S_BetCode["独赢" + hf],
                    BetExplain = "",
                    OddsSort = "1",
                    MainSort = main,
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
                    BetCode = Dict_S_BetCode["独赢" + hf],
                    BetExplain = "",
                    OddsSort = "x",
                    MainSort = main,
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
                    BetCode = Dict_S_BetCode["独赢" + hf],
                    BetExplain = "",
                    OddsSort = "2",
                    MainSort = main,
                    Odds = Utility.ObjConvertToDecimal(item.Odds_KY),
                    IsLive = IsLive,
                    CreateTime = now,
                    ModifyTime = now,
                    SourcePlatform = SourcePlatformEnum.SB.ToString()
                };
                string ID1 = string.Empty, IDx = string.Empty, ID2 = string.Empty;
                update_odds(odds_1, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out ID1);
                update_odds(odds_x, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDx);
                update_odds(odds_2, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out ID2);
                IDlist_1x2.Add(ID1);
                IDlist_1x2.Add(IDx);
                IDlist_1x2.Add(ID2);
                #endregion

                #region 让球
                O_Odds odds_ah1 = new O_Odds()
                {
                    ID = Guid.NewGuid().ToString("N"),
                    LeagueMatchID = lmid,
                    MatchID = mid,
                    BetCode = Dict_S_BetCode["让球" + hf],
                    BetExplain = "",
                    OddsSort = "1",
                    MainSort = main,
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
                    BetCode = Dict_S_BetCode["让球" + hf],
                    BetExplain = "",
                    OddsSort = "2",
                    MainSort = main,
                    Odds = Utility.ObjConvertToDecimal(item.Odds_RQKY),
                    IsLive = IsLive,
                    CreateTime = now,
                    ModifyTime = now,
                    SourcePlatform = SourcePlatformEnum.SB.ToString()
                };

                if (!string.IsNullOrEmpty(item.Text_ZRKQ.Trim()))//主让客
                {
                    string rq = item.Text_ZRKQ.Trim();
                    if (rq.Contains("/"))
                    {
                        string[] arr = rq.Split('/');
                        rq = arr[0].Trim() + "/" + arr[1].Trim();
                    }
                    odds_ah1.BetExplain = "+" + rq;
                    odds_ah2.BetExplain = "-" + rq;
                }
                else if (!string.IsNullOrEmpty(item.Text_KRZQ.Trim()))//客让主
                {
                    string rq = item.Text_KRZQ.Trim();
                    if (rq.Contains("/"))
                    {
                        string[] arr = rq.Split('/');
                        rq = arr[0].Trim() + "/" + arr[1].Trim();
                    }
                    odds_ah1.BetExplain = "-" + rq;
                    odds_ah2.BetExplain = "+" + rq;
                }
                string IDah1 = string.Empty, IDah2 = string.Empty;
                update_odds(odds_ah1, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDah1);
                update_odds(odds_ah2, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDah2);
                IDlist_ah.Add(IDah1);
                IDlist_ah.Add(IDah2);
                #endregion

                #region 大小球
                string dxq = string.Empty;
                if (!string.IsNullOrEmpty(item.Text_DQ.Trim()))
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
                    BetCode = Dict_S_BetCode["大小" + hf],
                    BetExplain = dxq,
                    OddsSort = "o",
                    MainSort = main,
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
                    BetCode = Dict_S_BetCode["大小" + hf],
                    BetExplain = dxq,
                    OddsSort = "u",
                    MainSort = main,
                    Odds = Utility.ObjConvertToDecimal(item.Odds_XQ),
                    IsLive = IsLive,
                    CreateTime = now,
                    ModifyTime = now,
                    SourcePlatform = SourcePlatformEnum.SB.ToString()
                };
                string IDouo = string.Empty, IDouu = string.Empty;
                update_odds(odds_ou_o, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDouo);
                update_odds(odds_ou_u, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDouu);
                IDlist_ou.Add(IDouo);
                IDlist_ou.Add(IDouu);
                #endregion

                #region 单双
                O_Odds odds_oe_o = new O_Odds()
                {
                    ID = Guid.NewGuid().ToString("N"),
                    LeagueMatchID = lmid,
                    MatchID = mid,
                    BetCode = Dict_S_BetCode["单/双" + hf],
                    BetExplain = "o",
                    OddsSort = "",
                    MainSort = main,
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
                    BetCode = Dict_S_BetCode["单/双" + hf],
                    BetExplain = "e",
                    OddsSort = "",
                    MainSort = main,
                    Odds = Utility.ObjConvertToDecimal(item.Odds_S),
                    IsLive = IsLive,
                    CreateTime = now,
                    ModifyTime = now,
                    SourcePlatform = SourcePlatformEnum.SB.ToString()
                };
                string IDoeo = string.Empty, IDoee = string.Empty;
                update_odds(odds_oe_o, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDoeo);
                update_odds(odds_oe_e, now, obll, orbll, ref needupdatecount, ref updatedcount, ref msgpush, out IDoee);
                IDlist_oe.Add(IDoeo);
                IDlist_oe.Add(IDoee);
                #endregion
            }
            //独赢
            StringBuilder ids1x2 = new StringBuilder();
            foreach (string item in IDlist_1x2)
            {
                ids1x2.Append($"'{item}',");
            }
            int delcount = 0;
            string delmsg = new O_OddsBll().DeleteBySQL($"delete O_Odds where LeagueMatchID='{lmid}' and MatchID='{mid}' and BetCode='{Dict_S_BetCode["独赢" + hf]}' and IsLive='{IsLive}' and SourcePlatform='{SourcePlatformEnum.SB.ToString()}' and ID not in({ids1x2.ToString().TrimEnd(',')})", out delcount);
            if (string.IsNullOrEmpty(delmsg))
            {
                deletedcount += delcount;
                this.Temp_O_Odds_Dict[Dict_S_BetCode["独赢" + hf]].RemoveAll(x =>
                x.LeagueMatchID == lmid &&
                x.MatchID == mid &&
                x.BetCode == Dict_S_BetCode["独赢" + hf] &&
                x.IsLive == IsLive &&
                (!IDlist_1x2.Contains(x.ID)) &&
                x.SourcePlatform == SourcePlatformEnum.SB.ToString());
            }
            else
            {
                delmsgpush += delmsg + "\r\n";
            }
            //让球
            StringBuilder idsah= new StringBuilder();
            foreach (string item in IDlist_ah)
            {
                idsah.Append($"'{item}',");
            }
            delmsg = new O_OddsBll().DeleteBySQL($"delete O_Odds where LeagueMatchID='{lmid}' and MatchID='{mid}' and BetCode='{Dict_S_BetCode["让球" + hf]}' and IsLive='{IsLive}' and SourcePlatform='{SourcePlatformEnum.SB.ToString()}' and ID not in({idsah.ToString().TrimEnd(',')})", out delcount);
            if (string.IsNullOrEmpty(delmsg))
            {
                deletedcount += delcount;
                this.Temp_O_Odds_Dict[Dict_S_BetCode["让球" + hf]].RemoveAll(x =>
                x.LeagueMatchID == lmid &&
                x.MatchID == mid &&
                x.BetCode == Dict_S_BetCode["让球" + hf] &&
                x.IsLive == IsLive &&
                (!IDlist_ah.Contains(x.ID)) &&
                x.SourcePlatform == SourcePlatformEnum.SB.ToString());
            }
            else
            {
                delmsgpush += delmsg + "\r\n";
            }
            //大小
            StringBuilder idsou = new StringBuilder();
            foreach (string item in IDlist_ou)
            {
                idsou.Append($"'{item}',");
            }
            delmsg = new O_OddsBll().DeleteBySQL($"delete O_Odds where LeagueMatchID='{lmid}' and MatchID='{mid}' and BetCode='{Dict_S_BetCode["大小" + hf]}' and IsLive='{IsLive}' and SourcePlatform='{SourcePlatformEnum.SB.ToString()}' and ID not in({idsou.ToString().TrimEnd(',')})", out delcount);
            if (string.IsNullOrEmpty(delmsg))
            {
                deletedcount += delcount;
                this.Temp_O_Odds_Dict[Dict_S_BetCode["大小" + hf]].RemoveAll(x =>
                x.LeagueMatchID == lmid &&
                x.MatchID == mid &&
                x.BetCode == Dict_S_BetCode["大小" + hf] &&
                x.IsLive == IsLive &&
                (!IDlist_ou.Contains(x.ID)) &&
                x.SourcePlatform == SourcePlatformEnum.SB.ToString());
            }
            else
            {
                delmsgpush += delmsg + "\r\n";
            }
            //单/双
            StringBuilder idsoe = new StringBuilder();
            foreach (string item in IDlist_oe)
            {
                idsoe.Append($"'{item}',");
            }
            delmsg = new O_OddsBll().DeleteBySQL($"delete O_Odds where LeagueMatchID='{lmid}' and MatchID='{mid}' and BetCode='{Dict_S_BetCode["单/双" + hf]}' and IsLive='{IsLive}' and SourcePlatform='{SourcePlatformEnum.SB.ToString()}' and ID not in({idsoe.ToString().TrimEnd(',')})", out delcount);
            if (string.IsNullOrEmpty(delmsg))
            {
                deletedcount += delcount;
                this.Temp_O_Odds_Dict[Dict_S_BetCode["单/双" + hf]].RemoveAll(x =>
                x.LeagueMatchID == lmid &&
                x.MatchID == mid &&
                x.BetCode == Dict_S_BetCode["单/双" + hf] &&
                x.IsLive == IsLive &&
                (!IDlist_oe.Contains(x.ID)) &&
                x.SourcePlatform == SourcePlatformEnum.SB.ToString());
            }
            else
            {
                delmsgpush += delmsg + "\r\n";
            }            

        }
        private void update_odds(O_Odds odds, DateTime now, O_OddsBll obll, O_OddsRecordBll orbll, ref int needupdatecount, ref int updatedcount, ref string msgpush,out string ID)
        {
            ID = "";
            string msg = string.Empty;
            var lastodds = this.Temp_O_Odds_Dict[odds.BetCode].Where(x => x.LeagueMatchID == odds.LeagueMatchID && x.MatchID == odds.MatchID && x.BetCode == odds.BetCode && x.BetExplain == odds.BetExplain && x.OddsSort == odds.OddsSort && x.MainSort == odds.MainSort).OrderByDescending(x => x.ModifyTime).FirstOrDefault();
            O_OddsRecord oddsrecord = new O_OddsRecord()
            {
                ID = Guid.NewGuid().ToString("N"),
                LeagueMatchID = odds.LeagueMatchID,
                MatchID = odds.MatchID,
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
                msg = obll.Create(odds);
                if (string.IsNullOrEmpty(msg))
                {
                    ID = odds.ID;
                    updatedcount++;
                    this.Temp_O_Odds_Dict[odds.BetCode].Add(odds);
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
                if (lastodds.Odds != odds.Odds)
                {
                    needupdatecount++;
                    var newo = (O_Odds)Utility.DeepCopy(lastodds);
                    newo.Odds = odds.Odds;
                    newo.IsLive = odds.IsLive;
                    newo.ModifyTime = now;
                    msg = obll.Update(newo);
                    if (string.IsNullOrEmpty(msg))
                    {
                        ID = newo.ID;
                        updatedcount++;
                        //this.Temp_O_Odds_Dict[odds.BetCode].Remove(lastodds);
                        //this.Temp_O_Odds_Dict[odds.BetCode].Add(newo);
                        this.Temp_O_Odds_Dict[odds.BetCode].Where(x => x.ID == newo.ID).ToList().ForEach(o=> 
                        {
                            o.Odds = odds.Odds;
                            o.IsLive = odds.IsLive;
                            o.ModifyTime = now;
                        });
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
            }
        }
        
        private void btn_Start_Click(object sender, EventArgs e)
        {
            if (_SourcePlatform == null)
            {
                foreach (Control item in this.pnl_SourcePlatform.Controls)
                {
                    if (item is RadioButton)
                    {
                        RadioButton rdo = item as RadioButton;
                        if (rdo.Checked)
                        {
                            _SourcePlatform = new SourcePlatform(rdo.Tag.ToString());
                            break;
                        }
                    }
                }
            }
            if (_SourcePlatform == null)
            {
                MessageBox.Show("平台信息异常");
                return;
            }
            _ZP_MS = 1000;
            _JR_MS = 1000;
            _GQ_MS = 1000;
            _IsRunning = true;
            this.btn_Start.Text = "运行中";
            this.btn_Start.Enabled = false;
            this.btn_Stop.Enabled = true;
            this.txt_ZP_MS.Enabled = false;
            this.txt_JR_MS.Enabled = false;
            this.txt_GQ_MS.Enabled = false;

            int zpms = 0, jrms = 0, gqms = 0;
            int.TryParse(this.txt_ZP_MS.Text.Trim(), out zpms);
            int.TryParse(this.txt_JR_MS.Text.Trim(), out jrms);
            int.TryParse(this.txt_GQ_MS.Text.Trim(), out gqms);
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
            _ZP_AllDay = this.chk_ZP_AllDay.Checked;
            foreach (Log item in _LogWinList)
            {
                item.Close();
            }
            foreach (IWebDriver item in _WebDriverList)
            {
                if (item!=null)
                {
                    item.Quit();
                }                
            }
            InitTempData();
            if (this.chk_Logined.Checked)
            {
                this.btn_Init.PerformClick();
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
                                            log.Text = "赛果，" + menuarr[1, 0, int.Parse(tags[1]) - 1];
                                            log.Show();
                                            chk.ImageKey = chk.Tag.ToString();
                                            chk.Tag = log;
                                            _LogWinList.Add(log);
                                            Task.Run(async () =>
                                            {
                                                await Task.Delay(100);
                                                GetResultData(tags[1], log);
                                            });
                                        }
                                        else
                                        {
                                            string[] tags = chk.Tag.ToString().Split('-');
                                            Log log = new Log();
                                            log.Text = menuarr[0, 0, int.Parse(tags[0]) - 1] + "，"
                                                + menuarr[1, 0, int.Parse(tags[1]) - 1] + "，"
                                                + menuarr[2, 0, int.Parse(tags[2]) - 1];
                                            log.Show();
                                            chk.ImageKey = chk.Tag.ToString();
                                            chk.Tag = log;
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
            if (count<1)
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
        private void chk_Football_CheckedChanged(object sender, EventArgs e)
        {
            if (this.chk_Football.Checked)
            {
                if (!this.chk_Logined.Checked)
                {
                    this.chk_Logined.Checked = true;
                }
            }
        }
        //登录平台
        private void btn_Init_Click(object sender, EventArgs e)
        {
            if (_SourcePlatform==null)
            {
                foreach (Control item in this.pnl_SourcePlatform.Controls)
                {
                    if (item is RadioButton)
                    {
                        RadioButton rdo = item as RadioButton;
                        if (rdo.Checked)
                        {
                            _SourcePlatform = new SourcePlatform(rdo.Tag.ToString());
                            break;
                        }
                    }
                }
            }
            if (_SourcePlatform == null)
            {
                MessageBox.Show("平台信息异常");
                return;
            }
            //用的session保存登录状态
            IWebDriver driver = WebDriverHelper.CreateChromeDriver(true, true);
            if (driver == null)
            {
                MessageBox.Show(WebDriverHelper.ErrorMessage);
                return;
            }
            //_WebDriverList.Add(driver);
            driver.Navigate().GoToUrl(_SourcePlatform.url);
            if (WebDriverHelper.AlertExist(driver))
            {
                driver.SwitchTo().Alert().Accept();
            }

            //IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            //js.ExecuteScript("window.open('about:blank','_blank');");
            //var wname = driver.WindowHandles;
            //driver.SwitchTo().Frame("sportsbook").SwitchTo().Frame("sportsFrame");
            if (_SourcePlatform.key=="TYC")
            {
                #region 太阳城
                bool iswh = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#sportsFrame"), _WebDriver_MS);
                if (!iswh)
                {
                    this._IsMaintenance = true;
                    GetMaintenanceInfo(null);
                    driver.Quit();
                    MessageBox.Show("异常，沙巴可能在维护。");
                    return;
                }
                driver.SwitchTo().Frame("sportsFrame");
                iswh = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#sbContainer"), _WebDriver_MS);
                if (!iswh)
                {
                    this._IsMaintenance = true;
                    GetMaintenanceInfo(null);
                    driver.Quit();
                    MessageBox.Show("异常，沙巴可能在维护。");
                    return;
                }
                string username = driver.FindElement(By.CssSelector("#sbContainer #sb-header #n1")).GetAttribute("innerHTML");
                if (!username.Contains(","))//未登录
                {
                    driver.SwitchTo().DefaultContent();
                    driver.Navigate().GoToUrl(_SourcePlatform.loginurl);
                    bool tycwh = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#authentication"), _WebDriver_MS);
                    if (!tycwh)
                    {
                        driver.Quit();
                        MessageBox.Show("异常，太阳城可能在维护。");
                        return;
                    }
                    driver.FindElement(By.CssSelector("#authentication form input[name='username']")).SendKeys(_SourcePlatform.loginname);
                    driver.FindElement(By.CssSelector("#authentication form input[name='password']")).SendKeys(_SourcePlatform.loginpassword);
                    Thread.Sleep(1000);
                    bool flag = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#authentication form input[type='submit']"), 50);
                    if (flag)
                    {
                        driver.FindElement(By.CssSelector("#authentication form input[type='submit']")).Click();
                    }
                    Thread.Sleep(1000);
                    //driver.SwitchTo().Frame("sportsbook").SwitchTo().Frame("sportsFrame");
                    bool flag01 = false;
                    while (!flag01)
                    {
                        flag01 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#sportsbook"), _WebDriver_MS);
                    }
                    driver.SwitchTo().DefaultContent();
                    string loginingurl = driver.FindElement(By.CssSelector("#sportsbook")).GetAttribute("src");
                    driver.Navigate().GoToUrl(loginingurl);
                    string newurl = driver.Url;
                    //driver.SwitchTo().Frame("sportsbook");
                    bool flag02 = false;
                    //while (!flag02)
                    //{
                    flag02 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#sportsFrame"), _WebDriver_MS);
                    //}
                    if (!flag02)
                    {
                        this._IsMaintenance = true;
                        GetMaintenanceInfo(null);
                        driver.Quit();
                        MessageBox.Show("异常，沙巴可能在维护。");
                        return;
                    }
                    driver.SwitchTo().Frame("sportsFrame");
                    bool flag03 = false;
                    while (!flag03)
                    {
                        flag03 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#header #betList"), _WebDriver_MS);
                    }
                    //Thread.Sleep(10000);
                    //MessageBox.Show(driver.Url);
                    if (newurl.Contains("NewIndex"))
                    {
                        newurl = newurl.Substring(0, newurl.IndexOf("?"));
                        newurl += "?webskintype=2&lang=cs";
                        _SourcePlatform.url = newurl;
                    }
                    //MessageBox.Show(newurl);
                    //string ccc = string.Empty;
                    //foreach (var item in driver.Manage().Cookies.AllCookies)
                    //{
                    //    ccc += item.Domain + "-" + item.Name + ":" + item.Value + "\r\n";
                    //}
                    //MessageBox.Show(ccc);
                    //return;
                    //driver.Navigate().GoToUrl(_SourcePlatform.url);
                    string strcookie = string.Empty;
                    _CookieList.Clear();
                    foreach (var item in driver.Manage().Cookies.AllCookies)
                    {
                        strcookie += item.Name + ":" + item.Value + "\r\n";
                        _CookieList.Add(item);
                    }
                    //赛果
                    driver.SwitchTo().DefaultContent();
                    driver.Navigate().GoToUrl(_SourcePlatform.resulturl);
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
                    driver.Navigate().GoToUrl(_SourcePlatform.resulturl);
                    _ResultCookieList.Clear();
                    foreach (var item in driver.Manage().Cookies.AllCookies)
                    {
                        _ResultCookieList.Add(item);
                    }
                    driver.Quit();
                }
                #endregion
            }
            else if (_SourcePlatform.key == "MS")
            {
                #region 明升
                //bool iswh = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#sbContainer"), _WebDriver_MS);
                //if (!iswh)
                //{
                //    MessageBox.Show("异常，可能在维护。");
                //    return;
                //}
                bool iswh = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#sportsFrame"), _WebDriver_MS);
                if (!iswh)
                {
                    this._IsMaintenance = true;
                    driver.Quit();
                    MessageBox.Show("异常，可能在维护。");
                    return;
                }
                driver.SwitchTo().Frame("sportsFrame");
                string username = driver.FindElement(By.CssSelector("#sbContainer #sb-header #n1")).GetAttribute("innerHTML");
                if (!username.Contains(","))//未登录
                {
                    driver.Navigate().GoToUrl(_SourcePlatform.loginurl);
                    driver.SwitchTo().Frame("frameLogin");
                    driver.FindElement(By.CssSelector("#loginForm input[name='txtLoginID']")).SendKeys(_SourcePlatform.loginname);
                    driver.FindElement(By.CssSelector("#loginForm #mockpass")).Click();
                    Thread.Sleep(100);
                    driver.FindElement(By.CssSelector("#loginForm input[name='txtPassword']")).SendKeys(_SourcePlatform.loginpassword);
                    Thread.Sleep(1000);
                    bool flag = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#loginForm input[type='submit']"), 50);
                    if (flag)
                    {
                        driver.FindElement(By.CssSelector("#loginForm input[type='submit']")).Click();
                    }
                    Thread.Sleep(1000);
                    bool flag01 = false;
                    while (!flag01)
                    {
                        flag01 = WebDriverHelper.WaitForElementInvisible(driver, By.CssSelector("#loginProcessing"), _WebDriver_MS);
                    }
                    driver.SwitchTo().DefaultContent();
                    driver.Navigate().GoToUrl(_SourcePlatform.url);
                    bool flag02 = false;
                    while (!flag02)
                    {
                        flag02 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#sportsFrame"), _WebDriver_MS);
                    }
                    driver.SwitchTo().Frame("sportsFrame");
                    bool flag03 = false;
                    while (!flag03)
                    {
                        flag03 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#sbContainer"), _WebDriver_MS);
                    }
                    //Thread.Sleep(10000);
                    //driver.SwitchTo().DefaultContent();
                    driver.Navigate().GoToUrl(_SourcePlatform.url);
                    string strcookie = string.Empty;
                    _CookieList.Clear();
                    foreach (var item in driver.Manage().Cookies.AllCookies)
                    {
                        strcookie += item.Name + ":" + item.Value + "\r\n";
                        _CookieList.Add(item);
                    }
                    //赛果
                    driver.SwitchTo().DefaultContent();
                    driver.Navigate().GoToUrl(_SourcePlatform.resulturl);
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
                    driver.Navigate().GoToUrl(_SourcePlatform.resulturl);
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
                    }
                }

            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            TimeSpan ts = DateTime.Now - _runstarttime;
            this.lbl_runtime.Text = ts.Days + "天" + ts.Hours + "小时" + ts.Minutes + "分" + ts.Seconds + "秒";
        }

        private void GetResultData(string sportsType, Log logwin)
        {
            int awaitms = 10000;
            IWebDriver driver = WebDriverHelper.CreateChromeDriver(chk_VisibleChrome.Checked);
            if (driver == null)
            {
                MessageBox.Show(WebDriverHelper.ErrorMessage);
                logwin.txt_log.AppendText(WebDriverHelper.ErrorMessage + "\r\n");
                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", WebDriverHelper.ErrorMessage + "\r\n");
                return;
            }
            _WebDriverList.Add(driver);
            this.Invoke(new MethodInvoker(delegate
            {
                _IsRunning = true;
                logwin.txt_log.AppendText("地址：" + _SourcePlatform.resulturl + "\r\n");
                logwin.txt_log.AppendText("\r\n");
                logwin.txt_log.AppendText("开始初次请求：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n");
                logwin.txt_log.AppendText("===========================================\r\n");
                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "地址：" + _SourcePlatform.resulturl + "\r\n\r\n开始初次请求：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n===========================================\r\n");
            }));
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            driver.Navigate().GoToUrl(_SourcePlatform.resulturl);
            driver.Manage().Cookies.DeleteAllCookies();
            foreach (var item in _ResultCookieList)
            {
                driver.Manage().Cookies.AddCookie(item);
            }
            driver.Navigate().Refresh();
            watch.Start();
            if (_SourcePlatform.key == "TYC")
            {
                bool iswh = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#rusultContainer"), _WebDriver_MS);
                if (!iswh)
                {
                    this.Invoke(new MethodInvoker(delegate
                    {
                        logwin.txt_log.AppendText("异常，沙巴可能在维护。\r\n");
                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "异常，沙巴可能在维护。\r\n");
                        GetMaintenanceInfo(logwin);
                        //this.btn_Stop.PerformClick();
                    }));
                    return;
                }
                int count = 0;
                Task.Run(async () =>
                {
                    while (_IsRunning)
                    {
                        await Task.Delay(awaitms);
                        count++;
                        if (count != 1)
                        {
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("开始第" + count + "次请求：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n");
                                logwin.txt_log.AppendText("===========================================\r\n");
                                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "开始第" + count + "次请求：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n===========================================\r\n");
                            }));
                        }
                        string html = string.Empty;
                        try
                        {
                            #region MyRegion
                            int flag01count = 0;
                            bool flag01 = false;
                            while (!flag01)
                            {
                                flag01 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#f_trigger_a"), _WebDriver_MS);
                                flag01count++;
                                if (flag01count>200)
                                {
                                    driver.Navigate().Refresh();
                                }
                            }
                            //选择今日
                            bool flagcheckresult = true;
                            driver.FindElement(By.CssSelector(".filterBlock>.filterRow>button:nth-child(2)")).Click();
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("选择今日\r\n");
                                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择今日\r\n");
                            }));
                            Thread.Sleep(500);
                            bool flag02 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#ddSport"), 500);
                            if (flag02)
                            {
                                switch (sportsType)
                                {
                                    case "1":
                                        //选择足球
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
                                                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择足球\r\n");
                                            }));
                                        }
                                        else
                                        {
                                            this.Invoke(new MethodInvoker(delegate
                                            {
                                                logwin.txt_log.AppendText("今日无足球\r\n");
                                                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "今日无足球\r\n");
                                            }));
                                            flagcheckresult = false;
                                        }
                                        break;
                                }
                            }
                            else
                            {
                                this.Invoke(new MethodInvoker(delegate
                                {
                                    logwin.txt_log.AppendText("今日无任何数据\r\n");
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "今日无任何数据\r\n");
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
                                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "今日暂无赛果\r\n===========================================\r\n");
                                    }));
                                }
                                else
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

                                    IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                                    js.ExecuteScript("$('#rusultContainer .tableBody>span.leagueName').addClass('xxxxxxxxxxxx');");
                                    Thread.Sleep(1000);

                                    html = driver.FindElement(By.CssSelector("#rusultContainer .tableBody")).GetAttribute("innerHTML");
                                }
                            }
                            bool flagcheckresult2 = true;
                            //选择昨日
                            driver.FindElement(By.CssSelector(".filterBlock>.filterRow>button:nth-child(3)")).Click();
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("选择昨日\r\n");
                                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择昨日\r\n");
                            }));
                            Thread.Sleep(500);
                            bool flag03 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#ddSport"), 500);
                            if (flag03)
                            {
                                switch (sportsType)
                                {
                                    case "1":
                                        //选择足球
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
                                                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择足球\r\n");
                                            }));
                                        }
                                        else
                                        {
                                            this.Invoke(new MethodInvoker(delegate
                                            {
                                                logwin.txt_log.AppendText("昨日无足球\r\n");
                                                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "昨日无足球\r\n");
                                            }));
                                            flagcheckresult2 = false;
                                        }
                                        break;
                                }
                            }
                            else
                            {
                                this.Invoke(new MethodInvoker(delegate
                                {
                                    logwin.txt_log.AppendText("昨日无任何数据\r\n");
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "昨日无任何数据\r\n");
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
                                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "昨日暂无赛果\r\n===========================================\r\n");
                                    }));
                                }
                                else
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

                                    IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                                    js.ExecuteScript("$('#rusultContainer .tableBody>span.leagueName').addClass('xxxxxxxxxxxx');");
                                    Thread.Sleep(1000);

                                    html += driver.FindElement(By.CssSelector("#rusultContainer .tableBody")).GetAttribute("innerHTML");
                                }
                            }
                            if (flag1 && flag2 && flag11 && flag22)
                            {
                                this.Invoke(new MethodInvoker(delegate
                                {
                                    logwin.txt_log.AppendText("无数据\r\n");
                                    logwin.txt_log.AppendText("===========================================\r\n");
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "无数据\r\n===========================================\r\n");
                                }));
                            }
                            else
                            {
                                this.Invoke(new MethodInvoker(delegate
                                {
                                    logwin.txt_log.AppendText("===========================================\r\n");
                                    logwin.txt_log.AppendText("数据抓取完毕\r\n");
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "===========================================\r\n数据抓取完毕\r\n");
                                }));
                                int updatecount = 0;
                                string msgpush = string.Empty;
                                //解析数据
                                AnalysisResultHtml_TYC(html, sportsType,ref updatecount, ref msgpush);
                                this.Invoke(new MethodInvoker(delegate
                                {
                                    logwin.txt_log.AppendText("错误：" + msgpush);
                                    logwin.txt_log.AppendText("更新成功：" + updatecount + "\r\n");
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "错误：" + msgpush + "更新成功：" + updatecount + "\r\n");
                                }));
                                watch.Stop();
                                this.Invoke(new MethodInvoker(delegate
                                {
                                    //logwin.txt_log.AppendText(html);
                                    logwin.txt_log.AppendText("===========================================\r\n");
                                    logwin.txt_log.AppendText("耗时：" + watch.Elapsed.TotalSeconds + "秒\r\n\r\n");
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "===========================================\r\n耗时：" + watch.Elapsed.TotalSeconds + "秒\r\n\r\n");
                                }));
                            }
                            #endregion

                        }
                        catch (Exception ex)
                        {
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("===========================================\r\n");
                                logwin.txt_log.AppendText("第" + count + "次异常：" + ex.Message + ex.StackTrace + "\r\n\r\n");
                                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "===========================================\r\n第" + count + "次异常：" + ex.Message + ex.StackTrace + "\r\n\r\n");
                            }));
                            continue;
                        }
                    }
                });
            }
            else if (_SourcePlatform.key == "MS")
            {
                bool iswh = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#rusultContainer"), _WebDriver_MS);
                if (!iswh)
                {
                    this.Invoke(new MethodInvoker(delegate
                    {
                        logwin.txt_log.AppendText("异常，可能在维护。\r\n");
                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "异常，可能在维护。\r\n");
                        //this.btn_Stop.PerformClick();
                    }));
                    return;
                }
                int count = 0;
                Task.Run(async () =>
                {
                    while (_IsRunning)
                    {
                        await Task.Delay(awaitms);
                        count++;
                        if (count != 1)
                        {
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("开始第" + count + "次请求：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n");
                                logwin.txt_log.AppendText("===========================================\r\n");
                                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "开始第" + count + "次请求：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n===========================================\r\n");
                            }));
                        }
                        string html = string.Empty;
                        try
                        {
                            #region MyRegion
                            int flag01count = 0;
                            bool flag01 = false;
                            while (!flag01)
                            {
                                flag01 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#f_trigger_a"), _WebDriver_MS);
                                flag01count++;
                                if (flag01count > 200)
                                {
                                    driver.Navigate().Refresh();
                                }
                            }
                            //选择今日
                            bool flagcheckresult = true;
                            driver.FindElement(By.CssSelector(".filterBlock>.filterRow>button:nth-child(2)")).Click();
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("选择今日\r\n");
                                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择今日\r\n");
                            }));
                            Thread.Sleep(500);
                            bool flag02 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#ddSport"), 500);
                            if (flag02)
                            {
                                switch (sportsType)
                                {
                                    case "1":
                                        //选择足球
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
                                                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择足球\r\n");
                                            }));
                                        }
                                        else
                                        {
                                            this.Invoke(new MethodInvoker(delegate
                                            {
                                                logwin.txt_log.AppendText("今日无足球\r\n");
                                                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "今日无足球\r\n");
                                            }));
                                            flagcheckresult = false;
                                        }
                                        break;
                                }
                            }
                            else
                            {
                                this.Invoke(new MethodInvoker(delegate
                                {
                                    logwin.txt_log.AppendText("今日无任何数据\r\n");
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "今日无任何数据\r\n");
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
                                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "今日暂无赛果\r\n===========================================\r\n");
                                    }));
                                }
                                else
                                {
                                    foreach (var item in driver.FindElements(By.CssSelector("#rusultContainer .tableBody .tableRow")))
                                    {
                                        bool flag = WebDriverHelper.ElementExist(item, By.CssSelector(".other .smallBtn"));
                                        if (flag)
                                        {
                                            item.FindElement(By.CssSelector(".other .smallBtn")).Click();
                                            await Task.Delay(50);
                                            if (!item.FindElement(By.CssSelector(".other .smallBtn")).GetAttribute("class").Contains("specialC"))
                                            {
                                                item.FindElement(By.CssSelector(".other .smallBtn")).Click();
                                            }
                                        }
                                    }
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("展开\r\n");
                                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "展开\r\n");
                                    }));

                                    IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                                    js.ExecuteScript("$('#rusultContainer .tableBody>span.leagueName').addClass('xxxxxxxxxxxx');");
                                    Thread.Sleep(1000);

                                    html = driver.FindElement(By.CssSelector("#rusultContainer .tableBody")).GetAttribute("innerHTML");
                                }
                            }
                            bool flagcheckresult2 = true;
                            //选择昨日
                            driver.FindElement(By.CssSelector(".filterBlock>.filterRow>button:nth-child(3)")).Click();
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("选择昨日\r\n");
                                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择昨日\r\n");
                            }));
                            Thread.Sleep(500);
                            bool flag03 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#ddSport"), 500);
                            if (flag03)
                            {
                                switch (sportsType)
                                {
                                    case "1":
                                        //选择足球
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
                                                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "选择足球\r\n");
                                            }));
                                        }
                                        else
                                        {
                                            this.Invoke(new MethodInvoker(delegate
                                            {
                                                logwin.txt_log.AppendText("昨日无足球\r\n");
                                                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "昨日无足球\r\n");
                                            }));
                                            flagcheckresult2 = false;
                                        }
                                        break;
                                }
                            }
                            else
                            {
                                this.Invoke(new MethodInvoker(delegate
                                {
                                    logwin.txt_log.AppendText("昨日无任何数据\r\n");
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "昨日无任何数据\r\n");
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
                                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "昨日暂无赛果\r\n===========================================\r\n");
                                    }));
                                }
                                else
                                {
                                    foreach (var item in driver.FindElements(By.CssSelector("#rusultContainer .tableBody .tableRow")))
                                    {
                                        bool flag = WebDriverHelper.ElementExist(item, By.CssSelector(".other .smallBtn"));
                                        if (flag)
                                        {
                                            item.FindElement(By.CssSelector(".other .smallBtn")).Click();
                                            await Task.Delay(50);
                                            if (!item.FindElement(By.CssSelector(".other .smallBtn")).GetAttribute("class").Contains("specialC"))
                                            {
                                                item.FindElement(By.CssSelector(".other .smallBtn")).Click();
                                            }
                                        }
                                    }
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("展开\r\n");
                                        Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "展开\r\n");
                                    }));

                                    IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                                    js.ExecuteScript("$('#rusultContainer .tableBody>span.leagueName').addClass('xxxxxxxxxxxx');");
                                    Thread.Sleep(1000);

                                    html += driver.FindElement(By.CssSelector("#rusultContainer .tableBody")).GetAttribute("innerHTML");
                                }
                            }
                            if (flag1 && flag2 && flag11 && flag22)
                            {
                                this.Invoke(new MethodInvoker(delegate
                                {
                                    logwin.txt_log.AppendText("无数据\r\n");
                                    logwin.txt_log.AppendText("===========================================\r\n");
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "无数据\r\n===========================================\r\n");
                                }));
                            }
                            else
                            {
                                this.Invoke(new MethodInvoker(delegate
                                {
                                    logwin.txt_log.AppendText("===========================================\r\n");
                                    logwin.txt_log.AppendText("数据抓取完毕\r\n");
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "===========================================\r\n数据抓取完毕\r\n");
                                }));
                                //解析数据
                                AnalysisResultHtml_MS(html, sportsType);
                                watch.Stop();
                                this.Invoke(new MethodInvoker(delegate
                                {
                                    //logwin.txt_log.AppendText(html);
                                    logwin.txt_log.AppendText("===========================================\r\n");
                                    logwin.txt_log.AppendText("耗时：" + watch.Elapsed.TotalSeconds + "秒\r\n\r\n");
                                    Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "===========================================\r\n耗时：" + watch.Elapsed.TotalSeconds + "秒\r\n\r\n");
                                }));
                            }
                            #endregion

                        }
                        catch (Exception ex)
                        {
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("===========================================\r\n");
                                logwin.txt_log.AppendText("第" + count + "次异常：" + ex.Message + ex.StackTrace + "\r\n\r\n");
                                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", "===========================================\r\n第" + count + "次异常：" + ex.Message + ex.StackTrace + "\r\n\r\n");
                            }));
                            continue;
                        }
                    }
                });
            }

            
        }       

        private void AnalysisResultHtml_TYC(string html, string sportsType,ref int updatecount, ref string msgpush)
        {            
            if (_IsRunning)
            {
                A_LeagueMatchBll lmbll = new A_LeagueMatchBll();
                A_TeamBll tbll = new A_TeamBll();
                A_MatchBll mbll = new A_MatchBll();
                A_MatchResultBll mrbll = new A_MatchResultBll();
                DateTime now = lmbll.GetServerDateTime();
                string msg = string.Empty;
                switch (sportsType)
                {
                    case "1"://足球
                        #region 足球
                        html = html.Replace("&nbsp;", "").Replace("\r\n", "").Trim();
                        //html = "<root>" + html + "</root>";
                        html = html.Replace("<span class=\"leagueName xxxxxxxxxxxx\">", "$************************$<span class=\"leagueName xxxxxxxxxxxx\">");
                        string[] htmlarr = html.Split(new string[] { "$************************$" }, StringSplitOptions.None);
                        foreach (var item in htmlarr)
                        {
                            if (string.IsNullOrEmpty(item))
                            {
                                continue;
                            }
                            string lnhtml = "<root>" + item + "</root>";
                            var doc = new System.Xml.XmlDocument();
                            doc.LoadXml(lnhtml);
                            string LeagueMatchName = doc.DocumentElement.SelectSingleNode("span[1]").InnerText.Trim();
                            if (string.IsNullOrEmpty(LeagueMatchName) 
                                || LeagueMatchName.Contains("梦幻对垒")
                                || LeagueMatchName.Contains("角球")
                                || LeagueMatchName.Contains("特定15分钟")
                                || LeagueMatchName.Contains("测试")
                                || LeagueMatchName.Contains("主场/客场")
                                || LeagueMatchName.Contains("哪一队先开球")
                                || LeagueMatchName.Contains("半场结束前受伤延长补时")
                                || LeagueMatchName.Contains("总入球分钟")
                                )
                            {
                                continue;
                            }
                            //联赛
                            A_LeagueMatch objlm = this.Temp_A_LeagueMatch.Where(x => x != null && x.Name == LeagueMatchName).FirstOrDefault();
                            if (objlm == null)
                            {
                                objlm = new A_LeagueMatch()
                                {
                                    ID = Guid.NewGuid().ToString("N"),
                                    Name = LeagueMatchName,
                                    ModifyTime = now,
                                    SourcePlatform = SourcePlatformEnum.SB.ToString(),
                                    SportsType = SportsTypeEnum.Football.ToString()
                                };
                                msg = lmbll.Create(objlm);
                                if (string.IsNullOrEmpty(msg))
                                {
                                    this.Temp_A_LeagueMatch.Add(objlm);
                                }
                                else
                                {
                                    this.Temp_A_LeagueMatch = lmbll.FindAll(SourcePlatformEnum.SB.ToString());
                                }
                            }
                            foreach (System.Xml.XmlNode tableRow in doc.DocumentElement.SelectNodes("div"))
                            {
                                if (!tableRow.Attributes["class"].Value.Contains("tableRow"))
                                {
                                    continue;
                                }
                                string status = tableRow.ChildNodes[5].FirstChild.InnerText.Trim();
                                if (status == "进行中" || status == "等待中")
                                {
                                    continue;
                                }
                                //DateTime starttime = DateTime.ParseExact(tableRow.FirstChild.InnerText.Trim(), "MM/dd/yyyy hh:mm tt", System.Globalization.CultureInfo.CurrentCulture);
                                DateTime starttime = Convert.ToDateTime(tableRow.FirstChild.InnerText.Trim());//MM/dd/yyyy hh:mm tt
                                var namedivs = tableRow.ChildNodes[1].SelectNodes("div[contains(@class,'name')]");
                                string htname = namedivs[0].InnerText.Trim();
                                string vtname = namedivs[1].InnerText.Trim();
                                if (string.IsNullOrEmpty(htname) || string.IsNullOrEmpty(vtname))
                                {
                                    continue;
                                }
                                if (htname.Contains("加时") || htname.Contains("点球") || vtname.Contains("加时") || vtname.Contains("点球"))
                                {
                                    continue;
                                }
                                //主队
                                A_Team objtH = this.Temp_A_Team.Where(x => x != null && x.LeagueMatchID == objlm.ID && x.Name == htname).FirstOrDefault();
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
                                        SportsType = SportsTypeEnum.Football.ToString()
                                    };
                                    msg1 = tbll.Create(objtH);
                                    if (string.IsNullOrEmpty(msg1))
                                    {
                                        this.Temp_A_Team.Add(objtH);
                                    }
                                    else
                                    {
                                        this.Temp_A_Team = tbll.FindAll(SourcePlatformEnum.SB.ToString());
                                    }
                                }
                                //客队
                                A_Team objtV = this.Temp_A_Team.Where(x => x != null && x.LeagueMatchID == objlm.ID && x.Name == vtname).FirstOrDefault();
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
                                        SportsType = SportsTypeEnum.Football.ToString()
                                    };
                                    msg2 = tbll.Create(objtV);
                                    if (string.IsNullOrEmpty(msg2))
                                    {
                                        this.Temp_A_Team.Add(objtV);
                                    }
                                    else
                                    {
                                        this.Temp_A_Team = tbll.FindAll(SourcePlatformEnum.SB.ToString());
                                    }
                                }
                                if (string.IsNullOrEmpty(msg1) && string.IsNullOrEmpty(msg2))
                                {
                                    A_Match objm = this.Temp_A_Match.Where(x => x != null && x.HomeTeamID == objtH.ID && x.VisitingTeamID == objtV.ID && x.SP_GameStartTime.Value.Date == starttime.Date).FirstOrDefault();
                                    if (objm!=null)
                                    {
                                        objm.IsEnd = "1";
                                        objm.ModifyTime = now;
                                        //objm.GameEndTime = now;
                                        mbll.Update(objm);
                                    }
                                    A_MatchResult objmr = this.Temp_A_MatchResult.Where(x => x != null && x.LeagueMatchID == objlm.ID && x.HomeTeamID == objtH.ID && x.VisitingTeamID == objtV.ID && x.GameStartTime == starttime).FirstOrDefault();
                                    if (objmr == null)
                                    {
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
                                            SportsType = SportsTypeEnum.Football.ToString(),
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
                                        msg = mrbll.Create(objmr);
                                        if (string.IsNullOrEmpty(msg))
                                        {
                                            updatecount++;
                                            this.Temp_A_MatchResult.Add(objmr);
                                        }
                                        else
                                        {
                                            msgpush += msg + "\r\n";
                                            this.Temp_A_MatchResult = mrbll.FindByDate(SourcePlatformEnum.SB.ToString(), now.Date.AddDays(-1));
                                        }
                                    }
                                }

                            }
                        }
                        #endregion
                        break;
                }
            }
        }

        

        private void AnalysisResultHtml_MS(string html, string sportsType)
        {
            if (_IsRunning)
            {
                A_LeagueMatchBll lmbll = new A_LeagueMatchBll();
                A_TeamBll tbll = new A_TeamBll();
                A_MatchResultBll mrbll = new A_MatchResultBll();
                DateTime now = lmbll.GetServerDateTime();
                string msg = string.Empty;
                switch (sportsType)
                {
                    case "1"://足球
                        #region 足球
                        html = html.Replace("&nbsp;", "").Replace("\r\n", "").Trim();
                        //html = "<root>" + html + "</root>";
                        html = html.Replace("<span class=\"leagueName xxxxxxxxxxxx\">", "$************************$<span class=\"leagueName xxxxxxxxxxxx\">");
                        string[] htmlarr = html.Split(new string[] { "$************************$" }, StringSplitOptions.None);
                        foreach (var item in htmlarr)
                        {
                            if (string.IsNullOrEmpty(item))
                            {
                                continue;
                            }
                            string lnhtml = "<root>" + item + "</root>";
                            var doc = new System.Xml.XmlDocument();
                            doc.LoadXml(lnhtml);
                            string LeagueMatchName = doc.DocumentElement.SelectSingleNode("span[1]").InnerText.Trim();
                            if (string.IsNullOrEmpty(LeagueMatchName)
                                || LeagueMatchName.Contains("梦幻对垒")
                                || LeagueMatchName.Contains("角球")
                                || LeagueMatchName.Contains("特定15分钟")
                                || LeagueMatchName.Contains("测试")
                                || LeagueMatchName.Contains("主场/客场")
                                || LeagueMatchName.Contains("哪一队先开球")
                                || LeagueMatchName.Contains("半场结束前受伤延长补时")
                                || LeagueMatchName.Contains("总入球分钟")
                                )
                            {
                                continue;
                            }
                            //联赛
                            A_LeagueMatch objlm = this.Temp_A_LeagueMatch.Where(x => x != null && x.Name == LeagueMatchName).FirstOrDefault();
                            if (objlm == null)
                            {
                                objlm = new A_LeagueMatch()
                                {
                                    ID = Guid.NewGuid().ToString("N"),
                                    Name = LeagueMatchName,
                                    ModifyTime = now,
                                    SourcePlatform = SourcePlatformEnum.SB.ToString(),
                                    SportsType = SportsTypeEnum.Football.ToString()
                                };
                                msg = lmbll.Create(objlm);
                                if (string.IsNullOrEmpty(msg))
                                {
                                    this.Temp_A_LeagueMatch.Add(objlm);
                                }
                                else
                                {
                                    this.Temp_A_LeagueMatch = lmbll.FindAll(SourcePlatformEnum.SB.ToString());
                                }
                            }
                            foreach (System.Xml.XmlNode tableRow in doc.DocumentElement.SelectNodes("div"))
                            {
                                if (!tableRow.Attributes["class"].Value.Contains("tableRow"))
                                {
                                    continue;
                                }
                                string status = tableRow.ChildNodes[5].FirstChild.InnerText.Trim();
                                if (status == "进行中" || status == "等待中")
                                {
                                    continue;
                                }
                                //DateTime starttime = DateTime.ParseExact(tableRow.FirstChild.InnerText.Trim(), "MM/dd/yyyy hh:mm tt", System.Globalization.CultureInfo.CurrentCulture);
                                DateTime starttime = Convert.ToDateTime(tableRow.FirstChild.InnerText.Trim());//MM/dd/yyyy hh:mm tt
                                var namedivs = tableRow.ChildNodes[1].SelectNodes("div[contains(@class,'name')]");
                                string htname = namedivs[0].InnerText.Trim();
                                string vtname = namedivs[1].InnerText.Trim();
                                if (string.IsNullOrEmpty(htname) || string.IsNullOrEmpty(vtname))
                                {
                                    continue;
                                }
                                if (htname.Contains("加时") || htname.Contains("点球") || vtname.Contains("加时") || vtname.Contains("点球"))
                                {
                                    continue;
                                }
                                //主队
                                A_Team objtH = this.Temp_A_Team.Where(x => x != null && x.LeagueMatchID == objlm.ID && x.Name == htname).FirstOrDefault();
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
                                        SportsType = SportsTypeEnum.Football.ToString()
                                    };
                                    msg1 = tbll.Create(objtH);
                                    if (string.IsNullOrEmpty(msg1))
                                    {
                                        this.Temp_A_Team.Add(objtH);
                                    }
                                    else
                                    {
                                        this.Temp_A_Team = tbll.FindAll(SourcePlatformEnum.SB.ToString());
                                    }
                                }
                                //客队
                                A_Team objtV = this.Temp_A_Team.Where(x => x != null && x.LeagueMatchID == objlm.ID && x.Name == vtname).FirstOrDefault();
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
                                        SportsType = SportsTypeEnum.Football.ToString()
                                    };
                                    msg2 = tbll.Create(objtV);
                                    if (string.IsNullOrEmpty(msg2))
                                    {
                                        this.Temp_A_Team.Add(objtV);
                                    }
                                    else
                                    {
                                        this.Temp_A_Team = tbll.FindAll(SourcePlatformEnum.SB.ToString());
                                    }
                                }
                                if (string.IsNullOrEmpty(msg1) && string.IsNullOrEmpty(msg2))
                                {
                                    A_MatchResult objmr = this.Temp_A_MatchResult.Where(x => x != null && x.LeagueMatchID == objlm.ID && x.HomeTeamID == objtH.ID && x.VisitingTeamID == objtV.ID && x.GameStartTime == starttime).FirstOrDefault();
                                    if (objmr == null)
                                    {
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

                                        string jqsx = string.Empty;
                                        if (tableRow.ChildNodes[4].ChildNodes.Count != 0)
                                        {
                                            string id = tableRow.ChildNodes[4].FirstChild.Attributes["onclick"].Value;
                                            id = id.Replace("OpenSoccerDetail(this,", "").Replace(");", "").Trim(); //OpenSoccerDetail(this, 27256330);
                                            var expandArea = doc.DocumentElement.SelectSingleNode("//span[@id='Detail_" + id + "']");
                                            if (expandArea != null)
                                            {
                                                //if (expandArea.LastChild.Attributes["class"].Value.Contains("col1"))
                                                //{
                                                //    jqsx = expandArea.LastChild.LastChild.FirstChild.FirstChild.InnerText.Trim();
                                                //}
                                                foreach (System.Xml.XmlNode col1 in expandArea.SelectNodes("div[contains(@class,'col1')]"))
                                                {
                                                    if (col1.FirstChild.FirstChild.InnerText.Trim() == "进球顺序")
                                                    {
                                                        jqsx = col1.LastChild.FirstChild.FirstChild.InnerText.Trim();
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                        objmr = new A_MatchResult()
                                        {
                                            ID = Guid.NewGuid().ToString("N"),
                                            LeagueMatchID = objlm.ID,
                                            SportsType = SportsTypeEnum.Football.ToString(),
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
                                        msg = mrbll.Create(objmr);
                                        if (string.IsNullOrEmpty(msg))
                                        {
                                            this.Temp_A_MatchResult.Add(objmr);
                                        }
                                        else
                                        {
                                            this.Temp_A_MatchResult = mrbll.FindByDate(SourcePlatformEnum.SB.ToString(), now.Date.AddDays(-1));
                                        }
                                    }
                                }

                            }
                        }
                        #endregion
                        break;
                }
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            lock (lockobj)
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
            IWebDriver driver = WebDriverHelper.CreateChromeDriver();
            if (driver == null)
            {
                MessageBox.Show(WebDriverHelper.ErrorMessage);
                return;
            }
            driver.Navigate().GoToUrl(_SourcePlatform.UMurl);
            driver.SwitchTo().Frame("mainframe");
            var div_cs_noInfo = driver.FindElement(By.CssSelector(".bgcpe #div_cs_noInfo"));
            string title = div_cs_noInfo.FindElement(By.CssSelector("h1")).GetAttribute("innerText");
            string info = div_cs_noInfo.FindElement(By.CssSelector(".UdrDogTeamClass h3")).GetAttribute("innerHTML").Trim();
            info = info.Substring(0, info.IndexOf("<br>"));
            string time = div_cs_noInfo.FindElement(By.CssSelector(".FavOddsClass")).GetAttribute("innerText").Trim();
            if (logwin == null)
            {
                MessageBox.Show(title + "\r\n" + info + "\r\n预计恢复时间：" + time + "\r\n");
            }
            else
            {
                logwin.txt_log.AppendText(title + "\r\n" + info + "\r\n预计恢复时间：" + time + "\r\n");
                Utility.AppendWrite(_LogPath + logwin.Text + ".txt", title + "\r\n" + info + "\r\n预计恢复时间：" + time + "\r\n");
            }
            driver.Quit();
        }
    }
}
