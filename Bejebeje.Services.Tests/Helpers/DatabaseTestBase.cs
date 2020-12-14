namespace Bejebeje.Services.Tests.Helpers
{
  using System;
  using DataAccess.Context;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Diagnostics;
  using Microsoft.Extensions.DependencyInjection;

  public abstract class DatabaseTestBase
  {
    protected BbContext Context { get; private set; }

    protected void SetupDataContext()
    {
      var services = new ServiceCollection();

      services.AddDbContext<BbContext>(options =>
      {
        options.ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));

        options.UseInMemoryDatabase("bejebeje" + Guid.NewGuid().ToString());
      });

      var serviceProvider = services.BuildServiceProvider();

      Context = serviceProvider.GetService<BbContext>();
    }
  }
}
