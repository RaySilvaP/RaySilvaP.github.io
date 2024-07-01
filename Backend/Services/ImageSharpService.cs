using System.Buffers.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using Shared.Models;

namespace Backend.Services;

public class ImageSharpService : IImageService
{
    public async Task CompressAsync(Shared.Models.Image image)
    {
        var bytes = Convert.FromBase64String(image.Base64String);
        using var stream = new MemoryStream(bytes);
        using var imageStream = await SixLabors.ImageSharp.Image.LoadAsync(stream);
        using var tempStream = new MemoryStream();
        var encoder = new JpegEncoder
        {
            Quality = 90
        };
        await imageStream.SaveAsync(tempStream, encoder);
        bytes = tempStream.ToArray();
        image.Base64String = Convert.ToBase64String(bytes);
        image.Type = "image/jpeg";
        image.Size = bytes.LongLength;
    }

    public async Task<bool> IsImageValidAsync(Shared.Models.Image image)
    {
        if (!Base64.IsValid(image.Base64String))
            return false;

        try
        {
            var bytes = Convert.FromBase64String(image.Base64String);
            using var stream = new MemoryStream(bytes);
            using var imageStream = await SixLabors.ImageSharp.Image.LoadAsync(stream);
            return true;
        }
        catch
        {
            return false;
        }

    }
}