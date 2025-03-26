using Shared.Models;

namespace Backend.Services;

public interface IImageService
{
    Task CompressAsync(Image image);

    Task<Image> CreateThumbnailAsync(Image image);
}
