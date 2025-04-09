using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Shared.Models;

namespace Backend.Serializers.Mongo;

public class ImagesSerializer : IBsonArraySerializer
{
    Type IBsonSerializer.ValueType => typeof(List<Image>);

    object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var images = new List<Image>();

        context.Reader.ReadStartArray();
        while (context.Reader.ReadBsonType() != BsonType.EndOfDocument)
        {
            images.Add(new Image { Id = context.Reader.ReadObjectId().ToString() });
        }
        context.Reader.ReadEndArray();

        return images;
    }

    public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
    {
        if (value is Image image && ObjectId.TryParse(image.Id, out var id))
        {
            context.Writer.WriteObjectId(id);
        }
        else
        {
            context.Writer.WriteStartArray();
            context.Writer.WriteEndArray();
        }
    }

    public bool TryGetItemSerializationInfo(out BsonSerializationInfo serializationInfo)
    {
        serializationInfo = new BsonSerializationInfo("Id", new ImagesSerializer(), typeof(Image));
        return true;
    }
}

