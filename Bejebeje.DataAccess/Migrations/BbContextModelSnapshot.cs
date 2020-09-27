namespace Bejebeje.DataAccess.Migrations
{
  using System;
  using Context;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Infrastructure;
  using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

  [DbContext(typeof(BbContext))]
  partial class BbContextModelSnapshot : ModelSnapshot
  {
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
      modelBuilder
          .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
          .HasAnnotation("ProductVersion", "3.1.8")
          .HasAnnotation("Relational:MaxIdentifierLength", 63);

      modelBuilder.Entity("Bejebeje.Domain.Artist", b =>
          {
            b.Property<int>("Id")
                      .ValueGeneratedOnAdd()
                      .HasColumnName("id")
                      .HasColumnType("integer")
                      .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            b.Property<DateTime>("CreatedAt")
                      .HasColumnName("created_at")
                      .HasColumnType("timestamp without time zone");

            b.Property<string>("FirstName")
                      .HasColumnName("first_name")
                      .HasColumnType("text");

            b.Property<string>("FullName")
                      .HasColumnName("full_name")
                      .HasColumnType("text");

            b.Property<bool>("HasImage")
                      .HasColumnName("has_image")
                      .HasColumnType("boolean");

            b.Property<bool>("IsApproved")
                      .HasColumnName("is_approved")
                      .HasColumnType("boolean");

            b.Property<bool>("IsDeleted")
                      .HasColumnName("is_deleted")
                      .HasColumnType("boolean");

            b.Property<string>("LastName")
                      .HasColumnName("last_name")
                      .HasColumnType("text");

            b.Property<DateTime?>("ModifiedAt")
                      .HasColumnName("modified_at")
                      .HasColumnType("timestamp without time zone");

            b.Property<int>("Sex")
                      .HasColumnName("sex")
                      .HasColumnType("integer");

            b.Property<string>("UserId")
                      .HasColumnName("user_id")
                      .HasColumnType("text");

            b.HasKey("Id")
                      .HasName("pk_artists");

            b.ToTable("artists");
          });

      modelBuilder.Entity("Bejebeje.Domain.ArtistSlug", b =>
          {
            b.Property<int>("Id")
                      .ValueGeneratedOnAdd()
                      .HasColumnName("id")
                      .HasColumnType("integer")
                      .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            b.Property<int>("ArtistId")
                      .HasColumnName("artist_id")
                      .HasColumnType("integer");

            b.Property<DateTime>("CreatedAt")
                      .HasColumnName("created_at")
                      .HasColumnType("timestamp without time zone");

            b.Property<bool>("IsDeleted")
                      .HasColumnName("is_deleted")
                      .HasColumnType("boolean");

            b.Property<bool>("IsPrimary")
                      .HasColumnName("is_primary")
                      .HasColumnType("boolean");

            b.Property<DateTime?>("ModifiedAt")
                      .HasColumnName("modified_at")
                      .HasColumnType("timestamp without time zone");

            b.Property<string>("Name")
                      .HasColumnName("name")
                      .HasColumnType("text");

            b.HasKey("Id")
                      .HasName("pk_artist_slugs");

            b.HasIndex("ArtistId")
                      .HasName("ix_artist_slugs_artist_id");

            b.ToTable("artist_slugs");
          });

      modelBuilder.Entity("Bejebeje.Domain.Author", b =>
          {
            b.Property<int>("Id")
                      .ValueGeneratedOnAdd()
                      .HasColumnName("id")
                      .HasColumnType("integer")
                      .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            b.Property<string>("Biography")
                      .HasColumnName("biography")
                      .HasColumnType("text");

            b.Property<DateTime>("CreatedAt")
                      .HasColumnName("created_at")
                      .HasColumnType("timestamp without time zone");

            b.Property<string>("FirstName")
                      .HasColumnName("first_name")
                      .HasColumnType("text");

            b.Property<string>("FullName")
                      .HasColumnName("full_name")
                      .HasColumnType("text");

            b.Property<bool>("HasImage")
                      .HasColumnName("has_image")
                      .HasColumnType("boolean");

            b.Property<bool>("IsApproved")
                      .HasColumnName("is_approved")
                      .HasColumnType("boolean");

            b.Property<bool>("IsDeleted")
                      .HasColumnName("is_deleted")
                      .HasColumnType("boolean");

            b.Property<string>("LastName")
                      .HasColumnName("last_name")
                      .HasColumnType("text");

            b.Property<DateTime?>("ModifiedAt")
                      .HasColumnName("modified_at")
                      .HasColumnType("timestamp without time zone");

            b.Property<int>("Sex")
                      .HasColumnName("sex")
                      .HasColumnType("integer");

            b.Property<string>("UserId")
                      .HasColumnName("user_id")
                      .HasColumnType("text");

            b.HasKey("Id")
                      .HasName("pk_authors");

            b.ToTable("authors");
          });

      modelBuilder.Entity("Bejebeje.Domain.AuthorSlug", b =>
          {
            b.Property<int>("Id")
                      .ValueGeneratedOnAdd()
                      .HasColumnName("id")
                      .HasColumnType("integer")
                      .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            b.Property<int>("AuthorId")
                      .HasColumnName("author_id")
                      .HasColumnType("integer");

            b.Property<DateTime>("CreatedAt")
                      .HasColumnName("created_at")
                      .HasColumnType("timestamp without time zone");

            b.Property<bool>("IsDeleted")
                      .HasColumnName("is_deleted")
                      .HasColumnType("boolean");

            b.Property<bool>("IsPrimary")
                      .HasColumnName("is_primary")
                      .HasColumnType("boolean");

            b.Property<DateTime?>("ModifiedAt")
                      .HasColumnName("modified_at")
                      .HasColumnType("timestamp without time zone");

            b.Property<string>("Name")
                      .HasColumnName("name")
                      .HasColumnType("text");

            b.HasKey("Id")
                      .HasName("pk_author_slugs");

            b.HasIndex("AuthorId")
                      .HasName("ix_author_slugs_author_id");

            b.ToTable("author_slugs");
          });

      modelBuilder.Entity("Bejebeje.Domain.Lyric", b =>
          {
            b.Property<int>("Id")
                      .ValueGeneratedOnAdd()
                      .HasColumnName("id")
                      .HasColumnType("integer")
                      .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            b.Property<int>("ArtistId")
                      .HasColumnName("artist_id")
                      .HasColumnType("integer");

            b.Property<int?>("AuthorId")
                      .HasColumnName("author_id")
                      .HasColumnType("integer");

            b.Property<string>("Body")
                      .HasColumnName("body")
                      .HasColumnType("text");

            b.Property<DateTime>("CreatedAt")
                      .HasColumnName("created_at")
                      .HasColumnType("timestamp without time zone");

            b.Property<bool>("IsApproved")
                      .HasColumnName("is_approved")
                      .HasColumnType("boolean");

            b.Property<bool>("IsDeleted")
                      .HasColumnName("is_deleted")
                      .HasColumnType("boolean");

            b.Property<DateTime?>("ModifiedAt")
                      .HasColumnName("modified_at")
                      .HasColumnType("timestamp without time zone");

            b.Property<string>("Title")
                      .HasColumnName("title")
                      .HasColumnType("text");

            b.Property<string>("UserId")
                      .HasColumnName("user_id")
                      .HasColumnType("text");

            b.HasKey("Id")
                      .HasName("pk_lyrics");

            b.HasIndex("ArtistId")
                      .HasName("ix_lyrics_artist_id");

            b.HasIndex("AuthorId")
                      .HasName("ix_lyrics_author_id");

            b.ToTable("lyrics");
          });

      modelBuilder.Entity("Bejebeje.Domain.LyricSlug", b =>
          {
            b.Property<int>("Id")
                      .ValueGeneratedOnAdd()
                      .HasColumnName("id")
                      .HasColumnType("integer")
                      .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            b.Property<DateTime>("CreatedAt")
                      .HasColumnName("created_at")
                      .HasColumnType("timestamp without time zone");

            b.Property<bool>("IsDeleted")
                      .HasColumnName("is_deleted")
                      .HasColumnType("boolean");

            b.Property<bool>("IsPrimary")
                      .HasColumnName("is_primary")
                      .HasColumnType("boolean");

            b.Property<int>("LyricId")
                      .HasColumnName("lyric_id")
                      .HasColumnType("integer");

            b.Property<DateTime?>("ModifiedAt")
                      .HasColumnName("modified_at")
                      .HasColumnType("timestamp without time zone");

            b.Property<string>("Name")
                      .HasColumnName("name")
                      .HasColumnType("text");

            b.HasKey("Id")
                      .HasName("pk_lyric_slugs");

            b.HasIndex("LyricId")
                      .HasName("ix_lyric_slugs_lyric_id");

            b.ToTable("lyric_slugs");
          });

      modelBuilder.Entity("Bejebeje.Domain.ArtistSlug", b =>
          {
            b.HasOne("Bejebeje.Domain.Artist", null)
                      .WithMany("Slugs")
                      .HasForeignKey("ArtistId")
                      .HasConstraintName("fk_artist_slugs_artists_artist_id")
                      .OnDelete(DeleteBehavior.Cascade)
                      .IsRequired();
          });

      modelBuilder.Entity("Bejebeje.Domain.AuthorSlug", b =>
          {
            b.HasOne("Bejebeje.Domain.Author", null)
                      .WithMany("Slugs")
                      .HasForeignKey("AuthorId")
                      .HasConstraintName("fk_author_slugs_authors_author_id")
                      .OnDelete(DeleteBehavior.Cascade)
                      .IsRequired();
          });

      modelBuilder.Entity("Bejebeje.Domain.Lyric", b =>
          {
            b.HasOne("Bejebeje.Domain.Artist", "Artist")
                      .WithMany("Lyrics")
                      .HasForeignKey("ArtistId")
                      .HasConstraintName("fk_lyrics_artists_artist_id")
                      .OnDelete(DeleteBehavior.Cascade)
                      .IsRequired();

            b.HasOne("Bejebeje.Domain.Author", "Author")
                      .WithMany("Lyrics")
                      .HasForeignKey("AuthorId")
                      .HasConstraintName("fk_lyrics_authors_author_id");
          });

      modelBuilder.Entity("Bejebeje.Domain.LyricSlug", b =>
          {
            b.HasOne("Bejebeje.Domain.Lyric", null)
                      .WithMany("Slugs")
                      .HasForeignKey("LyricId")
                      .HasConstraintName("fk_lyric_slugs_lyrics_lyric_id")
                      .OnDelete(DeleteBehavior.Cascade)
                      .IsRequired();
          });
#pragma warning restore 612, 618
    }
  }
}
