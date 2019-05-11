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
      string userId = "eb600251-1fdd-4c2e-bee0-a9dca87a271a";

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

          var tnt = new Lyric
          {
            Title = "TNT",
            Body = @"Oi, oi, oi
                    Oi, oi, oi
                    Oi, oi, oi
                    Oi, oi, oi
                    Oi, oi, oi
                    See me ride out of the sunset
                    On your color TV screen
                    Out for all that I can get
                    If you know what I mean
                    Women to the left of me
                    And women to the right
                    Ain't got no gun
                    Ain't got no knife
                    Don't you start no fight
                    'Cause I'm T.N.T. I'm dynamite
                    T.N.T. and I'll win the fight
                    T.N.T. I'm a power load
                    T.N.T. watch me explode
                    I'm dirty, mean and mighty unclean
                    I'm a wanted man
                    Public enemy number one
                    Understand
                    So lock up your daughter
                    Lock up your wife
                    Lock up your back door
                    And run for your life
                    The man is back in town
                    Don't you mess me 'round",
            UserId = userId,
            Slugs = new List<LyricSlug>
            {
              new LyricSlug
              {
                Name = "tnt",
                CreatedAt = DateTime.UtcNow,
                IsPrimary = true
              }
            },
            CreatedAt = DateTime.UtcNow,
            IsApproved = true
          };

          if (!context.Artists.Any())
          {
            Artist acdc = new Artist
            {
              FirstName = "AC/DC",
              LastName = string.Empty,
              Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "acdc",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true
                }
              },
              IsApproved = true,
              UserId = userId,
              CreatedAt = DateTime.UtcNow,
              Lyrics = new List<Lyric> { tnt }
            };

            Artist bbKing = new Artist
            {
              FirstName = "BB",
              LastName = "King",
              Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "bb-king",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true
                }
              },
              IsApproved = true,
              UserId = userId,
              CreatedAt = DateTime.UtcNow
            };

            Artist damianMarley = new Artist
            {
              FirstName = "Damian",
              LastName = "Marley",
              Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "damian-marley",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true
                }
              },
              IsApproved = true,
              UserId = userId,
              CreatedAt = DateTime.UtcNow
            };

            Artist canaanSmith = new Artist
            {
              FirstName = "Canaan",
              LastName = "Smith",
              Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "canaan-smith",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true
                }
              },
              IsApproved = true,
              UserId = userId,
              CreatedAt = DateTime.UtcNow
            };

            context.Artists.Add(acdc);
            context.Artists.Add(bbKing);
            context.Artists.Add(damianMarley);
            context.Artists.Add(canaanSmith);

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
