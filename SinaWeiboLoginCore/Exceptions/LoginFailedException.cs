using System;
using System.Collections.Generic;
using System.Text;

namespace SinaWeiboLoginCore.Exceptions
{
    public class LoginFailedException : Exception
    {
        public LoginFailedException(string message) : base(message)
        {
        }

        public LoginFailedException() : base()
        {

        }
    }
}
