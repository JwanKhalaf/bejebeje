namespace Bejebeje.Mvc.Controllers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bejebeje.Common.Extensions;
using Bejebeje.Models.Artist;
using Bejebeje.Services.Services.Interfaces;
using Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using ViewModels.Artists.CreateBandArtist;
using ViewModels.Artists.CreateIndividualArtist;

public class ArtistController : Controller
{
  private readonly IArtistsService _artistsService;
  private readonly IImagesService _imagesService;
  private readonly ILogger<ArtistController> _logger;

  public ArtistController(
    IArtistsService artistsService,
    IImagesService imagesService,
    ILogger<ArtistController> logger)
  {
    _artistsService = artistsService;
    _imagesService = imagesService;
    _logger = logger;
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
    _logger.LogInformation("Individual artist creation started for user {UserId}", User.GetUserId());

    // check the model state first before any processing
    if (!ModelState.IsValid)
    {
      _logger.LogWarning("Individual artist creation failed due to validation errors. Errors: {ValidationErrors}",
        string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
      return View(viewModel);
    }

    // Manual length validation (now that MaxLength attribute is removed)
    if (!string.IsNullOrEmpty(viewModel.FirstName) && viewModel.FirstName.Length > 100)
    {
      _logger.LogWarning("First name too long for user {UserId}: {Length} characters", User.GetUserId(),
        viewModel.FirstName.Length);
      ModelState.AddModelError(nameof(viewModel.FirstName), "First name cannot be longer than 100 characters");
      return View(viewModel);
    }

    if (!string.IsNullOrEmpty(viewModel.LastName) && viewModel.LastName.Length > 100)
    {
      _logger.LogWarning("Last name too long for user {UserId}: {Length} characters", User.GetUserId(),
        viewModel.LastName.Length);
      ModelState.AddModelError(nameof(viewModel.LastName), "Last name cannot be longer than 100 characters");
      return View(viewModel);
    }

    try
    {
      string userId = User.GetUserId().ToString();
      string firstName = viewModel.FirstName!.Trim();
      string lastName = string.IsNullOrEmpty(viewModel.LastName) ? string.Empty : viewModel.LastName.Trim();
      string fullName = string.IsNullOrEmpty(lastName) ? firstName : $"{firstName} {lastName}";
      string artistSlug = fullName.NormalizeStringForUrl();

      _logger.LogInformation("Processing individual artist creation for '{FullName}' (slug: {ArtistSlug})", fullName,
        artistSlug);

      bool artistExists = await _artistsService.ArtistExistsAsync(artistSlug);

      // check for duplicate
      if (artistExists)
      {
        _logger.LogWarning("Artist '{FullName}' already exists with slug '{ArtistSlug}'", fullName, artistSlug);
        viewModel.ExistingArtistFullName = fullName;
        viewModel.ExistingArtistSlug = artistSlug;
        viewModel.ExistingArtistImageUrl =
          "https://s3.eu-west-2.amazonaws.com/bejebeje.com/artist-images/placeholder-sm"; // Placeholder since we don't fetch full details
        viewModel.ExistingArtistImageAlternateText = $"Photo of {fullName}";

        ViewBag.ArtistExists = true;
        return View(viewModel);
      }

      string nonKurdishLetters = "ğıİöü";
      bool containsNonKurdishLetters =
        fullName.Any(c => nonKurdishLetters.Contains(c, StringComparison.OrdinalIgnoreCase));

      // check for non-kurdish letters
      if (containsNonKurdishLetters)
      {
        _logger.LogWarning("Artist name '{FullName}' contains non-Kurdish letters", fullName);
        ModelState.AddModelError(string.Empty,
          "Are you sure you're adding Kurdish artists? We've detected some non-Kurdish letters in the name.");
        return View(viewModel);
      }

      if (viewModel.Photo != null)
      {
        _logger.LogInformation(
          "Processing image upload for artist '{FullName}'. File size: {FileSize} bytes, Content type: {ContentType}, File name: {FileName}",
          fullName, viewModel.Photo.Length, viewModel.Photo.ContentType, viewModel.Photo.FileName);

        // check file size first
        if (viewModel.Photo is { Length: > 500_000 })
        {
          _logger.LogWarning("Image file too large for artist '{FullName}': {FileSize} bytes", fullName,
            viewModel.Photo.Length);
          ModelState.AddModelError(nameof(viewModel.Photo), "The photo size must not exceed 500KB.");

          // Clear photo to prevent issues while preserving other form data
          viewModel.Photo = null;
          return View(viewModel);
        }

        // check file extension (first line of defense)
        string fileExtension = Path.GetExtension(viewModel.Photo.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(fileExtension) ||
            (fileExtension != ".jpg" && fileExtension != ".jpeg" && fileExtension != ".png"))
        {
          _logger.LogWarning("Invalid file extension for artist '{FullName}': {Extension}", fullName, fileExtension);
          ModelState.AddModelError(nameof(viewModel.Photo), "Only JPG and PNG images are allowed.");

          // Clear photo to prevent issues while preserving other form data
          viewModel.Photo = null;
          return View(viewModel);
        }

        // check mime type (second line of defense)
        string[] allowedMimeTypes = ["image/jpeg", "image/jpg", "image/png"];
        if (!allowedMimeTypes.Contains(viewModel.Photo.ContentType.ToLowerInvariant()))
        {
          _logger.LogWarning("Invalid MIME type for artist '{FullName}': {MimeType}", fullName,
            viewModel.Photo.ContentType);
          ModelState.AddModelError(nameof(viewModel.Photo), "Only JPG and PNG images are allowed.");

          // Clear photo to prevent issues while preserving other form data
          viewModel.Photo = null;
          return View(viewModel);
        }

        try
        {
          // read and validate the actual image data (third line of defense)
          using var imageStream = viewModel.Photo.OpenReadStream();

          // reset stream position for proper reading
          imageStream.Position = 0;

          using Image<Rgba32> image = await Image.LoadAsync<Rgba32>(imageStream);

          // verify the image format matches what we expect
          var detectedFormat = image.Metadata.DecodedImageFormat;
          if (detectedFormat == null)
          {
            _logger.LogWarning("Could not detect image format for artist '{FullName}'", fullName);
            ModelState.AddModelError(nameof(viewModel.Photo),
              "Invalid image format. Only JPG and PNG images are allowed.");

            // Clear photo to prevent issues while preserving other form data
            viewModel.Photo = null;
            return View(viewModel);
          }

          string formatName = detectedFormat.Name.ToLowerInvariant();
          if (formatName != "jpeg" && formatName != "png")
          {
            _logger.LogWarning("Invalid detected image format for artist '{FullName}': {Format}", fullName, formatName);
            ModelState.AddModelError(nameof(viewModel.Photo),
              "Invalid image format. Only JPG and PNG images are allowed.");

            // Clear photo to prevent issues while preserving other form data
            viewModel.Photo = null;
            return View(viewModel);
          }

          // check image dimensions
          if (image.Width > 1000 || image.Height > 1000)
          {
            _logger.LogWarning("Image dimensions too large for artist '{FullName}': {Width}x{Height}", fullName,
              image.Width, image.Height);
            ModelState.AddModelError(nameof(viewModel.Photo), "The photo must be at most 1000x1000 pixels.");

            // Clear photo to prevent issues while preserving other form data
            viewModel.Photo = null;
            return View(viewModel);
          }

          // additional security: Check for minimum dimensions (helps detect malicious tiny files)
          if (image.Width < 10 || image.Height < 10)
          {
            _logger.LogWarning("Image dimensions too small for artist '{FullName}': {Width}x{Height}", fullName,
              image.Width, image.Height);
            ModelState.AddModelError(nameof(viewModel.Photo), "The image is too small. Minimum size is 10x10 pixels.");

            // Clear photo to prevent issues while preserving other form data
            viewModel.Photo = null;
            return View(viewModel);
          }

          _logger.LogInformation("Image validation passed for artist '{FullName}': {Width}x{Height}, Format: {Format}",
            fullName, image.Width, image.Height, formatName);
        }
        catch (InvalidImageContentException ex)
        {
          _logger.LogWarning(ex, "Invalid image content for artist '{FullName}'", fullName);
          ModelState.AddModelError(nameof(viewModel.Photo), "The uploaded file is not a valid image.");

          // Clear photo to prevent issues while preserving other form data
          viewModel.Photo = null;
          return View(viewModel);
        }
        catch (UnknownImageFormatException ex)
        {
          _logger.LogWarning(ex, "Unknown image format for artist '{FullName}'", fullName);
          ModelState.AddModelError(nameof(viewModel.Photo),
            "Unsupported image format. Only JPG and PNG images are allowed.");

          // Clear photo to prevent issues while preserving other form data
          viewModel.Photo = null;
          return View(viewModel);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Failed to process image for artist '{FullName}'", fullName);
          ModelState.AddModelError(nameof(viewModel.Photo),
            "Failed to process the uploaded image. Please try a different file.");

          // Clear photo to prevent issues while preserving other form data
          viewModel.Photo = null;
          return View(viewModel);
        }
      }

      CreateIndividualArtistDto dto = new()
      {
        FirstName = firstName,
        LastName = lastName,
        UserId = userId,
        Biography = viewModel.Biography?.Trim() ?? string.Empty,
      };

      _logger.LogInformation("Creating artist record for '{FullName}'", fullName);
      ArtistCreationResult result = await _artistsService.AddArtistAsync(dto);

      if (!result.IsSuccessful)
      {
        _logger.LogError("Failed to create artist record for '{FullName}'", fullName);
        ModelState.AddModelError(string.Empty, "Failed to create artist. Please try again.");
        return View(viewModel);
      }

      _logger.LogInformation("Successfully created artist '{FullName}' with ID {ArtistId}", fullName, result.ArtistId);

      if (viewModel.Photo != null)
      {
        _logger.LogInformation("Uploading image for artist {ArtistId}", result.ArtistId);
        try
        {
          using var imageStream = viewModel.Photo.OpenReadStream();
          bool imageUploadSuccess = await _imagesService.UploadArtistImageAsync(result.ArtistId, imageStream);

          if (imageUploadSuccess)
          {
            await _artistsService.UpdateArtistImageStatusAsync(result.ArtistId, true);
            _logger.LogInformation("Successfully uploaded and updated image status for artist {ArtistId}",
              result.ArtistId);
          }
          else
          {
            _logger.LogWarning("Image upload failed for artist {ArtistId}, but artist was created successfully",
              result.ArtistId);
          }
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Exception during image upload for artist {ArtistId}", result.ArtistId);
        }
      }

      _logger.LogInformation(
        "Individual artist creation completed successfully for '{FullName}', redirecting to artist page", fullName);
      return RedirectToAction("ArtistLyrics", "Lyric", new { artistSlug = result.PrimarySlug });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unexpected error during individual artist creation for user {UserId}", User.GetUserId());
      ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
      return View(viewModel);
    }
  }

  [Authorize]
  [Route("artists/{artistSlug}/update")]
  public async Task<IActionResult> Update(string artistSlug)
  {
    string userId = User.Identity is { IsAuthenticated: true }
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
      string userId = User.Identity is { IsAuthenticated: true }
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

  [Authorize]
  [Route("artists/new/band")]
  public IActionResult CreateBand()
  {
    CreateBandArtistViewModel viewModel = new();

    return View(viewModel);
  }

  [Authorize]
  [HttpPost]
  [Route("artists/new/band")]
  public async Task<IActionResult> CreateBand(CreateBandArtistViewModel viewModel)
  {
    _logger.LogInformation("Band artist creation started for user {UserId}", User.GetUserId());

    // check ModelState first before any processing
    if (!ModelState.IsValid)
    {
      _logger.LogWarning("Band artist creation failed due to validation errors. Errors: {ValidationErrors}",
        string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
      return View(viewModel);
    }

    // manual length validation (now that MaxLength attribute is removed)
    if (!string.IsNullOrEmpty(viewModel.BandName) && viewModel.BandName.Length > 200)
    {
      _logger.LogWarning("Band name too long for user {UserId}: {Length} characters", User.GetUserId(),
        viewModel.BandName.Length);
      ModelState.AddModelError(nameof(viewModel.BandName), "Band name cannot be longer than 200 characters");
      return View(viewModel);
    }

    try
    {
      string userId = User.GetUserId().ToString();

      string bandName = viewModel.BandName!.Trim();
      string artistSlug = bandName.NormalizeStringForUrl();

      _logger.LogInformation("Processing band artist creation for '{BandName}' (slug: {ArtistSlug})", bandName,
        artistSlug);

      bool artistExists = await _artistsService.ArtistExistsAsync(artistSlug);

      if (artistExists)
      {
        _logger.LogWarning("Band '{BandName}' already exists with slug '{ArtistSlug}'", bandName, artistSlug);
        viewModel.ExistingArtistFullName = bandName;
        viewModel.ExistingArtistSlug = artistSlug;
        viewModel.ExistingArtistImageUrl =
          "https://s3.eu-west-2.amazonaws.com/bejebeje.com/artist-images/placeholder-sm"; // Placeholder since we don't fetch full details
        viewModel.ExistingArtistImageAlternateText = $"Photo of {bandName}";

        ViewBag.ArtistExists = true;
        return View(viewModel);
      }

      string nonKurdishLetters = "ğıİöü";
      bool containsNonKurdishLetters =
        bandName.Any(c => nonKurdishLetters.Contains(c, StringComparison.OrdinalIgnoreCase));

      if (containsNonKurdishLetters)
      {
        _logger.LogWarning("Band name '{BandName}' contains non-Kurdish letters", bandName);
        ModelState.AddModelError(string.Empty,
          "Are you sure you're adding Kurdish artists? We've detected some non-Kurdish letters!");
        return View(viewModel);
      }

      if (viewModel.Photo != null)
      {
        _logger.LogInformation(
          "Processing image upload for band '{BandName}'. File size: {FileSize} bytes, Content type: {ContentType}, File name: {FileName}",
          bandName, viewModel.Photo.Length, viewModel.Photo.ContentType, viewModel.Photo.FileName);

        // check file size first
        if (viewModel.Photo is { Length: > 500_000 })
        {
          _logger.LogWarning("Image file too large for band '{BandName}': {FileSize} bytes", bandName,
            viewModel.Photo.Length);
          ModelState.AddModelError(nameof(viewModel.Photo), "The photo size must not exceed 500KB.");

          // clear photo to prevent issues while preserving other form data
          viewModel.Photo = null;
          return View(viewModel);
        }

        // check file extension (first line of defense)
        string fileExtension = Path.GetExtension(viewModel.Photo.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(fileExtension) ||
            (fileExtension != ".jpg" && fileExtension != ".jpeg" && fileExtension != ".png"))
        {
          _logger.LogWarning("Invalid file extension for band '{BandName}': {Extension}", bandName, fileExtension);
          ModelState.AddModelError(nameof(viewModel.Photo), "Only JPG and PNG images are allowed.");

          // clear photo to prevent issues while preserving other form data
          viewModel.Photo = null;
          return View(viewModel);
        }

        // check mime type (second line of defense)
        string[] allowedMimeTypes = ["image/jpeg", "image/jpg", "image/png"];
        if (!allowedMimeTypes.Contains(viewModel.Photo.ContentType.ToLowerInvariant()))
        {
          _logger.LogWarning("Invalid MIME type for band '{BandName}': {MimeType}", bandName,
            viewModel.Photo.ContentType);
          ModelState.AddModelError(nameof(viewModel.Photo), "Only JPG and PNG images are allowed.");

          // clear photo to prevent issues while preserving other form data
          viewModel.Photo = null;
          return View(viewModel);
        }

        try
        {
          // read and validate the actual image data (third line of defense)
          using var imageStream = viewModel.Photo.OpenReadStream();

          // reset stream position for proper reading
          imageStream.Position = 0;

          using Image<Rgba32> image = await Image.LoadAsync<Rgba32>(imageStream);

          // verify the image format matches what we expect
          var detectedFormat = image.Metadata.DecodedImageFormat;
          if (detectedFormat == null)
          {
            _logger.LogWarning("Could not detect image format for band '{BandName}'", bandName);
            ModelState.AddModelError(nameof(viewModel.Photo),
              "Invalid image format. Only JPG and PNG images are allowed.");

            // clear photo to prevent issues while preserving other form data
            viewModel.Photo = null;
            return View(viewModel);
          }

          string formatName = detectedFormat.Name.ToLowerInvariant();
          if (formatName != "jpeg" && formatName != "png")
          {
            _logger.LogWarning("Invalid detected image format for band '{BandName}': {Format}", bandName, formatName);
            ModelState.AddModelError(nameof(viewModel.Photo),
              "Invalid image format. Only JPG and PNG images are allowed.");

            // Clear photo to prevent issues while preserving other form data
            viewModel.Photo = null;
            return View(viewModel);
          }

          // check image dimensions
          if (image.Width > 1000 || image.Height > 1000)
          {
            _logger.LogWarning("Image dimensions too large for band '{BandName}': {Width}x{Height}", bandName,
              image.Width, image.Height);
            ModelState.AddModelError(nameof(viewModel.Photo), "The photo must be at most 1000x1000 pixels.");

            // clear photo to prevent issues while preserving other form data
            viewModel.Photo = null;
            return View(viewModel);
          }

          // additional security: Check for minimum dimensions (helps detect malicious tiny files)
          if (image.Width < 10 || image.Height < 10)
          {
            _logger.LogWarning("Image dimensions too small for band '{BandName}': {Width}x{Height}", bandName,
              image.Width, image.Height);
            ModelState.AddModelError(nameof(viewModel.Photo),
              "The image is too small. Minimum size is 10x10 pixels.");

            // clear photo to prevent issues while preserving other form data
            viewModel.Photo = null;
            return View(viewModel);
          }

          _logger.LogInformation("Image validation passed for band '{BandName}': {Width}x{Height}, Format: {Format}",
            bandName, image.Width, image.Height, formatName);
        }
        catch (InvalidImageContentException ex)
        {
          _logger.LogWarning(ex, "Invalid image content for band '{BandName}'", bandName);
          ModelState.AddModelError(nameof(viewModel.Photo), "The uploaded file is not a valid image.");

          // clear photo to prevent issues while preserving other form data
          viewModel.Photo = null;
          return View(viewModel);
        }
        catch (UnknownImageFormatException ex)
        {
          _logger.LogWarning(ex, "Unknown image format for band '{BandName}'", bandName);
          ModelState.AddModelError(nameof(viewModel.Photo),
            "Unsupported image format. Only JPG and PNG images are allowed.");

          // clear photo to prevent issues while preserving other form data
          viewModel.Photo = null;
          return View(viewModel);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Failed to process image for band '{BandName}'", bandName);
          ModelState.AddModelError(nameof(viewModel.Photo),
            "Failed to process the uploaded image. Please try a different file.");

          // clear photo to prevent issues while preserving other form data
          viewModel.Photo = null;
          return View(viewModel);
        }
      }

      CreateBandArtistDto dto = new()
      {
        BandName = bandName,
        UserId = userId,
        Biography = viewModel.Biography?.Trim() ?? string.Empty,
      };

      _logger.LogInformation("Creating band record for '{BandName}'", bandName);
      ArtistCreationResult result = await _artistsService.AddBandArtistAsync(dto);

      if (!result.IsSuccessful)
      {
        _logger.LogError("Failed to create band record for '{BandName}'", bandName);
        ModelState.AddModelError(string.Empty, "Failed to create band. Please try again.");
        return View(viewModel);
      }

      _logger.LogInformation("Successfully created band '{BandName}' with ID {ArtistId}", bandName, result.ArtistId);

      if (viewModel.Photo != null)
      {
        _logger.LogInformation("Uploading image for band {ArtistId}", result.ArtistId);
        try
        {
          using var imageStream = viewModel.Photo.OpenReadStream();
          bool imageUploadSuccess = await _imagesService.UploadArtistImageAsync(result.ArtistId, imageStream);

          if (imageUploadSuccess)
          {
            await _artistsService.UpdateArtistImageStatusAsync(result.ArtistId, true);
            _logger.LogInformation("Successfully uploaded and updated image status for band {ArtistId}",
              result.ArtistId);
          }
          else
          {
            _logger.LogWarning("Image upload failed for band {ArtistId}, but band was created successfully",
              result.ArtistId);
          }
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Exception during image upload for band {ArtistId}", result.ArtistId);
        }
      }

      _logger.LogInformation(
        "Band artist creation completed successfully for '{BandName}', redirecting to artist page",
        bandName);
      return RedirectToAction("ArtistLyrics", "Lyric", new { artistSlug = result.PrimarySlug });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unexpected error during band artist creation for user {UserId}", User.GetUserId());
      ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
      return View(viewModel);
    }
  }
}