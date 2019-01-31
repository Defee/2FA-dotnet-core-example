using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Net;
using System.Collections.Generic;
using HtmlAgilityPack;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OtpNet;
using Newtonsoft.Json;
using System.Threading;

namespace LoadTestingTest
{
    [TestClass]
    public class LoadTestingUnit
    {
        //TODO: think how to do it better way
        static readonly CookieContainer cookiesG = new CookieContainer();
        [AssemblyInitialize()]
        public static void AssemblyInit(TestContext context) {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            IWebDriver driver = new ChromeDriver();
            try
            {
                driver.Url = "https://localhost:5001";
                var username = new Random().Next(1, 1000000) + "@gmail.com";
                var password = "`12qweASD";

                IWebElement usernameField, passwordField;
                SeleniumRegisterPage(driver, username, password, out usernameField, out passwordField);


                // Accept cookies
                var acceptButton = driver.FindElement(By.XPath("//*[@id=\"cookieConsent\"]/button"));
                if (acceptButton != null) acceptButton.Click();

                // Click log in button
                //var loginButton = driver.FindElement(By.XPath("/html/body/header/nav/div/div/ul[1]/li[2]/a"));
                //loginButton.Click();

                // LoginPageSelenium(driver, username, password, out usernameField, out passwordField);

                // Enable two authentication
                var manage = driver.FindElement(By.XPath("/html/body/header/nav/div/div/ul[1]/li[1]/a"));
                manage.Click();

                var twoFactorRoute = driver.FindElement(By.Id("two-factor"));
                twoFactorRoute.Click();

                driver.FindElement(By.XPath("//*[@id=\"enable-authenticator\"]")).Click();

                var QRCode = driver.FindElement(By.TagName("kbd")).Text;
                //Console.WriteLine(QRCode);

                QRCode = QRCode.Replace(" ", "");
                var cookies = driver.Manage().Cookies;

                // Will generate different code for same key
                var otpKey = Base32Encoding.ToBytes(QRCode);
                var otp = new Totp(otpKey);
                var twoFactorCode = otp.ComputeTotp(DateTime.UtcNow);
                //Console.WriteLine(twoFactorCode);
                //var path = "./tmpData/token.json";
                //var fileInfo = new FileInfo(path);
                //if (!fileInfo.Exists)
                //{
                //    using (var st = fileInfo.CreateText())
                //    {
                //        st.Write(QRCode);
                //        st.Close();
                //    }
                //}
                //else
                //    File.WriteAllText(path, QRCode);

                // Enter the code to verify authenticator
                var verifyField = driver.FindElement(By.Id("Input_Code"));
                verifyField.SendKeys(twoFactorCode);
                var verify = driver.FindElement(By.XPath("//*[@id=\"send-code\"]/button"));
                if (verify != null) verify.Click();

                // Log out
                var logout = driver.FindElement(By.XPath("/html/body/header/nav/div/div/ul[1]/li[2]/form/button"));
                if (logout != null) logout.Click();

                // Log in again
                LoginPageSelenium(driver, username, password, out usernameField, out passwordField);

                // Get code for two factor
                var authenticatorCodeField = driver.FindElement(By.Id("Input_TwoFactorCode"));
                authenticatorCodeField.SendKeys(otp.ComputeTotp(DateTime.UtcNow));
                var rememberMachine = driver.FindElement(By.Id("Input_RememberMachine"));
                rememberMachine.Click();
                var authLoginButton = driver.FindElement(By.XPath("/html/body/div/main/div/div/form/div[4]/button"));
                authLoginButton.Click();
                var cookieCollection = new CookieCollection();
                foreach (var item in cookies.AllCookies)
                    cookieCollection.Add(new System.Net.Cookie(item.Name, item.Value, item.Path, item.Domain));


                cookiesG.Add(cookieCollection);
            }
            catch (Exception ex)
            {

                throw ex;
            }
            finally
            {
                //driver.Close();
                driver.Quit();
            }
        }

        [TestMethod]
        public void AccessProtectedArea()
        {
            //Parallel.For(0, 1000, (i) =>
            //{
            var handler = new HttpClientHandler();
            handler.CookieContainer = cookiesG;
            //Thread.Sleep(new Random().Next(10, 100));
            var client = new HttpClient(handler);
            client.BaseAddress = new System.Uri("https://localhost:5001");

            var response = client.GetAsync("/test/index").Result;
            response.EnsureSuccessStatusCode();
            var result = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
            //    i++;
            //});


        }

        //[TestMethod]
        //public void TestMethod1()
        //{
        //    var client = new HttpClient();
        //    //client.BaseAddress = new System.Uri("http://ec2-34-226-143-108.amazonaws.com");
        //    client.BaseAddress = new System.Uri("http://localhost:5111");
        //    client.DefaultRequestHeaders.Accept.Clear();
        //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //    var randomId = new Random().Next(-10, 10);
        //    Task<HttpResponseMessage> responseTask = client.GetAsync(
        //       $"home/TestMethod?id={randomId}");

        //    var response = responseTask.Result;


        //    Assert.IsNotNull(response);
        //    Assert.IsTrue(response.IsSuccessStatusCode);
        //    response.EnsureSuccessStatusCode();
        //}


