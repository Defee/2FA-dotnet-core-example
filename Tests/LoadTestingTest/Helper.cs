using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace LoadTestingTest
{
    public static class WebClientHelper
    {
        //todo: contentTypeDependency
        public static HttpResponseMessage SendRequest(string requestUrl, HttpMethod httpMethod, IDictionary<string, string> routeParameters,
            IDictionary<string, object> requestParameters = null,
            ICredentials credentials = null, string contentType = "application/json", string authenticationHeader = "Basic", CookieContainer cookies = null)
        {

            var handler = new HttpClientHandler();
            handler.CookieContainer = cookies;
            using (var client = handler == null ? new HttpClient() : new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //Set alternate credentials

                switch (authenticationHeader)
                {
                    case "Basic":
                        if (credentials != null)
                        {
                            var username = string.Empty;
                            var password = string.Empty;
                            var netCredentials = credentials as NetworkCredential;
                            if (netCredentials != null)
                            {
                                username = netCredentials.UserName;
                                password = netCredentials.Password;
                            }
                            client.DefaultRequestHeaders.Authorization =
                                new AuthenticationHeaderValue("Basic",
                                    Convert.ToBase64String(
                                        Encoding.ASCII.GetBytes(
                                            string.Format("{0}:{1}", username, password))));
                        }
                        break;
                    case "Bearer":
                        if (credentials != null)
                        {
                            var token = string.Empty;
                            var tokenCredentials = credentials as TokenCredentials;
                            if (tokenCredentials != null)
                                token = tokenCredentials.Token;
                            client.DefaultRequestHeaders.Authorization =
                                new AuthenticationHeaderValue("Bearer",
                                    Convert.ToBase64String(
                                        Encoding.ASCII.GetBytes(token)));
                        }
                        break;
                }


                var responseBody = string.Empty;
                var urlBuilder = new UriBuilder(requestUrl);
                
                //if (cookies.GetCookies(new Uri("http://" + urlBuilder.Host)).Count >0)
                //{
                //    var cookie = cookies.GetCookies(new Uri("http://" + urlBuilder.Host))[0];
                //    client.DefaultRequestHeaders.Add("RequestVerificationToken", cookie.Value);
                //}

                //var collection = QueryHelpers.ParseQuery(urlBuilder.Query);
                //foreach (var parameter in routeParameters)
                //    collection[parameter.Key] = parameter.Value;
                StringContent content;
                String contentString = string.Empty;
                switch (contentType)
                {
                    case "application/json":

                        contentString = JsonConvert.SerializeObject(requestParameters);
                        break;
                    case "application/x-www-form-urlencoded":
                        contentString = requestParameters.ToKeyValuePairsString();
                        break;
                }
                HttpResponseMessage response = null;
                if (httpMethod == HttpMethod.Get)
                {
                    //var items = collection.SelectMany(x => x.Value, (col, value) => new KeyValuePair<string, string>(col.Key, value)).ToList();
                    //var qb = new QueryBuilder(items);
                    //requestUrl = urlBuilder + qb.ToQueryString().Value;
                    response = client.GetAsync(requestUrl).Result;
                    response.EnsureSuccessStatusCode();
                    responseBody = response.Content.ReadAsStringAsync().Result;

                }

                if (httpMethod == HttpMethod.Post)
                {
                    content = new StringContent(contentString,
                            Encoding.UTF8,
                            contentType);

                    response = client.PostAsync(requestUrl, content).Result;
                    response.EnsureSuccessStatusCode();
                    responseBody = response.Content.ReadAsStringAsync().Result;
                }

                if (httpMethod == HttpMethod.Put)
                {
                    content = new StringContent(contentString,
                            Encoding.UTF8,
                            contentType);

                    response = client.PutAsync(requestUrl, content).Result;
                    response.EnsureSuccessStatusCode();
                    responseBody = response.Content.ReadAsStringAsync().Result;

                }
                if (httpMethod == HttpMethod.Delete)
                {
                    content = new StringContent(contentString,
                             Encoding.UTF8,
                             contentType);

                    response = client.DeleteAsync(requestUrl).Result;

                    response.EnsureSuccessStatusCode();
                    responseBody = response.Content.ReadAsStringAsync().Result;

                }
                return response;


            }
        }

    }

    public static class DictionaryExtensions
    {
        /// <summary>
        /// Converts IEnumerable of KeyValues to KeyValue Pair string
        /// </summary>
        /// <typeparam name="TKey">Key Type</typeparam>
        /// <typeparam name="TValue">Value Type</typeparam>
        /// <param name="items">Enumerable collection of key value pairs</param>
        /// <param name="format">String Format. Default is ( key=vallue )</param>
        /// <param name="separator">KevValue Pair Separator</param>
        /// <returns></returns>
        public static string ToKeyValuePairsString<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> items, string format = null, char separator = '&')
        {
            format = string.IsNullOrEmpty(format) ? "{0}={1}" + separator : format + separator;

            var itemString = new StringBuilder();
            foreach (var item in items)
                itemString.AppendFormat(format, item.Key, item.Value);

            return itemString.ToString().Trim(separator);
        }
    }

    public class TokenCredentials : ICredentials
    {
        public string Token { get; set; }
        public string OathType { get; set; }
        public NetworkCredential GetCredential(Uri uri, string authType)
        {
            throw new NotImplementedException();
        }

    }
}
