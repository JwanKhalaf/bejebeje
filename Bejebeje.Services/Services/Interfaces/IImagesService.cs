namespace Bejebeje.Services.Services.Interfaces
{
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Mvc;

  public interface IImagesService
  {
    Task<byte[]> GetArtistImageBytesAsync(int imageId);
  }
}
