using Shared.Models;

namespace Backend.Models;

public class PostImageDto
{
    public string ProjectId { get; set; } = string.Empty;
    public Image Image { get; set; } = new();
}