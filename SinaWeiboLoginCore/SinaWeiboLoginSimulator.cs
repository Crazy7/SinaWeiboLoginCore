using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SinaWeiboLoginCore.Exceptions;
using SinaWeiboPasswordJsEncoder;
using WebRequestExtension;

namespace SinaWeiboLoginCore
{
    public class SinaWeiboLoginSimulator
    {
        private readonly string _userName;
        private readonly string _password;
        private readonly CookieContainer _cookieContainer;
        private readonly IWebRequestEx _webRequestEx;

        public SinaWeiboLoginSimulator(string userName, string password, CookieContainer cookieContainer = null)
        {
            _userName = userName;
            _password = password;

            _cookieContainer = cookieContainer ?? new CookieContainer();
            _webRequestEx = new WebRequestEx(_cookieContainer);
        }

        public async Task<CookieContainer> LoginAsync()
        {
            var encodedUserName = EncodeUserName(_userName);

            var preLoginJsonResult = await PreLoginAsync(encodedUserName);
            var preLoginData = JToken.Parse(preLoginJsonResult);

            var serverTime = preLoginData["servertime"]?.ToString();
            var nonce = preLoginData["nonce"]?.ToString();
            var rsakv = preLoginData["rsakv"]?.ToString();
            var pubkey = "0" + preLoginData["pubkey"]?.ToString();
            var showpin = preLoginData["showpin"]?.ToString();
            var pcid = preLoginData["pcid"]?.ToString();

            var encodePassword = EncodePassword(_password, serverTime, nonce, pubkey);

            var postData = "entry=weibo&gateway=1&from=&savestate=7&useticket=1&vsnf=1&su=" + encodedUserName
                + "&service=miniblog&servertime=" + serverTime
                + "&nonce=" + nonce
                + "&pwencode=rsa2&rsakv=" + rsakv
                + "&sp=" + encodePassword
                + "&sr=" + Uri.EscapeDataString("1366 * 768")
                + "&prelt=282&encoding=UTF-8&url=" + Uri.EscapeDataString("http://weibo.com/ajaxlogin.php?framelogin=1&callback=parent.sinaSSOController.feedBackUrlCallBack")
                + "&returntype=META";

            const string loginUrl = "http://login.sina.com.cn/sso/login.php?client=ssologin.js(v1.4.18)";
            var loginResponse = await _webRequestEx.PostAsync(loginUrl, postData);

            if (loginResponse.IndexOf("reason=") >= 0)
            {
                throw new LoginFailedException(loginResponse);
            }

            var redirectUrl = GetLoginRedirectUrl(loginResponse);
            await _webRequestEx.GetAsync(redirectUrl);

            return _cookieContainer;
        }


        private static string EncodeUserName(string userName)
        {
            var encoded = Uri.EscapeDataString(userName);
            var bytes = Encoding.UTF8.GetBytes(encoded);
            var base64 = Convert.ToBase64String(bytes);

            return base64;
        }

        private static string EncodePassword(string password, string serverTime, string nonce, string pubKey)
        {
            var jsEncoder = new JsEncoder();
            var encodedPwd = jsEncoder.EncodePassword(password, serverTime, nonce, pubKey);
            return encodedPwd;
        }

        public async Task<string> PreLoginAsync(string userName)
        {
            var url = "http://login.sina.com.cn/sso/prelogin.php?entry=weibo&callback=sinaSSOController.preloginCallBack&su="
                    + userName + "&rsakt=mod&checkpin=1&client=ssologin.js(v1.4.18)&_="
                    + GetTimestamp();

            var rawResponse = await _webRequestEx.GetAsync(url);

            return FormatPreLoginResponse(rawResponse);
        }

        private static string FormatPreLoginResponse(string responseText)
        {
            var start = responseText.IndexOf('{');
            var end = responseText.LastIndexOf('}');

            return responseText.Substring(start, end - start + 1);
        }

        private static int GetTimestamp()
        {
            var epoic = new DateTime(1970, 1, 1);
            var ms = DateTime.Now.Subtract(epoic).TotalMilliseconds;

            return (int)ms;
        }

        private static string GetLoginRedirectUrl(string response)
        {
            var reg = new Regex("location\\.replace\\('(.*)'");
            var match = reg.Match(response);

            return match.Success ? match.Groups[1].Value : null;
        }
    }
}
