namespace Bejebeje.Mvc.Controllers;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bejebeje.Models.Artist;
using Bejebeje.Services.Services.Interfaces;
using Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

public class ArtistController : Controller
{
  private readonly IArtistsService _artistsService;

  public ArtistController(IArtistsService artistsService)
  {
    _artistsService = artistsService;
  }

  [Route("artists")]
  public async Task<IActionResult> Index()
  {
    IDictionary<char, List<LibraryArtistViewModel>> viewModel = await _artistsService
      .GetAllArtistsAsync();

    return View(viewModel);
  }

  [Authorize]
  [Route("artists/new")]
  public IActionResult Create()
  {
    CreateArtistViewModel viewModel = new CreateArtistViewModel();

    return View(viewModel);
  }

  [Authorize]
  [HttpPost]
  [Route("artists/new")]
  public async Task<IActionResult> Create(CreateArtistViewModel viewModel)
  {
    try
    {
      viewModel.UserId = User
        .GetUserId()
        .ToString();

      ArtistCreationResult result = await _artistsService
        .AddArtistAsync(viewModel);

      return RedirectToAction("ArtistLyrics", "Lyric", new { artistSlug = result.PrimarySlug });
    }
    catch
    {
      return View(viewModel);
    }
  }

  [Authorize]
  [Route("artists/{artistSlug}/update")]
  public async Task<IActionResult> Update(string artistSlug)
  {
    string userId = User.Identity.IsAuthenticated
      ? User.GetUserId().ToString()
      : string.Empty;

    ArtistViewModel artistViewModel = await _artistsService
      .GetArtistDetailsAsync(artistSlug, userId);

    if (string.IsNullOrEmpty(userId) || !artistViewModel.IsOwnSubmission ||
        !((DateTime.UtcNow - artistViewModel.CreatedAt).TotalDays < 2))
    {
      throw new Exception("User does not have access to update the artist.");
    }

    UpdateArtistViewModel viewModel = new UpdateArtistViewModel
    {
      Id = artistViewModel.Id,
      FirstName = artistViewModel.FirstName,
      LastName = artistViewModel.LastName,
    };

    return View(viewModel);
  }

  [Authorize]
  [HttpPost]
  [Route("artists/{artistSlug}/update")]
  public async Task<IActionResult> Update(UpdateArtistViewModel viewModel)
  {
    try
    {
      string userId = User.Identity.IsAuthenticated
        ? User.GetUserId().ToString()
        : string.Empty;

      ArtistViewModel artist = await _artistsService
        .GetArtistDetailsByIdAsync(viewModel.Id, userId);

      if (string.IsNullOrEmpty(userId) || !artist.IsOwnSubmission ||
          !((DateTime.UtcNow - artist.CreatedAt).TotalDays < 2))
      {
        throw new Exception("User does not have access to update the artist.");
      }

      ArtistUpdateResult result = await _artistsService
        .UpdateArtistAsync(viewModel);

      return RedirectToAction("ArtistLyrics", "Lyric", new { artistSlug = result.PrimarySlug });
    }
    catch
    {
      return View(viewModel);
    }
  }
}