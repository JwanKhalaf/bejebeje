namespace Bejebeje.DataAccess.Context
{
  using Bejebeje.Shared.Domain;
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

    public DbSet<User> Users { get; set; }

    public DbSet<PointEvent> PointEvents { get; set; }

    public DbSet<Like> Likes { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
      base.OnModelCreating(builder);

      builder.Entity<Like>(entity =>
      {
        entity.ToTable("likes");

        entity.HasKey(e => new { e.UserId, e.LyricId })
          .HasName("primary_key");
      });

      builder.Entity<LyricReport>(entity =>
      {
        entity.HasIndex(e => new { e.UserId, e.CreatedAt })
          .HasDatabaseName("ix_lyric_reports_user_id_created_at");

        entity.HasIndex(e => new { e.UserId, e.LyricId, e.Status })
          .HasDatabaseName("ix_lyric_reports_user_id_lyric_id_status");

        entity.HasIndex(e => e.LyricId)
          .HasDatabaseName("ix_lyric_reports_lyric_id");
      });

      builder.Entity<User>(entity =>
      {
        entity.ToTable("users");

        entity.HasIndex(e => e.CognitoUserId)
          .IsUnique()
          .HasDatabaseName("ix_users_cognito_user_id");

        entity.HasIndex(e => e.Username)
          .IsUnique()
          .HasDatabaseName("ix_users_username");

        entity.HasIndex(e => e.Slug)
          .IsUnique()
          .HasDatabaseName("ix_users_slug");
      });

      builder.Entity<PointEvent>(entity =>
      {
        entity.ToTable("point_events");

        entity.HasOne(e => e.User)
          .WithMany()
          .HasForeignKey(e => e.UserId);

        entity.HasIndex(e => new { e.UserId, e.CreatedAt })
          .IsDescending(false, true)
          .HasDatabaseName("ix_point_events_user_id_created_at");

        entity.HasIndex(e => new { e.UserId, e.ActionType, e.EntityId })
          .IsUnique()
          .HasDatabaseName("ix_point_events_user_id_action_type_entity_id");
      });
    }
  }
}