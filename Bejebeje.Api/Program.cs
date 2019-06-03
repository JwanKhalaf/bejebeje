namespace Bejebeje.Api
{
  using System;
  using System.Linq;
  using System.Net.Sockets;
  using Bejebeje.DataAccess.Data;
  using Microsoft.AspNetCore;
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.DependencyInjection;
  using Npgsql;
  using Polly;
  using Polly.Retry;
  using Serilog;
  using Serilog.Events;
  using Serilog.Sinks.SystemConsole.Themes;

  public class Program
  {
    public static void Main(string[] args)
    {
      string possibleSeedArgument = "-seed";

      bool seedIsRequested = args.Any(x => x == possibleSeedArgument);

      if (seedIsRequested)
      {
        args = args
          .Except(new string[] { possibleSeedArgument })
          .ToArray();
      }

      IWebHost host = CreateWebHostBuilder(args).Build();

      if (seedIsRequested)
      {
        Console.WriteLine("Database will be seeded with sample data if no data exists already.");

        IConfiguration config = host
          .Services
          .GetRequiredService<IConfiguration>();

        DataSeeder dataSeeder = host
          .Services
          .GetRequiredService<DataSeeder>();

        RetryPolicy retryPolicy = Policy
          .Handle<SocketException>()
          .Or<PostgresException>()
          .Retry(50);

        retryPolicy.Execute(() => dataSeeder.EnsureDataIsSeeded());
      }

      host.Run();
    }

    public static IWebHostBuilder CreateWebHostBuilder(string[] args)
    {
      return WebHost
        .CreateDefaultBuilder(args)
        .UseStartup<Startup>()
        .UseSerilog(
          (context, configuration) =>
          {
            configuration
              .MinimumLevel.Debug()
              .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
              .MinimumLevel.Override("System", LogEventLevel.Warning)
              .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
              .Enrich.FromLogContext()
              .WriteTo.File(@"api_log.txt")
              .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}",
                theme: AnsiConsoleTheme.Literate);
          });
    }
  }
}
