using Shared.Models;

namespace Backend.Models;

public class PutProjectImageDto
{
    public string Id { get; set; } = string.Empty;
    public Image Image { get; set; } = new();
}