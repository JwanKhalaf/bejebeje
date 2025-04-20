namespace Bejebeje.Mvc.Controllers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bejebeje.Common.Extensions;
using Bejebeje.Models.Artist;
using Bejebeje.Mvc.ViewModels.Artists.CreateIndividualArtist;
using Bejebeje.Services.Services.Interfaces;
using Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;

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
  [Route("artists/new/selector")]
  public IActionResult Selector()
  {
    return View();
  }

  [Authorize]
  [Route("artists/new/individual")]
  public IActionResult Create()
  {
    CreateIndividualArtistViewModel viewModel = new();

    return View(viewModel);
  }

  [Authorize]
  [HttpPost]
  [Route("artists/new/individual")]
  public async Task<IActionResult> Create(CreateIndividualArtistViewModel viewModel)
  {
    try
    {
      string userId = User.GetUserId().ToString();
      string firstName = viewModel.FirstName.Trim();
      string lastName = string.IsNullOrEmpty(viewModel.LastName) ? string.Empty : viewModel.LastName.Trim();
      string fullName = string.IsNullOrEmpty(lastName) ? firstName : $"{firstName} {lastName}";
      string artistSlug = fullName.NormalizeStringForUrl();
      var existingArtist = await _artistsService.GetArtistDetailsAsync(artistSlug, userId);

      // check for duplicate
      if (existingArtist != null)
      {
        viewModel.ExistingArtistFullName = fullName;
        viewModel.ExistingArtistSlug = existingArtist.PrimarySlug;
        viewModel.ExistingArtistImageUrl = existingArtist.ImageUrl;
        viewModel.ExistingArtistImageAlternateText = $"Photo of {existingArtist.FullName}";

        ViewBag.ArtistExists = true;
        return View(viewModel);
      }

      string turkishLetters = "ğıİöü";

      bool containsTurkishLetters = fullName.Any(c => turkishLetters.Contains(c, StringComparison.OrdinalIgnoreCase));

      // check for turkish letters
      if (containsTurkishLetters)
      {
        ModelState.AddModelError(string.Empty, "Are you sure you're adding Kurdish artists?");
        return View(viewModel);
      }

      if (viewModel.Photo != null)
      {
        // check image file size
        if (viewModel.Photo is { Length: > 500_000 })
        {
          ModelState.AddModelError(nameof(viewModel.Photo), "The photo size must not exceed 500KB.");
          return View(viewModel);
        }

        using Image<Rgba32> image = await Image.LoadAsync<Rgba32>(viewModel.Photo.OpenReadStream());

        if (image.Width > 1000 || image.Height > 1000)
        {
          ModelState.AddModelError(nameof(viewModel.Photo), "The photo must be at most 1000x1000 pixels.");
          return View(viewModel);
        }
      }

      CreateIndividualArtistDto dto = new()
      {
        FirstName = firstName,
        LastName = lastName,
        UserId = userId,
        Biography = viewModel.Biography.Trim(),
      };

      ArtistCreationResult result = await _artistsService
        .AddArtistAsync(dto);

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