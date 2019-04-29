using System;
using System.Collections.Generic;
using System.Linq;
using Bejebeje.DataAccess.Configuration;
using Bejebeje.DataAccess.Context;
using Bejebeje.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Bejebeje.DataAccess.Data
{
  public class DataSeeder
  {
    private InitialSeedConfiguration seedConfiguration { get; set; }

    public DataSeeder(IOptions<InitialSeedConfiguration> initialSeedConfiguration)
    {
      seedConfiguration = initialSeedConfiguration.Value;
    }

    public void EnsureDataIsSeeded()
    {
      ServiceCollection services = new ServiceCollection();

      services
        .AddDbContext<BbContext>(options => options.UseNpgsql(seedConfiguration.ConnectionString));

      using (ServiceProvider serviceProvider = services.BuildServiceProvider())
      {
        using (IServiceScope scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
          BbContext context = scope
            .ServiceProvider
            .GetService<BbContext>();

          context.Database.Migrate();

          if (!context.Artists.Any())
          {
            Artist sivanPerwer = new Artist
            {
              FirstName = "Şivan",
              LastName = "Perwer",
              Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "sivan-perwer",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true
                }
              },
              IsApproved = true,
              ImageUrl = "https://placehold.it/100x100",
              UserId = "eb600251-1fdd-4c2e-bee0-a9dca87a271a",
              CreatedAt = DateTime.UtcNow
            };

            Artist hesenzirek = new Artist
            {
              FirstName = "Hesen",
              LastName = "Zîrek",
              Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "hesen-zirek",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true
                },
                new ArtistSlug
                {
                  Name = "hasan-zirak",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = false
                }
              },
              IsApproved = true,
              ImageUrl = "https://placehold.it/100x100",
              UserId = "eb600251-1fdd-4c2e-bee0-a9dca87a271a",
              CreatedAt = DateTime.UtcNow
            };

            Artist nasirRezazi = new Artist
            {
              FirstName = "Nasir",
              LastName = "Rezazî",
              Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "nasir-rezazi",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true
                },
                new ArtistSlug
                {
                  Name = "naser-razazi",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = false
                }
              },
              IsApproved = true,
              ImageUrl = "https://placehold.it/100x100",
              UserId = "eb600251-1fdd-4c2e-bee0-a9dca87a271a",
              CreatedAt = DateTime.UtcNow
            };

            Artist ciwanHaco = new Artist
            {
              FirstName = "Ciwan",
              LastName = "Haco",
              Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "ciwan-haco",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true
                }
              },
              IsApproved = true,
              ImageUrl = "https://placehold.it/100x100",
              UserId = "eb600251-1fdd-4c2e-bee0-a9dca87a271a",
              CreatedAt = DateTime.UtcNow
            };

            context.Artists.Add(sivanPerwer);
            context.Artists.Add(hesenzirek);
            context.Artists.Add(nasirRezazi);
            context.Artists.Add(ciwanHaco);

            context.SaveChanges();
          }
          else
          {
            Console.WriteLine("Database already has sample data.");
          }
        }
      }
    }
  }
}
