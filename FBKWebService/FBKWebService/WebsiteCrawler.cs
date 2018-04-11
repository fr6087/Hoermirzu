using OpenQA.Selenium;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;


namespace FBKWebService
{
    /// <summary>
    /// this class is opening a connection to the web form. I provides methods for retreiving the HTML-Code behind a spechific form.
    /// </summary>
    public class WebsiteCrawler
    {
        static PhantomJSDriver persistingPhantomDriver = new PhantomJSDriver();

        public string GetWebContentPersist(string url, string finishedCondition = null)
        {
            string finishedCond = "document.readyState == 'complete'";

            if (finishedCondition != null)
            {
                finishedCond += " && " + finishedCondition;
            }

            persistingPhantomDriver.Navigate().GoToUrl(url);

            IWait<IWebDriver> wait = new OpenQA.Selenium.Support.UI.WebDriverWait(persistingPhantomDriver, TimeSpan.FromSeconds(30.00));
            //throws WebDriverTimeoutException if password or user name were not valid for online form
            try
            {
                wait.Until(driver1 => ((IJavaScriptExecutor)persistingPhantomDriver).ExecuteScript("return (" + finishedCond + ")").Equals(true));
            }
            catch (WebDriverException e)
            {
                //this is thrown after timeout if username +password was not correct
                return "error. " + e.Message;
            }
            catch (InvalidOperationException e)
            {
                return "error. Form with the sessioncookie is not available. "+e.Message;
            }
            return persistingPhantomDriver.PageSource;
        }
        /// <summary>
        /// same thing with cookies, but it doesn't work
        /// todo debug
        /// </summary>
        /// <param name="url"></param>
        /// <param name="finishedCondition"></param>
        /// <param name="cookies"></param>
        /// <returns></returns>
        public string GetWebContent(string url, string finishedCondition = null, List<Cookie> cookies = null)
        {
            using (IWebDriver phantomDriver = new PhantomJSDriver())
            {
                string finishedCond = "document.readyState == 'complete'";

                if (finishedCondition != null)
                {
                    finishedCond += " && " + finishedCondition;
                }

                if (cookies != null)
                {
                    phantomDriver.Navigate().GoToUrl(url);

                    foreach (Cookie cookie in cookies)
                    {
                        phantomDriver.Manage().Cookies.AddCookie(cookie);
                    }
                }

                phantomDriver.Navigate().GoToUrl(url);

                IWait<IWebDriver> wait = new OpenQA.Selenium.Support.UI.WebDriverWait(phantomDriver, TimeSpan.FromSeconds(30.00));
                wait.Until(driver1 => ((IJavaScriptExecutor)phantomDriver).ExecuteScript("return (" + finishedCond + ")").Equals(true));

                return phantomDriver.PageSource;
            }
        }
    }
}