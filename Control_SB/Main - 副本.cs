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
    public partial class Main : Form
    {
        private string _URL = "https://mkt.ljb04.com/NewIndex?webskintype=2&lang=cs";
        //立即博  https://mkt.ljb04.com/NewIndex?webskintype=2&lang=cs
        //太阳城  https://mkt.sss988n1jssx92.info/NewIndex?webskintype=2&lang=cs
        //明升  https://mkt.m88ms.com/mansion88.aspx?webskintype=2&lang=cs
        //乐天堂  https://sports.fun88asia.com/NewIndex?webskintype=2&lang=cs

        private string _URLTYC = "https://www.sss909.com/NewSportsbook";
        private string _LoginName = "zx1688", _LoginPWD = "cccccc123";
        
        private bool _IsRunning = false;//是否正在运行
        private List<IWebDriver> _WebDriverList = new List<IWebDriver>();
        private List<Log> _LogWinList = new List<Log>();

        private bool _ZP_AllDay = false;

        private int _ZP_MS = 10000;//早盘抓取间隔，毫秒
        private int _JR_MS = 10000;//今日赛事抓取间隔，毫秒
        private int _GQ_MS = 10000;//滚球抓取间隔，毫秒
        //private int _NODATA_MS = 10000;//无数据时刷新间隔

        private int _WebDriver_MS = 50;//判断元素时需等待的时间

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

        private List<B_SingleOdds_Last> Temp_B_SingleOdds_Last = new List<B_SingleOdds_Last>();
        private List<B_HandicapOdds_Last> Temp_B_HandicapOdds_Last = new List<B_HandicapOdds_Last>();
        private List<B_TotalOverUnderOdds_Last> Temp_B_TotalOverUnderOdds_Last = new List<B_TotalOverUnderOdds_Last>();
        private List<B_TotalSingleDoubleOdds_Last> Temp_B_TotalSingleDoubleOdds_Last = new List<B_TotalSingleDoubleOdds_Last>();
        private List<B_DoubleResultOdds_Last> Temp_B_DoubleResultOdds_Last = new List<B_DoubleResultOdds_Last>();
        private List<B_CorrectScoreOdds_Last> Temp_B_CorrectScoreOdds_Last = new List<B_CorrectScoreOdds_Last>();
        private List<B_OutrightOdds_Last> Temp_B_OutrightOdds_Last = new List<B_OutrightOdds_Last>();

        private void InitTempData()
        {
            A_LeagueMatchBll lmbll = new A_LeagueMatchBll();
            A_TeamBll tbll = new A_TeamBll();
            A_MatchBll mbll = new A_MatchBll();
            A_MatchScoreRecordBll msrbll = new A_MatchScoreRecordBll();

            DateTime now = lmbll.GetServerDateTime();

            this.Temp_A_LeagueMatch = lmbll.FindAll(SourcePlatformEnum.SB.ToString());
            this.Temp_A_Team=tbll.FindAll(SourcePlatformEnum.SB.ToString());
            this.Temp_A_Match = mbll.FindByDate(now.Date);
            this.Temp_A_MatchScoreRecord = msrbll.FindByDate(now.Date);

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
        }
        public Main()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key1">1：早盘，2：滚球</param>
        /// <param name="key2">1：足球</param>
        /// <param name="key3">足球{1：独赢 & 让球 & 大小 & 单/双，2：半场 / 全场，3：波胆，4：冠军}</param>
        private void GetData(string key1, string key2, string key3, Log logwin)
        {
            //var url = "https://mkt.ljb04.com/NewIndex?lang=cs";
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
                logwin.txt_log.AppendText("地址：" + _URL + "\r\n");
                logwin.txt_log.AppendText("\r\n");
                logwin.txt_log.AppendText("开始初次请求\r\n");
                logwin.txt_log.AppendText("===============================================\r\n");
            }));
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();            
            watch.Start();
            driver.Navigate().GoToUrl(_URL);
            string html = string.Empty;
            int pagecount = 1;
            bool flag_loading = true;
            try
            {
                driver.SwitchTo().Frame("sportsFrame");                
                //while (flag_loading)
                //{

                //    Thread.Sleep(_NODATA_MS);
                //}
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
                this.Invoke(new MethodInvoker(delegate
                {
                    logwin.txt_log.AppendText("===============================================\r\n");
                }));
                bool flag09 = false;
                do
                {
                    flag09 = WebDriverHelper.WaitForElementInvisible(driver, By.CssSelector("#container .loading"), _WebDriver_MS);
                } while (!flag09);
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
                    string pagecountstr = driver.FindElement(By.CssSelector("#container .page")).Text;
                    pagecountstr = pagecountstr.Split('/')[1];
                    pagecountstr = pagecountstr.Substring(0, pagecountstr.Length - 1);
                    int.TryParse(pagecountstr, out pagecount);
                    for (int i = 2; i <= pagecount; i++)
                    {
                        //翻页
                        driver.FindElement(By.CssSelector("#container .pagination .dropdown")).Click();
                        WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#container .pagination .dropdown>ul"), _WebDriver_MS);
                        driver.FindElement(By.CssSelector("#container .pagination .dropdown>ul>li:nth-child(" + i + ")")).Click();
                        html += "$************************$" + driver.FindElement(By.CssSelector("#container .match-container")).GetAttribute("innerHTML");
                    }
                    this.Invoke(new MethodInvoker(delegate
                    {
                        logwin.txt_log.AppendText("数据抓取完毕\r\n");
                    }));
                    //解析数据
                    AnalysisHtml(html, key1, key2, key3);
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
                driver.Quit();
                this.Invoke(new MethodInvoker(delegate
                {
                    logwin.txt_log.AppendText("异常：" + ex.Message + "\r\n");
                }));
                return;
            }

            int count = 1;
            Task.Run(async () =>
            {
                while (_IsRunning)
                {
                    await Task.Delay(awaitms);
                    try
                    {
                        count++;
                        if (!flag_loading)
                        {
                            //先切回第1页
                            driver.FindElement(By.CssSelector("#container .pagination .dropdown")).Click();
                            WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#container .pagination .dropdown>ul"), _WebDriver_MS);
                            driver.FindElement(By.CssSelector("#container .pagination .dropdown>ul>li:nth-child(1)")).Click();
                        }                        
                        this.Invoke(new MethodInvoker(delegate
                        {
                            logwin.txt_log.AppendText("\r\n开始第" + count + "次请求\r\n");
                            logwin.txt_log.AppendText("===============================================\r\n");
                        }));
                        watch.Restart();
                        //通过Selenium驱动点击页面的刷新按钮
                        driver.FindElement(By.CssSelector("#container .btn-toolbar .icon-refresh")).Click();
                        //判断ajax刷新是否完成                        
                        bool flag10 = true;
                        do
                        {
                            flag10 = WebDriverHelper.WaitForElementHasClass(driver, By.CssSelector("#container .btn-toolbar>li>a.btn"), "disable", _WebDriver_MS);
                        } while (flag10);
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
                            continue;
                        }
                        //第一页数据
                        html = driver.FindElement(By.CssSelector("#container .match-container")).GetAttribute("innerHTML");
                        string pagecountstr = driver.FindElement(By.CssSelector("#container .page")).Text;
                        pagecountstr = pagecountstr.Split('/')[1];
                        pagecountstr = pagecountstr.Substring(0, pagecountstr.Length - 1);
                        int.TryParse(pagecountstr, out pagecount);
                        for (int i = 2; i <= pagecount; i++)
                        {
                            //翻页
                            driver.FindElement(By.CssSelector("#container .pagination .dropdown")).Click();
                            bool flag11 = false;
                            do
                            {
                                flag11 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#container .pagination .dropdown>ul"), _WebDriver_MS);
                            } while (!flag11);                            
                            driver.FindElement(By.CssSelector("#container .pagination .dropdown>ul>li:nth-child(" + i + ")")).Click();
                            html += "$************************$" + driver.FindElement(By.CssSelector("#container .match-container")).GetAttribute("innerHTML");
                        }
                        this.Invoke(new MethodInvoker(delegate
                        {
                            logwin.txt_log.AppendText("数据抓取完毕\r\n");
                        }));
                        //解析数据
                        AnalysisHtml(html, key1, key2, key3);
                        watch.Stop();
                        this.Invoke(new MethodInvoker(delegate
                        {
                            logwin.txt_log.AppendText("===============================================\r\n");
                            logwin.txt_log.AppendText("第" + count + "次耗时：" + watch.Elapsed.TotalSeconds + "秒\r\n");
                        }));
                    }
                    catch (Exception ex)
                    {
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
        private void AnalysisHtml(string html, string key1, string key2, string key3)
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
            if (_IsRunning)
            {
                Transformation(lmList);
            }
        }
        private void Transformation(List<LeagueMatch> lmList)
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
                            SportsType = SportsTypeEnum.Foodball.ToString()
                        };                        
                        msg = lmbll.Create(objlm);
                        if (string.IsNullOrEmpty(msg))
                        {
                            this.Temp_A_LeagueMatch.Add(objlm);
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(objlm.Season) && !string.IsNullOrEmpty(sj) && objlm.Season != sj)
                        {
                            var oldlm = (A_LeagueMatch)Utility.DeepCopy(objlm);
                            objlm.Season = sj;
                            objlm.ModifyTime = now;
                            msg = lmbll.Update(objlm);
                            if (string.IsNullOrEmpty(msg))
                            {
                                this.Temp_A_LeagueMatch.Remove(oldlm);
                                this.Temp_A_LeagueMatch.Add(objlm);
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
                                    SportsType = SportsTypeEnum.Foodball.ToString()
                                };
                                msg = tbll.Create(objt);
                                if (string.IsNullOrEmpty(msg))
                                {
                                    this.Temp_A_Team.Add(objt);
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
                                var lastoo = this.Temp_B_OutrightOdds_Last.Where(x => x.TeamID == objt.ID).FirstOrDefault();
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
                                        this.Temp_B_OutrightOdds_Last.Add(objoo_last);
                                    }
                                    msg = oobll.Create(objoo);
                                }
                                else
                                {
                                    if (lastoo.Odds != objoo.Odds)
                                    {
                                        var oldoo = (B_OutrightOdds_Last)Utility.DeepCopy(lastoo);
                                        lastoo.Odds = objoo.Odds;
                                        lastoo.ModifyTime = now;
                                        msg = oo_lastbll.Update(lastoo);
                                        if (string.IsNullOrEmpty(msg))
                                        {
                                            this.Temp_B_OutrightOdds_Last.Remove(oldoo);
                                            this.Temp_B_OutrightOdds_Last.Add(lastoo);
                                        }
                                        msg = oobll.Create(objoo);
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                }
                else
                {
                    lm.Name = lm.Name.Trim().Trim('*');
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
                            SportsType = SportsTypeEnum.Foodball.ToString()
                        };
                        msg = lmbll.Create(objlm);
                        if (string.IsNullOrEmpty(msg))
                        {
                            this.Temp_A_LeagueMatch.Add(objlm);
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
                                        SportsType = SportsTypeEnum.Foodball.ToString()
                                    };
                                    msg1 = tbll.Create(objtH);
                                    if (string.IsNullOrEmpty(msg1))
                                    {
                                        this.Temp_A_Team.Add(objtH);
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
                                        SportsType = SportsTypeEnum.Foodball.ToString()
                                    };
                                    msg2 = tbll.Create(objtV);
                                    if (string.IsNullOrEmpty(msg2))
                                    {
                                        this.Temp_A_Team.Add(objtV);
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
                                            A_Match objm = this.Temp_A_Match.Where(x => x.HomeTeamID == objtH.ID && x.VisitingTeamID == objtV.ID && x.SP_GameStartTime >= bsdate && x.SP_GameStartTime < bsdate.AddDays(1)).FirstOrDefault();
                                            int hs = 0, vs = 0, ts = -1;
                                            int.TryParse(m.HomeTeamScore, out hs);
                                            int.TryParse(m.VisitingTeamScore, out vs);
                                            int.TryParse(m.timing, out ts);
                                            if (objm == null)
                                            {
                                                objm = new A_Match()
                                                {
                                                    ID = Guid.NewGuid().ToString("N"),
                                                    LeagueMatchID = objlm.ID,
                                                    SportsType = SportsTypeEnum.Foodball.ToString(),
                                                    HomeTeamID = objtH.ID,
                                                    VisitingTeamID = objtV.ID,
                                                    SourcePlatform = SourcePlatformEnum.SB.ToString(),
                                                    SP_GameStartTime = bsdate,
                                                    HomeTeamScore = hs,
                                                    VisitingTeamScore = vs,
                                                    IsStart = ts < 0 ? "0" : "1",
                                                    Timing = ts * 60,
                                                    IsEnd = "0",
                                                    ExistLive = "1",
                                                    ModifyTime = now
                                                };
                                                msg = mbll.Create(objm);
                                                if (string.IsNullOrEmpty(msg))
                                                {
                                                    this.Temp_A_Match.Add(objm);
                                                    A_MatchScoreRecord objmsr = new A_MatchScoreRecord()
                                                    {
                                                        ID = Guid.NewGuid().ToString("N"),
                                                        MatchID = objm.ID,
                                                        HomeTeamScore = hs,
                                                        VisitingTeamScore = vs,
                                                        Timing = ts,
                                                        CreateTime = now
                                                    };
                                                    if (m.halftype == "1H")
                                                    {
                                                        objmsr.MatchType = MatchTypeEnum_Football.Firsthalf.ToString();
                                                    }
                                                    else if (m.halftype == "2H")
                                                    {
                                                        objmsr.MatchType = MatchTypeEnum_Football.Secondhalf.ToString();
                                                    }
                                                    msg = msrbll.Create(objmsr);
                                                    if (string.IsNullOrEmpty(msg))
                                                    {
                                                        this.Temp_A_MatchScoreRecord.Add(objmsr);
                                                    }
                                                }
                                            }
                                            else//更新
                                            {
                                                var oldm = (A_Match)Utility.DeepCopy(objm);
                                                objm.HomeTeamScore = hs;
                                                objm.VisitingTeamScore = vs;
                                                objm.IsStart = ts < 0 ? "0" : "1";
                                                objm.Timing = ts * 60;
                                                objm.IsEnd = "0";
                                                msg = mbll.Update(objm);
                                                if (string.IsNullOrEmpty(msg))
                                                {
                                                    this.Temp_A_Match.Remove(oldm);
                                                    this.Temp_A_Match.Add(objm);
                                                }
                                            }
                                            if (string.IsNullOrEmpty(msg))
                                            {
                                                #region 比分记录
                                                A_MatchScoreRecord objmsr = new A_MatchScoreRecord()
                                                {
                                                    ID = Guid.NewGuid().ToString("N"),
                                                    MatchID = objm.ID,
                                                    HomeTeamScore = hs,
                                                    VisitingTeamScore = vs,
                                                    Timing = ts,
                                                    CreateTime = now
                                                };
                                                if (m.halftype == "1H")
                                                {
                                                    objmsr.MatchType = MatchTypeEnum_Football.Firsthalf.ToString();
                                                }
                                                else if (m.halftype == "2H")
                                                {
                                                    objmsr.MatchType = MatchTypeEnum_Football.Secondhalf.ToString();
                                                }
                                                //A_MatchScoreRecord lastmsr = msrbll.GetByMID(objm.ID);
                                                A_MatchScoreRecord lastmsr = this.Temp_A_MatchScoreRecord.Where(x => x.MatchID == objm.ID).FirstOrDefault();
                                                if (lastmsr != null)//比较比分
                                                {
                                                    if (lastmsr.HomeTeamScore != objmsr.HomeTeamScore || lastmsr.VisitingTeamScore != objmsr.VisitingTeamScore)
                                                    {
                                                        msg = msrbll.Create(objmsr);
                                                        if (string.IsNullOrEmpty(msg))
                                                        {
                                                            this.Temp_A_MatchScoreRecord.Add(objmsr);
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
                                                            MatchType = MatchTypeEnum_Football.Full.ToString(),
                                                            Odds_H = Utility.ObjConvertToDecimal(item.Odds_ZY),
                                                            Odds_V = Utility.ObjConvertToDecimal(item.Odds_KY),
                                                            Odds_Draw = Utility.ObjConvertToDecimal(item.Odds_HJ),
                                                            IsLive = "1",
                                                            CreateTime = now
                                                        };
                                                        //var lastso = so_lastbll.GetByMID(objm.ID, objso.MatchType, objso.IsLive);
                                                        var lastso = this.Temp_B_SingleOdds_Last.Where(x => x.MatchID == objm.ID && x.MatchType == objso.MatchType && x.IsLive == objso.IsLive).FirstOrDefault();
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
                                                                ModifyTime = now
                                                            };
                                                            msg = so_lastbll.Create(objso_last);
                                                            if (string.IsNullOrEmpty(msg))
                                                            {
                                                                this.Temp_B_SingleOdds_Last.Add(objso_last);
                                                            }
                                                            sobll.Create(objso);
                                                        }
                                                        else
                                                        {
                                                            if (lastso.Odds_H != objso.Odds_H || lastso.Odds_V != objso.Odds_V || lastso.Odds_Draw != objso.Odds_Draw)
                                                            {
                                                                var oldso = (B_SingleOdds_Last)Utility.DeepCopy(lastso);
                                                                lastso.Odds_H = objso.Odds_H;
                                                                lastso.Odds_V = objso.Odds_V;
                                                                lastso.Odds_Draw = objso.Odds_Draw;
                                                                lastso.ModifyTime = now;
                                                                msg = so_lastbll.Update(lastso);
                                                                if (string.IsNullOrEmpty(msg))
                                                                {
                                                                    this.Temp_B_SingleOdds_Last.Remove(oldso);
                                                                    this.Temp_B_SingleOdds_Last.Add(lastso);
                                                                }
                                                                sobll.Create(objso);
                                                            }
                                                        }
                                                        #endregion

                                                        #region 让球
                                                        B_HandicapOdds objho = new B_HandicapOdds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            MatchID = objm.ID,
                                                            MatchType = MatchTypeEnum_Football.Full.ToString(),
                                                            Odds_H = Utility.ObjConvertToDecimal(item.Odds_RQZY),
                                                            Odds_V = Utility.ObjConvertToDecimal(item.Odds_RQKY),
                                                            IsLive = "1",
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
                                                        var lastho = this.Temp_B_HandicapOdds_Last.Where(x => x.MatchID == objm.ID && x.MatchType == objho.MatchType && x.PointSpread == objho.PointSpread && x.PS_Number1 == objho.PS_Number1 && x.PS_Number2 == objho.PS_Number2 && x.IsLive == objho.IsLive).FirstOrDefault();
                                                        if (lastho == null)
                                                        {
                                                            B_HandicapOdds_Last objho_last = new B_HandicapOdds_Last()
                                                            {
                                                                ID = Guid.NewGuid().ToString("N"),
                                                                MatchID = objm.ID,
                                                                MatchType = objho.MatchType,
                                                                PointSpread = objho.PointSpread,
                                                                PS_Number1 = objho.PS_Number1,
                                                                PS_Number2 = objho.PS_Number2,
                                                                Odds_H = objho.Odds_H,
                                                                Odds_V = objho.Odds_V,
                                                                IsLive = objho.IsLive,
                                                                ModifyTime = now
                                                            };
                                                            msg = ho_lastbll.Create(objho_last);
                                                            if (string.IsNullOrEmpty(msg))
                                                            {
                                                                this.Temp_B_HandicapOdds_Last.Add(objho_last);
                                                            }
                                                            hobll.Create(objho);
                                                        }
                                                        else
                                                        {
                                                            if (lastho.Odds_H != objho.Odds_H || lastho.Odds_V != objho.Odds_V)
                                                            {
                                                                var oldho = (B_HandicapOdds_Last)Utility.DeepCopy(lastho);
                                                                lastho.Odds_H = objho.Odds_H;
                                                                lastho.Odds_V = objho.Odds_V;
                                                                lastho.ModifyTime = now;
                                                                msg = ho_lastbll.Update(lastho);
                                                                if (string.IsNullOrEmpty(msg))
                                                                {
                                                                    this.Temp_B_HandicapOdds_Last.Remove(oldho);
                                                                    this.Temp_B_HandicapOdds_Last.Add(lastho);
                                                                }

                                                                hobll.Create(objho);
                                                            }
                                                        }
                                                        #endregion

                                                        #region 大小球
                                                        B_TotalOverUnderOdds objtouo = new B_TotalOverUnderOdds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            MatchID = objm.ID,
                                                            MatchType = MatchTypeEnum_Football.Full.ToString(),
                                                            Odds_Over = Utility.ObjConvertToDecimal(item.Odds_DQ),
                                                            Odds_Under = Utility.ObjConvertToDecimal(item.Odds_XQ),
                                                            IsLive = "1",
                                                            CreateTime = now
                                                        };
                                                        string dxq = string.Empty;
                                                        if (!string.IsNullOrEmpty(item.Text_DQ.Trim()))
                                                        {
                                                            dxq = item.Text_DQ.Replace("大", "").Trim();
                                                        }
                                                        if (rq.Contains("/"))
                                                        {
                                                            string[] arr = rq.Split('/');
                                                            objtouo.Goals1 = Utility.ObjConvertToDecimal(arr[0].Trim());
                                                            objtouo.Goals2 = Utility.ObjConvertToDecimal(arr[1].Trim());
                                                        }
                                                        else
                                                        {
                                                            objtouo.Goals1 = Utility.ObjConvertToDecimal(rq);
                                                        }
                                                        //var lasttouo = touo_lastbll.GetByMID(objm.ID, objtouo.MatchType, objtouo.Goals1, objtouo.Goals2, objtouo.IsLive);
                                                        var lasttouo = this.Temp_B_TotalOverUnderOdds_Last.Where(x => x.MatchID == objm.ID && x.MatchType == objtouo.MatchType && x.Goals1 == objtouo.Goals1 && x.Goals2 == objtouo.Goals2 && x.IsLive == objtouo.IsLive).FirstOrDefault();
                                                        if (lasttouo == null)
                                                        {
                                                            B_TotalOverUnderOdds_Last objtouo_last = new B_TotalOverUnderOdds_Last()
                                                            {
                                                                ID = Guid.NewGuid().ToString("N"),
                                                                MatchID = objm.ID,
                                                                MatchType = objtouo.MatchType,
                                                                Goals1 = objtouo.Goals1,
                                                                Goals2 = objtouo.Goals2,
                                                                Odds_Over = objtouo.Odds_Over,
                                                                Odds_Under = objtouo.Odds_Under,
                                                                IsLive = objtouo.IsLive,
                                                                ModifyTime = now
                                                            };
                                                            msg = touo_lastbll.Create(objtouo_last);
                                                            if (string.IsNullOrEmpty(msg))
                                                            {
                                                                this.Temp_B_TotalOverUnderOdds_Last.Add(objtouo_last);
                                                            }
                                                            touobll.Create(objtouo);
                                                        }
                                                        else
                                                        {
                                                            if (lasttouo.Odds_Over != objtouo.Odds_Over || lasttouo.Odds_Under != objtouo.Odds_Under)
                                                            {
                                                                var oldtouo = (B_TotalOverUnderOdds_Last)Utility.DeepCopy(lasttouo);
                                                                lasttouo.Odds_Over = objtouo.Odds_Over;
                                                                lasttouo.Odds_Under = objtouo.Odds_Under;
                                                                lastho.ModifyTime = now;
                                                                msg = touo_lastbll.Update(lasttouo);
                                                                if (string.IsNullOrEmpty(msg))
                                                                {
                                                                    this.Temp_B_TotalOverUnderOdds_Last.Remove(oldtouo);
                                                                    this.Temp_B_TotalOverUnderOdds_Last.Add(lasttouo);
                                                                }

                                                                touobll.Create(objtouo);
                                                            }
                                                        }
                                                        #endregion

                                                        #region 单双
                                                        B_TotalSingleDoubleOdds objtsdo = new B_TotalSingleDoubleOdds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            MatchID = objm.ID,
                                                            MatchType = MatchTypeEnum_Football.Full.ToString(),
                                                            Odds_Single = Utility.ObjConvertToDecimal(item.Odds_D),
                                                            Odds_Double = Utility.ObjConvertToDecimal(item.Odds_S),
                                                            IsLive = "1",
                                                            CreateTime = now
                                                        };
                                                        //var lasttsdo = tsdo_lastbll.GetByMID(objm.ID, objtsdo.MatchType, "0");
                                                        var lasttsdo = this.Temp_B_TotalSingleDoubleOdds_Last.Where(x => x.MatchID == objm.ID && x.MatchType == objtsdo.MatchType && x.IsLive == objtsdo.IsLive).FirstOrDefault();
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
                                                                ModifyTime = now
                                                            };
                                                            msg = tsdo_lastbll.Create(objtsdo_last);
                                                            if (string.IsNullOrEmpty(msg))
                                                            {
                                                                this.Temp_B_TotalSingleDoubleOdds_Last.Add(objtsdo_last);
                                                            }
                                                            tsdobll.Create(objtsdo);
                                                        }
                                                        else
                                                        {
                                                            if (lasttsdo.Odds_Single != objtsdo.Odds_Single || lasttsdo.Odds_Double != objtsdo.Odds_Double)
                                                            {
                                                                var oldtsdo = (B_TotalSingleDoubleOdds_Last)Utility.DeepCopy(lasttsdo);
                                                                lasttsdo.Odds_Single = objtsdo.Odds_Single;
                                                                lasttsdo.Odds_Double = objtsdo.Odds_Double;
                                                                lasttsdo.ModifyTime = now;
                                                                msg = tsdo_lastbll.Update(lasttsdo);
                                                                if (string.IsNullOrEmpty(msg))
                                                                {
                                                                    this.Temp_B_TotalSingleDoubleOdds_Last.Remove(oldtsdo);
                                                                    this.Temp_B_TotalSingleDoubleOdds_Last.Add(lasttsdo);
                                                                }

                                                                tsdobll.Create(objtsdo);
                                                            }
                                                        }
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
                                                            MatchType = MatchTypeEnum_Football.Firsthalf.ToString(),
                                                            Odds_H = Utility.ObjConvertToDecimal(item.Odds_ZY),
                                                            Odds_V = Utility.ObjConvertToDecimal(item.Odds_KY),
                                                            Odds_Draw = Utility.ObjConvertToDecimal(item.Odds_HJ),
                                                            IsLive = "1",
                                                            CreateTime = now
                                                        };
                                                        //var lastso = so_lastbll.GetByMID(objm.ID, objso.MatchType, objso.IsLive);
                                                        var lastso = this.Temp_B_SingleOdds_Last.Where(x => x.MatchID == objm.ID && x.MatchType == objso.MatchType && x.IsLive == objso.IsLive).FirstOrDefault();
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
                                                                ModifyTime = now
                                                            };
                                                            msg = so_lastbll.Create(objso_last);
                                                            if (string.IsNullOrEmpty(msg))
                                                            {
                                                                this.Temp_B_SingleOdds_Last.Add(objso_last);
                                                            }
                                                            sobll.Create(objso);
                                                        }
                                                        else
                                                        {
                                                            if (lastso.Odds_H != objso.Odds_H || lastso.Odds_V != objso.Odds_V || lastso.Odds_Draw != objso.Odds_Draw)
                                                            {
                                                                var oldso = (B_SingleOdds_Last)Utility.DeepCopy(lastso);
                                                                lastso.Odds_H = objso.Odds_H;
                                                                lastso.Odds_V = objso.Odds_V;
                                                                lastso.Odds_Draw = objso.Odds_Draw;
                                                                lastso.ModifyTime = now;
                                                                msg = so_lastbll.Update(lastso);
                                                                if (string.IsNullOrEmpty(msg))
                                                                {
                                                                    this.Temp_B_SingleOdds_Last.Remove(oldso);
                                                                    this.Temp_B_SingleOdds_Last.Add(lastso);
                                                                }

                                                                sobll.Create(objso);
                                                            }
                                                        }
                                                        #endregion

                                                        #region 让球
                                                        B_HandicapOdds objho = new B_HandicapOdds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            MatchID = objm.ID,
                                                            MatchType = MatchTypeEnum_Football.Firsthalf.ToString(),
                                                            Odds_H = Utility.ObjConvertToDecimal(item.Odds_RQZY),
                                                            Odds_V = Utility.ObjConvertToDecimal(item.Odds_RQKY),
                                                            IsLive = "1",
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
                                                        var lastho = this.Temp_B_HandicapOdds_Last.Where(x => x.MatchID == objm.ID && x.MatchType == objho.MatchType && x.PointSpread == objho.PointSpread && x.PS_Number1 == objho.PS_Number1 && x.PS_Number2 == objho.PS_Number2 && x.IsLive == objho.IsLive).FirstOrDefault();
                                                        if (lastho == null)
                                                        {
                                                            B_HandicapOdds_Last objho_last = new B_HandicapOdds_Last()
                                                            {
                                                                ID = Guid.NewGuid().ToString("N"),
                                                                MatchID = objm.ID,
                                                                MatchType = objho.MatchType,
                                                                PointSpread = objho.PointSpread,
                                                                PS_Number1 = objho.PS_Number1,
                                                                PS_Number2 = objho.PS_Number2,
                                                                Odds_H = objho.Odds_H,
                                                                Odds_V = objho.Odds_V,
                                                                IsLive = objho.IsLive,
                                                                ModifyTime = now
                                                            };
                                                            msg = ho_lastbll.Create(objho_last);
                                                            if (string.IsNullOrEmpty(msg))
                                                            {
                                                                this.Temp_B_HandicapOdds_Last.Add(objho_last);
                                                            }
                                                            hobll.Create(objho);
                                                        }
                                                        else
                                                        {
                                                            if (lastho.Odds_H != objho.Odds_H || lastho.Odds_V != objho.Odds_V)
                                                            {
                                                                var oldho = (B_HandicapOdds_Last)Utility.DeepCopy(lastho);
                                                                lastho.Odds_H = objho.Odds_H;
                                                                lastho.Odds_V = objho.Odds_V;
                                                                lastho.ModifyTime = now;
                                                                msg = ho_lastbll.Update(lastho);
                                                                if (string.IsNullOrEmpty(msg))
                                                                {
                                                                    this.Temp_B_HandicapOdds_Last.Remove(oldho);
                                                                    this.Temp_B_HandicapOdds_Last.Add(lastho);
                                                                }

                                                                hobll.Create(objho);
                                                            }
                                                        }
                                                        #endregion

                                                        #region 大小球
                                                        B_TotalOverUnderOdds objtouo = new B_TotalOverUnderOdds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            MatchID = objm.ID,
                                                            MatchType = MatchTypeEnum_Football.Firsthalf.ToString(),
                                                            Odds_Over = Utility.ObjConvertToDecimal(item.Odds_DQ),
                                                            Odds_Under = Utility.ObjConvertToDecimal(item.Odds_XQ),
                                                            IsLive = "1",
                                                            CreateTime = now
                                                        };
                                                        string dxq = string.Empty;
                                                        if (!string.IsNullOrEmpty(item.Text_DQ.Trim()))
                                                        {
                                                            dxq = item.Text_DQ.Replace("大", "").Trim();
                                                        }
                                                        if (rq.Contains("/"))
                                                        {
                                                            string[] arr = rq.Split('/');
                                                            objtouo.Goals1 = Utility.ObjConvertToDecimal(arr[0].Trim());
                                                            objtouo.Goals2 = Utility.ObjConvertToDecimal(arr[1].Trim());
                                                        }
                                                        else
                                                        {
                                                            objtouo.Goals1 = Utility.ObjConvertToDecimal(rq);
                                                        }
                                                        //var lasttouo = touo_lastbll.GetByMID(objm.ID, objtouo.MatchType, objtouo.Goals1, objtouo.Goals2, "0");
                                                        var lasttouo = this.Temp_B_TotalOverUnderOdds_Last.Where(x => x.MatchID == objm.ID && x.MatchType == objtouo.MatchType && x.Goals1 == objtouo.Goals1 && x.Goals2 == objtouo.Goals2 && x.IsLive == objtouo.IsLive).FirstOrDefault();
                                                        if (lasttouo == null)
                                                        {
                                                            B_TotalOverUnderOdds_Last objtouo_last = new B_TotalOverUnderOdds_Last()
                                                            {
                                                                ID = Guid.NewGuid().ToString("N"),
                                                                MatchID = objm.ID,
                                                                MatchType = objtouo.MatchType,
                                                                Goals1 = objtouo.Goals1,
                                                                Goals2 = objtouo.Goals2,
                                                                Odds_Over = objtouo.Odds_Over,
                                                                Odds_Under = objtouo.Odds_Under,
                                                                IsLive = objtouo.IsLive,
                                                                ModifyTime = now
                                                            };
                                                            msg = touo_lastbll.Create(objtouo_last);
                                                            if (string.IsNullOrEmpty(msg))
                                                            {
                                                                this.Temp_B_TotalOverUnderOdds_Last.Add(objtouo_last);
                                                            }
                                                            touobll.Create(objtouo);
                                                        }
                                                        else
                                                        {
                                                            if (lasttouo.Odds_Over != objtouo.Odds_Over || lasttouo.Odds_Under != objtouo.Odds_Under)
                                                            {
                                                                var oldtouo = (B_TotalOverUnderOdds_Last)Utility.DeepCopy(lasttouo);
                                                                lasttouo.Odds_Over = objtouo.Odds_Over;
                                                                lasttouo.Odds_Under = objtouo.Odds_Under;
                                                                lastho.ModifyTime = now;
                                                                msg = touo_lastbll.Update(lasttouo);
                                                                if (string.IsNullOrEmpty(msg))
                                                                {
                                                                    this.Temp_B_TotalOverUnderOdds_Last.Remove(oldtouo);
                                                                    this.Temp_B_TotalOverUnderOdds_Last.Add(lasttouo);
                                                                }

                                                                touobll.Create(objtouo);
                                                            }
                                                        }
                                                        #endregion

                                                        #region 单双
                                                        B_TotalSingleDoubleOdds objtsdo = new B_TotalSingleDoubleOdds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            MatchID = objm.ID,
                                                            MatchType = MatchTypeEnum_Football.Firsthalf.ToString(),
                                                            Odds_Single = Utility.ObjConvertToDecimal(item.Odds_D),
                                                            Odds_Double = Utility.ObjConvertToDecimal(item.Odds_S),
                                                            IsLive = "1",
                                                            CreateTime = now
                                                        };
                                                        //var lasttsdo = tsdo_lastbll.GetByMID(objm.ID, objso.MatchType, "0");
                                                        var lasttsdo = this.Temp_B_TotalSingleDoubleOdds_Last.Where(x => x.MatchID == objm.ID && x.MatchType == objtsdo.MatchType && x.IsLive == objtsdo.IsLive).FirstOrDefault();
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
                                                                ModifyTime = now
                                                            };
                                                            msg = tsdo_lastbll.Create(objtsdo_last);
                                                            if (string.IsNullOrEmpty(msg))
                                                            {
                                                                this.Temp_B_TotalSingleDoubleOdds_Last.Add(objtsdo_last);
                                                            }
                                                            tsdobll.Create(objtsdo);
                                                        }
                                                        else
                                                        {
                                                            if (lasttsdo.Odds_Single != objtsdo.Odds_Single || lasttsdo.Odds_Double != objtsdo.Odds_Double)
                                                            {
                                                                var oldtsdo = (B_TotalSingleDoubleOdds_Last)Utility.DeepCopy(lasttsdo);
                                                                lasttsdo.Odds_Single = objtsdo.Odds_Single;
                                                                lasttsdo.Odds_Double = objtsdo.Odds_Double;
                                                                lasttsdo.ModifyTime = now;
                                                                msg = tsdo_lastbll.Update(lasttsdo);
                                                                if (string.IsNullOrEmpty(msg))
                                                                {
                                                                    this.Temp_B_TotalSingleDoubleOdds_Last.Remove(oldtsdo);
                                                                    this.Temp_B_TotalSingleDoubleOdds_Last.Add(lasttsdo);
                                                                }

                                                                tsdobll.Create(objtsdo);
                                                            }
                                                        }
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
                                                            CreateTime = now
                                                        };
                                                        if (item.type == 1)
                                                        {
                                                            objcso.MatchType = MatchTypeEnum_Football.Full.ToString();
                                                        }
                                                        else if (item.type == 2)
                                                        {
                                                            objcso.MatchType = MatchTypeEnum_Football.Firsthalf.ToString();
                                                        }
                                                        else if (item.type == 3)
                                                        {
                                                            objcso.MatchType = MatchTypeEnum_Football.Secondhalf.ToString();
                                                        }
                                                        //var lastcso = cso_lastbll.GetByMID(objm.ID, objcso.MatchType, objcso.HomeTeamScore, objcso.VisitingTeamScore, objcso.IsLive);
                                                        var lastcso = this.Temp_B_CorrectScoreOdds_Last.Where(x => x.MatchID == objm.ID && x.MatchType == objcso.MatchType && x.HomeTeamScore == objcso.HomeTeamScore && x.VisitingTeamScore == objcso.VisitingTeamScore && x.IsLive == objcso.IsLive).FirstOrDefault();
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
                                                                IsLive = "1",
                                                                ModifyTime = now
                                                            };
                                                            msg = cso_lastbll.Create(objcso_last);
                                                            if (string.IsNullOrEmpty(msg))
                                                            {
                                                                this.Temp_B_CorrectScoreOdds_Last.Add(objcso_last);
                                                            }
                                                            csobll.Create(objcso);
                                                        }
                                                        else
                                                        {
                                                            if (lastcso.Odds != objcso.Odds)
                                                            {
                                                                var oldcso = (B_CorrectScoreOdds_Last)Utility.DeepCopy(lastcso);
                                                                lastcso.Odds = objcso.Odds;
                                                                lastcso.ModifyTime = now;
                                                                msg = cso_lastbll.Update(lastcso);
                                                                if (string.IsNullOrEmpty(msg))
                                                                {
                                                                    this.Temp_B_CorrectScoreOdds_Last.Remove(oldcso);
                                                                    this.Temp_B_CorrectScoreOdds_Last.Add(lastcso);
                                                                }

                                                                csobll.Create(objcso);
                                                            }
                                                        }
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
                                            A_Match objm = this.Temp_A_Match.Where(x => x.HomeTeamID == objtH.ID && x.VisitingTeamID == objtV.ID && x.SP_GameStartTime == bstime).FirstOrDefault();
                                            int hs = 0, vs = 0;
                                            int.TryParse(m.HomeTeamScore, out hs);
                                            int.TryParse(m.VisitingTeamScore, out vs);
                                            if (objm == null)
                                            {
                                                objm = new A_Match()
                                                {
                                                    ID = Guid.NewGuid().ToString("N"),
                                                    LeagueMatchID = objlm.ID,
                                                    SportsType = SportsTypeEnum.Foodball.ToString(),
                                                    HomeTeamID = objtH.ID,
                                                    VisitingTeamID = objtV.ID,
                                                    SourcePlatform = SourcePlatformEnum.SB.ToString(),
                                                    SP_GameStartTime = bstime,
                                                    ExistLive = string.IsNullOrEmpty(m.GQ) ? "0" : "1",
                                                    ModifyTime = now
                                                };
                                                msg = mbll.Create(objm);
                                                if (string.IsNullOrEmpty(msg))
                                                {
                                                    this.Temp_A_Match.Add(objm);
                                                }
                                            }
                                            else//更新
                                            {
                                                var oldm = (A_Match)Utility.DeepCopy(objm);
                                                objm.SP_GameStartTime = bstime;
                                                objm.ExistLive = string.IsNullOrEmpty(m.GQ) ? "0" : "1";
                                                msg = mbll.Update(objm);
                                                if (string.IsNullOrEmpty(msg))
                                                {
                                                    this.Temp_A_Match.Remove(oldm);
                                                    this.Temp_A_Match.Add(objm);
                                                }
                                            }
                                            if (string.IsNullOrEmpty(msg))
                                            {
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
                                                            MatchType = MatchTypeEnum_Football.Full.ToString(),
                                                            Odds_H = Utility.ObjConvertToDecimal(item.Odds_ZY),
                                                            Odds_V = Utility.ObjConvertToDecimal(item.Odds_KY),
                                                            Odds_Draw = Utility.ObjConvertToDecimal(item.Odds_HJ),
                                                            IsLive = "0",
                                                            CreateTime = now
                                                        };
                                                        var lastso = so_lastbll.GetByMID(objm.ID, objso.MatchType, objso.IsLive);   
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
                                                                ModifyTime = now
                                                            };
                                                            so_lastbll.Create(objso_last);
                                                            sobll.Create(objso);
                                                        }
                                                        else
                                                        {
                                                            if (lastso.Odds_H != objso.Odds_H || lastso.Odds_V != objso.Odds_V || lastso.Odds_Draw != objso.Odds_Draw)
                                                            {
                                                                lastso.Odds_H = objso.Odds_H;
                                                                lastso.Odds_V = objso.Odds_V;
                                                                lastso.Odds_Draw = objso.Odds_Draw;
                                                                lastso.ModifyTime = now;
                                                                so_lastbll.Update(lastso);

                                                                sobll.Create(objso);
                                                            }
                                                        }
                                                        #endregion

                                                        #region 让球
                                                        B_HandicapOdds objho = new B_HandicapOdds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            MatchID = objm.ID,
                                                            MatchType = MatchTypeEnum_Football.Full.ToString(),
                                                            Odds_H = Utility.ObjConvertToDecimal(item.Odds_RQZY),
                                                            Odds_V = Utility.ObjConvertToDecimal(item.Odds_RQKY),
                                                            IsLive = "0",
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
                                                        var lastho = ho_lastbll.GetByMID(objm.ID, objho.MatchType, objho.PointSpread, objho.PS_Number1, objho.PS_Number2, "0");
                                                        if (lastho==null)
                                                        {
                                                            B_HandicapOdds_Last objho_last = new B_HandicapOdds_Last()
                                                            {
                                                                ID = Guid.NewGuid().ToString("N"),
                                                                MatchID = objm.ID,
                                                                MatchType = objho.MatchType,
                                                                PointSpread = objho.PointSpread,
                                                                PS_Number1 = objho.PS_Number1,
                                                                PS_Number2 = objho.PS_Number2,
                                                                Odds_H = objho.Odds_H,
                                                                Odds_V = objho.Odds_V,
                                                                IsLive = objho.IsLive,
                                                                ModifyTime = now
                                                            };
                                                            ho_lastbll.Create(objho_last);
                                                            hobll.Create(objho);
                                                        }
                                                        else
                                                        {
                                                            if (lastho.Odds_H != objho.Odds_H || lastho.Odds_V != objho.Odds_V)
                                                            {
                                                                lastho.Odds_H = objho.Odds_H;
                                                                lastho.Odds_V = objho.Odds_V;
                                                                lastho.ModifyTime = now;
                                                                ho_lastbll.Update(lastho);

                                                                hobll.Create(objho);
                                                            }
                                                        }
                                                        #endregion

                                                        #region 大小球
                                                        B_TotalOverUnderOdds objtouo = new B_TotalOverUnderOdds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            MatchID = objm.ID,
                                                            MatchType = MatchTypeEnum_Football.Full.ToString(),
                                                            Odds_Over = Utility.ObjConvertToDecimal(item.Odds_DQ),
                                                            Odds_Under = Utility.ObjConvertToDecimal(item.Odds_XQ),
                                                            IsLive = "0",
                                                            CreateTime = now
                                                        };
                                                        string dxq = string.Empty;
                                                        if (!string.IsNullOrEmpty(item.Text_DQ.Trim()))
                                                        {
                                                            dxq = item.Text_DQ.Replace("大", "").Trim();
                                                        }
                                                        if (rq.Contains("/"))
                                                        {
                                                            string[] arr = rq.Split('/');
                                                            objtouo.Goals1 = Utility.ObjConvertToDecimal(arr[0].Trim());
                                                            objtouo.Goals2 = Utility.ObjConvertToDecimal(arr[1].Trim());
                                                        }
                                                        else
                                                        {
                                                            objtouo.Goals1 = Utility.ObjConvertToDecimal(rq);
                                                        }
                                                        var lasttouo = touo_lastbll.GetByMID(objm.ID, objtouo.MatchType, objtouo.Goals1, objtouo.Goals2, "0");
                                                        if (lasttouo == null)
                                                        {
                                                            B_TotalOverUnderOdds_Last objtouo_last = new B_TotalOverUnderOdds_Last()
                                                            {
                                                                ID = Guid.NewGuid().ToString("N"),
                                                                MatchID = objm.ID,
                                                                MatchType = objtouo.MatchType,
                                                                Goals1 = objtouo.Goals1,
                                                                Goals2 = objtouo.Goals2,
                                                                Odds_Over = objtouo.Odds_Over,
                                                                Odds_Under = objtouo.Odds_Under,
                                                                IsLive = objtouo.IsLive,
                                                                ModifyTime = now
                                                            };
                                                            touo_lastbll.Create(objtouo_last);
                                                            touobll.Create(objtouo);
                                                        }
                                                        else
                                                        {
                                                            if (lasttouo.Odds_Over != objtouo.Odds_Over || lasttouo.Odds_Under != objtouo.Odds_Under)
                                                            {
                                                                lasttouo.Odds_Over = objtouo.Odds_Over;
                                                                lasttouo.Odds_Under = objtouo.Odds_Under;
                                                                lastho.ModifyTime = now;
                                                                touo_lastbll.Update(lasttouo);

                                                                touobll.Create(objtouo);
                                                            }
                                                        }
                                                        #endregion

                                                        #region 单双
                                                        B_TotalSingleDoubleOdds objtsdo = new B_TotalSingleDoubleOdds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            MatchID = objm.ID,
                                                            MatchType = MatchTypeEnum_Football.Full.ToString(),
                                                            Odds_Single = Utility.ObjConvertToDecimal(item.Odds_D),
                                                            Odds_Double = Utility.ObjConvertToDecimal(item.Odds_S),
                                                            IsLive = "0",
                                                            CreateTime = now
                                                        };
                                                        var lasttsdo = tsdo_lastbll.GetByMID(objm.ID, objtsdo.MatchType, "0");
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
                                                                ModifyTime = now
                                                            };
                                                            tsdo_lastbll.Create(objtsdo_last);
                                                            tsdobll.Create(objtsdo);
                                                        }
                                                        else
                                                        {
                                                            if (lasttsdo.Odds_Single != objtsdo.Odds_Single || lasttsdo.Odds_Double != objtsdo.Odds_Double)
                                                            {
                                                                lasttsdo.Odds_Single = objtsdo.Odds_Single;
                                                                lasttsdo.Odds_Double = objtsdo.Odds_Double;
                                                                lasttsdo.ModifyTime = now;
                                                                tsdo_lastbll.Update(lasttsdo);

                                                                tsdobll.Create(objtsdo);
                                                            }
                                                        }
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
                                                            MatchType = MatchTypeEnum_Football.Firsthalf.ToString(),
                                                            Odds_H = Utility.ObjConvertToDecimal(item.Odds_ZY),
                                                            Odds_V = Utility.ObjConvertToDecimal(item.Odds_KY),
                                                            Odds_Draw = Utility.ObjConvertToDecimal(item.Odds_HJ),
                                                            IsLive = "0",
                                                            CreateTime = now
                                                        };
                                                        var lastso = so_lastbll.GetByMID(objm.ID, objso.MatchType, "0");
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
                                                                ModifyTime = now
                                                            };
                                                            so_lastbll.Create(objso_last);
                                                            sobll.Create(objso);
                                                        }
                                                        else
                                                        {
                                                            if (lastso.Odds_H != objso.Odds_H || lastso.Odds_V != objso.Odds_V || lastso.Odds_Draw != objso.Odds_Draw)
                                                            {
                                                                lastso.Odds_H = objso.Odds_H;
                                                                lastso.Odds_V = objso.Odds_V;
                                                                lastso.Odds_Draw = objso.Odds_Draw;
                                                                lastso.ModifyTime = now;
                                                                so_lastbll.Update(lastso);

                                                                sobll.Create(objso);
                                                            }
                                                        }
                                                        #endregion

                                                        #region 让球
                                                        B_HandicapOdds objho = new B_HandicapOdds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            MatchID = objm.ID,
                                                            MatchType = MatchTypeEnum_Football.Firsthalf.ToString(),
                                                            Odds_H = Utility.ObjConvertToDecimal(item.Odds_RQZY),
                                                            Odds_V = Utility.ObjConvertToDecimal(item.Odds_RQKY),
                                                            IsLive = "0",
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
                                                        var lastho = ho_lastbll.GetByMID(objm.ID, objho.MatchType, objho.PointSpread, objho.PS_Number1, objho.PS_Number2, "0");
                                                        if (lastho == null)
                                                        {
                                                            B_HandicapOdds_Last objho_last = new B_HandicapOdds_Last()
                                                            {
                                                                ID = Guid.NewGuid().ToString("N"),
                                                                MatchID = objm.ID,
                                                                MatchType = objho.MatchType,
                                                                PointSpread = objho.PointSpread,
                                                                PS_Number1 = objho.PS_Number1,
                                                                PS_Number2 = objho.PS_Number2,
                                                                Odds_H = objho.Odds_H,
                                                                Odds_V = objho.Odds_V,
                                                                IsLive = objho.IsLive,
                                                                ModifyTime = now
                                                            };
                                                            ho_lastbll.Create(objho_last);
                                                            hobll.Create(objho);
                                                        }
                                                        else
                                                        {
                                                            if (lastho.Odds_H != objho.Odds_H || lastho.Odds_V != objho.Odds_V)
                                                            {
                                                                lastho.Odds_H = objho.Odds_H;
                                                                lastho.Odds_V = objho.Odds_V;
                                                                lastho.ModifyTime = now;
                                                                ho_lastbll.Update(lastho);

                                                                hobll.Create(objho);
                                                            }
                                                        }
                                                        #endregion

                                                        #region 大小球
                                                        B_TotalOverUnderOdds objtouo = new B_TotalOverUnderOdds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            MatchID = objm.ID,
                                                            MatchType = MatchTypeEnum_Football.Firsthalf.ToString(),
                                                            Odds_Over = Utility.ObjConvertToDecimal(item.Odds_DQ),
                                                            Odds_Under = Utility.ObjConvertToDecimal(item.Odds_XQ),
                                                            IsLive = "0",
                                                            CreateTime = now
                                                        };
                                                        string dxq = string.Empty;
                                                        if (!string.IsNullOrEmpty(item.Text_DQ.Trim()))
                                                        {
                                                            dxq = item.Text_DQ.Replace("大", "").Trim();
                                                        }
                                                        if (rq.Contains("/"))
                                                        {
                                                            string[] arr = rq.Split('/');
                                                            objtouo.Goals1 = Utility.ObjConvertToDecimal(arr[0].Trim());
                                                            objtouo.Goals2 = Utility.ObjConvertToDecimal(arr[1].Trim());
                                                        }
                                                        else
                                                        {
                                                            objtouo.Goals1 = Utility.ObjConvertToDecimal(rq);
                                                        }
                                                        var lasttouo = touo_lastbll.GetByMID(objm.ID, objtouo.MatchType, objtouo.Goals1, objtouo.Goals2, "0");
                                                        if (lasttouo == null)
                                                        {
                                                            B_TotalOverUnderOdds_Last objtouo_last = new B_TotalOverUnderOdds_Last()
                                                            {
                                                                ID = Guid.NewGuid().ToString("N"),
                                                                MatchID = objm.ID,
                                                                MatchType = objtouo.MatchType,
                                                                Goals1 = objtouo.Goals1,
                                                                Goals2 = objtouo.Goals2,
                                                                Odds_Over = objtouo.Odds_Over,
                                                                Odds_Under = objtouo.Odds_Under,
                                                                IsLive = objtouo.IsLive,
                                                                ModifyTime = now
                                                            };
                                                            touo_lastbll.Create(objtouo_last);
                                                            touobll.Create(objtouo);
                                                        }
                                                        else
                                                        {
                                                            if (lasttouo.Odds_Over != objtouo.Odds_Over || lasttouo.Odds_Under != objtouo.Odds_Under)
                                                            {
                                                                lasttouo.Odds_Over = objtouo.Odds_Over;
                                                                lasttouo.Odds_Under = objtouo.Odds_Under;
                                                                lastho.ModifyTime = now;
                                                                touo_lastbll.Update(lasttouo);

                                                                touobll.Create(objtouo);
                                                            }
                                                        }
                                                        #endregion

                                                        #region 单双
                                                        B_TotalSingleDoubleOdds objtsdo = new B_TotalSingleDoubleOdds()
                                                        {
                                                            ID = Guid.NewGuid().ToString("N"),
                                                            MatchID = objm.ID,
                                                            MatchType = MatchTypeEnum_Football.Firsthalf.ToString(),
                                                            Odds_Single = Utility.ObjConvertToDecimal(item.Odds_D),
                                                            Odds_Double = Utility.ObjConvertToDecimal(item.Odds_S),
                                                            IsLive = "0",
                                                            CreateTime = now
                                                        };
                                                        var lasttsdo = tsdo_lastbll.GetByMID(objm.ID, objso.MatchType, "0");
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
                                                                ModifyTime = now
                                                            };
                                                            tsdo_lastbll.Create(objtsdo_last);
                                                            tsdobll.Create(objtsdo);
                                                        }
                                                        else
                                                        {
                                                            if (lasttsdo.Odds_Single != objtsdo.Odds_Single || lasttsdo.Odds_Double != objtsdo.Odds_Double)
                                                            {
                                                                lasttsdo.Odds_Single = objtsdo.Odds_Single;
                                                                lasttsdo.Odds_Double = objtsdo.Odds_Double;
                                                                lasttsdo.ModifyTime = now;
                                                                tsdo_lastbll.Update(lasttsdo);

                                                                tsdobll.Create(objtsdo);
                                                            }
                                                        }
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
                                                        MatchType = MatchTypeEnum_Football.Full.ToString(),
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
                                                        CreateTime = now
                                                    };
                                                    var lastdro = dro_lastbll.GetByMID(objm.ID, "0");
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
                                                            ModifyTime = now
                                                        };
                                                        dro_lastbll.Create(objdro_last);
                                                        drobll.Create(objdro);
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
                                                            lastdro.Odds_HH = objdro.Odds_HH;
                                                            lastdro.Odds_HD = objdro.Odds_HD;
                                                            lastdro.Odds_HV = objdro.Odds_HV;
                                                            lastdro.Odds_DH = objdro.Odds_DH;
                                                            lastdro.Odds_DD = objdro.Odds_DD;
                                                            lastdro.Odds_DV = objdro.Odds_DV;
                                                            lastdro.Odds_VH = objdro.Odds_VH;
                                                            lastdro.Odds_VD = objdro.Odds_VD;
                                                            lastdro.Odds_VV = objdro.Odds_VV;
                                                            lastdro.ModifyTime = now;
                                                            dro_lastbll.Update(lastdro);

                                                            drobll.Create(objdro);
                                                        }
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
                                                            IsLive = "0",
                                                            CreateTime = now
                                                        };
                                                        if (item.type == 1)
                                                        {
                                                            objcso.MatchType = MatchTypeEnum_Football.Full.ToString();
                                                        }
                                                        else if (item.type == 2)
                                                        {
                                                            objcso.MatchType = MatchTypeEnum_Football.Firsthalf.ToString();
                                                        }
                                                        else if (item.type == 3)
                                                        {
                                                            objcso.MatchType = MatchTypeEnum_Football.Secondhalf.ToString();
                                                        }
                                                        var lastcso = cso_lastbll.GetByMID(objm.ID, objcso.MatchType, objcso.HomeTeamScore, objcso.VisitingTeamScore, "0");
                                                        if (lastcso == null)
                                                        {
                                                            B_CorrectScoreOdds_Last objcso_last = new B_CorrectScoreOdds_Last()
                                                            {
                                                                ID = Guid.NewGuid().ToString("N"),
                                                                MatchID = objcso.MatchID,
                                                                MatchType= objcso.MatchType,
                                                                HomeTeamScore = objcso.HomeTeamScore,
                                                                VisitingTeamScore = objcso.VisitingTeamScore,
                                                                Odds = objcso.Odds,
                                                                IsLive = "0",
                                                                ModifyTime = now
                                                            };
                                                            cso_lastbll.Create(objcso_last);
                                                            csobll.Create(objcso);
                                                        }
                                                        else
                                                        {
                                                            if (objcso.Odds != lastcso.Odds)
                                                            {
                                                                lastcso.Odds = objcso.Odds;
                                                                lastcso.ModifyTime = now;
                                                                cso_lastbll.Update(lastcso);

                                                                csobll.Create(objcso);
                                                            }
                                                        }
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
        private void dy(B_SingleOdds objso, DateTime now, B_SingleOddsBll sobll, B_SingleOdds_LastBll so_lastbll)
        {
            string msg = string.Empty;
            var lastso = this.Temp_B_SingleOdds_Last.Where(x => x.MatchID == objso.MatchID && x.MatchType == objso.MatchType && x.IsLive == objso.IsLive).FirstOrDefault();
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
                    ModifyTime = now
                };
                msg = so_lastbll.Create(objso_last);
                if (string.IsNullOrEmpty(msg))
                {
                    this.Temp_B_SingleOdds_Last.Add(objso_last);
                }
                sobll.Create(objso);
            }
            else
            {
                if (lastso.Odds_H != objso.Odds_H || lastso.Odds_V != objso.Odds_V || lastso.Odds_Draw != objso.Odds_Draw)
                {
                    var oldso = (B_SingleOdds_Last)Utility.DeepCopy(lastso);
                    lastso.Odds_H = objso.Odds_H;
                    lastso.Odds_V = objso.Odds_V;
                    lastso.Odds_Draw = objso.Odds_Draw;
                    lastso.ModifyTime = now;
                    msg = so_lastbll.Update(lastso);
                    if (string.IsNullOrEmpty(msg))
                    {
                        this.Temp_B_SingleOdds_Last.Remove(oldso);
                        this.Temp_B_SingleOdds_Last.Add(lastso);
                    }
                    sobll.Create(objso);
                }
            }
        }
        private void rq()
        {

        }
        private void dx()
        {

        }
        private void ds()
        {

        }
        private void bd()
        {

        }
        private void bcqc()
        {

        }

        private void btn_Start_Click(object sender, EventArgs e)
        {
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
                item.Quit();
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
            if (count>0)
            {
                _IsRunning = true;
                this.btn_Start.Text = "运行中";
                this.btn_Start.Enabled = false;
                this.btn_Stop.Enabled = true;                
                this.txt_ZP_MS.Enabled = false;
                this.txt_JR_MS.Enabled = false;
                this.txt_GQ_MS.Enabled = false;
            }
            else
            {
                MessageBox.Show("请选择要抓的数据");
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

        private void chk_ZP_DRDD_CheckedChanged(object sender, EventArgs e)
        {
            MessageBox.Show("Test");
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

        private void btn_Init_Click(object sender, EventArgs e)
        {
            //用的session保存登录状态
            IWebDriver driver = WebDriverHelper.CreateChromeDriver(chk_VisibleChrome.Checked);//new ChromeDriver();
            if (driver == null)
            {
                MessageBox.Show(WebDriverHelper.ErrorMessage);
                return;
            }
            _WebDriverList.Add(driver);
            driver.Navigate().GoToUrl("https://mkt.sss988n1jssx92.info/NewIndex?webskintype=2&lang=cs");
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("window.open('about:blank','_blank');");
            var wname = driver.WindowHandles;
            return;
            //driver.SwitchTo().Frame("sportsbook").SwitchTo().Frame("sportsFrame");
            driver.SwitchTo().Frame("sportsFrame");
            string username = driver.FindElement(By.CssSelector("#sbContainer #sb-header #n1")).GetAttribute("innerHTML");
            if (!username.Contains(","))//未登录
            {
                driver.SwitchTo().DefaultContent();
                driver.Navigate().GoToUrl(_URLTYC);
                driver.FindElement(By.CssSelector("#authentication form input[name='username']")).SendKeys(_LoginName);
                driver.FindElement(By.CssSelector("#authentication form input[name='password']")).SendKeys(_LoginPWD);
                Thread.Sleep(1000);
                bool flag = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#authentication form input[type='submit']"), 50);
                if (flag)
                {
                    driver.FindElement(By.CssSelector("#authentication form input[type='submit']")).Click();
                }
                driver.SwitchTo().Frame("sportsbook").SwitchTo().Frame("sportsFrame");
                bool flag01 = false;
                do
                {
                    flag01 = WebDriverHelper.WaitForElementVisible(driver, By.CssSelector("#mainArea"), 100);
                } while (!flag01);
                Thread.Sleep(10000);
                driver.SwitchTo().DefaultContent();
                driver.Navigate().GoToUrl("https://mkt.sss988n1jssx92.info/NewIndex?webskintype=2&lang=cs");
                string strcookie = string.Empty;
                foreach (var item in driver.Manage().Cookies.AllCookies)
                {
                    strcookie += item.Name + ":" + item.Value + "\r\n";
                    if (item.Name == "ASP.NET_SessionId")
                    {
                        continue;
                    }
                }
                MessageBox.Show(strcookie);
                //IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                //js.ExecuteScript("window.open('about:blank','_blank');");
            }
            else
            {
                MessageBox.Show("已登录");
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
    }
}
