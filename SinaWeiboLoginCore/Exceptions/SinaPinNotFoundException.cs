using SinaWeiboLoginCore.Utilities;

namespace SinaWeiboLoginCore.Exceptions
{
    public class SinaPinNotFoundException : PinNotFoundException
    {
        public SinaPinNotFoundException(string pcid, string message = null) : base(SinaWeiboUtility.GeneratePinImageUrl(pcid), message)
        {
        }
    }
}
