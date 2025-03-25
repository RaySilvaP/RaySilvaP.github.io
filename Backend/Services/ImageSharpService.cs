using System.Buffers.Text;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp;

namespace Backend.Services;

public class ImageSharpService : IImageService
{
    public async Task CompressAsync(Shared.Models.Image image)
    {
        var bytes = Convert.FromBase64String(image.Base64String);
        using var stream = new MemoryStream(bytes);
        using var imageStream = await Image.LoadAsync(stream);
        using var tempStream = new MemoryStream();
        var encoder = new JpegEncoder { Quality = 95 };

        await imageStream.SaveAsync(tempStream, encoder);
        bytes = tempStream.ToArray();
        image.Base64String = Convert.ToBase64String(bytes);
        image.Format = "image/jpeg";
        image.Size = bytes.LongLength;
    }

    public async Task<Shared.Models.Image> CreateThumbnailAsync(Shared.Models.Image image)
    {
        var bytes = Convert.FromBase64String(image.Base64String);
        using var stream = new MemoryStream(bytes);
        using var tempStream = new MemoryStream();
        using var imageStream = await Image.LoadAsync(stream);
        var width = imageStream.Width;
        var height = imageStream.Height;
        if(image.Size > 30000) {
            width /= 3;
            height /= 3;
        }
        imageStream.Mutate(x => x.Resize(width, height));
        await imageStream.SaveAsJpegAsync(tempStream);
        bytes = tempStream.ToArray();

        return new Shared.Models.Image
        {
            Id = image.Id,
            Base64String = Convert.ToBase64String(bytes),
            Name = image.Name,
            Format = "image/jpeg",
            Size = bytes.LongLength
        };
    }

    public async Task<bool> IsImageValidAsync(Shared.Models.Image image)
    {
        if (!Base64.IsValid(image.Base64String))
            return false;

        try
        {
            var bytes = Convert.FromBase64String(image.Base64String);
            using var stream = new MemoryStream(bytes);
            using var imageStream = await Image.LoadAsync(stream);
            return true;
        }
        catch
        {
            return false;
        }

    }
}