        //public void TestAuthUnit()
        //{
        //    NetworkCredential proxyCredentials;
        //    proxyCredentials = new NetworkCredential();
        //    proxyCredentials.Domain = "";
        //    proxyCredentials.UserName = "pirmosk@gmail.com";
        //    proxyCredentials.Password = "`12qweASD";
        //    IDictionary<string, object> parms = new Dictionary<string, object> {
        //        { "username", "pirmosk@gmail.com" },
        //        { "password", "`12qweASD" },

        //    };
        //    var cookies = new CookieContainer();

        //    var resp1= WebClientHelper.SendRequest("http://localhost:5111/Identity/Account/Login", HttpMethod.Get, null, cookies:cookies);

        //    var html = new HtmlDocument();
        //    html.LoadHtml(resp1.Content.ReadAsStringAsync().Result);
        //    var root = html.DocumentNode;
        //    var token = root.Descendants().Where(it => it.GetAttributeValue("name", "").Equals("__RequestVerificationToken")).Single().GetAttributeValue("value", "Value");
        //    resp1.Dispose();

        //    parms.Add("__RequestVerificationToken", token);
        //    var resp2 =WebClientHelper.SendRequest("http://localhost:5111/Identity/Account/Login", HttpMethod.Post,null, requestParameters: parms, credentials: proxyCredentials,cookies:cookies);
        //}

        //[TestMethod]
        //public void TestProtectedAreaBasic()
        //{
        //    NetworkCredential proxyCredentials;
        //    proxyCredentials = new NetworkCredential();
        //    proxyCredentials.Domain = "";
        //    proxyCredentials.UserName = "pirmosk@gmail.com";
        //    proxyCredentials.Password = "`12qweASD";
        //    var cookies = new CookieContainer();
        //    var resp1 = WebClientHelper.SendRequest("http://localhost:5111/home/TestMethod?id=0", HttpMethod.Get, null,credentials: proxyCredentials, cookies: cookies);

        //    resp1.Dispose();
        //}

        //[TestMethod]
        //public async Task TestAuthUnit2()
        //{
        //    var cookies = new CookieContainer();
        //    var handler = new HttpClientHandler();
        //    handler.CookieContainer = cookies;
        //    var client = new HttpClient(handler);
        //    client.BaseAddress = new System.Uri("http://localhost:5111");
        //    var response = await client.GetAsync("/home/TestMethod?id=0");


        //    response = await client.GetAsync("/Identity/Account/Logout");
        //    response.EnsureSuccessStatusCode();


        //    response = await client.GetAsync("/Identity/Account/Login");
        //    response.EnsureSuccessStatusCode();

        //    string antiForgeryToken = await AntiForgeryHelper.ExtractAntiForgeryToken(response);

        //    var formPostBodyData = new Dictionary<string, string>
        //    {
        //        { "Input.Email", "pirmosk@gmail.com" },
        //        { "Input.Password", "`12qweASD" },
        //        { "Input.RememberMe","true" },
        //        { "__RequestVerificationToken", antiForgeryToken }
        //    };

        //    var requestMessage = PostRequestHelper.Create("http://localhost:5111/Identity/Account/Login", formPostBodyData);

        //    response = await client.SendAsync(requestMessage);
        //    response.EnsureSuccessStatusCode();
        //    var myCookies2 = cookies.GetCookies(new Uri("http://localhost:5111"));
        //    var myCookies = CookiesHelper.ExtractCookiesFromResponse(response);

        //    response = await client.GetAsync("http://localhost:5111/home/TestMethod?id=0");
        //}

        private static void LoginPageSelenium(IWebDriver driver, string username, string password, out IWebElement usernameField, out IWebElement passwordField)
        {
            var loginButton = driver.FindElement(By.XPath("/html/body/header/nav/div/div/ul[1]/li[2]/a"));
            loginButton.Click();
            // Enter username
            usernameField = driver.FindElement(By.Name("Input.Email"));
            usernameField.SendKeys(username);

            // Enter password
            passwordField = driver.FindElement(By.Name("Input.Password"));
            passwordField.SendKeys(password);
            // Hit log in
            loginButton = driver.FindElement(By.XPath("//*[@id=\"account\"]/div[5]/button"));
            loginButton.Click();
        }

        private static void SeleniumRegisterPage(IWebDriver driver, string username, string password, out IWebElement usernameField, out IWebElement passwordField)
        {
            var clickRegister = driver.FindElement(By.XPath("/html/body/header/nav/div/div/ul[1]/li[1]/a"));
            if (clickRegister != null) clickRegister.Click();

            // Enter username
            usernameField = driver.FindElement(By.Name("Input.Email"));
            usernameField.SendKeys(username);

            // Enter password
            passwordField = driver.FindElement(By.Name("Input.Password"));
            passwordField.SendKeys(password);
            // confirm password
            var cpwd = driver.FindElement(By.Name("Input.ConfirmPassword"));
            cpwd.SendKeys(password);
            var registerButton = driver.FindElement(By.XPath("/html/body/div/main/div/div/form/button"));
            registerButton.Click();
        }

    }
}
