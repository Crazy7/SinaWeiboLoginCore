using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SinaWeiboLoginCore;

namespace SinaWeiboLoginTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var u = ReadInput("UserName : ");
                var pwd = ReadInput("Password : ");

                var simulator = new SinaWeiboLoginSimulator(u, pwd);
                var cc = await simulator.LoginAsync();

                var cookies = cc.GetCookies(new Uri("http://sina.com.cn"));
                foreach (var c in cookies)
                {
                    Console.WriteLine(JsonConvert.SerializeObject(c));
                }

                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        static string ReadInput(string message)
        {
            Console.WriteLine(message);

            return Console.ReadLine();
        }
    }
}
