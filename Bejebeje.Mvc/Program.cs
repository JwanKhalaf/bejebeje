namespace Bejebeje.Mvc
{
  using System;
  using System.Net;
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.Extensions.Hosting;

  public class Program
  {
    public static void Main(string[] args)
    {
      CreateHostBuilder(args)
        .Build()
        .Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
              webBuilder
                .UseStartup<Startup>()
                .UseSentry(options =>
                {
                  options.Release = "1";
                  options.MaxBreadcrumbs = 200;
                  options.HttpProxy = null;
                  options.DecompressionMethods = DecompressionMethods.None;
                  options.MaxQueueItems = 100;
                  options.ShutdownTimeout = TimeSpan.FromSeconds(5);
                });
            });
  }
}
