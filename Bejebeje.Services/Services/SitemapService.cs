namespace Bejebeje.Services.Services
{
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Bejebeje.Models.Sitemap;
  using Config;
  using Interfaces;
  using Microsoft.Extensions.Options;
  using Npgsql;

  public class SitemapService : ISitemapService
  {
    private readonly DatabaseOptions databaseOptions;

    public SitemapService(
      IOptionsMonitor<DatabaseOptions> optionsAccessor)
    {
      this.databaseOptions = optionsAccessor.CurrentValue;
    }

    public async Task<IEnumerable<ArtistUrlViewModel>> GetAllArtistsAsync()
    {
      List<ArtistUrlViewModel> artistUrls = new List<ArtistUrlViewModel>();

      await using NpgsqlConnection connection = new NpgsqlConnection(this.databaseOptions.ConnectionString);
      await connection.OpenAsync();

      await using NpgsqlCommand command = new NpgsqlCommand("select \"as\".name from artists a inner join artist_slugs \"as\" on a.id = \"as\".artist_id where a.is_approved = true and a.is_deleted = false;", connection);

      await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

      while (await reader.ReadAsync())
      {
        ArtistUrlViewModel artistUrlViewModel = new ArtistUrlViewModel();

        string artistPrimarySlug = Convert.ToString(reader[0]);

        artistUrlViewModel.ArtistPrimarySlug = artistPrimarySlug;

        artistUrls.Add(artistUrlViewModel);
      }

      return artistUrls;
    }

    public async Task<IEnumerable<LyricUrlViewModel>> GetAllLyricsAsync()
    {
      List<LyricUrlViewModel> lyricUrls = new List<LyricUrlViewModel>();

      await using NpgsqlConnection connection = new NpgsqlConnection(this.databaseOptions.ConnectionString);
      await connection.OpenAsync();

      await using NpgsqlCommand command = new NpgsqlCommand("select artist_slugs.name as artist_slug, lslugs.name as primary_lyric_slug from lyrics as l inner join artists as a on l.artist_id = a.id inner join artist_slugs on artist_slugs.artist_id = a.id left join artist_images as ai on ai.artist_id = a.id inner join lyric_slugs as lslugs on lslugs.lyric_id = l.id where l.is_approved = true and l.is_deleted = false and a.is_deleted = false and a.is_approved = true;", connection);

      await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

      while (await reader.ReadAsync())
      {
        LyricUrlViewModel lyricUrlViewModel = new LyricUrlViewModel();

        string artistPrimarySlug = Convert.ToString(reader[0]);
        string lyricPrimarySlug = Convert.ToString(reader[1]);

        lyricUrlViewModel.ArtistPrimarySlug = artistPrimarySlug;
        lyricUrlViewModel.LyricPrimarySlug = lyricPrimarySlug;

        lyricUrls.Add(lyricUrlViewModel);
      }

      return lyricUrls;
    }
  }
}
