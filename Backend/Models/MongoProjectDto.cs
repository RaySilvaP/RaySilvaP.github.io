using System.Runtime.CompilerServices;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Shared.Models;

namespace Backend.Models;

public class MongoProjectDto
{
    public ObjectId Id { get; set; }
    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;
    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;
    [BsonElement("thumbnailId")]
    public string ThumbnailId { get; set; } = string.Empty;
    [BsonElement("imagesIds")]
    public List<string>? ImageIds { get; set; }

    public static explicit operator ProjectDto(MongoProjectDto v)
    {
        return new ProjectDto
        {
            Id = v.Id.ToString(),
            Name = v.Name,
            Description = v.Description,
            ThumbnailId = v.ThumbnailId,
            ImageIds = v.ImageIds
        };
    }

    public static explicit operator MongoProjectDto(ProjectDto v)
    {
        return new MongoProjectDto
        {
            Id = v.Id == string.Empty ? ObjectId.GenerateNewId() : new ObjectId(v.Id),
            Name = v.Name,
            Description = v.Description,
            ThumbnailId = v.ThumbnailId,
            ImageIds = v.ImageIds
        };
    }
}