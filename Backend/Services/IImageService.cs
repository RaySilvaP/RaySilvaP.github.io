namespace Backend.Services;

public interface IImageService
{
    Task<byte[]> CompressAsync(string base64String);

    Task<bool> IsBase64Valid(string base64String);
}