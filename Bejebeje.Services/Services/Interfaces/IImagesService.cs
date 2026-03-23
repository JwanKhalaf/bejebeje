namespace Bejebeje.Services.Services.Interfaces
{
  using System.IO;
  using System.Threading.Tasks;

  public interface IImagesService
  {
    Task<bool> UploadArtistImageAsync(int artistId, Stream imageStream);
    Task<bool> UploadAuthorImageAsync(int authorId, Stream imageStream);
    Task<bool> DeleteArtistImageAsync(int artistId);
    Task<bool> DeleteAuthorImageAsync(int authorId);
  }
}
