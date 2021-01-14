namespace Bejebeje.Services.Services
{
  using System;
  using System.Threading.Tasks;
  using Bejebeje.Common.Extensions;
  using Config;
  using Domain;
  using Interfaces;
  using Microsoft.Extensions.Options;
  using Models.ArtistSlug;
  using NodaTime;
  using Npgsql;

  public class ArtistSlugsService : IArtistSlugsService
  {
    private readonly DatabaseOptions _databaseOptions;

    public ArtistSlugsService(
      IOptionsMonitor<DatabaseOptions> optionsAccessor)
    {
      _databaseOptions = optionsAccessor.CurrentValue;
    }

    public string GetArtistSlug(
      string name)
    {
      if (string.IsNullOrEmpty(name))
      {
        throw new ArgumentNullException(nameof(name));
      }

      string artistFullNameLowercase = name.Trim().ToLower();

      return artistFullNameLowercase.NormalizeStringForUrl();
    }

    public ArtistSlug BuildArtistSlug(
      string artistFullName)
    {
      if (string.IsNullOrEmpty(artistFullName))
      {
        throw new ArgumentNullException(nameof(artistFullName));
      }

      ArtistSlug artistSlug = new ArtistSlug
      {
        Name = GetArtistSlug(artistFullName),
        CreatedAt = SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc(),
        IsPrimary = true,
      };

      return artistSlug;
    }

    public async Task AddArtistSlugAsync(
      ArtistSlugCreateViewModel artistSlug)
    {
      string connectionString = _databaseOptions.ConnectionString;
      string sqlStatement = "insert into artist_slugs (name, is_primary, created_at, is_deleted, artist_id) values (@name, @is_primary, @created_at, @is_deleted, @artist_id)";

      using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
      {
        NpgsqlCommand command = new NpgsqlCommand(sqlStatement, connection);

        string name = artistSlug.Name.NormalizeStringForUrl();
        DateTime createdAt = DateTime.UtcNow;

        command.Parameters.AddWithValue("@name", name);
        command.Parameters.AddWithValue("@is_primary", artistSlug.IsPrimary);
        command.Parameters.AddWithValue("@created_at", createdAt);
        command.Parameters.AddWithValue("@is_deleted", artistSlug.IsDeleted);
        command.Parameters.AddWithValue("@artist_id", artistSlug.ArtistId);

        try
        {
          await connection.OpenAsync();
          await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.Message);
        }
      }
    }
  }
}
