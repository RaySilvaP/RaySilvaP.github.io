namespace Shared.Models;

public class ProjectDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Github { get; set; } = string.Empty;
    public string ThumbnailId { get; set; } = string.Empty;
    public List<string>? ImageIds { get; set; }
}