using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using AlzaSeleniumTest.HelpMethods;
using FluentAssertions;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Path = System.IO.Path;

namespace AlzaSeleniumTest.Tests
{
    public class E2ECheapestProductTest :TestBase
    {
        private const string DeliveryPlace = "Olomouc";
        private const int Timeout = 3000;
        private string _tempDirectory;
        private const string CustomerEmail = "fake@email.com";
        private const string PhoneNumber = "777555222";
        private ChromeOptions _chromeOptions;
        
        [SetUp]
        public void Setup()
        {
            _tempDirectory = Utils.GetTemporaryDirectory();
            _chromeOptions = SetChromeOptions(_tempDirectory);
        }

        [Test]
        public void tests()
        {
            var webDriver = new ChromeDriver(_chromeOptions);
            webDriver.Navigate().GoToUrl("https://www.alza.cz/muj-ucet/objednavka-436473053.htm?x=0FD246E78Ba975F1Au5BFDhSt");

            WaitUntilElementExists(webDriver, By.ClassName("mat-raised-button"));
            var elements = webDriver.FindElements(By.ClassName("mat-raised-button"));

            var elementPdf = elements.FirstOrDefault(e => e.Text == "Stáhnout PDF");
            elementPdf.Click();

            var text = Utils.GetPdfText(Path.Combine(_tempDirectory, "436473053.pdf"));
        }

        [Test]
        public void BuyCheapestProductFromCategory()
        {
            var webDriver = new ChromeDriver(_chromeOptions);
            webDriver.Navigate().GoToUrl("https://www.alza.cz/");

            //GIVEN information about cheapest product in category
            
            //open category
            FindElement(webDriver, By.Id("litp18843445")).Click();
            var minimalPrice = FindElement(webDriver, By.CssSelector(".js-min-value.min-value")).GetAttribute("value");
            //sort from lowest price
            FindElement(webDriver, By.Id("ui-id-6")).Click();
            WaitForElementWithMinimalPrice(webDriver, By.ClassName("c2"), minimalPrice);
            var elementsPrice = webDriver.FindElements(By.ClassName("price"));
            var elementPrice = elementsPrice.First(e => e.Text.Contains(minimalPrice));
            elementPrice.FindElement(By.ClassName("btnk1")).Click();

            //WHEN it is possible to go through order procedure
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

            Thread.Sleep(Timeout);
            FindElement(webDriver, By.Id("personal-pickup__search__input")).SendKeys("Alza " + DeliveryPlace);
            ElementIsClickable(webDriver, By.Id("search-results__alza"));
            FindElement(webDriver, By.Id("search-results__alza")).Click();

            FindElement(webDriver, By.XPath("//span[text()='Vyzvednout zde']")).Click();

            Thread.Sleep(Timeout);
            ElementIsClickable(webDriver, By.XPath("//span[text()='Hotově / kartou (při vyzvednutí)']"));
            FindElement(webDriver, By.XPath("//span[text()='Hotově / kartou (při vyzvednutí)']")).Click();

            Thread.Sleep(Timeout);
            FindElement(webDriver, By.Id("confirmOrder2Button")).Click();

            FindElement(webDriver, By.Id("userEmail")).Clear();
            FindElement(webDriver, By.Id("userEmail")).SendKeys(CustomerEmail);
            ElementIsClickable(webDriver, By.Id("inpTelNumber"));
            FindElement(webDriver, By.Id("inpTelNumber")).SendKeys(PhoneNumber);

            FindElement(webDriver, By.XPath("//span[text()='Dokončit objednávku']")).Click();

            var doneInfoBlock = FindElement(webDriver, By.ClassName("doneInfoBlock"));
            var orderNumber = GetOrderNumber(doneInfoBlock.Text);
            var successText =  $"Objednávka {orderNumber} úspěšně dokončena";
            
            //THEN we can finalizes the order
            StringAssert.Contains(successText, doneInfoBlock.Text);
            FindElement(webDriver, By.XPath($"//a[text()='{orderNumber}']")).Click();

            Thread.Sleep(Timeout);
            WaitUntilElementExists(webDriver, By.ClassName("mat-raised-button"));
            var elements = webDriver.FindElements(By.ClassName("mat-raised-button"));

            elements.First(e => e.Text == "Stáhnout PDF").Click();

            Thread.Sleep(Timeout);
            orderNumber = orderNumber.Replace(" ", "");
            var orderDetails = Utils.GetPdfText(Path.Combine(_tempDirectory, orderNumber + ".pdf"));
            orderDetails.Should().Contain("Objednávka " + orderNumber);
            orderDetails.Should().Contain($"Způsob úhrady: Hotově - {DeliveryPlace}");
            orderDetails.Should().Contain(PhoneNumber);
            orderDetails.Should().Contain("344,00 Kč");
            orderDetails.Should().Contain("Mobilní telefon Maxcom MM135");
        }

        static string GetOrderNumber(string orderText)
        {
            return Regex.Match(orderText, "(\\s[\\d]{3}){3}").Value.Trim();
        }
     
        static ChromeOptions SetChromeOptions(string tempDirectory)
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddUserProfilePreference("download.default_directory", tempDirectory);
            chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
            chromeOptions.AddUserProfilePreference("download.directory_upgrade", true);
            chromeOptions.AddUserProfilePreference("plugins.plugins_disabled", "Chrome PDF Viewer");
            chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
            return chromeOptions;
        }
    }
}