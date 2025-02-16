namespace Bejebeje.Mvc.Controllers;

using System.Threading.Tasks;
using Bejebeje.Models.Author;
using Bejebeje.Services.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

public class AuthorController : Controller
{
  private readonly IAuthorService _authorService;

  public AuthorController(IAuthorService authorService)
  {
    _authorService = authorService;
  }

  [Route("authors/{authorSlug}")]
  public async Task<IActionResult> Get(string authorSlug)
  {
    AuthorDetailsViewModel viewModel = await _authorService.GetAuthorDetailsAsync(authorSlug);

    return View(viewModel);
  }
}