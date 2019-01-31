using Microsoft.VisualStudio.TestTools.WebTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OtpNet;
using System.IO;

namespace LoadTestingTest
{
    public class AuthPlugin: WebTestPlugin
    {
        static readonly string _cookieName = "GlobalCookie";
        public override void PreWebTest(object sender, PreWebTestEventArgs e)
        {
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

                e.WebTest.Context.Add(_cookieName, cookieCollection);
            }
            catch (Exception ex)
            {

                throw ex;
            }
            finally
            {
                driver.Close();
                driver.Quit();
            }
            base.PreWebTest(sender,e);

            // Create a WebProxy object for your proxy
            //WebProxy webProxy = new WebProxy("<http://yourproxy>");
            //Set the WebProxy so that even local addresses use the proxy
            // webProxy.BypassProxyOnLocal = false;
            // Use this WebProxy for the Web test
            //e.WebTest.WebProxy = webProxy;
            //e.WebTest.PreAuthenticate = true;

            //NetworkCredential proxyCredentials;
            //proxyCredentials = new NetworkCredential();
            //proxyCredentials.Domain = "";
            //proxyCredentials.UserName = "pirmosk@gmail.com";
            //proxyCredentials.Password = ;
            //e.WebTest.WebProxy.Credentials = proxyCredentials;
            //var client = new WebClient();
            //client.Proxy = webProxy;
            
            //WebClientHelper.SendRequest("http://localhost:5111/Account/Login",HttpMethod.Post,null, credentials: proxyCredentials);
        }

        public override void PreRequest(object sender, PreRequestEventArgs e)
        {
            object cookieObj;
            e.WebTest.Context.TryGetValue(_cookieName,out cookieObj);
            var cookie = (CookieCollection)cookieObj;

            //var cookies = new CookieContainer();
            //var handler = new HttpClientHandler();
            //handler.CookieContainer = cookies;
            ////var client = new HttpClient(handler);
            ////client.BaseAddress = new System.Uri("http://localhost:5111");
            //e.Request.Headers.Add("Authorization", "Basic " + 
            //                        Convert.ToBase64String(
            //                            Encoding.ASCII.GetBytes(
            //                                string.Format("{0}:{1}", "pirmosk@gmail.com", "`12qweASD"))));

            //var response = client.GetAsync("/Identity/Account/Login").Result;
            //response.EnsureSuccessStatusCode();

            //string antiForgeryToken = AntiForgeryHelper.ExtractAntiForgeryToken(response).Result;

            //var formPostBodyData = new Dictionary<string, string>
            //{
            //    { "Input.Email", "pirmosk@gmail.com" },
            //    { "Input.Password", "`12qweASD" },
            //    { "Input.RememberMe","true" },
            //    { "__RequestVerificationToken", antiForgeryToken }
            //};

            //var requestMessage = PostRequestHelper.Create("http://localhost:5111/Identity/Account/Login", formPostBodyData);

            //response = client.SendAsync(requestMessage).Result;
            //response.EnsureSuccessStatusCode();
            // var myCookies = cookies.GetCookies(new Uri("http://localhost:5111"));
            //e.Request.Cookies.Add(myCookies);


            var cookies = new CookieContainer();
            //cookies.Add(cookie);
            var handler = new HttpClientHandler();
            handler.CookieContainer = cookies;
            var client = new HttpClient(handler);
            
            client.BaseAddress = new System.Uri("https://localhost:5001");
            
            var response = client.GetAsync("").Result;
            response.EnsureSuccessStatusCode();

            e.Request.Cookies.Add(cookie);

            base.PreRequest(sender, e);
        }

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
