using System;
using System.Collections.Generic;
using System.Text;

namespace SinaWeiboLoginCore.Models
{
    struct PreLoginData
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public dynamic ServerData { get; set; }
    }
}
