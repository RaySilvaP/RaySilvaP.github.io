namespace Shared.Models;

public class Project
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Github { get; set; } = string.Empty;
    public Image? Thumbnail { get; set; }
    public List<Image>? Images { get; set; }
}