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
            Body = @"<p>Oi, oi, oi<br/>Oi, oi, oi<br/>Oi, oi, oi<br/>Oi, oi, oi<br/>Oi, oi, oi</p><p>See me ride out of the sunset<br/>On your color TV screen<br/>Out for all that I can get<br/>If you know what I mean<br/>Women to the left of me<br/>And women to the right<br/>Ain't got no gun<br/>Ain't got no knife<br/>Don't you start no fight</p><p>'Cause I'm T.N.T. I'm dynamite<br/>T.N.T. and I'll win the fight<br/>T.N.T. I'm a power load<br/>T.N.T. watch me explode</p><p>I'm dirty, mean and mighty unclean<br/>I'm a wanted man<br/>Public enemy number one<br/>Understand<br/>So lock up your daughter<br/>Lock up your wife<br/>Lock up your back door<br/>And run for your life<br/>The man is back in town<br/>Don't you mess me 'round</p>",
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
            Body = @"<p>I was caught<br/>In the middle of a railroad track (thunder)<br/>I looked round<br/>And I knew there was no turning back (thunder)<br/>My mind raced<br/>And I thought what could I do (thunder)<br/>And I knew<br/>There was no help, no help from you (thunder)<br/>Sound of the drums<br/>Beating in my heart<br/>The thunder of guns<br/>Tore me apart<br/>You've been<br/>Thunderstruck</p><p>Rode down the highway<br/>Broke the limit, we hit the town<br/>Went through to Texas, yeah Texas, and we had some fun<br/>We met some girls<br/>Some dancers who gave a good time<br/>Broke all the rules<br/>Played all the fools<br/>Yeah yeah they, they, they blew our minds<br/>And I was shaking at the knees<br/>Could I come again please<br/>Yeah them ladies were too kind<br/>You've been<br/>Thunderstruck</p>",
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
            Body = @"<p>The thrill is gone<br/>The thrill is gone away<br/>The thrill is gone baby<br/>The thrill is gone away<br/>You know you done me wrong baby<br/>And you'll be sorry someday</p><p>The thrill is gone<br/>It's gone away from me<br/>The thrill is gone baby<br/>The thrill is gone away from me<br/>Although, I'll still live on<br/>But so lonely I'll be</p><p>The thrill is gone<br/>It's gone away for good<br/>The thrill is gone baby<br/>It's gone away for good<br/>Someday I know I'll be over it all baby<br/>Just like I know a good man should</p><p>You know I'm free, free now baby<br/>I'm free from your spell<br/>Oh I'm free, free, free now<br/>I'm free from your spell<br/>And now that it's all over<br/>All I can do is wish you well</p>",
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
