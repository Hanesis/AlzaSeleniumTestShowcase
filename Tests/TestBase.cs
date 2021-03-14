using System;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AlzaSeleniumTest.Tests
{
    public class TestBase
    {

        public static IWebElement WaitForElementWithText(IWebDriver webDriver, By by, string textValue, int timeout = 10)
        {
            var i = 0;
            while (i < timeout)
            {
                i++;
                var elements = webDriver.FindElements(by);

                try
                {
                    var element = elements.First(el => el.Text == textValue);
                    return element;
                }
                catch { }
                Thread.Sleep(1000);
            }

            return null;
        }

        public static IWebElement ElementIsClickable(IWebDriver driver, By by, int timeout = 5)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeout));
            return wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(by));
        }


        public static bool WaitUntilElementExists(IWebDriver driver, By by, int timeout = 3)
        {
            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeout));
                return wait.Until(drv => drv.FindElements(by).Count >= 1);
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static bool WaitForElementWithMinimalPrice(IWebDriver driver, By by, string minimalPrice, int timeout = 3)
        {
            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeout));
                return wait.Until(drv => drv.FindElement(by).Text.Contains(minimalPrice));
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static IWebElement FindElement(IWebDriver driver, By by, int timeoutInSeconds = 5)
        {
            if (timeoutInSeconds > 0)
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                return wait.Until(drv => drv.FindElement(by));
            }
            return driver.FindElement(by);
        }

    }
}
