using Shared.Models;

namespace Backend.Services;

public interface IImageService
{
    Task CompressAsync(Image image);

    Task<bool> IsImageValidAsync(Image image);
}