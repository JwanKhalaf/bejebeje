namespace Bejebeje.Services.Services
{
  using System;
  using System.IO;
  using System.Threading.Tasks;
  using Amazon.S3;
  using Amazon.S3.Model;
  using Interfaces;
  using Microsoft.Extensions.Logging;
  using SixLabors.ImageSharp;
  using SixLabors.ImageSharp.Formats.Jpeg;
  using SixLabors.ImageSharp.Formats.Webp;
  using SixLabors.ImageSharp.PixelFormats;
  using SixLabors.ImageSharp.Processing;

  public class ImagesService : IImagesService
  {
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<ImagesService> _logger;
    private const string BucketName = "bejebeje.com";

    public ImagesService(
      IAmazonS3 s3Client,
      ILogger<ImagesService> logger)
    {
      _s3Client = s3Client;
      _logger = logger;
    }

    public async Task<bool> UploadArtistImageAsync(int artistId, Stream imageStream)
    {
      return await UploadImageAsync(imageStream, "artist-images", artistId);
    }

    public async Task<bool> UploadAuthorImageAsync(int authorId, Stream imageStream)
    {
      return await UploadImageAsync(imageStream, "author-images", authorId);
    }

    public async Task<bool> DeleteArtistImageAsync(int artistId)
    {
      return await DeleteImageAsync("artist-images", artistId);
    }

    public async Task<bool> DeleteAuthorImageAsync(int authorId)
    {
      return await DeleteImageAsync("author-images", authorId);
    }

    private async Task<bool> UploadImageAsync(Stream imageStream, string folder, int entityId)
    {
      try
      {
        using var originalImage = await Image.LoadAsync<Rgba32>(imageStream);
        
        var imagesToUpload = new[]
        {
          new { Size = "s", Width = 300, Height = 300 },
          new { Size = "sm", Width = 80, Height = 80 },
          new { Size = "xsm", Width = 60, Height = 60 }
        };

        foreach (var imageSpec in imagesToUpload)
        {
          using var resizedImage = originalImage.Clone(x => x.Resize(imageSpec.Width, imageSpec.Height));
          
          await UploadImageFormat(resizedImage, folder, entityId, imageSpec.Size, "jpg");
          await UploadImageFormat(resizedImage, folder, entityId, imageSpec.Size, "webp");
        }

        return true;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Failed to upload image for {Folder} {EntityId}", folder, entityId);
        return false;
      }
    }

    private async Task UploadImageFormat(Image<Rgba32> image, string folder, int entityId, string size, string format)
    {
      using var stream = new MemoryStream();
      
      if (format == "jpg")
      {
        await image.SaveAsJpegAsync(stream, new JpegEncoder { Quality = 75 });
      }
      else
      {
        await image.SaveAsWebpAsync(stream);
      }

      stream.Position = 0;
      
      var key = $"{folder}/{entityId}-{size}.{format}";
      
      var request = new PutObjectRequest
      {
        BucketName = BucketName,
        Key = key,
        InputStream = stream,
        ContentType = format == "jpg" ? "image/jpeg" : "image/webp",
        CannedACL = S3CannedACL.PublicRead
      };

      await _s3Client.PutObjectAsync(request);
    }

    private async Task<bool> DeleteImageAsync(string folder, int entityId)
    {
      try
      {
        var sizes = new[] { "s", "sm", "xsm" };
        var formats = new[] { "jpg", "webp" };

        foreach (var size in sizes)
        {
          foreach (var format in formats)
          {
            var key = $"{folder}/{entityId}-{size}.{format}";
            await _s3Client.DeleteObjectAsync(BucketName, key);
          }
        }

        return true;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Failed to delete images for {Folder} {EntityId}", folder, entityId);
        return false;
      }
    }
  }
}
