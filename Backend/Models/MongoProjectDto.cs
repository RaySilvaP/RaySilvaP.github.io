using System.Runtime.CompilerServices;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Shared.Models;

namespace Backend.Models;

public class MongoProjectDto
{
    public ObjectId Id { get; set; }
    [BsonElement("name")]
    public string Name { get; set; } = "";
    [BsonElement("description")]
    public string Description { get; set; } = "";
    [BsonElement("image")]
    public Image Image { get; set; } = new();

    public static explicit operator Project(MongoProjectDto v)
    {
        return new Project
        {
            Id = v.Id.ToString(),
            Name = v.Name,
            Description = v.Description,
            Image = v.Image
        };
    }

    public static explicit operator MongoProjectDto(Project v)
    {
        return new MongoProjectDto
        {
            Id = v.Id == string.Empty ? ObjectId.GenerateNewId() : new ObjectId(v.Id),
            Name = v.Name,
            Description = v.Description,
            Image = v.Image
        };
    }
}