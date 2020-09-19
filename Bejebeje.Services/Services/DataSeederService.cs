namespace Bejebeje.Services.Services
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Threading.Tasks;
  using DataAccess.Context;
  using Domain;
  using Interfaces;
  using Microsoft.EntityFrameworkCore;

  public class DataSeederService : IDataSeederService
  {
    private const string UserId = "eb600251-1fdd-4c2e-bee0-a9dca87a271a";

    private readonly BbContext _context;

    public DataSeederService(
      BbContext context)
    {
      _context = context;
    }

    public async Task SeedDataAsync()
    {
      await _context.Database.MigrateAsync();

      if (!_context.Artists.Any())
      {
        Author acdcAuthor = new Author
        {
          FirstName = "ac/dc",
          FullName = "ac/dc",
          Biography = @"AC/DC are an Australian rock band formed in Sydney in 1973 by Scottish-born brothers Malcolm and Angus Young. Their music has been variously described as hard rock, blues rock, and heavy metal; however, the band themselves describe their music as simply 'rock and roll'.
          
AC/DC underwent several line-up changes before releasing their first album, High Voltage, in 1975. Membership subsequently stabilised around the Young brothers, singer Bon Scott, drummer Phil Rudd, and bass player Mark Evans. Evans was replaced by Cliff Williams in 1977 for the album Powerage.
          
In February 1980, a few months after recording the album Highway to Hell, lead singer and co-songwriter Bon Scott died of acute alcohol poisoning. The group considered disbanding but stayed together, bringing in Brian Johnson as replacement for Scott. Later that year, the band released their first album with Johnson, Back in Black, which they dedicated to Scott's memory. The album launched them to new heights of success and became one of the best selling albums of all time.",
          Slugs = new List<AuthorSlug>
              {
                new AuthorSlug
                {
                  Name = "acdc",
                  IsPrimary = true,
                  CreatedAt = DateTime.UtcNow,
                },
              },
          CreatedAt = DateTime.UtcNow,
        };

        Author hWolfAuthor = new Author
        {
          FirstName = "chester arthur",
          LastName = "burnett",
          FullName = "chester arthur burnett",
          Biography = @"Chester Arthur Burnett (June 10, 1910 – January 10, 1976), known as Howlin' Wolf, was a Chicago blues singer, guitarist, and harmonica player, originally from Mississippi.
          
With a booming voice and imposing physical presence, he is one of the best-known Chicago blues artists. The musician and critic Cub Koda noted, 'no one could match Howlin' Wolf for the singular ability to rock the house down to the foundation while simultaneously scaring its patrons out of its wits.'
          
Producer Sam Phillips recalled, 'When I heard Howlin' Wolf, I said, 'This is for me. This is where the soul of man never dies.' Several of his songs, including 'Smokestack Lightnin', 'Killing Floor' and 'Spoonful', have become blues and blues rock standards. In 2011, Rolling Stone magazine ranked him number 54 on its list of the '100 Greatest Artists of All Time'.",
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

        _context.Authors.Add(acdcAuthor);
        _context.Authors.Add(hWolfAuthor);
        _context.SaveChanges();

        Lyric tnt = new Lyric
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
          UserId = UserId,
          Slugs = new List<LyricSlug>
              {
                new LyricSlug
                {
                  Name = "tnt",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true,
                },
              },
          CreatedAt = DateTime.UtcNow,
          IsApproved = true,
          AuthorId = acdcAuthor.Id,
        };

        Lyric thunderstruck = new Lyric
        {
          Title = "Thunderstruck",
          Body = @"I was caught
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
Thunderstruck

Rode down the highway
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
Thunderstruck",
          UserId = UserId,
          Slugs = new List<LyricSlug>
              {
                new LyricSlug
                {
                  Name = "thunderstruck",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true,
                },
              },
          CreatedAt = DateTime.UtcNow,
          IsApproved = true,
          AuthorId = acdcAuthor.Id,
        };

        Lyric theThrillIsGone = new Lyric
        {
          Title = "The Thrill Is Gone",
          Body = @"The thrill is gone
The thrill is gone away
The thrill is gone baby
The thrill is gone away
You know you done me wrong baby
And you'll be sorry someday

The thrill is gone
It's gone away from me
The thrill is gone baby
The thrill is gone away from me
Although, I'll still live on
But so lonely I'll be

The thrill is gone
It's gone away for good
The thrill is gone baby
It's gone away for good
Someday I know I'll be over it all baby
Just like I know a good man should

You know I'm free, free now baby
I'm free from your spell
Oh I'm free, free, free now
I'm free from your spell
And now that it's all over
All I can do is wish you well",
          UserId = UserId,
          Slugs = new List<LyricSlug>
              {
                new LyricSlug
                {
                  Name = "the-thrill-is-gone",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true,
                },
              },
          CreatedAt = DateTime.UtcNow,
          IsApproved = true,
        };

        Artist acdc = new Artist
        {
          FirstName = "ac/dc",
          LastName = string.Empty,
          FullName = "ac/dc",
          Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "acdc",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true,
                },
              },
          IsApproved = true,
          UserId = UserId,
          CreatedAt = DateTime.UtcNow,
          Lyrics = new List<Lyric> { tnt, thunderstruck },
          Image = GetArtistImage("acdc.jpg"),
        };

        Artist bbKing = new Artist
        {
          FirstName = "bb",
          LastName = "king",
          FullName = "bb king",
          Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "bb-king",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true,
                },
              },
          IsApproved = true,
          UserId = UserId,
          CreatedAt = DateTime.UtcNow,
          Lyrics = new List<Lyric> { theThrillIsGone },
          Image = GetArtistImage("bbking.jpg"),
        };

        Artist canaanSmith = new Artist
        {
          FirstName = "canaan",
          LastName = "smith",
          FullName = "canaan smith",
          Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "canaan-smith",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true,
                },
              },
          IsApproved = true,
          UserId = UserId,
          CreatedAt = DateTime.UtcNow,
          Image = GetArtistImage("csmith.jpg"),
        };

        Artist damianMarley = new Artist
        {
          FirstName = "damian",
          LastName = "marley",
          FullName = "damian marley",
          Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "damian-marley",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true,
                },
              },
          IsApproved = true,
          UserId = UserId,
          CreatedAt = DateTime.UtcNow,
          Image = GetArtistImage("damian-marley.jpg"),
        };

        Artist davidBowie = new Artist
        {
          FirstName = "david",
          LastName = "bowie",
          FullName = "david bowie",
          Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "david-bowie",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true,
                },
              },
          IsApproved = true,
          UserId = UserId,
          CreatedAt = DateTime.UtcNow,
          Image = GetArtistImage("dbowie.jpg"),
        };

        Artist edSheeran = new Artist
        {
          FirstName = "ed",
          LastName = "sheeran",
          FullName = "ed sheeran",
          Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "ed-sheeran",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true,
                },
              },
          IsApproved = true,
          UserId = UserId,
          CreatedAt = DateTime.UtcNow,
          Image = GetArtistImage("esheeran.jpg"),
        };

        Artist fleetwoodMac = new Artist
        {
          FirstName = "fleetwood",
          LastName = "mac",
          FullName = "fleetwood mac",
          Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "fleetwood-mac",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true,
                },
              },
          IsApproved = true,
          UserId = UserId,
          CreatedAt = DateTime.UtcNow,
          Image = GetArtistImage("fmac.jpg"),
        };

        Artist georgeMichael = new Artist
        {
          FirstName = "george",
          LastName = "michael",
          FullName = "george michael",
          Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "george-michael",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true,
                },
              },
          IsApproved = true,
          UserId = UserId,
          CreatedAt = DateTime.UtcNow,
          Image = GetArtistImage("gmichael.jpg"),
        };

        Artist howlingWolf = new Artist
        {
          FirstName = "howling",
          LastName = "wolf",
          FullName = "howling wolf",
          Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "howling-wolf",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true,
                },
              },
          IsApproved = true,
          UserId = UserId,
          CreatedAt = DateTime.UtcNow,
          Image = GetArtistImage("hwolf.jpg"),
        };

        Artist iceT = new Artist
        {
          FirstName = "ice",
          LastName = "t",
          FullName = "ice t",
          Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "ice-t",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true,
                },
              },
          IsApproved = true,
          UserId = UserId,
          CreatedAt = DateTime.UtcNow,
          Image = GetArtistImage("itea.jpg"),
        };

        Artist jenniferLopez = new Artist
        {
          FirstName = "jennifer",
          LastName = "lopez",
          FullName = "jennifer lopez",
          Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "jennifer-lopez",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true,
                },
              },
          IsApproved = true,
          UserId = UserId,
          CreatedAt = DateTime.UtcNow,
          Image = GetArtistImage("jlopez.jpg"),
        };

        Artist kennyRogers = new Artist
        {
          FirstName = "kenny",
          LastName = "rogers",
          FullName = "kenny rogers",
          Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "kenny-rogers",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true,
                },
              },
          IsApproved = true,
          UserId = UserId,
          CreatedAt = DateTime.UtcNow,
          Image = GetArtistImage("krogers.jpg"),
        };

        Artist ladyGaga = new Artist
        {
          FirstName = "lady",
          LastName = "gaga",
          FullName = "lady gaga",
          Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "lady-gaga",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true,
                },
              },
          IsApproved = true,
          UserId = UserId,
          CreatedAt = DateTime.UtcNow,
          Image = GetArtistImage("lgaga.jpg"),
        };

        Artist muddyWaters = new Artist
        {
          FirstName = "muddy",
          LastName = "waters",
          FullName = "muddy waters",
          Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "muddy-waters",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true,
                },
              },
          IsApproved = true,
          UserId = UserId,
          CreatedAt = DateTime.UtcNow,
          Image = GetArtistImage("mwaters.jpg"),
        };

        Artist neilYoung = new Artist
        {
          FirstName = "neil",
          LastName = "young",
          FullName = "neil young",
          Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "neil-young",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true,
                },
              },
          IsApproved = true,
          UserId = UserId,
          CreatedAt = DateTime.UtcNow,
          Image = GetArtistImage("nyoung.jpg"),
        };

        Artist ozzyOsbourne = new Artist
        {
          FirstName = "ozzy",
          LastName = "osbourne",
          FullName = "ozzy osbourne",
          Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "ozzy-osbourne",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true,
                },
              },
          IsApproved = true,
          UserId = UserId,
          CreatedAt = DateTime.UtcNow,
          Image = GetArtistImage("oosbourne.jpg"),
        };

        Artist pattiSmith = new Artist
        {
          FirstName = "patti",
          LastName = "smith",
          FullName = "patti smith",
          Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "patti-smith",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true,
                },
              },
          IsApproved = true,
          UserId = UserId,
          CreatedAt = DateTime.UtcNow,
          Image = GetArtistImage("psmith.jpg"),
        };

        Artist queen = new Artist
        {
          FirstName = "queen",
          LastName = string.Empty,
          FullName = "queen",
          Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "queen",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true,
                },
              },
          IsApproved = true,
          UserId = UserId,
          CreatedAt = DateTime.UtcNow,
          Image = GetArtistImage("queen.jpg"),
        };

        Artist rayCharles = new Artist
        {
          FirstName = "ray",
          LastName = "charles",
          FullName = "ray charles",
          Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "ray-charles",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true,
                },
              },
          IsApproved = true,
          UserId = UserId,
          CreatedAt = DateTime.UtcNow,
          Image = GetArtistImage("rcharles.jpg"),
        };

        Artist sonnyBoyWilliamson = new Artist
        {
          FirstName = "sonny boy",
          LastName = "williamson",
          FullName = "sonny boy williamson",
          Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "sonny-boy-williamson",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true,
                },
              },
          IsApproved = true,
          UserId = UserId,
          CreatedAt = DateTime.UtcNow,
          Image = GetArtistImage("sboywilliamson.jpg"),
        };

        Artist tBoneWalker = new Artist
        {
          FirstName = "t bone",
          LastName = "walker",
          FullName = "t bone walker",
          Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "t-bone-walker",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true,
                },
              },
          IsApproved = true,
          UserId = UserId,
          CreatedAt = DateTime.UtcNow,
          Image = GetArtistImage("tbonewalker.jpg"),
        };

        Artist u2 = new Artist
        {
          FirstName = "u2",
          LastName = string.Empty,
          FullName = "u2",
          Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "u2",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true,
                },
              },
          IsApproved = true,
          UserId = UserId,
          CreatedAt = DateTime.UtcNow,
          Image = GetArtistImage("u2.jpg"),
        };

        Artist vanHalen = new Artist
        {
          FirstName = "van",
          LastName = "halen",
          FullName = "van halen",
          Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "van-halen",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true,
                },
              },
          IsApproved = true,
          UserId = UserId,
          CreatedAt = DateTime.UtcNow,
          Image = GetArtistImage("vhalen.jpg"),
        };

        Artist wheatus = new Artist
        {
          FirstName = "wheatus",
          LastName = string.Empty,
          FullName = "wheatus",
          Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "wheatus",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true,
                },
              },
          IsApproved = true,
          UserId = UserId,
          CreatedAt = DateTime.UtcNow,
        };

        Artist xtc = new Artist
        {
          FirstName = "xtc",
          LastName = string.Empty,
          FullName = "xtc",
          Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "xtc",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true,
                },
              },
          IsApproved = true,
          UserId = UserId,
          CreatedAt = DateTime.UtcNow,
          Image = GetArtistImage("xtc.jpg"),
        };

        Artist yolandaBeCool = new Artist
        {
          FirstName = "yolanda",
          LastName = "be cool",
          FullName = "yolanda be cool",
          Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "yolanda-be-cool",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true,
                },
              },
          IsApproved = true,
          UserId = UserId,
          CreatedAt = DateTime.UtcNow,
          Image = GetArtistImage("yolanda-be-cool.jpg"),
        };

        Artist zacBrown = new Artist
        {
          FirstName = "zac",
          LastName = "brown",
          FullName = "zac brown",
          Slugs = new List<ArtistSlug>
              {
                new ArtistSlug
                {
                  Name = "zac-brown",
                  CreatedAt = DateTime.UtcNow,
                  IsPrimary = true,
                },
              },
          IsApproved = true,
          UserId = UserId,
          CreatedAt = DateTime.UtcNow,
          Image = GetArtistImage("zbrown.jpg"),
        };

        List<Artist> artists = new List<Artist> { acdc, bbKing, canaanSmith, damianMarley, davidBowie, edSheeran, fleetwoodMac, georgeMichael, howlingWolf, iceT, jenniferLopez, kennyRogers, ladyGaga, muddyWaters, neilYoung, ozzyOsbourne, pattiSmith, queen, rayCharles, sonnyBoyWilliamson, tBoneWalker, u2, vanHalen, wheatus, xtc, yolandaBeCool, zacBrown };

        _context.Artists.AddRange(artists);
        _context.SaveChanges();
      }
      else
      {
        Console.WriteLine("Database already has sample data.");
      }
    }

    private ArtistImage GetArtistImage(
      string imageName)
    {
      string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
      string imageFilePath = $"{baseDirectory}/Data/SeedImages/" + imageName;

      if (File.Exists(imageFilePath))
      {
        byte[] imagesBytes = File.ReadAllBytes(imageFilePath);

        ArtistImage artistImage = new ArtistImage
        {
          Data = imagesBytes,
          CreatedAt = DateTime.UtcNow,
        };

        return artistImage;
      }

      return null;
    }

    private AuthorImage GetAuthorImage(
      string imageName)
    {
      string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
      string imageFilePath = $"{baseDirectory}/Data/SeedImages/" + imageName;

      if (File.Exists(imageFilePath))
      {
        byte[] imagesBytes = File.ReadAllBytes(imageFilePath);

        AuthorImage authorImage = new AuthorImage
        {
          Data = imagesBytes,
          CreatedAt = DateTime.UtcNow,
        };

        return authorImage;
      }

      return null;
    }
  }
}
