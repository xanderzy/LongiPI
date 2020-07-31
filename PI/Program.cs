using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ApplicationCore.Interfaces;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace PI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseStartup<Startup>()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
               .UseKestrel(options =>
               {
                    /*options.Listen(IPAddress.Parse("10.6.6.199"), 9514, listenOptions =>
                         {
                             listenOptions.UseHttps("C:\\daochuzhengshu\\longicertificate.cer", "gIkGMrkF");        
                         }
                        );
                       options.Listen(IPAddress.Parse("10.6.6.199"), 80, listenOptions => { });*/
                   options.Listen(IPAddress.Loopback, 5001, listenOptions => { });
               })
                .Build();
            var provider = host.Services;
            provider.GetRequiredService<IMailQueueManager>().Run();
            host.Run();
        }



        /*public static void Main(string[] args)
      {
         var host = new WebHostBuilder()
              .UseKestrel()
              .UseContentRoot(Directory.GetCurrentDirectory())
              .UseUrls("http://*:80")
              .UseIISIntegration()
              .UseStartup<Startup>()
              .Build();
          var provider = host.Services;
          provider.GetRequiredService<IMailQueueManager>().Run();
          host.Run();
     }*/
    }
}

