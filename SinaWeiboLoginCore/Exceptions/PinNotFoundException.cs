using System;
using System.Collections.Generic;
using System.Text;

namespace SinaWeiboLoginCore.Exceptions
{
    public class PinNotFoundException : LoginFailedException
    {
        public string PinImageUrl { get; protected set; }


        public PinNotFoundException(string pinImageUrl, string message = null) : base(message ?? "Pin not found")
        {
            PinImageUrl = message;
        }
    }
}
