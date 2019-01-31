using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SampleUnitTests
{
    [TestClass]
    public class SampleTests
    {
        
        [TestMethod]
        public void SampleHttpRequestTest()
        {
            var client = new HttpClient();
            client.BaseAddress = new System.Uri("https://localhost:5001");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.Timeout = TimeSpan.FromSeconds(15);
            var randomId = new Random().Next(-10, 10);
            var responseTask = client.GetAsync("");

            var response = responseTask.Result;


            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccessStatusCode);
            response.EnsureSuccessStatusCode();
        }
    }
}
