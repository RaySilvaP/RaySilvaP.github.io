namespace Shared.Models;

public class Image
{
    public string Id { get; set; } = string.Empty;
    public string Base64String { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public long Size { get; set; }
}