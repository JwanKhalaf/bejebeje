namespace Bejebeje.Mvc.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

/// <summary>
/// handles http status code pages (404, 500, etc.) with user-friendly views.
/// </summary>
[AllowAnonymous]
public class StatusCodeController : Controller
{
  private readonly ILogger<StatusCodeController> _logger;

  public StatusCodeController(ILogger<StatusCodeController> logger)
  {
    _logger = logger;
  }

  /// <summary>
  /// handles 404 not found responses.
  /// </summary>
  [Route("/not-found")]
  public IActionResult NotFound()
  {
    Response.StatusCode = 404;

    string? originalPath = HttpContext.Features.Get<IStatusCodeReExecuteFeature>()?.OriginalPath;

    _logger.LogWarning("404 not found: {OriginalPath}", originalPath ?? "unknown");

    ViewData["OriginalPath"] = originalPath;

    return View("NotFound");
  }

  /// <summary>
  /// handles 500 internal server error responses.
  /// </summary>
  [Route("/server-error")]
  public IActionResult ServerError()
  {
    Response.StatusCode = 500;

    var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

    if (exceptionFeature != null)
    {
      _logger.LogError(
        exceptionFeature.Error,
        "unhandled exception at {Path}",
        exceptionFeature.Path);
    }

    string requestId = HttpContext.TraceIdentifier;
    ViewData["RequestId"] = requestId;

    return View("ServerError");
  }

  /// <summary>
  /// generic handler for other status codes (403, 401, etc.)
  /// </summary>
  [Route("/status/{code:int}")]
  public IActionResult StatusCodePage(int code)
  {
    Response.StatusCode = code;

    return code switch
    {
      404 => RedirectToAction(nameof(NotFound)),
      500 => RedirectToAction(nameof(ServerError)),
      _ => View("GenericError", code)
    };
  }
}
