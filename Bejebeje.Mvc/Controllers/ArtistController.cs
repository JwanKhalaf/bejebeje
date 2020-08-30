using Bejebeje.Services.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Bejebeje.Mvc.Controllers
{
  public class ArtistController : Controller
  {
    private readonly ILyricsService lyricsService;

    private readonly ILogger<HomeController> logger;

    public ArtistController(
      ILyricsService lyricsService,
      ILogger<HomeController> logger)
    {
      this.lyricsService = lyricsService;
      this.logger = logger;
    }
  }
}
