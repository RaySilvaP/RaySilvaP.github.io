using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using Shared.Models;

namespace Backend.Serializers.Mongo;

public class ImageSerializer : IBsonSerializer<Image?>
{
    Type IBsonSerializer.ValueType => typeof(Image);

    Image? IBsonSerializer<Image?>.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var type = context.Reader.CurrentBsonType;
        switch (type)
        {
            case BsonType.ObjectId:
                return new Image
                {
                    Id = context.Reader.ReadObjectId().ToString()
                };
            default:
                context.Reader.ReadNull();
                return null;
        }
    }

    object? IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var type = context.Reader.CurrentBsonType;
        switch (type)
        {
            case BsonType.ObjectId:
                return new Image
                {
                    Id = context.Reader.ReadObjectId().ToString()
                };
            default:
                context.Reader.ReadNull();
                return null;
        }
    }

    void IBsonSerializer<Image?>.Serialize(BsonSerializationContext context, BsonSerializationArgs args, Image? value)
    {
        if (value != null && ObjectId.TryParse(value.Id, out var id))
            context.Writer.WriteObjectId(id);
        else
            context.Writer.WriteNull();
    }

    void IBsonSerializer.Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
    {
        context.Writer.WriteNull();
    }
}

