using BLL;
using Common;
using Model;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Control_SB
{
    public partial class MainOld : Form
    {
        private string _URL = "https://mkt.sss988n1jssx92.info/NewIndex?webskintype=2&lang=cs";
        //立即博  https://mkt.ljb04.com/NewIndex?webskintype=2&lang=cs
        //太阳城  https://mkt.sss988n1jssx92.info/NewIndex?webskintype=2&lang=cs
        //明升  https://mkt.m88ms.com/mansion88.aspx?webskintype=2&lang=cs
        //乐天堂  https://sports.fun88asia.com/NewIndex?webskintype=2&lang=cs

        object lockobj = new object();

        private DateTime _runstarttime = DateTime.Now;
        private SourcePlatform _SourcePlatform = null;

        private bool _IsRunning = false;//是否正在运行
        private List<IWebDriver> _WebDriverList = new List<IWebDriver>();
        private List<Log> _LogWinList = new List<Log>();

        private bool _ZP_AllDay = false;//早盘，所有日期

        private int _ZP_MS = 1000;//早盘抓取间隔，毫秒
        private int _JR_MS = 1000;//今日赛事抓取间隔，毫秒
        private int _GQ_MS = 1000;//滚球抓取间隔，毫秒

        private int _WebDriver_MS = 50;//判断元素时需等待的时间，毫秒

        private List<Cookie> _CookieList = new List<Cookie>();
        private List<Cookie> _ResultCookieList = new List<Cookie>();

        string[,,] menuarr = new string[,,] 
        {
            {{"早盘","今日赛事","滚球",""}},
            {{"足球","","",""}},
            {{"独赢 & 让球 & 大小 & 单/双","半场 / 全场","波胆","冠军"}},
        };

        private List<A_LeagueMatch> Temp_A_LeagueMatch = new List<A_LeagueMatch>();
        private List<A_Team> Temp_A_Team = new List<A_Team>();
        private List<A_Match> Temp_A_Match = new List<A_Match>();
        private List<A_MatchScoreRecord> Temp_A_MatchScoreRecord = new List<A_MatchScoreRecord>();
        private List<A_MatchResult> Temp_A_MatchResult = new List<A_MatchResult>();

        private List<B_SingleOdds_Last> Temp_B_SingleOdds_Last = new List<B_SingleOdds_Last>();
        private List<B_HandicapOdds_Last> Temp_B_HandicapOdds_Last = new List<B_HandicapOdds_Last>();
        private List<B_TotalOverUnderOdds_Last> Temp_B_TotalOverUnderOdds_Last = new List<B_TotalOverUnderOdds_Last>();
        private List<B_TotalSingleDoubleOdds_Last> Temp_B_TotalSingleDoubleOdds_Last = new List<B_TotalSingleDoubleOdds_Last>();
        private List<B_DoubleResultOdds_Last> Temp_B_DoubleResultOdds_Last = new List<B_DoubleResultOdds_Last>();
        private List<B_CorrectScoreOdds_Last> Temp_B_CorrectScoreOdds_Last = new List<B_CorrectScoreOdds_Last>();
        private List<B_OutrightOdds_Last> Temp_B_OutrightOdds_Last = new List<B_OutrightOdds_Last>();

        private void InitTempData()
        {
            try
            {
                A_LeagueMatchBll lmbll = new A_LeagueMatchBll();

                DateTime now = lmbll.GetServerDateTime();
                                
                B_SingleOdds_LastBll sobll = new B_SingleOdds_LastBll();
                B_HandicapOdds_LastBll hobll = new B_HandicapOdds_LastBll();
                B_TotalOverUnderOdds_LastBll touobll = new B_TotalOverUnderOdds_LastBll();
                B_TotalSingleDoubleOdds_LastBll tsdobll = new B_TotalSingleDoubleOdds_LastBll();
                B_DoubleResultOdds_LastBll drobll = new B_DoubleResultOdds_LastBll();
                B_CorrectScoreOdds_LastBll csobll = new B_CorrectScoreOdds_LastBll();
                B_OutrightOdds_LastBll oobll = new B_OutrightOdds_LastBll();

                this.Temp_B_SingleOdds_Last = sobll.FindByDate(now.Date);
                this.Temp_B_HandicapOdds_Last = hobll.FindByDate(now.Date);
                this.Temp_B_TotalOverUnderOdds_Last = touobll.FindByDate(now.Date);
                this.Temp_B_TotalSingleDoubleOdds_Last = tsdobll.FindByDate(now.Date);
                this.Temp_B_DoubleResultOdds_Last = drobll.FindByDate(now.Date);
                this.Temp_B_CorrectScoreOdds_Last = csobll.FindByDate(now.Date);
                this.Temp_B_OutrightOdds_Last = oobll.FindByDate(now.Date);

                A_TeamBll tbll = new A_TeamBll();
                A_MatchBll mbll = new A_MatchBll();
                A_MatchScoreRecordBll msrbll = new A_MatchScoreRecordBll();
                A_MatchResultBll mrbll = new A_MatchResultBll();

                this.Temp_A_Match = mbll.FindByDate(SourcePlatformEnum.SB.ToString(),"", now.Date);
                this.Temp_A_MatchScoreRecord = msrbll.FindByDate(SourcePlatformEnum.SB.ToString(), "", now.Date);
                this.Temp_A_MatchResult = mrbll.FindByDate(SourcePlatformEnum.SB.ToString(), "", now.Date);
                this.Temp_A_LeagueMatch = lmbll.FindAll(SourcePlatformEnum.SB.ToString(), "");
                this.Temp_A_Team = tbll.FindAll(SourcePlatformEnum.SB.ToString(), "");
            }
            catch (Exception ex)
            {
                //InitTempData();
            }
        }
        public MainOld()
        {
            InitializeComponent();
            this.timer2.Interval = 1000 * 60 * 10;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key1">1：早盘，2：滚球</param>
        /// <param name="key2">1：足球</param>
        /// <param name="key3">足球{1：独赢 & 让球 & 大小 & 单/双，2：半场 / 全场，3：波胆，4：冠军}</param>
        private void GetData(string key1, string key2, string key3, Log logwin)
        {
            bool islogin = false;
            bool isexception = false;
            int awaitms = 10000;
            IWebDriver driver = WebDriverHelper.CreateChromeDriver(chk_VisibleChrome.Checked);//new ChromeDriver();
            if (driver == null)
            {
                MessageBox.Show(WebDriverHelper.ErrorMessage);
                logwin.txt_log.AppendText(WebDriverHelper.ErrorMessage + "\r\n");
                return;
            }
            _WebDriverList.Add(driver);
            this.Invoke(new MethodInvoker(delegate
            {
                _IsRunning = true;
                islogin = this.chk_Logined.Checked;
                logwin.txt_log.AppendText("地址：" + _SourcePlatform.url + "\r\n");
                logwin.txt_log.AppendText("\r\n");
                logwin.txt_log.AppendText("开始初次请求：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n");
                logwin.txt_log.AppendText("===============================================\r\n");
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
            //if (_SourcePlatform.key=="TYC")
            //{
            //    bool iswh = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#sportsFrame"), _WebDriver_MS);
            //    if (!iswh)
            //    {
            //        this.Invoke(new MethodInvoker(delegate
            //        {
            //            logwin.txt_log.AppendText("异常，可能在维护。\r\n");
            //            this.btn_Stop.PerformClick();
            //        }));
            //        return;
            //    }
            //    //driver.SwitchTo().Frame("sportsFrame");
            //}
            //else if (_SourcePlatform.key == "MS")
            //{
            //    bool iswh = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#sbContainer"), _WebDriver_MS);
            //    if (!iswh)
            //    {
            //        this.Invoke(new MethodInvoker(delegate
            //        {
            //            logwin.txt_log.AppendText("异常，可能在维护。\r\n");
            //            this.btn_Stop.PerformClick();
            //        }));
            //        return;
            //    }
            //}

            int count = 1;
            try
            {
                //while (flag_loading)
                //{

                //    Thread.Sleep(_NODATA_MS);
                //}
                choosemenu(key1, key2, key3, logwin, driver, ref awaitms);
                this.Invoke(new MethodInvoker(delegate
                {
                    logwin.txt_log.AppendText("===============================================\r\n");
                }));
                //bool flag09 = false;
                //do
                //{
                //    flag09 = WebDriverHelper.WaitForElementInvisible(driver, By.CssSelector("#container .loading"), _WebDriver_MS);
                //} while (!flag09);
                checkloading(key1, key2, key3, logwin, driver, ref count, ref awaitms);
                this.Invoke(new MethodInvoker(delegate
                {
                    logwin.txt_log.AppendText("loading结束\r\n");
                }));
                //判断是否有数据
                bool flag1 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#container .message-box"), _WebDriver_MS);
                bool flag2 = WebDriverHelper.WaitForElementInvisible(driver, By.CssSelector("#container .pagination"), _WebDriver_MS);
                if (flag1 && flag2)
                {
                    this.Invoke(new MethodInvoker(delegate
                    {
                        //无数据，点击导航栏刷新
                        logwin.txt_log.AppendText("无赛事\r\n");
                        logwin.txt_log.AppendText("===============================================\r\n");
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
                        }));
                    }
                    this.Invoke(new MethodInvoker(delegate
                    {
                        logwin.txt_log.AppendText("数据抓取完毕\r\n");
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
                            }));
                        }
                        else
                        {
                            if (_IsRunning)
                            {
                                int updatecount = 0;
                                string msgpush = string.Empty;
                                Transformation(lmList,ref updatecount, ref msgpush);
                                this.Invoke(new MethodInvoker(delegate
                                {
                                    if (!string.IsNullOrEmpty(msgpush))
                                    {
                                        logwin.txt_log.AppendText("错误：\r\n" + msgpush);
                                    }
                                    logwin.txt_log.AppendText("更新成功：" + updatecount + "\r\n");
                                }));
                            }
                        }
                    }                    
                    watch.Stop();
                    this.Invoke(new MethodInvoker(delegate
                    {
                        logwin.txt_log.AppendText("===============================================\r\n");
                        logwin.txt_log.AppendText("耗时：" + watch.Elapsed.TotalSeconds + "秒\r\n");
                    }));
                }                
            }
            catch (Exception ex)
            {
                isexception = true;
                driver.Quit();
                this.Invoke(new MethodInvoker(delegate
                {
                    logwin.txt_log.AppendText("异常：" + ex.ToString() + "\r\n");
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
                        if (isexception)
                        {
                            flag_loading = true;
                            driver.SwitchTo().DefaultContent();
                            driver.Navigate().GoToUrl(_SourcePlatform.url);
                            choosemenu(key1, key2, key3, logwin, driver, ref awaitms);
                            checkloading(key1, key2, key3, logwin, driver,ref count, ref awaitms);
                        }
                        if (!flag_loading)
                        {
                            //先切回第1页
                            driver.FindElement(By.CssSelector("#container .pagination .dropdown")).Click();
                            WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#container .pagination .dropdown>ul"), _WebDriver_MS);
                            driver.FindElement(By.CssSelector("#container .pagination .dropdown>ul>li:nth-child(1)")).Click();
                        }                        
                        this.Invoke(new MethodInvoker(delegate
                        {
                            logwin.txt_log.AppendText("\r\n开始第" + count + "次请求：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n");
                            logwin.txt_log.AppendText("===============================================\r\n");
                        }));
                        watch.Restart();
                        //通过Selenium驱动点击页面的刷新按钮
                        driver.FindElement(By.CssSelector("#container .btn-toolbar .icon-refresh")).Click();
                        await Task.Delay(200);
                        //判断ajax刷新是否完成                        
                        //bool flag10 = true;
                        //do
                        //{
                        //    flag10 = WebDriverHelper.WaitForElementHasClass(driver, By.CssSelector("#container .btn-toolbar>li>a.btn"), "disable", _WebDriver_MS);
                        //} while (flag10);
                        checkloading(key1, key2, key3, logwin, driver, ref count, ref awaitms);
                        this.Invoke(new MethodInvoker(delegate
                        {
                            logwin.txt_log.AppendText("loading结束\r\n");
                        }));
                        //判断是否有数据
                        bool flag1 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#container .message-box"), _WebDriver_MS);
                        //this.Invoke(new MethodInvoker(delegate
                        //{
                        //    logwin.txt_log.AppendText("无赛事提示文字："+ flag1 + "\r\n");
                        //}));
                        bool flag2 = WebDriverHelper.WaitForElementInvisible(driver, By.CssSelector("#container .pagination"), _WebDriver_MS);
                        //this.Invoke(new MethodInvoker(delegate
                        //{
                        //    logwin.txt_log.AppendText("页码：" + flag2 + "\r\n");
                        //}));
                        if (flag1 && flag2)
                        {
                            this.Invoke(new MethodInvoker(delegate
                            {
                                //无数据，点击导航栏刷新
                                logwin.txt_log.AppendText("无赛事\r\n");
                                logwin.txt_log.AppendText("===============================================\r\n");
                            }));
                            continue;
                        }
                        //第一页数据
                        html = driver.FindElement(By.CssSelector("#container .match-container")).GetAttribute("innerHTML");
                        this.Invoke(new MethodInvoker(delegate
                        {
                            logwin.txt_log.AppendText("第1页数据完\r\n");
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
                            }));
                        }
                        this.Invoke(new MethodInvoker(delegate
                        {
                            logwin.txt_log.AppendText("数据抓取完毕\r\n");
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
                                }));
                                flag_loading = true;
                                driver.SwitchTo().DefaultContent();
                                //if (islogin)
                                //{
                                //    driver.Manage().Cookies.DeleteAllCookies();
                                //    foreach (var item in _CookieList)
                                //    {
                                //        driver.Manage().Cookies.AddCookie(item);
                                //    }
                                //}
                                //driver.Navigate().GoToUrl(_SourcePlatform.url);
                                driver.Navigate().Refresh();
                                choosemenu(key1, key2, key3, logwin, driver, ref awaitms);
                                checkloading(key1, key2, key3, logwin, driver,ref count, ref awaitms);
                                continue;
                            }
                            else
                            {
                                if (_IsRunning)
                                {
                                    int updatecount = 0;
                                    string msgpush = string.Empty;
                                    Transformation(lmList, ref updatecount, ref msgpush);
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        if (!string.IsNullOrEmpty(msgpush))
                                        {
                                            logwin.txt_log.AppendText("错误：\r\n" + msgpush);
                                        }                                        
                                        logwin.txt_log.AppendText("更新成功：" + updatecount + "\r\n");
                                    }));
                                }
                            }
                        }
                        watch.Stop();
                        this.Invoke(new MethodInvoker(delegate
                        {
                            logwin.txt_log.AppendText("===============================================\r\n");
                            logwin.txt_log.AppendText("第" + count + "次耗时：" + watch.Elapsed.TotalSeconds + "秒\r\n");
                        }));
                        isexception = false;
                    }
                    catch (Exception ex)
                    {
                        isexception = true;
                        this.Invoke(new MethodInvoker(delegate
                        {
                            logwin.txt_log.AppendText("===============================================\r\n");
                            logwin.txt_log.AppendText("第" + count + "次异常：" + ex.Message + "\r\n");
                        }));
                        continue;
                    }
                }
            });
        }
        private void choosemenu(string key1, string key2, string key3, Log logwin, IWebDriver driver,ref int awaitms)
        {
            if (_SourcePlatform.key == "TYC")
            {
                bool iswh = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#sportsFrame"), _WebDriver_MS);
                if (!iswh)
                {
                    this.Invoke(new MethodInvoker(delegate
                    {
                        logwin.txt_log.AppendText("异常，可能在维护。\r\n");
                        //this.btn_Stop.PerformClick();
                    }));
                    return;
                }
                //driver.SwitchTo().Frame("sportsFrame");
            }
            else if (_SourcePlatform.key == "MS")
            {
                bool iswh = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#sbContainer"), _WebDriver_MS);
                if (!iswh)
                {
                    this.Invoke(new MethodInvoker(delegate
                    {
                        logwin.txt_log.AppendText("异常，可能在维护。\r\n");
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
                case "1":
                    awaitms = _ZP_MS;
                    //进入早盘
                    #region 进入早盘
                    this.Invoke(new MethodInvoker(delegate
                    {
                        logwin.txt_log.AppendText("进入早盘\r\n");
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
                        }));
                        driver.FindElement(By.CssSelector("#container .filter-date>ul>li:last-child>a")).Click();
                    }
                    switch (key2)
                    {
                        case "1":
                            //选择足球
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("选择足球\r\n");
                            }));
                            driver.FindElement(By.CssSelector("#category .category-sports>ul>li:nth-child(1)")).Click();
                            bool flag02 = false;
                            do
                            {
                                flag02 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#category .category-bettype"), _WebDriver_MS);
                            } while (!flag02);
                            switch (key3)
                            {
                                case "1":
                                    //独赢 & 让球 & 大小 & 单/双
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择独赢 & 让球 & 大小 & 单/双\r\n");
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(1)")).Click();
                                    break;
                                case "2":
                                    //半场 / 全场
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择半场 / 全场\r\n");
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(4)")).Click();
                                    break;
                                case "3":
                                    //波胆
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择波胆\r\n");
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(2)")).Click();
                                    break;
                                case "4":
                                    //冠军
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择冠军\r\n");
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(6)")).Click();
                                    //选择联赛
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择联赛\r\n");
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
                case "2":
                    awaitms = _JR_MS;
                    //进入今日赛事
                    #region 进入今日赛事
                    this.Invoke(new MethodInvoker(delegate
                    {
                        logwin.txt_log.AppendText("进入今日赛事\r\n");
                    }));
                    driver.FindElement(By.CssSelector("#sb-header>.header-tab>ul>li:nth-child(2)")).Click();
                    bool flag05 = false;
                    do
                    {
                        flag05 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#category .category-sports"), _WebDriver_MS);
                    } while (!flag05);
                    switch (key2)
                    {
                        case "1":
                            //选择足球
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("选择足球\r\n");
                            }));
                            driver.FindElement(By.CssSelector("#category .category-sports>ul>li:nth-child(1)")).Click();
                            bool flag06 = false;
                            do
                            {
                                flag06 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#category .category-bettype"), _WebDriver_MS);
                            } while (!flag06);
                            switch (key3)
                            {
                                case "1":
                                    //独赢 & 让球 & 大小 & 单/双
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择独赢 & 让球 & 大小 & 单/双\r\n");
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(1)")).Click();
                                    break;
                                case "2":
                                    //半场 / 全场
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择半场 / 全场\r\n");
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(4)")).Click();
                                    break;
                                case "3":
                                    //波胆
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择波胆\r\n");
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(2)")).Click();
                                    break;
                            }
                            break;
                    }
                    #endregion
                    break;
                case "3":
                    awaitms = _GQ_MS;
                    //进入滚球
                    #region 进入滚球
                    this.Invoke(new MethodInvoker(delegate
                    {
                        logwin.txt_log.AppendText("进入滚球\r\n");
                    }));
                    driver.FindElement(By.CssSelector("#sb-header>.header-tab>ul>li:nth-child(1)")).Click();
                    bool flag07 = false;
                    do
                    {
                        flag07 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#category .category-sports"), _WebDriver_MS);
                    } while (!flag07);
                    switch (key2)
                    {
                        case "1":
                            //选择足球
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("选择足球\r\n");
                            }));
                            driver.FindElement(By.CssSelector("#category .category-sports>ul>li:nth-child(1)")).Click();
                            bool flag08 = false;
                            do
                            {
                                flag08 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#category .category-bettype"), _WebDriver_MS);
                            } while (!flag08);
                            switch (key3)
                            {
                                case "1":
                                    //独赢 & 让球 & 大小 & 单/双
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择独赢 & 让球 & 大小 & 单/双\r\n");
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(1)")).Click();
                                    break;
                                case "3":
                                    //波胆
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        logwin.txt_log.AppendText("选择波胆\r\n");
                                    }));
                                    driver.FindElement(By.CssSelector("#category .category-bettype>ul>li:nth-child(2)")).Click();
                                    break;
                            }
                            break;
                    }
                    #endregion
                    break;
            }
        }
        private void checkloading(string key1, string key2, string key3, Log logwin, IWebDriver driver,ref int count, ref int awaitms)
        {            
            int loadingcount = 0;
            bool flag09 = false;
            while (!flag09)
            {
                flag09 = WebDriverHelper.WaitForElementInvisible(driver, By.CssSelector("#container .loading"), _WebDriver_MS);
                this.Invoke(new MethodInvoker(delegate
                {
                    logwin.txt_log.AppendText("等待loading："+ flag09 + "\r\n");
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
                    }));
                    //if (islogin)
                    //{
                    //    driver.Manage().Cookies.DeleteAllCookies();
                    //    foreach (var item in _CookieList)
                    //    {
                    //        driver.Manage().Cookies.AddCookie(item);
                    //    }                        
                    //}
                    //driver.Navigate().GoToUrl(_SourcePlatform.url);
                    driver.Navigate().Refresh();
                    choosemenu(key1, key2, key3, logwin, driver, ref awaitms);
                    checkloading(key1, key2, key3, logwin, driver, ref count, ref awaitms);
                }
                if (count % 100 == 0)
                {
                    int c = count;
                    count++;
                    this.Invoke(new MethodInvoker(delegate
                    {
                        islogin = this.chk_Logined.Checked;
                        logwin.txt_log.AppendText("第" + c + "次，页面重刷\r\n");
                    }));
                    //if (islogin)
                    //{
                    //    driver.Manage().Cookies.DeleteAllCookies();
                    //    foreach (var item in _CookieList)
                    //    {
                    //        driver.Manage().Cookies.AddCookie(item);
                    //    }
                    //}
                    //driver.Navigate().GoToUrl(_SourcePlatform.url);
                    driver.Navigate().Refresh();
                    choosemenu(key1, key2, key3, logwin, driver, ref awaitms);
                    checkloading(key1, key2, key3, logwin, driver, ref count, ref awaitms);
                }
            }
        }
        private List<LeagueMatch> AnalysisHtml(string html, string key1, string key2, string key3)
        {
            List<LeagueMatch> lmList = new List<LeagueMatch>();
            if (html.Contains("$************************$"))
            {
                string[] htmlarr = html.Split(new string[] { "$************************$" }, StringSplitOptions.None);
                foreach (var item in htmlarr)
                {
                    switch (key1)
                    {
                        case "1"://早盘
                            #region 早盘
                            switch (key2)
                            {
                                case "1"://足球
                                    switch (key3)
                                    {
                                        case "1":
                                            //独赢 & 让球 & 大小 & 单/双
                                            lmList.AddRange(AnalysisHtmlHelper.SB_Football_ZP_DRDD_List(item));
                                            break;
                                        case "2":
                                            //半场 / 全场
                                            lmList.AddRange(AnalysisHtmlHelper.SB_Football_ZP_BCQC_List(item));
                                            break;
                                        case "3":
                                            //波胆
                                            lmList.AddRange(AnalysisHtmlHelper.SB_Football_ZP_BD_List(item));
                                            break;
                                        case "4":
                                            //冠军
                                            lmList.AddRange(AnalysisHtmlHelper.SB_Football_ZP_GJ_List(item));
                                            break;
                                    }
                                    break;
                            } 
                            #endregion
                            break;
                        case "2"://今日赛事
                            #region 今日赛事
                            switch (key2)
                            {
                                case "1"://足球
                                    switch (key3)
                                    {
                                        case "1":
                                            //独赢 & 让球 & 大小 & 单/双
                                            lmList.AddRange(AnalysisHtmlHelper.SB_Football_ZP_DRDD_List(item));
                                            break;
                                        case "2":
                                            //半场 / 全场
                                            lmList.AddRange(AnalysisHtmlHelper.SB_Football_ZP_BCQC_List(item));
                                            break;
                                        case "3":
                                            //波胆
                                            lmList.AddRange(AnalysisHtmlHelper.SB_Football_ZP_BD_List(item));
                                            break;
                                    }
                                    break;
                            }
                            #endregion
                            break;
                        case "3"://滚球
                            #region 滚球
                            switch (key2)
                            {
                                case "1"://足球
                                    switch (key3)
                                    {
                                        case "1":
                                            //独赢 & 让球 & 大小 & 单/双
                                            lmList.AddRange(AnalysisHtmlHelper.SB_Football_GQ_DRDD_List(item));
                                            break;
                                        case "3":
                                            //波胆
                                            lmList.AddRange(AnalysisHtmlHelper.SB_Football_GQ_BD_List(item));
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
                    case "1"://早盘
                        #region 早盘
                        switch (key2)
                        {
                            case "1"://足球
                                switch (key3)
                                {
                                    case "1":
                                        //独赢 & 让球 & 大小 & 单/双
                                        lmList.AddRange(AnalysisHtmlHelper.SB_Football_ZP_DRDD_List(html));
                                        break;
                                    case "2":
                                        //半场 / 全场
                                        lmList.AddRange(AnalysisHtmlHelper.SB_Football_ZP_BCQC_List(html));
                                        break;
                                    case "3":
                                        //波胆
                                        lmList.AddRange(AnalysisHtmlHelper.SB_Football_ZP_BD_List(html));
                                        break;
                                    case "4":
                                        //冠军
                                        lmList.AddRange(AnalysisHtmlHelper.SB_Football_ZP_GJ_List(html));
                                        break;
                                }
                                break;
                        } 
                        #endregion
                        break;
                    case "2"://今日赛事
                        #region 今日赛事
                        switch (key2)
                        {
                            case "1"://足球
                                switch (key3)
                                {
                                    case "1":
                                        //独赢 & 让球 & 大小 & 单/双
                                        lmList.AddRange(AnalysisHtmlHelper.SB_Football_ZP_DRDD_List(html));
                                        break;
                                    case "2":
                                        //半场 / 全场
                                        lmList.AddRange(AnalysisHtmlHelper.SB_Football_ZP_BCQC_List(html));
                                        break;
                                    case "3":
                                        //波胆
                                        lmList.AddRange(AnalysisHtmlHelper.SB_Football_ZP_BD_List(html));
                                        break;
                                }
                                break;
                        }
                        #endregion
                        break;
                    case "3"://滚球
                        #region 滚球
                        switch (key2)
                        {
                            case "1"://足球
                                switch (key3)
                                {
                                    case "1":
                                        //独赢 & 让球 & 大小 & 单/双
                                        lmList.AddRange(AnalysisHtmlHelper.SB_Football_GQ_DRDD_List(html));
                                        break;
                                    case "3":
                                        //波胆
                                        lmList.AddRange(AnalysisHtmlHelper.SB_Football_GQ_BD_List(html));
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
        private void Transformation(List<LeagueMatch> lmList, ref int updatecount, ref string msgpush)
        {
            A_LeagueMatchBll lmbll = new A_LeagueMatchBll();
            A_TeamBll tbll = new A_TeamBll();
            A_MatchBll mbll = new A_MatchBll();
            A_MatchScoreRecordBll msrbll = new A_MatchScoreRecordBll();
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
                    A_LeagueMatch objlm = this.Temp_A_LeagueMatch.Where(x => x.Name == lm.Name).FirstOrDefault();
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
                            this.Temp_A_LeagueMatch = lmbll.FindAll(SourcePlatformEnum.SB.ToString(),"");
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
                                this.Temp_A_LeagueMatch = lmbll.FindAll(SourcePlatformEnum.SB.ToString(),"");
                            }
                        }
                    }
                    if (string.IsNullOrEmpty(msg))
                    {
                        #region 冠军
                        foreach (Team t in lm.TeamList)
                        {
                            if (string.IsNullOrEmpty(t.Name.Trim()))
                            {
                                continue;
                            }
                            //A_Team objt = tbll.GetByLMIDName(objlm.ID, t.Name.Trim());
                            A_Team objt = this.Temp_A_Team.Where(x => x.LeagueMatchID == objlm.ID && x.Name == t.Name.Trim()).FirstOrDefault();
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
                                    this.Temp_A_Team = tbll.FindAll(SourcePlatformEnum.SB.ToString(),"");
                                }
                            }
                            if (string.IsNullOrEmpty(msg))
                            {
                                B_OutrightOddsBll oobll = new B_OutrightOddsBll();
                                B_OutrightOdds_LastBll oo_lastbll = new B_OutrightOdds_LastBll();
                                B_OutrightOdds objoo = new B_OutrightOdds()
                                {
                                    ID = Guid.NewGuid().ToString("N"),
                                    LeagueMatchID = objlm.ID,
                                    TeamID = objt.ID,
                                    Odds = Utility.ObjConvertToDecimal(t.OutrightOdds),
                                    CreateTime = now
                                };
                                //var lastoo = oo_lastbll.GetByTID(objt.ID);
                                update_gj(objoo, now, oobll, oo_lastbll, ref updatecount, ref msgpush);
                            }
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
                    A_LeagueMatch objlm = this.Temp_A_LeagueMatch.Where(x => x.Name == lm.Name).FirstOrDefault();
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
                            this.Temp_A_LeagueMatch = lmbll.FindAll(SourcePlatformEnum.SB.ToString(),"");
                        }
                    }
                    if (string.IsNullOrEmpty(msg))
                    {
                        if (lm.MatchList.Count > 0)//独赢 & 让球 & 大小 & 单/双 & 半场 / 全场 & 波胆
                        {
                            #region 独赢 & 让球 & 大小 & 单/双 & 半场 / 全场 & 波胆
                            foreach (Match m in lm.MatchList)
                            {
                                if (string.IsNullOrEmpty(m.HomeTeam.Trim()) || string.IsNullOrEmpty(m.VisitingTeam.Trim()))
                                {
                                    continue;
                                }
                                //A_Team objtH = tbll.GetByLMIDName(objlm.ID, m.HomeTeam.Trim());
                                A_Team objtH = this.Temp_A_Team.Where(x => x.LeagueMatchID == objlm.ID && x.Name == m.HomeTeam.Trim()).FirstOrDefault();
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
                                        this.Temp_A_Team = tbll.FindAll(SourcePlatformEnum.SB.ToString(),"");
                                    }
                                }
                                //A_Team objtV = tbll.GetByLMIDName(objlm.ID, m.VisitingTeam.Trim());
                                A_Team objtV = this.Temp_A_Team.Where(x => x.LeagueMatchID == objlm.ID && x.Name == m.VisitingTeam.Trim()).FirstOrDefault();
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
                                        this.Temp_A_Team = tbll.FindAll(SourcePlatformEnum.SB.ToString(),"");
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
                                            List<string> _NoLockMID = new List<string>();
                                            A_Match objm = this.Temp_A_Match.Where(x => x.HomeTeamID == objtH.ID && x.VisitingTeamID == objtV.ID && x.SP_GameStartTime.Value.Date== bsdate).FirstOrDefault();
                                            int ts = -1;
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
                                                //    objm.MatchType = MatchTypeEnum.Firsthalf.ToString();
                                                //}
                                                //else if (m.halftype == "2H")
                                                //{
                                                //    objm.MatchType = MatchTypeEnum.Secondhalf.ToString();
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
                                                _NoLockMID.Add(objm.ID);
                                                var newm = (A_Match)Utility.DeepCopy(objm);
                                                newm.HomeTeamScore = m.HomeTeamScore;
                                                newm.VisitingTeamScore = m.VisitingTeamScore;
                                                newm.IsStart = ts < 0 ? "0" : "1";
                                                newm.Timing = ts * 60;
                                                newm.IsEnd = "0";
                                                newm.IsLock = "0";
                                                newm.ModifyTime = now;
                                                if (m.halftype == "1H")
                                                {
                                                    newm.MatchType = MatchTypeEnum.Firsthalf.ToString();
                                                }
                                                else if (m.halftype == "2H")
                                                {
                                                    newm.MatchType = MatchTypeEnum.Secondhalf.ToString();
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
                                                    this.Temp_A_Match = mbll.FindByDate(SourcePlatformEnum.SB.ToString(),"", now.Date.AddDays(-1));
                                                }
                                            }
                                            if (objm != null && string.IsNullOrEmpty(msg))
                                            {
                                                //锁定消失赛事   && x.ExistLive == "1"
                                                this.Temp_A_Match.Where(x => x.IsStart == "1" && x.IsLock == "0" && (!_NoLockMID.Contains(x.ID))).ToList().ForEach(x => 
                                                {
                                                    x.IsLock = "1";
                                                    x.ModifyTime = now;
                                                    mbll.Update(x);
                                                });

                                                #region 比分记录
                                                A_MatchScoreRecord objmsr = new A_MatchScoreRecord()
                                                {
                                                    ID = Guid.NewGuid().ToString("N"),
                                                    MatchID = objm.ID,
                                                    MatchType = objm.MatchType,
                                                    HomeTeamScore = m.HomeTeamScore,
                                                    VisitingTeamScore = m.VisitingTeamScore,
                                                    Timing = ts,
                                                    CreateTime = now
                                                };
                                                //A_MatchScoreRecord lastmsr = msrbll.GetByMID(objm.ID);
                                                A_MatchScoreRecord lastmsr = this.Temp_A_MatchScoreRecord.Where(x => x.MatchID == objm.ID).OrderByDescending(x => x.CreateTime).FirstOrDefault();
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
                                                            this.Temp_A_MatchScoreRecord = msrbll.FindByDate(SourcePlatformEnum.SB.ToString(),"", now.Date.AddDays(-1));
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
                                                        this.Temp_A_MatchScoreRecord = msrbll.FindByDate(SourcePlatformEnum.SB.ToString(),"", now.Date.AddDays(-1));
                                                    }
                                                }
                                                #endregion

                                                B_SingleOddsBll sobll = new B_SingleOddsBll();
                                                B_SingleOdds_LastBll so_lastbll = new B_SingleOdds_LastBll();
                                                B_HandicapOddsBll hobll = new B_HandicapOddsBll();
                                                B_HandicapOdds_LastBll ho_lastbll = new B_HandicapOdds_LastBll();
                                                B_TotalOverUnderOddsBll touobll = new B_TotalOverUnderOddsBll();
                                                B_TotalOverUnderOdds_LastBll touo_lastbll = new B_TotalOverUnderOdds_LastBll();
                                                B_TotalSingleDoubleOddsBll tsdobll = new B_TotalSingleDoubleOddsBll();
                                                B_TotalSingleDoubleOdds_LastBll tsdo_lastbll = new B_TotalSingleDoubleOdds_LastBll();
                                                #region 全场赔率
                                                if (m.FullCourtList.Count > 0)
                                                {
                                                    foreach (Odds item in m.FullCourtList)
                                                    {
                                                        #region 独赢
                                                        B_SingleOdds objso = new B_SingleOdds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            MatchID = objm.ID,
                                                            MatchType = MatchTypeEnum.Full.ToString(),
                                                            Odds_H = Utility.ObjConvertToDecimal(item.Odds_ZY),
                                                            Odds_V = Utility.ObjConvertToDecimal(item.Odds_KY),
                                                            Odds_Draw = Utility.ObjConvertToDecimal(item.Odds_HJ),
                                                            IsLive = "1",
                                                            IsLock = item.IsDisable_DY ?? "0",
                                                            CreateTime = now
                                                        };
                                                        //var lastso = so_lastbll.GetByMID(objm.ID, objso.MatchType, objso.IsLive);
                                                        update_dy(objso, now, sobll, so_lastbll, ref updatecount, ref msgpush);
                                                        #endregion

                                                        #region 让球
                                                        B_HandicapOdds objho = new B_HandicapOdds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            MatchID = objm.ID,
                                                            MatchType = MatchTypeEnum.Full.ToString(),
                                                            Odds_H = Utility.ObjConvertToDecimal(item.Odds_RQZY),
                                                            Odds_V = Utility.ObjConvertToDecimal(item.Odds_RQKY),
                                                            IsLive = "1",
                                                            IsLock = item.IsDisable_RQ ?? "0",
                                                            CreateTime = now
                                                        };
                                                        string rq = string.Empty;
                                                        if (!string.IsNullOrEmpty(item.Text_ZRKQ.Trim()))
                                                        {
                                                            objho.PointSpread = "1";
                                                            rq = item.Text_ZRKQ.Trim();
                                                        }
                                                        else if (!string.IsNullOrEmpty(item.Text_KRZQ.Trim()))
                                                        {
                                                            objho.PointSpread = "2";
                                                            rq = item.Text_KRZQ.Trim();
                                                        }
                                                        if (rq.Contains("/"))
                                                        {
                                                            string[] arr = rq.Split('/');
                                                            objho.PS_Number1 = Utility.ObjConvertToDecimal(arr[0].Trim());
                                                            objho.PS_Number2 = Utility.ObjConvertToDecimal(arr[1].Trim());
                                                        }
                                                        else
                                                        {
                                                            objho.PS_Number1 = Utility.ObjConvertToDecimal(rq);
                                                        }
                                                        //var lastho = ho_lastbll.GetByMID(objm.ID, objho.MatchType, objho.PointSpread, objho.PS_Number1, objho.PS_Number2, objho.IsLive);
                                                        update_rq(objho, now, hobll, ho_lastbll, ref updatecount, ref msgpush);
                                                        #endregion

                                                        #region 大小球
                                                        B_TotalOverUnderOdds objtouo = new B_TotalOverUnderOdds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            MatchID = objm.ID,
                                                            MatchType = MatchTypeEnum.Full.ToString(),
                                                            Odds_Over = Utility.ObjConvertToDecimal(item.Odds_DQ),
                                                            Odds_Under = Utility.ObjConvertToDecimal(item.Odds_XQ),
                                                            IsLive = "1",
                                                            IsLock = item.IsDisable_DX ?? "0",
                                                            CreateTime = now
                                                        };
                                                        string dxq = string.Empty;
                                                        if (!string.IsNullOrEmpty(item.Text_DQ.Trim()))
                                                        {
                                                            dxq = item.Text_DQ.Replace("大", "").Replace("小", "").Trim();
                                                        }
                                                        if (dxq.Contains("/"))
                                                        {
                                                            string[] arr = dxq.Split('/');
                                                            objtouo.Goals1 = Utility.ObjConvertToDecimal(arr[0].Trim());
                                                            objtouo.Goals2 = Utility.ObjConvertToDecimal(arr[1].Trim());
                                                        }
                                                        else
                                                        {
                                                            objtouo.Goals1 = Utility.ObjConvertToDecimal(rq);
                                                        }
                                                        //var lasttouo = touo_lastbll.GetByMID(objm.ID, objtouo.MatchType, objtouo.Goals1, objtouo.Goals2, objtouo.IsLive);
                                                        update_dx(objtouo, now, touobll, touo_lastbll, ref updatecount, ref msgpush);
                                                        #endregion

                                                        #region 单双
                                                        B_TotalSingleDoubleOdds objtsdo = new B_TotalSingleDoubleOdds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            MatchID = objm.ID,
                                                            MatchType = MatchTypeEnum.Full.ToString(),
                                                            Odds_Single = Utility.ObjConvertToDecimal(item.Odds_D),
                                                            Odds_Double = Utility.ObjConvertToDecimal(item.Odds_S),
                                                            IsLive = "1",
                                                            IsLock = item.IsDisable_DS ?? "0",
                                                            CreateTime = now
                                                        };
                                                        //var lasttsdo = tsdo_lastbll.GetByMID(objm.ID, objtsdo.MatchType, "0");
                                                        update_ds(objtsdo, now, tsdobll, tsdo_lastbll, ref updatecount, ref msgpush);
                                                        #endregion
                                                    }
                                                }
                                                #endregion
                                                #region 上半场赔率
                                                if (m.HalfCourtList.Count > 0)
                                                {
                                                    foreach (Odds item in m.HalfCourtList)
                                                    {
                                                        #region 独赢
                                                        B_SingleOdds objso = new B_SingleOdds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            MatchID = objm.ID,
                                                            MatchType = MatchTypeEnum.Firsthalf.ToString(),
                                                            Odds_H = Utility.ObjConvertToDecimal(item.Odds_ZY),
                                                            Odds_V = Utility.ObjConvertToDecimal(item.Odds_KY),
                                                            Odds_Draw = Utility.ObjConvertToDecimal(item.Odds_HJ),
                                                            IsLive = "1",
                                                            IsLock = item.IsDisable_DY ?? "0",
                                                            CreateTime = now
                                                        };
                                                        //var lastso = so_lastbll.GetByMID(objm.ID, objso.MatchType, objso.IsLive);
                                                        update_dy(objso, now, sobll, so_lastbll, ref updatecount, ref msgpush);
                                                        #endregion

                                                        #region 让球
                                                        B_HandicapOdds objho = new B_HandicapOdds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            MatchID = objm.ID,
                                                            MatchType = MatchTypeEnum.Firsthalf.ToString(),
                                                            Odds_H = Utility.ObjConvertToDecimal(item.Odds_RQZY),
                                                            Odds_V = Utility.ObjConvertToDecimal(item.Odds_RQKY),
                                                            IsLive = "1",
                                                            IsLock = item.IsDisable_RQ ?? "0",
                                                            CreateTime = now
                                                        };
                                                        string rq = string.Empty;
                                                        if (!string.IsNullOrEmpty(item.Text_ZRKQ.Trim()))
                                                        {
                                                            objho.PointSpread = "1";
                                                            rq = item.Text_ZRKQ.Trim();
                                                        }
                                                        else if (!string.IsNullOrEmpty(item.Text_KRZQ.Trim()))
                                                        {
                                                            objho.PointSpread = "2";
                                                            rq = item.Text_KRZQ.Trim();
                                                        }
                                                        if (rq.Contains("/"))
                                                        {
                                                            string[] arr = rq.Split('/');
                                                            objho.PS_Number1 = Utility.ObjConvertToDecimal(arr[0].Trim());
                                                            objho.PS_Number2 = Utility.ObjConvertToDecimal(arr[1].Trim());
                                                        }
                                                        else
                                                        {
                                                            objho.PS_Number1 = Utility.ObjConvertToDecimal(rq);
                                                        }
                                                        //var lastho = ho_lastbll.GetByMID(objm.ID, objho.MatchType, objho.PointSpread, objho.PS_Number1, objho.PS_Number2, objho.IsLive);
                                                        update_rq(objho, now, hobll, ho_lastbll, ref updatecount, ref msgpush);
                                                        #endregion

                                                        #region 大小球
                                                        B_TotalOverUnderOdds objtouo = new B_TotalOverUnderOdds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            MatchID = objm.ID,
                                                            MatchType = MatchTypeEnum.Firsthalf.ToString(),
                                                            Odds_Over = Utility.ObjConvertToDecimal(item.Odds_DQ),
                                                            Odds_Under = Utility.ObjConvertToDecimal(item.Odds_XQ),
                                                            IsLive = "1",
                                                            IsLock = item.IsDisable_DX ?? "0",
                                                            CreateTime = now
                                                        };
                                                        string dxq = string.Empty;
                                                        if (!string.IsNullOrEmpty(item.Text_DQ.Trim()))
                                                        {
                                                            dxq = item.Text_DQ.Replace("大", "").Replace("小", "").Trim();
                                                        }
                                                        if (dxq.Contains("/"))
                                                        {
                                                            string[] arr = dxq.Split('/');
                                                            objtouo.Goals1 = Utility.ObjConvertToDecimal(arr[0].Trim());
                                                            objtouo.Goals2 = Utility.ObjConvertToDecimal(arr[1].Trim());
                                                        }
                                                        else
                                                        {
                                                            objtouo.Goals1 = Utility.ObjConvertToDecimal(rq);
                                                        }
                                                        //var lasttouo = touo_lastbll.GetByMID(objm.ID, objtouo.MatchType, objtouo.Goals1, objtouo.Goals2, "0");
                                                        update_dx(objtouo, now, touobll, touo_lastbll, ref updatecount, ref msgpush);
                                                        #endregion

                                                        #region 单双
                                                        B_TotalSingleDoubleOdds objtsdo = new B_TotalSingleDoubleOdds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            MatchID = objm.ID,
                                                            MatchType = MatchTypeEnum.Firsthalf.ToString(),
                                                            Odds_Single = Utility.ObjConvertToDecimal(item.Odds_D),
                                                            Odds_Double = Utility.ObjConvertToDecimal(item.Odds_S),
                                                            IsLive = "1",
                                                            IsLock = item.IsDisable_DS ?? "0",
                                                            CreateTime = now
                                                        };
                                                        //var lasttsdo = tsdo_lastbll.GetByMID(objm.ID, objso.MatchType, "0");
                                                        update_ds(objtsdo, now, tsdobll, tsdo_lastbll, ref updatecount, ref msgpush);
                                                        #endregion
                                                    }
                                                }
                                                #endregion
                                                #region 波胆
                                                if (m.CorrectScoreList.Count > 0)
                                                {
                                                    B_CorrectScoreOddsBll csobll = new B_CorrectScoreOddsBll();
                                                    B_CorrectScoreOdds_LastBll cso_lastbll = new B_CorrectScoreOdds_LastBll();
                                                    foreach (OddsBD item in m.CorrectScoreList)
                                                    {
                                                        B_CorrectScoreOdds objcso = new B_CorrectScoreOdds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            MatchID = objm.ID,
                                                            HomeTeamScore = item.Text_H,
                                                            VisitingTeamScore = item.Text_V,
                                                            Odds = Utility.ObjConvertToDecimal(item.Odds_BD),
                                                            IsLive = "1",
                                                            IsLock = item.IsDisable ?? "0",
                                                            CreateTime = now
                                                        };
                                                        if (item.type == 1)
                                                        {
                                                            objcso.MatchType = MatchTypeEnum.Full.ToString();
                                                        }
                                                        else if (item.type == 2)
                                                        {
                                                            objcso.MatchType = MatchTypeEnum.Firsthalf.ToString();
                                                        }
                                                        else if (item.type == 3)
                                                        {
                                                            objcso.MatchType = MatchTypeEnum.Secondhalf.ToString();
                                                        }
                                                        //var lastcso = cso_lastbll.GetByMID(objm.ID, objcso.MatchType, objcso.HomeTeamScore, objcso.VisitingTeamScore, objcso.IsLive);
                                                        update_bd(objcso, now, csobll, cso_lastbll, ref updatecount, ref msgpush);
                                                    }
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
                                        #region 早盘
                                        try
                                        {
                                            DateTime bstime = Convert.ToDateTime(now.Year + "-" + m.time);
                                            //A_Match objm = mbll.GetByHVTime(objtH.ID, objtV.ID, bstime);
                                            List<string> _NoLockMID = new List<string>();
                                            A_Match objm = this.Temp_A_Match.Where(x => x.HomeTeamID == objtH.ID && x.VisitingTeamID == objtV.ID && x.SP_GameStartTime.Value.Date == bstime.Date).FirstOrDefault();                                            
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
                                                    SP_GameStartTime = bstime,
                                                    ExistLive = string.IsNullOrEmpty(m.GQ) ? "0" : "1",
                                                    IsLock = "0",
                                                    ModifyTime = now
                                                };
                                                msg = mbll.Create(objm);
                                                if (string.IsNullOrEmpty(msg))
                                                {
                                                    _NoLockMID.Add(objm.ID);
                                                    this.Temp_A_Match.Add(objm);
                                                }
                                                else
                                                {
                                                    this.Temp_A_Match = mbll.FindByDate(SourcePlatformEnum.SB.ToString(), "", now.Date.AddDays(-1));
                                                }
                                            }
                                            else//更新
                                            {
                                                _NoLockMID.Add(objm.ID);                                                
                                                var newm = (A_Match)Utility.DeepCopy(objm);
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
                                                    this.Temp_A_Match = mbll.FindByDate(SourcePlatformEnum.SB.ToString(), "", now.Date.AddDays(-1));
                                                }
                                            }
                                            if (string.IsNullOrEmpty(msg))
                                            {
                                                //锁定消失赛事
                                                this.Temp_A_Match.Where(x => x.IsStart == null && x.IsLock == "0" && !_NoLockMID.Contains(x.ID)).ToList().ForEach(x =>
                                                {
                                                    x.IsLock = "1";
                                                    x.ModifyTime = now;
                                                    mbll.Update(x);
                                                });

                                                B_SingleOddsBll sobll = new B_SingleOddsBll();
                                                B_SingleOdds_LastBll so_lastbll = new B_SingleOdds_LastBll();
                                                B_HandicapOddsBll hobll = new B_HandicapOddsBll();
                                                B_HandicapOdds_LastBll ho_lastbll = new B_HandicapOdds_LastBll();
                                                B_TotalOverUnderOddsBll touobll = new B_TotalOverUnderOddsBll();
                                                B_TotalOverUnderOdds_LastBll touo_lastbll = new B_TotalOverUnderOdds_LastBll();
                                                B_TotalSingleDoubleOddsBll tsdobll = new B_TotalSingleDoubleOddsBll();
                                                B_TotalSingleDoubleOdds_LastBll tsdo_lastbll = new B_TotalSingleDoubleOdds_LastBll();
                                                #region 全场赔率
                                                if (m.FullCourtList.Count > 0)
                                                {
                                                    foreach (Odds item in m.FullCourtList)
                                                    {
                                                        #region 独赢
                                                        B_SingleOdds objso = new B_SingleOdds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            MatchID = objm.ID,
                                                            MatchType = MatchTypeEnum.Full.ToString(),
                                                            Odds_H = Utility.ObjConvertToDecimal(item.Odds_ZY),
                                                            Odds_V = Utility.ObjConvertToDecimal(item.Odds_KY),
                                                            Odds_Draw = Utility.ObjConvertToDecimal(item.Odds_HJ),
                                                            IsLive = "0",
                                                            IsLock = item.IsDisable_DY ?? "0",
                                                            CreateTime = now
                                                        };
                                                        update_dy(objso, now, sobll, so_lastbll, ref updatecount, ref msgpush);
                                                        #endregion

                                                        #region 让球
                                                        B_HandicapOdds objho = new B_HandicapOdds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            MatchID = objm.ID,
                                                            MatchType = MatchTypeEnum.Full.ToString(),
                                                            Odds_H = Utility.ObjConvertToDecimal(item.Odds_RQZY),
                                                            Odds_V = Utility.ObjConvertToDecimal(item.Odds_RQKY),
                                                            IsLive = "0",
                                                            IsLock = item.IsDisable_RQ,
                                                            CreateTime = now
                                                        };
                                                        string rq = string.Empty;
                                                        if (!string.IsNullOrEmpty(item.Text_ZRKQ.Trim()))
                                                        {
                                                            objho.PointSpread = "1";
                                                            rq = item.Text_ZRKQ.Trim();
                                                        }
                                                        else if (!string.IsNullOrEmpty(item.Text_KRZQ.Trim()))
                                                        {
                                                            objho.PointSpread = "2";
                                                            rq = item.Text_KRZQ.Trim();
                                                        }
                                                        if (rq.Contains("/"))
                                                        {
                                                            string[] arr = rq.Split('/');
                                                            objho.PS_Number1 = Utility.ObjConvertToDecimal(arr[0].Trim());
                                                            objho.PS_Number2 = Utility.ObjConvertToDecimal(arr[1].Trim());
                                                        }
                                                        else
                                                        {
                                                            objho.PS_Number1 = Utility.ObjConvertToDecimal(rq);
                                                        }
                                                        update_rq(objho, now, hobll, ho_lastbll, ref updatecount, ref msgpush);
                                                        #endregion

                                                        #region 大小球
                                                        B_TotalOverUnderOdds objtouo = new B_TotalOverUnderOdds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            MatchID = objm.ID,
                                                            MatchType = MatchTypeEnum.Full.ToString(),
                                                            Odds_Over = Utility.ObjConvertToDecimal(item.Odds_DQ),
                                                            Odds_Under = Utility.ObjConvertToDecimal(item.Odds_XQ),
                                                            IsLive = "0",
                                                            IsLock = item.IsDisable_DX ?? "0",
                                                            CreateTime = now
                                                        };
                                                        string dxq = string.Empty;
                                                        if (!string.IsNullOrEmpty(item.Text_DQ.Trim()))
                                                        {
                                                            dxq = item.Text_DQ.Replace("大", "").Replace("小", "").Trim();
                                                        }
                                                        if (dxq.Contains("/"))
                                                        {
                                                            string[] arr = dxq.Split('/');
                                                            objtouo.Goals1 = Utility.ObjConvertToDecimal(arr[0].Trim());
                                                            objtouo.Goals2 = Utility.ObjConvertToDecimal(arr[1].Trim());
                                                        }
                                                        else
                                                        {
                                                            objtouo.Goals1 = Utility.ObjConvertToDecimal(rq);
                                                        }
                                                        update_dx(objtouo, now, touobll, touo_lastbll, ref updatecount, ref msgpush);
                                                        #endregion

                                                        #region 单双
                                                        B_TotalSingleDoubleOdds objtsdo = new B_TotalSingleDoubleOdds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            MatchID = objm.ID,
                                                            MatchType = MatchTypeEnum.Full.ToString(),
                                                            Odds_Single = Utility.ObjConvertToDecimal(item.Odds_D),
                                                            Odds_Double = Utility.ObjConvertToDecimal(item.Odds_S),
                                                            IsLive = "0",
                                                            IsLock = item.IsDisable_DS ?? "0",
                                                            CreateTime = now
                                                        };
                                                        update_ds(objtsdo, now, tsdobll, tsdo_lastbll, ref updatecount, ref msgpush);
                                                        #endregion
                                                    }
                                                }
                                                #endregion
                                                #region 上半场赔率
                                                if (m.HalfCourtList.Count > 0)
                                                {
                                                    foreach (Odds item in m.HalfCourtList)
                                                    {
                                                        #region 独赢
                                                        B_SingleOdds objso = new B_SingleOdds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            MatchID = objm.ID,
                                                            MatchType = MatchTypeEnum.Firsthalf.ToString(),
                                                            Odds_H = Utility.ObjConvertToDecimal(item.Odds_ZY),
                                                            Odds_V = Utility.ObjConvertToDecimal(item.Odds_KY),
                                                            Odds_Draw = Utility.ObjConvertToDecimal(item.Odds_HJ),
                                                            IsLive = "0",
                                                            IsLock = item.IsDisable_DY ?? "0",
                                                            CreateTime = now
                                                        };
                                                        update_dy(objso, now, sobll, so_lastbll, ref updatecount, ref msgpush);
                                                        #endregion

                                                        #region 让球
                                                        B_HandicapOdds objho = new B_HandicapOdds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            MatchID = objm.ID,
                                                            MatchType = MatchTypeEnum.Firsthalf.ToString(),
                                                            Odds_H = Utility.ObjConvertToDecimal(item.Odds_RQZY),
                                                            Odds_V = Utility.ObjConvertToDecimal(item.Odds_RQKY),
                                                            IsLive = "0",
                                                            IsLock = item.IsDisable_RQ ?? "0",
                                                            CreateTime = now
                                                        };
                                                        string rq = string.Empty;
                                                        if (!string.IsNullOrEmpty(item.Text_ZRKQ.Trim()))
                                                        {
                                                            objho.PointSpread = "1";
                                                            rq = item.Text_ZRKQ.Trim();
                                                        }
                                                        else if (!string.IsNullOrEmpty(item.Text_KRZQ.Trim()))
                                                        {
                                                            objho.PointSpread = "2";
                                                            rq = item.Text_KRZQ.Trim();
                                                        }
                                                        if (rq.Contains("/"))
                                                        {
                                                            string[] arr = rq.Split('/');
                                                            objho.PS_Number1 = Utility.ObjConvertToDecimal(arr[0].Trim());
                                                            objho.PS_Number2 = Utility.ObjConvertToDecimal(arr[1].Trim());
                                                        }
                                                        else
                                                        {
                                                            objho.PS_Number1 = Utility.ObjConvertToDecimal(rq);
                                                        }
                                                        update_rq(objho, now, hobll, ho_lastbll, ref updatecount, ref msgpush);
                                                        #endregion

                                                        #region 大小球
                                                        B_TotalOverUnderOdds objtouo = new B_TotalOverUnderOdds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            MatchID = objm.ID,
                                                            MatchType = MatchTypeEnum.Firsthalf.ToString(),
                                                            Odds_Over = Utility.ObjConvertToDecimal(item.Odds_DQ),
                                                            Odds_Under = Utility.ObjConvertToDecimal(item.Odds_XQ),
                                                            IsLive = "0",
                                                            IsLock = item.IsDisable_DX ?? "0",
                                                            CreateTime = now
                                                        };
                                                        string dxq = string.Empty;
                                                        if (!string.IsNullOrEmpty(item.Text_DQ.Trim()))
                                                        {
                                                            dxq = item.Text_DQ.Replace("大", "").Replace("小", "").Trim();
                                                        }
                                                        if (dxq.Contains("/"))
                                                        {
                                                            string[] arr = dxq.Split('/');
                                                            objtouo.Goals1 = Utility.ObjConvertToDecimal(arr[0].Trim());
                                                            objtouo.Goals2 = Utility.ObjConvertToDecimal(arr[1].Trim());
                                                        }
                                                        else
                                                        {
                                                            objtouo.Goals1 = Utility.ObjConvertToDecimal(rq);
                                                        }
                                                        update_dx(objtouo, now, touobll, touo_lastbll, ref updatecount, ref msgpush);
                                                        #endregion

                                                        #region 单双
                                                        B_TotalSingleDoubleOdds objtsdo = new B_TotalSingleDoubleOdds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            MatchID = objm.ID,
                                                            MatchType = MatchTypeEnum.Firsthalf.ToString(),
                                                            Odds_Single = Utility.ObjConvertToDecimal(item.Odds_D),
                                                            Odds_Double = Utility.ObjConvertToDecimal(item.Odds_S),
                                                            IsLive = "0",
                                                            IsLock = item.IsDisable_DS ?? "0",
                                                            CreateTime = now
                                                        };
                                                        update_ds(objtsdo, now, tsdobll, tsdo_lastbll, ref updatecount, ref msgpush);
                                                        #endregion
                                                    }
                                                }
                                                #endregion
                                                #region 半场 / 全场
                                                if (m.DoubleResult != null)
                                                {
                                                    B_DoubleResultOddsBll drobll = new B_DoubleResultOddsBll();
                                                    B_DoubleResultOdds_LastBll dro_lastbll = new B_DoubleResultOdds_LastBll();
                                                    B_DoubleResultOdds objdro = new B_DoubleResultOdds()
                                                    {
                                                        ID = Guid.NewGuid().ToString("N"),
                                                        MatchID = objm.ID,
                                                        MatchType = MatchTypeEnum.Full.ToString(),
                                                        Odds_HH = Utility.ObjConvertToDecimal(m.DoubleResult.Odds_HH),
                                                        Odds_HD = Utility.ObjConvertToDecimal(m.DoubleResult.Odds_HD),
                                                        Odds_HV = Utility.ObjConvertToDecimal(m.DoubleResult.Odds_HV),
                                                        Odds_DH = Utility.ObjConvertToDecimal(m.DoubleResult.Odds_DH),
                                                        Odds_DD = Utility.ObjConvertToDecimal(m.DoubleResult.Odds_DD),
                                                        Odds_DV = Utility.ObjConvertToDecimal(m.DoubleResult.Odds_DV),
                                                        Odds_VH = Utility.ObjConvertToDecimal(m.DoubleResult.Odds_VH),
                                                        Odds_VD = Utility.ObjConvertToDecimal(m.DoubleResult.Odds_VD),
                                                        Odds_VV = Utility.ObjConvertToDecimal(m.DoubleResult.Odds_VV),
                                                        IsLive = "0",
                                                        IsLock = m.DoubleResult.IsDisable ?? "0",
                                                        CreateTime = now
                                                    };
                                                    update_bcqc(objdro, now, drobll, dro_lastbll, ref updatecount, ref msgpush);
                                                }
                                                #endregion
                                                #region 波胆
                                                if (m.CorrectScoreList.Count > 0)
                                                {
                                                    B_CorrectScoreOddsBll csobll = new B_CorrectScoreOddsBll();
                                                    B_CorrectScoreOdds_LastBll cso_lastbll = new B_CorrectScoreOdds_LastBll();
                                                    foreach (OddsBD item in m.CorrectScoreList)
                                                    {
                                                        B_CorrectScoreOdds objcso = new B_CorrectScoreOdds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            MatchID = objm.ID,
                                                            HomeTeamScore = item.Text_H,
                                                            VisitingTeamScore = item.Text_V,
                                                            Odds = Utility.ObjConvertToDecimal(item.Odds_BD),
                                                            IsLive = "0",
                                                            IsLock = item.IsDisable ?? "0",
                                                            CreateTime = now
                                                        };
                                                        if (item.type == 1)
                                                        {
                                                            objcso.MatchType = MatchTypeEnum.Full.ToString();
                                                        }
                                                        else if (item.type == 2)
                                                        {
                                                            objcso.MatchType = MatchTypeEnum.Firsthalf.ToString();
                                                        }
                                                        else if (item.type == 3)
                                                        {
                                                            objcso.MatchType = MatchTypeEnum.Secondhalf.ToString();
                                                        }
                                                        update_bd(objcso, now, csobll, cso_lastbll, ref updatecount, ref msgpush);
                                                    }
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
        }
        private void update_dy(B_SingleOdds objso, DateTime now, B_SingleOddsBll sobll, B_SingleOdds_LastBll so_lastbll, ref int updatecount, ref string msgpush)
        {
            if (objso.Odds_H == 0 && objso.Odds_V == 0 && objso.Odds_Draw == 0 && objso.IsLock == "0")
            {
                return;
            }
            string msg = string.Empty;// && x.IsLive == objso.IsLive
            var lastso = this.Temp_B_SingleOdds_Last.Where(x => x.MatchID == objso.MatchID && x.MatchType == objso.MatchType).OrderByDescending(x => x.ModifyTime).FirstOrDefault();
            if (lastso == null)
            {
                B_SingleOdds_Last objso_last = new B_SingleOdds_Last()
                {
                    ID = Guid.NewGuid().ToString("N"),
                    MatchID = objso.MatchID,
                    MatchType = objso.MatchType,
                    Odds_H = objso.Odds_H,
                    Odds_V = objso.Odds_V,
                    Odds_Draw = objso.Odds_Draw,
                    IsLive = objso.IsLive,
                    IsLock = objso.IsLock,
                    ModifyTime = now
                };
                msg = so_lastbll.Create(objso_last);
                if (string.IsNullOrEmpty(msg))
                {
                    updatecount++;
                    this.Temp_B_SingleOdds_Last.Add(objso_last);
                }
                else
                {
                    msgpush += msg + "\r\n";
                }
                msg = sobll.Create(objso);
                if (!string.IsNullOrEmpty(msg))
                {
                    msgpush += msg + "\r\n";
                }
            }
            else
            {
                if (lastso.Odds_H != objso.Odds_H || lastso.Odds_V != objso.Odds_V || lastso.Odds_Draw != objso.Odds_Draw)
                {
                    var newso = (B_SingleOdds_Last)Utility.DeepCopy(lastso);
                    newso.Odds_H = objso.Odds_H;
                    newso.Odds_V = objso.Odds_V;
                    newso.Odds_Draw = objso.Odds_Draw;
                    newso.IsLive = objso.IsLive;
                    newso.IsLock = objso.IsLock;
                    newso.ModifyTime = now;
                    msg = so_lastbll.Update(newso);
                    if (string.IsNullOrEmpty(msg))
                    {
                        updatecount++;
                        this.Temp_B_SingleOdds_Last.Remove(lastso);
                        this.Temp_B_SingleOdds_Last.Add(newso);
                    }
                    else
                    {
                        msgpush += msg + "\r\n";
                    }
                    msg = sobll.Create(objso);
                    if (!string.IsNullOrEmpty(msg))
                    {
                        msgpush += msg + "\r\n";
                    }
                }
            }
        }
        private void update_rq(B_HandicapOdds objho, DateTime now, B_HandicapOddsBll hobll, B_HandicapOdds_LastBll ho_lastbll, ref int updatecount, ref string msgpush)
        {
            if (objho.Odds_H == 0 && objho.Odds_V == 0 && objho.IsLock == "0")
            {
                return;
            }
            string msg = string.Empty;
            var lastho = this.Temp_B_HandicapOdds_Last.Where(x => x.MatchID == objho.MatchID && x.MatchType == objho.MatchType && x.PointSpread == objho.PointSpread && x.PS_Number1 == objho.PS_Number1 && x.PS_Number2 == objho.PS_Number2).OrderByDescending(x => x.ModifyTime).FirstOrDefault();
            if (lastho == null)
            {
                B_HandicapOdds_Last objho_last = new B_HandicapOdds_Last()
                {
                    ID = Guid.NewGuid().ToString("N"),
                    MatchID = objho.MatchID,
                    MatchType = objho.MatchType,
                    PointSpread = objho.PointSpread,
                    PS_Number1 = objho.PS_Number1,
                    PS_Number2 = objho.PS_Number2,
                    Odds_H = objho.Odds_H,
                    Odds_V = objho.Odds_V,
                    IsLive = objho.IsLive,
                    IsLock = objho.IsLock,
                    ModifyTime = now
                };
                msg = ho_lastbll.Create(objho_last);
                if (string.IsNullOrEmpty(msg))
                {
                    updatecount++;
                    this.Temp_B_HandicapOdds_Last.Add(objho_last);
                }
                else
                {
                    msgpush += msg + "\r\n";
                }
                msg = hobll.Create(objho);
                if (!string.IsNullOrEmpty(msg))
                {
                    msgpush += msg + "\r\n";
                }
            }
            else
            {
                if (lastho.Odds_H != objho.Odds_H || lastho.Odds_V != objho.Odds_V)
                {
                    var newho = (B_HandicapOdds_Last)Utility.DeepCopy(lastho);
                    newho.Odds_H = objho.Odds_H;
                    newho.Odds_V = objho.Odds_V;
                    newho.IsLive = objho.IsLive;
                    newho.IsLock = objho.IsLock;
                    newho.ModifyTime = now;
                    msg = ho_lastbll.Update(newho);
                    if (string.IsNullOrEmpty(msg))
                    {
                        updatecount++;
                        this.Temp_B_HandicapOdds_Last.Remove(lastho);
                        this.Temp_B_HandicapOdds_Last.Add(newho);
                    }
                    else
                    {
                        msgpush += msg + "\r\n";
                    }
                    msg = hobll.Create(objho);
                    if (!string.IsNullOrEmpty(msg))
                    {
                        msgpush += msg + "\r\n";
                    }
                }
            }
        }
        private void update_dx(B_TotalOverUnderOdds objtouo,DateTime now, B_TotalOverUnderOddsBll touobll, B_TotalOverUnderOdds_LastBll touo_lastbll, ref int updatecount, ref string msgpush)
        {
            if (objtouo.Odds_Over == 0 && objtouo.Odds_Under == 0 && objtouo.IsLock == "0")
            {
                return;
            }
            string msg = string.Empty;
            var lasttouo = this.Temp_B_TotalOverUnderOdds_Last.Where(x => x.MatchID == objtouo.MatchID && x.MatchType == objtouo.MatchType && x.Goals1 == objtouo.Goals1 && x.Goals2 == objtouo.Goals2).OrderByDescending(x => x.ModifyTime).FirstOrDefault();
            if (lasttouo == null)
            {
                B_TotalOverUnderOdds_Last objtouo_last = new B_TotalOverUnderOdds_Last()
                {
                    ID = Guid.NewGuid().ToString("N"),
                    MatchID = objtouo.MatchID,
                    MatchType = objtouo.MatchType,
                    Goals1 = objtouo.Goals1,
                    Goals2 = objtouo.Goals2,
                    Odds_Over = objtouo.Odds_Over,
                    Odds_Under = objtouo.Odds_Under,
                    IsLive = objtouo.IsLive,
                    IsLock = objtouo.IsLock,
                    ModifyTime = now
                };
                msg = touo_lastbll.Create(objtouo_last);
                if (string.IsNullOrEmpty(msg))
                {
                    updatecount++;
                    this.Temp_B_TotalOverUnderOdds_Last.Add(objtouo_last);
                }
                else
                {
                    msgpush += msg + "\r\n";
                }
                msg = touobll.Create(objtouo);
                if (!string.IsNullOrEmpty(msg))
                {
                    msgpush += msg + "\r\n";
                }
            }
            else
            {
                if (lasttouo.Odds_Over != objtouo.Odds_Over || lasttouo.Odds_Under != objtouo.Odds_Under)
                {
                    var newtouo = (B_TotalOverUnderOdds_Last)Utility.DeepCopy(lasttouo);
                    newtouo.Odds_Over = objtouo.Odds_Over;
                    newtouo.Odds_Under = objtouo.Odds_Under;
                    newtouo.IsLive = objtouo.IsLive;
                    newtouo.IsLock = objtouo.IsLock;
                    newtouo.ModifyTime = now;
                    msg = touo_lastbll.Update(newtouo);
                    if (string.IsNullOrEmpty(msg))
                    {
                        updatecount++;
                        this.Temp_B_TotalOverUnderOdds_Last.Remove(lasttouo);
                        this.Temp_B_TotalOverUnderOdds_Last.Add(newtouo);
                    }
                    else
                    {
                        msgpush += msg + "\r\n";
                    }
                    msg = touobll.Create(objtouo);
                    if (!string.IsNullOrEmpty(msg))
                    {
                        msgpush += msg + "\r\n";
                    }
                }
            }
        }
        private void update_ds(B_TotalSingleDoubleOdds objtsdo, DateTime now, B_TotalSingleDoubleOddsBll tsdobll, B_TotalSingleDoubleOdds_LastBll tsdo_lastbll, ref int updatecount, ref string msgpush)
        {
            if (objtsdo.Odds_Single == 0 && objtsdo.Odds_Double == 0 && objtsdo.IsLock == "0")
            {
                return;
            }
            string msg = string.Empty;
            var lasttsdo = this.Temp_B_TotalSingleDoubleOdds_Last.Where(x => x.MatchID == objtsdo.MatchID && x.MatchType == objtsdo.MatchType).OrderByDescending(x => x.ModifyTime).FirstOrDefault();
            if (lasttsdo == null)
            {
                B_TotalSingleDoubleOdds_Last objtsdo_last = new B_TotalSingleDoubleOdds_Last()
                {
                    ID = Guid.NewGuid().ToString("N"),
                    MatchID = objtsdo.MatchID,
                    MatchType = objtsdo.MatchType,
                    Odds_Single = objtsdo.Odds_Single,
                    Odds_Double = objtsdo.Odds_Double,
                    IsLive = objtsdo.IsLive,
                    IsLock = objtsdo.IsLock,
                    ModifyTime = now
                };
                msg = tsdo_lastbll.Create(objtsdo_last);
                if (string.IsNullOrEmpty(msg))
                {
                    updatecount++;
                    this.Temp_B_TotalSingleDoubleOdds_Last.Add(objtsdo_last);
                }
                else
                {
                    msgpush += msg + "\r\n";
                }
                msg = tsdobll.Create(objtsdo);
                if (!string.IsNullOrEmpty(msg))
                {
                    msgpush += msg + "\r\n";
                }
            }
            else
            {
                if (lasttsdo.Odds_Single != objtsdo.Odds_Single || lasttsdo.Odds_Double != objtsdo.Odds_Double)
                {
                    var newtsdo = (B_TotalSingleDoubleOdds_Last)Utility.DeepCopy(lasttsdo);
                    newtsdo.Odds_Single = objtsdo.Odds_Single;
                    newtsdo.Odds_Double = objtsdo.Odds_Double;
                    newtsdo.IsLive = objtsdo.IsLive;
                    newtsdo.IsLock = objtsdo.IsLock;
                    newtsdo.ModifyTime = now;
                    msg = tsdo_lastbll.Update(newtsdo);
                    if (string.IsNullOrEmpty(msg))
                    {
                        updatecount++;
                        this.Temp_B_TotalSingleDoubleOdds_Last.Remove(lasttsdo);
                        this.Temp_B_TotalSingleDoubleOdds_Last.Add(newtsdo);
                    }
                    else
                    {
                        msgpush += msg + "\r\n";
                    }
                    msg = tsdobll.Create(objtsdo);
                    if (!string.IsNullOrEmpty(msg))
                    {
                        msgpush += msg + "\r\n";
                    }
                }
            }
        }
        private void update_bd(B_CorrectScoreOdds objcso, DateTime now, B_CorrectScoreOddsBll csobll, B_CorrectScoreOdds_LastBll cso_lastbll, ref int updatecount, ref string msgpush)
        {
            string msg = string.Empty;
            var lastcso = this.Temp_B_CorrectScoreOdds_Last.Where(x => x.MatchID == objcso.MatchID && x.MatchType == objcso.MatchType && x.HomeTeamScore == objcso.HomeTeamScore && x.VisitingTeamScore == objcso.VisitingTeamScore).OrderByDescending(x => x.ModifyTime).FirstOrDefault();
            if (lastcso == null)
            {
                B_CorrectScoreOdds_Last objcso_last = new B_CorrectScoreOdds_Last()
                {
                    ID = Guid.NewGuid().ToString("N"),
                    MatchID = objcso.MatchID,
                    MatchType = objcso.MatchType,
                    HomeTeamScore = objcso.HomeTeamScore,
                    VisitingTeamScore = objcso.VisitingTeamScore,
                    Odds = objcso.Odds,
                    IsLive = objcso.IsLive,
                    IsLock = objcso.IsLock,
                    ModifyTime = now
                };
                msg = cso_lastbll.Create(objcso_last);
                if (string.IsNullOrEmpty(msg))
                {
                    updatecount++;
                    this.Temp_B_CorrectScoreOdds_Last.Add(objcso_last);
                }
                else
                {
                    msgpush += msg + "\r\n";
                }
                msg = csobll.Create(objcso);
                if (!string.IsNullOrEmpty(msg))
                {
                    msgpush += msg + "\r\n";
                }
            }
            else
            {
                if (lastcso.Odds != objcso.Odds)
                {
                    var newcso = (B_CorrectScoreOdds_Last)Utility.DeepCopy(lastcso);
                    newcso.Odds = objcso.Odds;
                    newcso.IsLive = objcso.IsLive;
                    newcso.IsLock = objcso.IsLock;
                    newcso.ModifyTime = now;
                    msg = cso_lastbll.Update(newcso);
                    if (string.IsNullOrEmpty(msg))
                    {
                        updatecount++;
                        this.Temp_B_CorrectScoreOdds_Last.Remove(lastcso);
                        this.Temp_B_CorrectScoreOdds_Last.Add(newcso);
                    }
                    else
                    {
                        msgpush += msg + "\r\n";
                    }
                    msg = csobll.Create(objcso);
                    if (!string.IsNullOrEmpty(msg))
                    {
                        msgpush += msg + "\r\n";
                    }
                }
            }
        }
        private void update_bcqc(B_DoubleResultOdds objdro, DateTime now, B_DoubleResultOddsBll drobll, B_DoubleResultOdds_LastBll dro_lastbll, ref int updatecount, ref string msgpush)
        {
            string msg = string.Empty;
            var lastdro = this.Temp_B_DoubleResultOdds_Last.Where(x => x.MatchID == objdro.MatchID).OrderByDescending(x => x.ModifyTime).FirstOrDefault();
            if (lastdro == null)
            {
                B_DoubleResultOdds_Last objdro_last = new B_DoubleResultOdds_Last()
                {
                    ID = Guid.NewGuid().ToString("N"),
                    MatchID = objdro.MatchID,
                    MatchType = objdro.MatchType,
                    Odds_HH = objdro.Odds_HH,
                    Odds_HD = objdro.Odds_HD,
                    Odds_HV = objdro.Odds_HV,
                    Odds_DH = objdro.Odds_DH,
                    Odds_DD = objdro.Odds_DD,
                    Odds_DV = objdro.Odds_DV,
                    Odds_VH = objdro.Odds_VH,
                    Odds_VD = objdro.Odds_VD,
                    Odds_VV = objdro.Odds_VV,
                    IsLive = objdro.IsLive,
                    IsLock = objdro.IsLock,
                    ModifyTime = now
                };
                msg = dro_lastbll.Create(objdro_last);
                if (string.IsNullOrEmpty(msg))
                {
                    updatecount++;
                    this.Temp_B_DoubleResultOdds_Last.Add(objdro_last);
                }
                else
                {
                    msgpush += msg + "\r\n";
                }
                msg = drobll.Create(objdro);
                if (!string.IsNullOrEmpty(msg))
                {
                    msgpush += msg + "\r\n";
                }
            }
            else
            {
                if (objdro.Odds_HH != lastdro.Odds_HH ||
                objdro.Odds_HD != lastdro.Odds_HD ||
                objdro.Odds_HV != lastdro.Odds_HV ||
                objdro.Odds_DH != lastdro.Odds_DH ||
                objdro.Odds_DD != lastdro.Odds_DD ||
                objdro.Odds_DV != lastdro.Odds_DV ||
                objdro.Odds_VH != lastdro.Odds_VH ||
                objdro.Odds_VD != lastdro.Odds_VD ||
                objdro.Odds_VV != lastdro.Odds_VV)//有变化
                {
                    var newdro = (B_DoubleResultOdds_Last)Utility.DeepCopy(lastdro);
                    newdro.Odds_HH = objdro.Odds_HH;
                    newdro.Odds_HD = objdro.Odds_HD;
                    newdro.Odds_HV = objdro.Odds_HV;
                    newdro.Odds_DH = objdro.Odds_DH;
                    newdro.Odds_DD = objdro.Odds_DD;
                    newdro.Odds_DV = objdro.Odds_DV;
                    newdro.Odds_VH = objdro.Odds_VH;
                    newdro.Odds_VD = objdro.Odds_VD;
                    newdro.Odds_VV = objdro.Odds_VV;
                    newdro.IsLive = objdro.IsLive;
                    newdro.IsLock = objdro.IsLock;
                    newdro.ModifyTime = now;
                    msg = dro_lastbll.Update(newdro);
                    if (string.IsNullOrEmpty(msg))
                    {
                        updatecount++;
                        this.Temp_B_DoubleResultOdds_Last.Remove(lastdro);
                        this.Temp_B_DoubleResultOdds_Last.Add(newdro);
                    }
                    else
                    {
                        msgpush += msg + "\r\n";
                    }
                    msg = drobll.Create(objdro);
                    if (!string.IsNullOrEmpty(msg))
                    {
                        msgpush += msg + "\r\n";
                    }
                }
            }
        }
        private void update_gj(B_OutrightOdds objoo, DateTime now, B_OutrightOddsBll oobll, B_OutrightOdds_LastBll oo_lastbll, ref int updatecount, ref string msgpush)
        {
            string msg = string.Empty;
            var lastoo = this.Temp_B_OutrightOdds_Last.Where(x => x.TeamID == objoo.TeamID).OrderByDescending(x => x.ModifyTime).FirstOrDefault();
            if (lastoo == null)
            {
                B_OutrightOdds_Last objoo_last = new B_OutrightOdds_Last()
                {
                    ID = Guid.NewGuid().ToString("N"),
                    LeagueMatchID = objoo.LeagueMatchID,
                    TeamID = objoo.TeamID,
                    Odds = objoo.Odds,
                    ModifyTime = now
                };
                msg = oo_lastbll.Create(objoo_last);
                if (string.IsNullOrEmpty(msg))
                {
                    updatecount++;
                    this.Temp_B_OutrightOdds_Last.Add(objoo_last);
                }
                else
                {
                    msgpush += msg + "\r\n";
                }
                msg = oobll.Create(objoo);
                if (!string.IsNullOrEmpty(msg))
                {
                    msgpush += msg + "\r\n";
                }
            }
            else
            {
                if (lastoo.Odds != objoo.Odds)
                {
                    var newoo = (B_OutrightOdds_Last)Utility.DeepCopy(lastoo);
                    newoo.Odds = objoo.Odds;
                    newoo.ModifyTime = now;
                    msg = oo_lastbll.Update(newoo);
                    if (string.IsNullOrEmpty(msg))
                    {
                        updatecount++;
                        this.Temp_B_OutrightOdds_Last.Remove(lastoo);
                        this.Temp_B_OutrightOdds_Last.Add(newoo);
                    }
                    else
                    {
                        msgpush += msg + "\r\n";
                    }
                    msg = oobll.Create(objoo);
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
                                                GetData(tags[0], tags[1], tags[2], log);
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
            _WebDriverList.Add(driver);
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
                    MessageBox.Show("异常，可能在维护。");
                    return;
                }
                driver.SwitchTo().Frame("sportsFrame");
                string username = driver.FindElement(By.CssSelector("#sbContainer #sb-header #n1")).GetAttribute("innerHTML");
                if (!username.Contains(","))//未登录
                {
                    driver.SwitchTo().DefaultContent();
                    driver.Navigate().GoToUrl(_SourcePlatform.loginurl);
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
                    driver.SwitchTo().Frame("sportsbook");
                    bool flag02 = false;
                    while (!flag02)
                    {
                        flag02 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#sportsFrame"), _WebDriver_MS);
                    }
                    driver.SwitchTo().Frame("sportsFrame");
                    bool flag03 = false;
                    while (!flag03)
                    {
                        flag03 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#header #betList"), _WebDriver_MS);
                    }
                    //Thread.Sleep(10000);
                    driver.SwitchTo().DefaultContent();
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
                return;
            }
            _WebDriverList.Add(driver);
            this.Invoke(new MethodInvoker(delegate
            {
                _IsRunning = true;
                logwin.txt_log.AppendText("地址：" + _SourcePlatform.resulturl + "\r\n");
                logwin.txt_log.AppendText("\r\n");
                logwin.txt_log.AppendText("开始初次请求：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n");
                logwin.txt_log.AppendText("===============================================\r\n");
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
                        logwin.txt_log.AppendText("异常，可能在维护。\r\n");
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
                                logwin.txt_log.AppendText("===============================================\r\n");
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
                                            }));
                                        }
                                        else
                                        {
                                            this.Invoke(new MethodInvoker(delegate
                                            {
                                                logwin.txt_log.AppendText("今日无足球\r\n");
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
                                        logwin.txt_log.AppendText("===============================================\r\n");
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
                                            }));
                                        }
                                        else
                                        {
                                            this.Invoke(new MethodInvoker(delegate
                                            {
                                                logwin.txt_log.AppendText("昨日无足球\r\n");
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
                                        logwin.txt_log.AppendText("===============================================\r\n");
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
                                    logwin.txt_log.AppendText("===============================================\r\n");
                                }));
                            }
                            else
                            {
                                this.Invoke(new MethodInvoker(delegate
                                {
                                    logwin.txt_log.AppendText("===============================================\r\n");
                                    logwin.txt_log.AppendText("数据抓取完毕\r\n");
                                }));
                                int updatecount = 0;
                                string msgpush = string.Empty;
                                //解析数据
                                AnalysisResultHtml_TYC(html, sportsType,ref updatecount, ref msgpush);
                                this.Invoke(new MethodInvoker(delegate
                                {
                                    logwin.txt_log.AppendText(msgpush);
                                    logwin.txt_log.AppendText("更新成功：" + updatecount + "\r\n");
                                }));
                                watch.Stop();
                                this.Invoke(new MethodInvoker(delegate
                                {
                                    //logwin.txt_log.AppendText(html);
                                    logwin.txt_log.AppendText("===============================================\r\n");
                                    logwin.txt_log.AppendText("耗时：" + watch.Elapsed.TotalSeconds + "秒\r\n\r\n");
                                }));
                            }
                            #endregion

                        }
                        catch (Exception ex)
                        {
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("===============================================\r\n");
                                logwin.txt_log.AppendText("第" + count + "次异常：" + ex.Message + "\r\n\r\n");
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
                                logwin.txt_log.AppendText("===============================================\r\n");
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
                                            }));
                                        }
                                        else
                                        {
                                            this.Invoke(new MethodInvoker(delegate
                                            {
                                                logwin.txt_log.AppendText("今日无足球\r\n");
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
                                        logwin.txt_log.AppendText("===============================================\r\n");
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
                                            }));
                                        }
                                        else
                                        {
                                            this.Invoke(new MethodInvoker(delegate
                                            {
                                                logwin.txt_log.AppendText("昨日无足球\r\n");
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
                                        logwin.txt_log.AppendText("===============================================\r\n");
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
                                    logwin.txt_log.AppendText("===============================================\r\n");
                                }));
                            }
                            else
                            {
                                this.Invoke(new MethodInvoker(delegate
                                {
                                    logwin.txt_log.AppendText("===============================================\r\n");
                                    logwin.txt_log.AppendText("数据抓取完毕\r\n");
                                }));
                                //解析数据
                                AnalysisResultHtml_MS(html, sportsType);
                                watch.Stop();
                                this.Invoke(new MethodInvoker(delegate
                                {
                                    //logwin.txt_log.AppendText(html);
                                    logwin.txt_log.AppendText("===============================================\r\n");
                                    logwin.txt_log.AppendText("耗时：" + watch.Elapsed.TotalSeconds + "秒\r\n\r\n");
                                }));
                            }
                            #endregion

                        }
                        catch (Exception ex)
                        {
                            this.Invoke(new MethodInvoker(delegate
                            {
                                logwin.txt_log.AppendText("===============================================\r\n");
                                logwin.txt_log.AppendText("第" + count + "次异常：" + ex.Message + "\r\n\r\n");
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
                            A_LeagueMatch objlm = this.Temp_A_LeagueMatch.Where(x => x.Name == LeagueMatchName).FirstOrDefault();
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
                                    this.Temp_A_LeagueMatch = lmbll.FindAll(SourcePlatformEnum.SB.ToString(), "");
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
                                A_Team objtH = this.Temp_A_Team.Where(x => x.LeagueMatchID == objlm.ID && x.Name == htname).FirstOrDefault();
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
                                        this.Temp_A_Team = tbll.FindAll(SourcePlatformEnum.SB.ToString(), "");
                                    }
                                }
                                //客队
                                A_Team objtV = this.Temp_A_Team.Where(x => x.LeagueMatchID == objlm.ID && x.Name == vtname).FirstOrDefault();
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
                                        this.Temp_A_Team = tbll.FindAll(SourcePlatformEnum.SB.ToString(), "");
                                    }
                                }
                                if (string.IsNullOrEmpty(msg1) && string.IsNullOrEmpty(msg2))
                                {
                                    A_Match objm = this.Temp_A_Match.Where(x => x.HomeTeamID == objtH.ID && x.VisitingTeamID == objtV.ID && x.SP_GameStartTime.Value.Date == starttime.Date).FirstOrDefault();
                                    if (objm!=null)
                                    {
                                        objm.IsEnd = "1";
                                        objm.ModifyTime = now;
                                        //objm.GameEndTime = now;
                                        mbll.Update(objm);
                                    }
                                    A_MatchResult objmr = this.Temp_A_MatchResult.Where(x => x.LeagueMatchID == objlm.ID && x.HomeTeamID == objtH.ID && x.VisitingTeamID == objtV.ID && x.GameStartTime == starttime).FirstOrDefault();
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
                                            updatecount++;
                                            this.Temp_A_MatchResult.Add(objmr);
                                        }
                                        else
                                        {
                                            msgpush += msg + "\r\n";
                                            this.Temp_A_MatchResult = mrbll.FindByDate(SourcePlatformEnum.SB.ToString(), "", now.Date.AddDays(-1));
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
                            A_LeagueMatch objlm = this.Temp_A_LeagueMatch.Where(x => x.Name == LeagueMatchName).FirstOrDefault();
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
                                    this.Temp_A_LeagueMatch = lmbll.FindAll(SourcePlatformEnum.SB.ToString(), "");
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
                                A_Team objtH = this.Temp_A_Team.Where(x => x.LeagueMatchID == objlm.ID && x.Name == htname).FirstOrDefault();
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
                                        this.Temp_A_Team = tbll.FindAll(SourcePlatformEnum.SB.ToString(), "");
                                    }
                                }
                                //客队
                                A_Team objtV = this.Temp_A_Team.Where(x => x.LeagueMatchID == objlm.ID && x.Name == vtname).FirstOrDefault();
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
                                        this.Temp_A_Team = tbll.FindAll(SourcePlatformEnum.SB.ToString(), "");
                                    }
                                }
                                if (string.IsNullOrEmpty(msg1) && string.IsNullOrEmpty(msg2))
                                {
                                    A_MatchResult objmr = this.Temp_A_MatchResult.Where(x => x.LeagueMatchID == objlm.ID && x.HomeTeamID == objtH.ID && x.VisitingTeamID == objtV.ID && x.GameStartTime == starttime).FirstOrDefault();
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
                                            this.Temp_A_MatchResult = mrbll.FindByDate(SourcePlatformEnum.SB.ToString(), "", now.Date.AddDays(-1));
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
    }
}
