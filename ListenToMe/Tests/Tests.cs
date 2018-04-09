using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using Windows.UI.Popups;
using Xunit;

namespace ListenToMe.Tests
{
    /// <summary>
    /// xunit test methods
    /// usage:
    /// </summary>
    public class Tests
    {
        [Theory]
        [InlineData("max-mustermann@musterstadt.de", "^[a-zA-Z0-9äöüÄÖÜß!#\\$%&'\\*\\+\\-/=\\?\\^_`\\{\\}\\|~]+(\\.[a-zA-Z0-9äöüÄÖÜß!#\\$%&'\\*\\+\\-/=\\?\\^_`\\{\\}\\|~]+)*@[a-zA-Z0-9äöüÄÖÜß_-]+(\\.[a-zA-Z0-9äöüÄÖÜß_-]{2,})+$")]
        /// <summary>
        /// testing one of the regices returned by the Webclient for correctness. The Http.Decode() function had some unexpected output: it could not resolve
        /// sequences like &amp;amp;
        /// </summary>
        private void TestRegex(string to_compare, string regex)
        {

            //unfortunately, what WCFService returns looks like this:
            //"^[a-zA-Z0-9äöüÄÖÜß!#\\$%&  \'\\*\\+\\-/=\\?\\^_`\\{\\}\\|~]+(\\ .[a-zA-Z0-9äöüÄÖÜß!#\\$%&\'   \\*\\+\\-/=\\?\\^_`\\{\\}\\|~]+)*@[a-zA-Z0-9äöüÄÖÜß_-]+(\\.[a-zA-Z0-9äöüÄÖÜß_-]{2,})+$"

            Regex myregex = new Regex(regex);
            bool isMatch = myregex.IsMatch(to_compare);
            Assert.True(isMatch);
        }

        /// <summary>
        /// testing connection to the website with Http.NetAPI.
        /// the problem with this approach is that the javascript functions haven't yet finished binding strings in
        /// reference: https://blogs.windows.com/buildingapps/2015/11/23/demystifying-httpclient-apis-in-the-universal-windows-platform/#kzmsLAKtjKLGJFAU.97
        /// reference: https://docs.microsoft.com/en-us/aspnet/web-api/overview/advanced/calling-a-web-api-from-a-net-client
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        [Fact]
        public async void TestNetConnectionSmaller()
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("de-DE"));

            //ecapedatastring supposedly converts @ to /u%40
            string password = Uri.EscapeDataString(App.UserPassword);

            using (var handler = new HttpClientHandler { UseCookies = false })
            using (httpClient)
            {

                //first request
                Uri loginUri = new Uri("http://10.150.50.21/irj/portal?login_submit=on&login_do_redirect=1&no_cert_storing=on&j_salt=l4ZnvAPEMHwyxIeUN7JmhGQwJps%3D&j_username="+App.UserName+"&j_password=" + password + "&uidPasswordLogon=Anmelden: undefined");
                var req = new HttpRequestMessage(HttpMethod.Get, loginUri);
                req.Headers.Connection.Add("Keep-Alive");
                req.Content = new StringContent("", Encoding.UTF8, "text/html"); //Mozilla / 5.0(Windows NT 10.0; WOW64; rv: 51.0) Gecko / 20100101 Firefox / 51.0

                var response = await httpClient.SendAsync(req);
                var contentAfterGet = await httpClient.GetStringAsync(loginUri);


                req = new HttpRequestMessage(HttpMethod.Get, loginUri = new Uri("http://10.150.50.21/formularservice/formular/A_FOREX_ANTRAG_ESF_2/appl/e220f8f6-0726-11e8-b0c6-47de0dd015a0/?lang=de&backURL=aHR0cCUzQSUyRiUyRjEwLjE1MC41MC4yMSUyRmlyaiUyRnBvcnRhbCUzRk5hdmlnYXRpb25UYXJnZXQlM0RST0xFUyUzQSUyRnBvcnRhbF9jb250ZW50JTJGRVUtRExSX1JlZmFjdG9yaW5nJTJGT0FNX1BPUlRBTF9BUFBMSUNBTlRfSU5ESVZJRFVBTCUyRk9ubGluZUFwcGxpY2F0aW9uQUUlMjZhcHBsaWNhdGlvbklEJTNEODEwMDQ3OTE%3D&transactionID=ad833f54-d81d-42c0-9e17-bd8419d52ff4"));
                req.Headers.Host = "10.150.50.21";
                req.Headers.Add("Cookie", "PortalAlias=portal; saplb_*=(J2EE1212320)1212350; MYSAPSSO2=AjExMDAgAA1wb3J0YWw6RkdFSVNTiAATYmFzaWNhdXRoZW50aWNhdGlvbgEABkZHRUlTUwIAAzAwMAMAA09RMgQADDIwMTgwMjAxMDg0NAUABAAAAAgKAAZGR0VJU1P%2FAQQwggEABgkqhkiG9w0BBwKggfIwge8CAQExCzAJBgUrDgMCGgUAMAsGCSqGSIb3DQEHATGBzzCBzAIBATAiMB0xDDAKBgNVBAMTA09RMjENMAsGA1UECxMESjJFRQIBADAJBgUrDgMCGgUAoF0wGAYJKoZIhvcNAQkDMQsGCSqGSIb3DQEHATAcBgkqhkiG9w0BCQUxDxcNMTgwMjAxMDg0NDUyWjAjBgkqhkiG9w0BCQQxFgQUAj8yMUVHOexgzDwgpoPSOd8AChAwCQYHKoZIzjgEAwQuMCwCFE7teFnYo6DfWSjiIV!vyHr2sgrnAhR3x2STi90Bldx!!RqNxzOA3lG4CA%3D%3D; JSESSIONID=z2u-IaSeQCdbb25xyMEOL67exGpQYQG-fxIA_SAPBw9JMucfgxm5gJf1vl2NpDvc; JSESSIONMARKID=xHhiWwBy4_HZO9i142NeCAQmAMT8SzwOzvVb5_EgA; sap-usercontext=sap-language=DE&sap-client=901; SAP_SESSIONID_FQ2_901=RMomO77_lNkGykRdT4qjLVt8oOoHJxHogO0AUFarFvM%3d; XSRF-TOKEN=FfCQZKRUUE9SdreEy1Tf-oYKScuz00oxjiFBo2_ax8jQ2pTY4VYcg1Hct9qNO1mLbKlkGtt68vuQ8VD3hxZ7YYRwgs91qiYDvdhHhUTtYdykB6c7QUpzpffwOazJMkOKmQD-K25JZZuj2G5K04gQI5k6H_nlBJSxpsiz3jGBsTXlEwA9cVJs998JHYrCVPeaOhEIfENVWYd-JKo7TTpXnkf0DvuRq_Ac0MggdWO4_cuBqSxTbAd0E6l8GAS-ErFeBaMlIyBoEj_RpkERCmAzToqc-MDicI6_PakzG4onxNcxH6o83_W6IKpJKo5e6UCqv8ozL5l-3vJcZp6SdsRMeQ==");
                req.Headers.Connection.Add("Keep-Alive");
                req.Content = new StringContent("", Encoding.UTF8, "text/html");
                var resp = httpClient.SendAsync(req, HttpCompletionOption.ResponseContentRead);
                var myContentString = await httpClient.GetStringAsync(loginUri);
                Assert.Contains("section", myContentString);
                Debug.WriteLine(myContentString);

                }
        }
    }
}
