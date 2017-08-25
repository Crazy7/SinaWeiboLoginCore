namespace SinaWeiboLoginCore.Exceptions
{
    public class SinaPinNotFoundException : PinNotFoundException
    {
        public SinaPinNotFoundException(string pcid, string message = null) : base(GenerateUrl(pcid), message)
        {
        }

        private static string GenerateUrl(string pcid)
        {
            return "http://login.sina.com.cn/cgi/pin.php?p=" + pcid;
        }
    }
}
