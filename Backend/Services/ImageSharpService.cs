using System.Buffers.Text;
using System.Text;
using MongoDB.Driver;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace Backend.Services;

public class ImageSharpService : IImageService
{
    public async Task<byte[]> CompressAsync(string base64String)
    {
        var bytes = Convert.FromBase64String(base64String);
        using var stream = new MemoryStream(bytes);
        using var image = await Image.LoadAsync(stream);
        using var tempStream = new MemoryStream();
        var encoder = new JpegEncoder
        {
            Quality = 90
        };

        await image.SaveAsync(tempStream, encoder);
        return tempStream.ToArray();
    }

    public async Task<bool> IsBase64Valid(string base64String)
    {
        if (!Base64.IsValid(base64String))
            return false;

        try
        {
            var bytes = Convert.FromBase64String(base64String);
            using var stream = new MemoryStream(bytes);
            using var image = await Image.LoadAsync(stream);
            return true;
        }
        catch
        {
            return false;
        }

    }
}