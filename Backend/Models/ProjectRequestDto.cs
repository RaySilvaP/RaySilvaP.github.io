using Shared.Models;

namespace Backend.Models;

public class ProjectRequestDto
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public Image Image { get; set; } = new();
}