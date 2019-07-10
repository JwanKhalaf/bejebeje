namespace Bejebeje.DataAccess.Data
{
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

          if (!context.Artists.Any())
          {
            Author acdcAuthor = new Author
            {
              FirstName = "AC/DC",
              FullName = "AC/DC",
              Biography = "<p>AC/DC are an Australian rock band formed in Sydney in 1973 by Scottish-born brothers Malcolm and Angus Young.[1] Their music has been variously described as hard rock, blues rock, and heavy metal;[2] however, the band themselves describe their music as simply 'rock and roll'.</p><p>AC/DC underwent several line-up changes before releasing their first album, High Voltage, in 1975. Membership subsequently stabilised around the Young brothers, singer Bon Scott, drummer Phil Rudd, and bass player Mark Evans. Evans was replaced by Cliff Williams in 1977 for the album Powerage.</p><p>In February 1980, a few months after recording the album Highway to Hell, lead singer and co-songwriter Bon Scott died of acute alcohol poisoning.[4] The group considered disbanding but stayed together, bringing in Brian Johnson as replacement for Scott.[5] Later that year, the band released their first album with Johnson, Back in Black, which they dedicated to Scott's memory. The album launched them to new heights of success and became one of the best selling albums of all time.</p>",
              Slugs = new List<AuthorSlug>
              {
                new AuthorSlug
                {
                  Name = "acdc",
                  IsPrimary = true,
                  CreatedAt = DateTime.UtcNow,
                },
              },
              Image = GetAuthorImage("acdc.jpg"),
              CreatedAt = DateTime.UtcNow,
            };

            Author hWolfAuthor = new Author
            {
              FirstName = "Chester Arthur",
              LastName = "Burnett",
              FullName = "Chester Arthur Burnett",
              Biography = "<p>Chester Arthur Burnett (June 10, 1910 – January 10, 1976), known as Howlin' Wolf, was a Chicago blues singer, guitarist, and harmonica player, originally from Mississippi.</p><p>With a booming voice and imposing physical presence, he is one of the best-known Chicago blues artists. The musician and critic Cub Koda noted, 'no one could match Howlin' Wolf for the singular ability to rock the house down to the foundation while simultaneously scaring its patrons out of its wits.'</p><p>Producer Sam Phillips recalled, 'When I heard Howlin' Wolf, I said, 'This is for me. This is where the soul of man never dies.' Several of his songs, including 'Smokestack Lightnin', 'Killing Floor' and 'Spoonful', have become blues and blues rock standards. In 2011, Rolling Stone magazine ranked him number 54 on its list of the '100 Greatest Artists of All Time'.</p>",
              Slugs = new List<AuthorSlug>
              {
                new AuthorSlug
                {
                  Name = "howling-wolf",
                  IsPrimary = true,
                  CreatedAt = DateTime.UtcNow,
                },
              },
              Image = GetAuthorImage("hwolf.jpg"),
              CreatedAt = DateTime.UtcNow,
              ModifiedAt = DateTime.UtcNow.AddMinutes(1),
            };

            context.Authors.Add(acdcAuthor);
            context.Authors.Add(hWolfAuthor);
            context.SaveChanges();

            Lyric tnt = new Lyric
            {
              Title = "TNT",
              MarkdownBody = @"<p>Oi, oi, oi<br/>Oi, oi, oi<br/>Oi, oi, oi<br/>Oi, oi, oi<br/>Oi, oi, oi</p><p>See me ride out of the sunset<br/>On your color TV screen<br/>Out for all that I can get<br/>If you know what I mean<br/>Women to the left of me<br/>And women to the right<br/>Ain't got no gun<br/>Ain't got no knife<br/>Don't you start no fight</p><p>'Cause I'm T.N.T. I'm dynamite<br/>T.N.T. and I'll win the fight<br/>T.N.T. I'm a power load<br/>T.N.T. watch me explode</p><p>I'm dirty, mean and mighty unclean<br/>I'm a wanted man<br/>Public enemy number one<br/>Understand<br/>So lock up your daughter<br/>Lock up your wife<br/>Lock up your back door<br/>And run for your life<br/>The man is back in town<br/>Don't you mess me 'round</p>",
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
              IsApproved = true,
              AuthorId = acdcAuthor.Id,
            };

            Lyric thunderstruck = new Lyric
            {
              Title = "Thunderstruck",
              MarkdownBody = @"<p>I was caught<br/>In the middle of a railroad track (thunder)<br/>I looked round<br/>And I knew there was no turning back (thunder)<br/>My mind raced<br/>And I thought what could I do (thunder)<br/>And I knew<br/>There was no help, no help from you (thunder)<br/>Sound of the drums<br/>Beating in my heart<br/>The thunder of guns<br/>Tore me apart<br/>You've been<br/>Thunderstruck</p><p>Rode down the highway<br/>Broke the limit, we hit the town<br/>Went through to Texas, yeah Texas, and we had some fun<br/>We met some girls<br/>Some dancers who gave a good time<br/>Broke all the rules<br/>Played all the fools<br/>Yeah yeah they, they, they blew our minds<br/>And I was shaking at the knees<br/>Could I come again please<br/>Yeah them ladies were too kind<br/>You've been<br/>Thunderstruck</p>",
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
              IsApproved = true,
              AuthorId = acdcAuthor.Id,
            };

            Lyric theThrillIsGone = new Lyric
            {
              Title = "The Thrill Is Gone",
              MarkdownBody = @"<p>The thrill is gone<br/>The thrill is gone away<br/>The thrill is gone baby<br/>The thrill is gone away<br/>You know you done me wrong baby<br/>And you'll be sorry someday</p><p>The thrill is gone<br/>It's gone away from me<br/>The thrill is gone baby<br/>The thrill is gone away from me<br/>Although, I'll still live on<br/>But so lonely I'll be</p><p>The thrill is gone<br/>It's gone away for good<br/>The thrill is gone baby<br/>It's gone away for good<br/>Someday I know I'll be over it all baby<br/>Just like I know a good man should</p><p>You know I'm free, free now baby<br/>I'm free from your spell<br/>Oh I'm free, free, free now<br/>I'm free from your spell<br/>And now that it's all over<br/>All I can do is wish you well</p>",
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

            Artist acdc = new Artist
            {
              FirstName = "AC/DC",
              LastName = string.Empty,
              FullName = "AC/DC",
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
              Image = GetArtistImage("acdc.jpg")
            };

            Artist bbKing = new Artist
            {
              FirstName = "BB",
              LastName = "King",
              FullName = "BB King",
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
              Image = GetArtistImage("bbking.jpg")
            };

            Artist canaanSmith = new Artist
            {
              FirstName = "Canaan",
              LastName = "Smith",
              FullName = "Canaan Smith",
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
              Image = GetArtistImage("csmith.jpg")
            };

            Artist damianMarley = new Artist
            {
              FirstName = "Damian",
              LastName = "Marley",
              FullName = "Damian Marley",
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
              Image = GetArtistImage("damian-marley.jpg")
            };

            Artist davidBowie = new Artist
            {
              FirstName = "David",
              LastName = "Bowie",
              FullName = "David Bowie",
              Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "david-bowie",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true
                }
              },
              IsApproved = true,
              UserId = userId,
              CreatedAt = DateTime.UtcNow,
              Image = GetArtistImage("dbowie.jpg")
            };

            Artist edSheeran = new Artist
            {
              FirstName = "Ed",
              LastName = "Sheeran",
              FullName = "Ed Sheeran",
              Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "ed-sheeran",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true
                }
              },
              IsApproved = true,
              UserId = userId,
              CreatedAt = DateTime.UtcNow,
              Image = GetArtistImage("esheeran.jpg")
            };

            Artist fleetwoodMac = new Artist
            {
              FirstName = "Fleetwood",
              LastName = "Mac",
              FullName = "Fleetwood Mac",
              Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "fleetwood-mac",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true
                }
              },
              IsApproved = true,
              UserId = userId,
              CreatedAt = DateTime.UtcNow,
              Image = GetArtistImage("fmac.jpg")
            };

            Artist georgeMichael = new Artist
            {
              FirstName = "George",
              LastName = "Michael",
              FullName = "George Michael",
              Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "george-michael",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true
                }
              },
              IsApproved = true,
              UserId = userId,
              CreatedAt = DateTime.UtcNow,
              Image = GetArtistImage("gmichael.jpg")
            };

            Artist howlingWolf = new Artist
            {
              FirstName = "Howling",
              LastName = "Wolf",
              FullName = "Howling Wolf",
              Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "howling-wolf",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true
                }
              },
              IsApproved = true,
              UserId = userId,
              CreatedAt = DateTime.UtcNow,
              Image = GetArtistImage("hwolf.jpg")
            };

            Artist iceT = new Artist
            {
              FirstName = "Ice",
              LastName = "T",
              FullName = "Ice T",
              Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "ice-t",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true
                }
              },
              IsApproved = true,
              UserId = userId,
              CreatedAt = DateTime.UtcNow,
              Image = GetArtistImage("itea.jpg")
            };

            Artist jenniferLopez = new Artist
            {
              FirstName = "Jennifer",
              LastName = "Lopez",
              FullName = "Jennifer Lopez",
              Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "jennifer-lopez",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true
                }
              },
              IsApproved = true,
              UserId = userId,
              CreatedAt = DateTime.UtcNow,
              Image = GetArtistImage("jlopez.jpg")
            };

            Artist kennyRogers = new Artist
            {
              FirstName = "Kenny",
              LastName = "Rogers",
              FullName = "Kenny Rogers",
              Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "kenny-rogers",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true
                }
              },
              IsApproved = true,
              UserId = userId,
              CreatedAt = DateTime.UtcNow,
              Image = GetArtistImage("krogers.jpg")
            };

            Artist ladyGaga = new Artist
            {
              FirstName = "Lady",
              LastName = "Gaga",
              FullName = "Lady Gaga",
              Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "lady-gaga",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true
                }
              },
              IsApproved = true,
              UserId = userId,
              CreatedAt = DateTime.UtcNow,
              Image = GetArtistImage("lgaga.jpg")
            };

            Artist muddyWaters = new Artist
            {
              FirstName = "Muddy",
              LastName = "Waters",
              FullName = "Muddy Waters",
              Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "muddy-waters",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true
                }
              },
              IsApproved = true,
              UserId = userId,
              CreatedAt = DateTime.UtcNow,
              Image = GetArtistImage("mwaters.jpg")
            };

            Artist neilYoung = new Artist
            {
              FirstName = "Neil",
              LastName = "Young",
              FullName = "Neil Young",
              Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "neil-young",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true
                }
              },
              IsApproved = true,
              UserId = userId,
              CreatedAt = DateTime.UtcNow,
              Image = GetArtistImage("nyoung.jpg")
            };

            Artist ozzyOsbourne = new Artist
            {
              FirstName = "Ozzy",
              LastName = "Osbourne",
              FullName = "Ozzy Osbourne",
              Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "ozzy-osbourne",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true
                }
              },
              IsApproved = true,
              UserId = userId,
              CreatedAt = DateTime.UtcNow,
              Image = GetArtistImage("oosbourne.jpg")
            };

            Artist pattiSmith = new Artist
            {
              FirstName = "Patti",
              LastName = "Smith",
              FullName = "Patti Smith",
              Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "patti-smith",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true
                }
              },
              IsApproved = true,
              UserId = userId,
              CreatedAt = DateTime.UtcNow,
              Image = GetArtistImage("psmith.jpg")
            };

            Artist queen = new Artist
            {
              FirstName = "Queen",
              LastName = "",
              FullName = "Queen",
              Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "queen",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true
                }
              },
              IsApproved = true,
              UserId = userId,
              CreatedAt = DateTime.UtcNow,
              Image = GetArtistImage("queen.jpg")
            };

            Artist rayCharles = new Artist
            {
              FirstName = "Ray",
              LastName = "Charles",
              FullName = "Ray Charles",
              Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "ray-charles",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true
                }
              },
              IsApproved = true,
              UserId = userId,
              CreatedAt = DateTime.UtcNow,
              Image = GetArtistImage("rcharles.jpg")
            };

            Artist sonnyBoyWilliamson = new Artist
            {
              FirstName = "Sonny Boy",
              LastName = "Williamson",
              FullName = "Sonny Boy Williamson",
              Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "sonny-boy-williamson",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true
                }
              },
              IsApproved = true,
              UserId = userId,
              CreatedAt = DateTime.UtcNow,
              Image = GetArtistImage("sboywilliamson.jpg")
            };

            Artist tBoneWalker = new Artist
            {
              FirstName = "T Bone",
              LastName = "Walker",
              FullName = "T Bone Walker",
              Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "t-bone-walker",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true
                }
              },
              IsApproved = true,
              UserId = userId,
              CreatedAt = DateTime.UtcNow,
              Image = GetArtistImage("tbonewalker.jpg")
            };

            Artist u2 = new Artist
            {
              FirstName = "U2",
              LastName = "",
              FullName = "U2",
              Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "u2",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true
                }
              },
              IsApproved = true,
              UserId = userId,
              CreatedAt = DateTime.UtcNow,
              Image = GetArtistImage("u2.jpg")
            };

            Artist vanHalen = new Artist
            {
              FirstName = "Van",
              LastName = "Halen",
              FullName = "Van Halen",
              Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "van-halen",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true
                }
              },
              IsApproved = true,
              UserId = userId,
              CreatedAt = DateTime.UtcNow,
              Image = GetArtistImage("vhalen.jpg")
            };

            Artist wheatus = new Artist
            {
              FirstName = "Wheatus",
              LastName = "",
              FullName = "Wheatus",
              Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "wheatus",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true
                }
              },
              IsApproved = true,
              UserId = userId,
              CreatedAt = DateTime.UtcNow,
              Image = GetArtistImage("wheatus.jpg")
            };

            Artist xtc = new Artist
            {
              FirstName = "XTC",
              LastName = "",
              FullName = "XTC",
              Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "xtc",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true
                }
              },
              IsApproved = true,
              UserId = userId,
              CreatedAt = DateTime.UtcNow,
              Image = GetArtistImage("xtc.jpg")
            };

            Artist yolandaBeCool = new Artist
            {
              FirstName = "Yolanda",
              LastName = "Be Cool",
              FullName = "Yolanda Be Cool",
              Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "yolanda-be-cool",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true
                }
              },
              IsApproved = true,
              UserId = userId,
              CreatedAt = DateTime.UtcNow,
              Image = GetArtistImage("yolanda-be-cool.jpg")
            };

            Artist zacBrown = new Artist
            {
              FirstName = "Zac",
              LastName = "Brown",
              FullName = "Zac Brown",
              Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "zac-brown",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true
                }
              },
              IsApproved = true,
              UserId = userId,
              CreatedAt = DateTime.UtcNow,
              Image = GetArtistImage("zbrown.jpg")
            };

            List<Artist> artists = new List<Artist> { acdc, bbKing, canaanSmith, damianMarley, davidBowie, edSheeran, fleetwoodMac, georgeMichael, howlingWolf, iceT, jenniferLopez, kennyRogers, ladyGaga, muddyWaters, neilYoung, ozzyOsbourne, pattiSmith, queen, rayCharles, sonnyBoyWilliamson, tBoneWalker, u2, vanHalen, wheatus, xtc, yolandaBeCool, zacBrown };

            context.Artists.AddRange(artists);
            context.SaveChanges();
          }
          else
          {
            Console.WriteLine("Database already has sample data.");
          }
        }
      }
    }

    private ArtistImage GetArtistImage(string imageName)
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

    private AuthorImage GetAuthorImage(string imageName)
    {
      string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
      string imageFilePath = $"{baseDirectory}/Data/SeedImages/" + imageName;

      if (File.Exists(imageFilePath))
      {
        byte[] imagesBytes = File.ReadAllBytes(imageFilePath);

        AuthorImage authorImage = new AuthorImage
        {
          Data = imagesBytes,
          CreatedAt = DateTime.UtcNow
        };

        return authorImage;
      }

      return null;
    }
  }
}
