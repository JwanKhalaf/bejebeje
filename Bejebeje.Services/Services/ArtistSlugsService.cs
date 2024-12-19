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

      string updateSql = "update artist_slugs set is_primary = false, modified_at = current_timestamp where artist_id = @artist_id";
      string insertSql = "insert into artist_slugs (name, is_primary, created_at, is_deleted, artist_id) " +
                         "values (@name, @is_primary, @created_at, @is_deleted, @artist_id)";

      using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
      {
        await connection.OpenAsync();

        // begin a transaction to ensure atomicity
        using (var transaction = await connection.BeginTransactionAsync())
        {
          try
          {
            if (artistSlug.IsPrimary)
            {
              // step 1: update existing slugs to set is_primary = false
              using (NpgsqlCommand updateCommand = new NpgsqlCommand(updateSql, connection))
              {
                updateCommand.Parameters.AddWithValue("@artist_id", artistSlug.ArtistId);
                await updateCommand.ExecuteNonQueryAsync();
              }
            }

            // step 2: insert the new slug
            using (NpgsqlCommand insertCommand = new NpgsqlCommand(insertSql, connection))
            {
              string name = artistSlug.Name.NormalizeStringForUrl();
              DateTime createdAt = DateTime.UtcNow;

              insertCommand.Parameters.AddWithValue("@name", name);
              insertCommand.Parameters.AddWithValue("@is_primary", artistSlug.IsPrimary);
              insertCommand.Parameters.AddWithValue("@created_at", createdAt);
              insertCommand.Parameters.AddWithValue("@is_deleted", artistSlug.IsDeleted);
              insertCommand.Parameters.AddWithValue("@artist_id", artistSlug.ArtistId);

              await insertCommand.ExecuteNonQueryAsync();
            }

            // commit the transaction
            await transaction.CommitAsync();
          }
          catch (Exception ex)
          {
            // rollback the transaction in case of an error
            await transaction.RollbackAsync();
            Console.WriteLine(ex.Message);
            throw; // re-throw the exception for higher-level handling
          }
        }
      }
    }
  }
}