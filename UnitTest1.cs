using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace AlzaSeleniumTest
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void Test1()
        {
            var webDriver = new ChromeDriver();
            webDriver.Navigate().GoToUrl("https://www.alza.cz/");

            //open cellphone category
            FindElement(webDriver, By.Id("litp18843445")).Click();
            var minimalPrice = FindElement(webDriver, By.CssSelector(".js-min-value.min-value")).GetAttribute("value");
            //sort from lowest price
            FindElement(webDriver, By.Id("ui-id-6")).Click();
            WaitForElementWithMinimalPrice(webDriver, By.ClassName("c2"), minimalPrice);
            var elements = webDriver.FindElements(By.ClassName("price"));
            var element = elements.First(e => e.Text.Contains(minimalPrice));
            element.FindElement(By.ClassName("btnk1")).Click();


            if (WaitUntilElementExists(webDriver, By.ClassName("alzaDialogBody")))
            {
                webDriver.FindElement(By.XPath("//span[text()='Nepřidávat nic']")).Click();
            }
            else
            {
                var button = FindElement(webDriver, By.Id("varBToBasketButton"));
                button.Click();
            }

            webDriver.FindElement(By.XPath("//span[text()='Pokračovat']")).Click();

            if (WaitUntilElementExists(webDriver, By.ClassName("alzaDialogBody")))
            {
                FindElement(webDriver, By.XPath("//span[text()='Nepřidávat nic']")).Click();
            }
            else
            {
                webDriver.FindElement(By.XPath("//span[text()='Pokračovat']")).Click();
            }

            FindElement(webDriver, By.ClassName("deliveryCheckboxContainer")).Click();

            //Disable AlzaBox
            FindElement(webDriver, By.CssSelector(".alzacheckbox.checkboxa.type-2.checked")).Click();

            Thread.Sleep(1500);
            FindElement(webDriver, By.Id("personal-pickup__search__input")).SendKeys("Alza Olomouc");
            ElementIsClickable(webDriver, By.Id("search-results__alza"));
            FindElement(webDriver, By.Id("search-results__alza")).Click();

            FindElement(webDriver, By.XPath("//span[text()='Vyzvednout zde']")).Click();

            ElementIsClickable(webDriver, By.XPath("//span[text()='Hotově / kartou (při vyzvednutí)']"));
            FindElement(webDriver, By.XPath("//span[text()='Hotově / kartou (při vyzvednutí)']")).Click();

            Thread.Sleep(1500);
            FindElement(webDriver, By.Id("confirmOrder2Button")).Click();

            FindElement(webDriver, By.Id("userEmail")).Clear();
            FindElement(webDriver, By.Id("userEmail")).SendKeys("fake@email.com");
            ElementIsClickable(webDriver, By.Id("inpTelNumber"));
            FindElement(webDriver, By.Id("inpTelNumber")).SendKeys("777333111");

            FindElement(webDriver, By.XPath("//span[text()='Dokončit objednávku']")).Click();

            var doneInfoBlock = FindElement(webDriver, By.ClassName("doneInfoBlock"));
            var isSuccess = Regex.Match(doneInfoBlock.Text, "Objednávka(\\s[\\d]{3}){3}\\súspěšně dokončena").Success;
            Assert.IsTrue(isSuccess);


            //fa fa-print
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