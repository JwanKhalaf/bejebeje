namespace Bejebeje.Services.Services.Interfaces
{
  using System.Threading.Tasks;

  public interface IImagesService
  {
    Task<byte[]> GetArtistImageBytesAsync(string artistSlug);
  }
}
