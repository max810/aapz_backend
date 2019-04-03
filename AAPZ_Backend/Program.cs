using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


namespace BLL
{
    public class Program
    {
        //public static ConcurrentDictionary<string, bool> AliveConnections = new ConcurrentDictionary<string, bool>();
        public static void Main(string[] args)
        {
            //IPEndPoint x = new IPEndPoint(IPAddress.Parse("192.168.0.1"), 5005);
            //Console.WriteLine(x.ToString());
            //Console.ReadLine();
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel(options => options.ListenAnyIP(5000))
                .UseStartup<Startup>()
                .Build();
    }
}
