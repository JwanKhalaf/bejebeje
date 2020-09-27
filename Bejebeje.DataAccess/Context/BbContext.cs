namespace Bejebeje.DataAccess.Context
{
  using Domain;
  using Microsoft.EntityFrameworkCore;

  public class BbContext : DbContext
  {
    public BbContext(DbContextOptions<BbContext> options)
        : base(options)
    {
    }

    public DbSet<Artist> Artists { get; set; }

    public DbSet<ArtistSlug> ArtistSlugs { get; set; }

    public DbSet<Lyric> Lyrics { get; set; }

    public DbSet<LyricSlug> LyricSlugs { get; set; }

    public DbSet<Author> Authors { get; set; }

    public DbSet<AuthorSlug> AuthorSlugs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
      base.OnModelCreating(builder);
    }
  }
}