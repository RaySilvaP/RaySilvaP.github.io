using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Backend.Models;

public class MongoProjectDto
{
    public ObjectId Id { get; set; }
    [BsonElement("name")]
    public string Name { get; set; } = "";
    [BsonElement("description")]
    public string Description { get; set; } = "";
    [BsonElement("imageBase64")]
    public string? Base64Image { get; set; }
}