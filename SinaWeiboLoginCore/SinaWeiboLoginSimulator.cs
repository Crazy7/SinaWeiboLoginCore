using System;
using System.Dynamic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SinaWeiboLoginCore.Exceptions;
using SinaWeiboLoginCore.Models;
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

        public string Pin { get; set; }

        public SinaWeiboLoginSimulator(string userName, string password, CookieContainer cookieContainer = null)
        {
            _userName = userName;
            _password = password;

            _cookieContainer = cookieContainer ?? new CookieContainer();
            _webRequestEx = new WebRequestEx(_cookieContainer);
        }

        public async Task<CookieContainer> LoginAsync()
        {
            var preLoginResult = await PreLoginAsync();
            var postObj = PreparePostBodyForLogin(preLoginResult);

            const string loginUrl = "http://login.sina.com.cn/sso/login.php?client=ssologin.js(v1.4.18)";
            var loginResponse = await _webRequestEx.PostAsync(loginUrl, postObj);

            if (loginResponse.IndexOf("reason=") >= 0)
            {
                throw new LoginFailedException(loginResponse);
            }

            var redirectUrl = GetLoginRedirectUrl(loginResponse);
            await _webRequestEx.GetAsync(redirectUrl);

            return _cookieContainer;
        }

        private dynamic PreparePostBodyForLogin(PreLoginData preLoginData)
        {
            var showPin = preLoginData.ServerData.showpin == "1";
            if(showPin && string.IsNullOrEmpty(Pin))
            {
                throw new SinaPinNotFoundException(preLoginData.ServerData.pcid);
            }

            dynamic postObj = new ExpandoObject();
            postObj.entry = "weibo";
            postObj.gateway = 1;
            postObj.from = string.Empty;
            postObj.savestate = 7;
            postObj.useticket = 1;
            postObj.vsnf = 1;
            postObj.su = preLoginData.UserName;
            postObj.service = "miniblog";
            postObj.servertime = preLoginData.ServerData.serverTime;
            postObj.nonce = preLoginData.ServerData.nonce;
            postObj.pwencode = "rsa2";
            postObj.rsakv = preLoginData.ServerData.rsakv;
            postObj.sp = preLoginData.Password;
            postObj.sr = "1366*768";
            postObj.prelt = 282;
            postObj.encoding = "UTF-8";
            postObj.url = "http://weibo.com/ajaxlogin.php?framelogin=1&callback=parent.sinaSSOController.feedBackUrlCallBack";
            postObj.returntype = "META";

            return postObj;
        }
        
        private async Task<PreLoginData> PreLoginAsync()
        {
            var userName = EncodeUserName(_userName);

            var preLoginJsonResult = await PreLoginAsync(userName);
            var preLoginData = JToken.Parse(preLoginJsonResult);

            dynamic serverData = new ExpandoObject();
            serverData.serverTime = preLoginData["servertime"]?.ToString();
            serverData.nonce = preLoginData["nonce"]?.ToString();
            serverData.rsakv = preLoginData["rsakv"]?.ToString();
            serverData.pubkey = "0" + preLoginData["pubkey"]?.ToString();
            serverData.showpin = preLoginData["showpin"]?.ToString();
            serverData.pcid = preLoginData["pcid"]?.ToString();

            var data = new PreLoginData
            {
                UserName = userName,
                Password = EncodePassword(_password, serverData.serverTime, serverData.nonce, serverData.pubkey),
                ServerData = serverData
            };

            return data;
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
