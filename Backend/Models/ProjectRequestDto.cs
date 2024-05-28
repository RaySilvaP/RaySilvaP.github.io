using MongoDB.Bson.Serialization.Attributes;

namespace Backend.Models;

public class ProjectRequestDto
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string? Base64Image { get; set; }
}