using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Shared.Models;

namespace Backend.Models;

public class MongoImageDto
{
    public ObjectId Id { get; set; }
    [BsonElement("base64String")]
    public string Base64String { get; set; } = string.Empty;
    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;
    [BsonElement("format")]
    public string Format { get; set; } = string.Empty;
    [BsonElement("size")]
    public long Size { get; set; }

    public static explicit operator Image(MongoImageDto imageDto)
    {
        return new Image
        {
            Id = imageDto.Id.ToString(),
            Base64String = imageDto.Base64String,
            Name = imageDto.Name,
            Format = imageDto.Format,
            Size = imageDto.Size
        };
    }

    public static explicit operator MongoImageDto(Image image)
    {
        return new MongoImageDto
        {
            Id = image.Id == string.Empty ? ObjectId.GenerateNewId() : new ObjectId(image.Id),
            Base64String = image.Base64String,
            Name = image.Name,
            Format = image.Format,
            Size = image.Size
        };
    }
}

