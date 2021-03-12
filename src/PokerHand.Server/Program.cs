using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace PokerHand.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var rootPath = Path.Combine(AppContext.BaseDirectory, "wwwroot");
            var profileImagesPath = Path.Combine(AppContext.BaseDirectory, "wwwroot", "profileImages");
            
            if (Directory.Exists(profileImagesPath) is false)
            {
                Directory.CreateDirectory(rootPath);
                Directory.CreateDirectory(profileImagesPath);
            }
            
            
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(
                    Path.Combine(AppContext.BaseDirectory, "wwwroot", "logs", "log.txt"),
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}{NewLine}",
                    fileSizeLimitBytes: 10_000_000,
                    rollOnFileSizeLimit: true,
                    shared: true)
                .CreateLogger(); 
            
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
