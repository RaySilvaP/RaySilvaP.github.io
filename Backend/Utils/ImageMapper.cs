using Shared.Models;

namespace Backend.Utils;

public class ImageMapper
{
    public static async Task<Image> IFormFileToImageAsync(IFormFile file)
    {
        var buffer = new byte[file.Length];
        await file.OpenReadStream().ReadAsync(buffer);
        return new Image
        {
            Name = file.FileName,
            Format = file.ContentType,
            Size = file.Length,
            Base64String = Convert.ToBase64String(buffer)
        };
    }
}
