namespace Shared.Extensions;

public static class StreamExtension
{
    public static async Task<string> ConvertToBase64Async(this Stream stream)
    {
        if (stream is MemoryStream memoryStream)
            return Convert.ToBase64String(memoryStream.ToArray());

        var bytes = new byte[stream.Length];
        stream.Seek(0, SeekOrigin.Begin);
        await stream.ReadAsync(bytes);

        return Convert.ToBase64String(bytes);
    }
}