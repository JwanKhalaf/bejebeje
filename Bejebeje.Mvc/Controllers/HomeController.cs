using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Bejebeje.Models.Lyric;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Bejebeje.Mvc.Models;
using Bejebeje.Services.Services.Interfaces;

namespace Bejebeje.Mvc.Controllers
{
  public class HomeController : Controller
  {
    private readonly ILyricsService _lyricsService;

    private readonly ILogger<HomeController> _logger;

    public HomeController(
      ILyricsService lyricsService,
      ILogger<HomeController> logger)
    {
      _lyricsService = lyricsService;
      _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
      LyricRecentSubmissionViewModel lyricRecentSubmissionViewModel = await _lyricsService.GetRecentLyricsAsync();
      
      return View(lyricRecentSubmissionViewModel);
    }

    public IActionResult Privacy()
    {
      return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
      return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
  }
}
