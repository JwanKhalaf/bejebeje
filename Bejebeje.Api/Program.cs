namespace Bejebeje.Api
{
  using System;
  using System.Linq;
  using System.Net;
  using System.Net.Sockets;
  using System.Threading.Tasks;
  using Bejebeje.Services.Services.Interfaces;
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Hosting;
  using Npgsql;
  using Polly;
  using Polly.Retry;
  using Sentry;

  public class Program
  {
    public static async Task Main(string[] args)
    {
      string possibleSeedArgument = "-seed";

      bool seedIsRequested = args.Any(x => x == possibleSeedArgument);

      if (seedIsRequested)
      {
        args = args
          .Except(new string[] { possibleSeedArgument })
          .ToArray();
      }

      IHost host = CreateHostBuilder(args).Build();

      if (seedIsRequested)
      {
        Console.WriteLine("Database will be seeded with sample data if no data exists already.");

        using (IServiceScope serviceScope = host.Services.CreateScope())
        {
          IDataSeederService dataSeederService = serviceScope.ServiceProvider.GetService<IDataSeederService>();

          AsyncRetryPolicy retryPolicy = Policy
            .Handle<SocketException>()
            .Or<PostgresException>()
            .RetryAsync(50);

          await retryPolicy.ExecuteAsync(() => dataSeederService.SeedDataAsync());
        }
      }

      await host.RunAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
      return Host
        .CreateDefaultBuilder(args)
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
              options.ConfigureScope(s => s.SetTag("Always sent", "this tag"));
            });
        });
    }
  }
}
