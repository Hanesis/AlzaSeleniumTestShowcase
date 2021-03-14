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
        private ChromeDriver _webDriver;
        
        [SetUp]
        public void Setup()
        {
            _tempDirectory = Utils.GetTemporaryDirectory();
            _chromeOptions = SetChromeOptions(_tempDirectory);
        }

        [TearDown]
        public void TearDown()
        {
            _webDriver.Dispose();
        }

        [TestCase("litp18843445")]
        public void BuyCheapestProductFromCategory(string category)
        {
            _webDriver = new ChromeDriver(_chromeOptions);
            _webDriver.Navigate().GoToUrl("https://www.alza.cz/");
           
            //GIVEN information about cheapest product in category
            var minimalPriceInCategory = OpenCategoryAndLocateMinimalPrice(_webDriver, category);
            LocateProductWithMinimalPrice(_webDriver, minimalPriceInCategory);

            //WHEN it is possible to go through order procedure
            var productName = ConfirmOrderInShopingCart(_webDriver);
            SetPickupPointAndPaymantMethod(_webDriver);
            FillinCustomerInformation(_webDriver);

            //THEN Order is success and Pdf has valid information
            var doneInfoBlock = FindElement(_webDriver, By.ClassName("doneInfoBlock"));
            var orderNumber = GetOrderNumber(doneInfoBlock.Text);
            var successText =  $"Objednávka {orderNumber} úspěšně dokončena";
            
            StringAssert.Contains(successText, doneInfoBlock.Text);

            FindElement(_webDriver, By.XPath($"//a[text()='{orderNumber}']")).Click();

            CloseAdvertisementIfShown(_webDriver);

            DownloadOrderPdf(_webDriver, ref orderNumber);
            
            orderNumber = orderNumber.Replace(" ", "");
            var orderDetails = Utils.GetPdfText(Path.Combine(_tempDirectory, orderNumber + ".pdf"));

            orderDetails.Should().Contain(minimalPriceInCategory.Replace("-", "00"));
            orderDetails.Should().Contain($"Způsob úhrady: Hotově - {DeliveryPlace}");
            orderDetails.Should().Contain("Objednávka " + orderNumber);
            orderDetails.Should().Contain(PhoneNumber);
            orderDetails.Should().Contain(productName);
            
            CancelOrder();
        }

        private void CancelOrder()
        {
            //Order has to be processed - sometime it takes a while
            var cancelElement = WaitForElementWithText(_webDriver, By.ClassName("mat-raised-button"), "Zrušit objednávku",30);
            cancelElement.Click();
            FindElement(_webDriver, By.ClassName("flat-button")).Click();
            ElementIsClickable(_webDriver, By.CssSelector(".mat-raised-button.ng-star-inserted"));
            FindElement(_webDriver, By.CssSelector(".mat-raised-button.ng-star-inserted")).Click();
        }
        
        private static void DownloadOrderPdf(IWebDriver webDriver, ref string orderNumber)
        {

            var downloadElement = WaitForElementWithText(webDriver, By.ClassName("mat-raised-button"), "Stáhnout PDF");
            downloadElement.Click();
            Thread.Sleep(Timeout); //Wait until pdf is available
        }

        private static void FillinCustomerInformation(ChromeDriver webDriver)
        {
            FindElement(webDriver, By.Id("userEmail")).Clear();
            FindElement(webDriver, By.Id("userEmail")).SendKeys(CustomerEmail);
            ElementIsClickable(webDriver, By.Id("inpTelNumber"));
            FindElement(webDriver, By.Id("inpTelNumber")).SendKeys(PhoneNumber);

            FindElement(webDriver, By.XPath("//span[text()='Dokončit objednávku']")).Click();
        }

        private static string OpenCategoryAndLocateMinimalPrice(ChromeDriver webDriver, string category)
        {
            FindElement(webDriver, By.Id(category)).Click();
            var minimalPriceInCategory =
                FindElement(webDriver, By.CssSelector(".js-min-value.min-value")).GetAttribute("value");
            return minimalPriceInCategory;
        }
        private static void CloseAdvertisementIfShown(IWebDriver webDriver)
        {
            if (WaitUntilElementExists(webDriver, By.ClassName("alzaDialogBody"),2))
            {
                FindElement(webDriver, By.ClassName("or-btn__inner")).Click();
                webDriver.Navigate().Back();
            }
        }


        private static string ConfirmOrderInShopingCart(IWebDriver webDriver)
        {
            var productName = FindElement(webDriver, By.ClassName("productInfo__texts__productName")).Text;
            FindElement(webDriver, By.Id("varBToBasketButton")).Click();

            if (WaitUntilElementExists(webDriver, By.ClassName("alzaDialogBody")))
            {
                FindElement(webDriver, By.XPath("//span[text()='Nepřidávat nic']")).Click();
            }
            else
            {
                webDriver.FindElement(By.XPath("//span[text()='Pokračovat']")).Click();
            }

            return productName;
        }

        private static void SetPickupPointAndPaymantMethod(ChromeDriver webDriver)
        {
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
        }

        private static void LocateProductWithMinimalPrice(ChromeDriver webDriver, string minimalPrice)
        {
            //sort from lowest price
            FindElement(webDriver, By.Id("ui-id-6")).Click();
            WaitForElementWithMinimalPrice(webDriver, By.ClassName("c2"), minimalPrice);
            var elementsPrice = webDriver.FindElements(By.ClassName("price"));
            var elementPrice = elementsPrice.First(e => e.Text.Contains(minimalPrice));
            elementPrice.FindElement(By.ClassName("btnk1")).Click();
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