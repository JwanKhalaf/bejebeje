using System;
using System.Collections.Generic;
using System.IO;
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

          Lyric tnt = new Lyric
          {
            Title = "TNT",
            Body = @"<p>Oi, oi, oi
                    Oi, oi, oi
                    Oi, oi, oi
                    Oi, oi, oi
                    Oi, oi, oi</p>
                    <p>See me ride out of the sunset
                    On your color TV screen
                    Out for all that I can get
                    If you know what I mean
                    Women to the left of me
                    And women to the right
                    Ain't got no gun
                    Ain't got no knife
                    Don't you start no fight</p>
                    <p>'Cause I'm T.N.T. I'm dynamite
                    T.N.T. and I'll win the fight
                    T.N.T. I'm a power load
                    T.N.T. watch me explode</p>
                    <p>I'm dirty, mean and mighty unclean
                    I'm a wanted man
                    Public enemy number one
                    Understand
                    So lock up your daughter
                    Lock up your wife
                    Lock up your back door
                    And run for your life
                    The man is back in town
                    Don't you mess me 'round</p>",
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

          Lyric thunderstruck = new Lyric
          {
            Title = "Thunderstruck",
            Body = @"<p>I was caught
                  In the middle of a railroad track (thunder)
                  I looked round
                  And I knew there was no turning back (thunder)
                  My mind raced
                  And I thought what could I do (thunder)
                  And I knew
                  There was no help, no help from you (thunder)
                  Sound of the drums
                  Beating in my heart
                  The thunder of guns
                  Tore me apart
                  You've been
                  Thunderstruck</p>

                  <p>Rode down the highway
                  Broke the limit, we hit the town
                  Went through to Texas, yeah Texas, and we had some fun
                  We met some girls
                  Some dancers who gave a good time
                  Broke all the rules
                  Played all the fools
                  Yeah yeah they, they, they blew our minds
                  And I was shaking at the knees
                  Could I come again please
                  Yeah them ladies were too kind
                  You've been
                  Thunderstruck</p>",
            UserId = userId,
            Slugs = new List<LyricSlug>
            {
              new LyricSlug
              {
                Name = "thunderstruck",
                CreatedAt = DateTime.UtcNow,
                IsPrimary = true
              }
            },
            CreatedAt = DateTime.UtcNow,
            IsApproved = true
          };

          Lyric theThrillIsGone = new Lyric
          {
            Title = "The Thrill Is Gone",
            Body = @"<p>The thrill is gone
                    The thrill is gone away
                    The thrill is gone baby
                    The thrill is gone away
                    You know you done me wrong baby
                    And you'll be sorry someday</p>

                    <p>The thrill is gone
                    It's gone away from me
                    The thrill is gone baby
                    The thrill is gone away from me
                    Although, I'll still live on
                    But so lonely I'll be</p>

                    <p>The thrill is gone
                    It's gone away for good
                    The thrill is gone baby
                    It's gone away for good
                    Someday I know I'll be over it all baby
                    Just like I know a good man should</p>

                    <p>You know I'm free, free now baby
                    I'm free from your spell
                    Oh I'm free, free, free now
                    I'm free from your spell
                    And now that it's all over
                    All I can do is wish you well</p>",
            UserId = userId,
            Slugs = new List<LyricSlug>
            {
              new LyricSlug
              {
                Name = "the-thrill-is-gone",
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
              Lyrics = new List<Lyric> { tnt, thunderstruck },
              Image = GetImage("acdc.jpg")
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
              CreatedAt = DateTime.UtcNow,
              Lyrics = new List<Lyric> { theThrillIsGone },
              Image = GetImage("bbking.jpg")
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
              CreatedAt = DateTime.UtcNow,
              Image = GetImage("damian-marley.jpg")
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
              CreatedAt = DateTime.UtcNow,
              Image = GetImage("csmith.jpg")
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

    private ArtistImage GetImage(string imageName)
    {
      string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
      string imageFilePath = $"{baseDirectory}/Data/SeedImages/" + imageName;

      if (File.Exists(imageFilePath))
      {
        byte[] imagesBytes = File.ReadAllBytes(imageFilePath);

        ArtistImage artistImage = new ArtistImage
        {
          Data = imagesBytes,
          CreatedAt = DateTime.UtcNow
        };

        return artistImage;
      }

      return null;
    }
  }
}
