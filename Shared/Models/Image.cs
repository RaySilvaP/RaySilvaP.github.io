namespace Shared.Models;

public class Image
{
    public string Base64String { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public long Size { get; set; }

    public Image()
    {
        Base64String = string.Empty;
        Name = string.Empty;
        Type = string.Empty;
    }
}