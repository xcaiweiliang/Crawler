using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Control_SB
{
    public class WebDriverHelper
    {
        public static string ErrorMessage { get; set; }
        public static IWebDriver CreateWebDriver(string browser, bool showview = false, bool maximized = false, string proxyIpAndPort = "")
        {
            if (browser == "gc")
            {
                return CreateChromeDriver(showview, maximized, proxyIpAndPort);
            }
            else if (browser == "ff")
            {
                return CreateFirefoxDriver(showview, maximized, proxyIpAndPort);
            }
            return null;
        }
        public static IWebDriver CreateChromeDriver(bool showview, bool maximized, string proxyIpAndPort)
        {
            var service = ChromeDriverService.CreateDefaultService();
            try
            {
                service.HideCommandPromptWindow = true; //隐藏 命令窗口   47.99.111.233:3808
                Proxy proxy = new Proxy();
                proxy.HttpProxy = proxyIpAndPort;
                proxy.SslProxy = proxyIpAndPort;
                proxy.FtpProxy = proxyIpAndPort;
                proxy.IsAutoDetect = false;
                if (string.IsNullOrEmpty(proxyIpAndPort))
                {
                    proxy.NoProxy = "None";
                }
                var option = new ChromeOptions();
                option.Proxy = proxy;

                //option.AddArgument("--proxy-server=http://47.99.111.233:3808");

                option.AddArgument("disable-infobars"); //隐藏 自动化标题
                //option.AddArgument("--no-sandbox"); //非沙盒
                if (maximized)
                {
                    option.AddArgument("--start-maximized"); //最大化
                }
                if (!showview)
                {
                    option.AddArgument("headless"); //隐藏 chorme浏览器
                }
                //option.AddArgument("--incognito");//隐身模式
                option.AddArgument("--ignore-certificate-errors");//https非信任站点

                //设置用户配置文件夹
                //option.AddArgument("--user-data-dir=C:\\Users\\zx\\AppData\\Local\\Google\\Chrome\\User Data\\Temp");
                //设置用户
                //option.AddArgument("--profile-directory=Profile Temp");
                var driver = new ChromeDriver(service, option, TimeSpan.FromSeconds(90));
                //设置页面加载超时时间为2分钟
                driver.Manage().Timeouts().PageLoad = TimeSpan.FromMinutes(2);
                //try
                //{

                //}
                //catch (Exception ex)
                //{
                //    driver.Close(); driver.Dispose();
                //}
                return driver;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            return null;
        }

        public static IWebDriver CreateFirefoxDriver(bool showview, bool maximized, string proxyIpAndPort)
        {
            var service = FirefoxDriverService.CreateDefaultService();
            try
            {
                service.HideCommandPromptWindow = true; //隐藏 命令窗口   47.99.111.233:3808
                Proxy proxy = new Proxy();
                proxy.HttpProxy = proxyIpAndPort;
                proxy.SslProxy = proxyIpAndPort;
                proxy.FtpProxy = proxyIpAndPort;
                proxy.IsAutoDetect = false;
                if (string.IsNullOrEmpty(proxyIpAndPort))
                {
                    proxy.NoProxy = "None";
                }
                var option = new FirefoxOptions();
                option.Proxy = proxy;

                //允许访问https非信任站点
                var profile = new FirefoxProfile(System.Windows.Forms.Application.StartupPath + "/SeleniumFirefox");
                profile.AcceptUntrustedCertificates = true;                
                profile.AssumeUntrustedCertificateIssuer = true;
                option.Profile = profile;

                option.AddArgument("-disable-gpu"); //关闭gpu加速
                
                if (!showview)
                {
                    option.AddArgument("-headless"); //隐藏 chorme浏览器
                }
                
                var driver = new FirefoxDriver(service, option, TimeSpan.FromSeconds(120));                
                //设置页面加载超时时间为2分钟
                driver.Manage().Timeouts().PageLoad = TimeSpan.FromMinutes(2);
                if (maximized)
                {
                    driver.Manage().Window.Maximize();
                }
                //try
                //{

                //}
                //catch (Exception ex)
                //{
                //    driver.Close(); driver.Dispose();
                //}
                return driver;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            return null;
        }

        /// <summary>
        /// 判断元素是否在指定时间内隐藏或者消失，消失或隐藏返回true
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="by"></param>
        /// <param name="milliseconds"></param>
        /// <returns>消失或隐藏返回true</returns>
        public static bool WaitForElementInvisible(IWebDriver driver, By by, int milliseconds)
        {
            try
            {
                var el = new WebDriverWait(driver, TimeSpan.FromMilliseconds(milliseconds)).Until<IWebElement>((d) =>
                {
                    IWebElement element = driver.FindElement(by);
                    if (element != null && element.Displayed == true)
                    {
                        return element;
                    }
                    return null;
                });
                if (el == null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                return true;
            }
        }
        /// <summary>
        /// 判断元素是否在指定时间内存在并显示，存在并显示true
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="by"></param>
        /// <param name="milliseconds"></param>
        /// <returns>存在并显示true</returns>
        public static bool WaitForElementVisible(IWebDriver driver, By by, int milliseconds)
        {
            try
            {
                var el = new WebDriverWait(driver, TimeSpan.FromMilliseconds(milliseconds)).Until<IWebElement>((d) =>
                {
                    IWebElement element = driver.FindElement(by);
                    if (element != null && element.Displayed == true)
                    {
                        return element;
                    }
                    return null;
                });
                if (el == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        /// <summary>
        /// 判断元素是否在指定时间内包含指定class，包含指定class true
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="by"></param>
        /// <param name="milliseconds"></param>
        /// <returns>包含指定class true</returns>
        public static bool WaitForElementHasClass(IWebDriver driver, By by, string classname, int milliseconds)
        {
            try
            {
                var el = new WebDriverWait(driver, TimeSpan.FromMilliseconds(milliseconds)).Until<IWebElement>((d) =>
                {
                    IWebElement element = driver.FindElement(by);
                    if (element != null && element.GetAttribute("class").Contains(classname))
                    {
                        return element;
                    }
                    return null;
                });
                if (el == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        /// <summary>
        /// 判断元素是否存在
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="by"></param>
        /// <returns>存在并显示true</returns>
        public static bool ElementExist(IWebElement element, By by)
        {
            try
            {
                var el = element.FindElement(by);
                if (el == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        /// <summary>
        /// 判断是否存在alert弹框
        /// </summary>
        /// <param name="driver"></param>
        /// <returns>存在true</returns>
        public static bool AlertExist(IWebDriver driver)
        {
            try
            {
                driver.SwitchTo().Alert();
                return true;
            }
            catch (NoAlertPresentException e)
            {
                return false;
            }
        }
    }
}
