using System;
using System.Collections.Generic;
using System.Text;

namespace SinaWeiboLoginCore.Utilities
{
    static class SinaWeiboUtility
    {
        public static string GeneratePinImageUrl(string pcid)
        {
            return "http://login.sina.com.cn/cgi/pin.php?p=" + pcid;
        }
    }
}
