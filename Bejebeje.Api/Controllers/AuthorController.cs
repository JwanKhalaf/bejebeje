namespace Bejebeje.Api.Controllers
{
  using System;
  using System.Threading.Tasks;
  using Bejebeje.Common.Exceptions;
  using Bejebeje.Models.Author;
  using Bejebeje.Services.Services.Interfaces;
  using Microsoft.AspNetCore.Mvc;

  [ApiController]
  public class AuthorController : ControllerBase
  {
    private IAuthorService authorService;

    public AuthorController(IAuthorService authorService)
    {
      this.authorService = authorService;
    }

    [Route("[controller]/{authorSlug}")]
    [HttpGet]
    public async Task<IActionResult> GetAuthorDetails(string authorSlug)
    {
      if (string.IsNullOrEmpty(authorSlug))
      {
        throw new ArgumentNullException(nameof(authorSlug));
      }

      try
      {
        AuthorDetailsResponse author = await authorService.GetAuthorDetailsAsync(authorSlug);
        return Ok(author);
      }
      catch (AuthorNotFoundException)
      {
        return NotFound();
      }
    }
  }
}
