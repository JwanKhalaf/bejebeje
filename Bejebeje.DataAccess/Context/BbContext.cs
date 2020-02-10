namespace Bejebeje.DataAccess.Context
{
  using Bejebeje.Common.Extensions;
  using Bejebeje.Domain;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Metadata;

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

    public DbSet<ArtistImage> ArtistImages { get; set; }

    public DbSet<Author> Authors { get; set; }

    public DbSet<AuthorImage> AuthorImages { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
      base.OnModelCreating(builder);

      builder.Entity<Artist>()
            .HasOne(a => a.Image)
            .WithOne(a => a.Artist)
            .HasForeignKey<ArtistImage>(ai => ai.ArtistId);

      builder.Entity<Author>()
            .HasOne(a => a.Image)
            .WithOne(a => a.Author)
            .HasForeignKey<AuthorImage>(ai => ai.AuthorId);
    }
  }
}