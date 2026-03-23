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

    public DbSet<LyricReport> LyricReports { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
      base.OnModelCreating(builder);

      builder.Entity<LyricReport>(entity =>
      {
        entity.HasIndex(e => new { e.UserId, e.CreatedAt })
          .HasDatabaseName("ix_lyric_reports_user_id_created_at");

        entity.HasIndex(e => new { e.UserId, e.LyricId, e.Status })
          .HasDatabaseName("ix_lyric_reports_user_id_lyric_id_status");

        entity.HasIndex(e => e.LyricId)
          .HasDatabaseName("ix_lyric_reports_lyric_id");
      });
    }
  }
}