﻿namespace Bejebeje.Services.Services.Interfaces
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Models.Artist;
  using Models.Search;

  public interface IArtistsService
  {
    Task<int> GetArtistIdAsync(
      string artistSlug);

    Task<bool> ArtistExistsAsync(
      string artistSlug);

    Task<IEnumerable<RandomFemaleArtistItemViewModel>> GetTopTenFemaleArtistsByLyricsCountAsync();

    Task<ArtistViewModel> GetArtistDetailsAsync(
      string artistSlug, string userId);

    Task<CreateNewArtistResponse> CreateNewArtistAsync(
      CreateNewArtistRequest request);

    Task<IEnumerable<SearchArtistResultViewModel>> SearchArtistsAsync(
      string artistName);

    Task<IDictionary<char, List<LibraryArtistViewModel>>> GetAllArtistsAsync();

    Task<ArtistCreationResult> AddArtistAsync(
      CreateArtistViewModel viewModel);
  }
}
