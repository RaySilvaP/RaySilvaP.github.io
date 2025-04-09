using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using Shared.Models;

namespace Backend.Serializers.Mongo;

public class ImageSerializer : IBsonSerializer<Image?>
{
    Type IBsonSerializer.ValueType => typeof(Image);

    public List<Image>? Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        if (context.Reader.CurrentBsonType == BsonType.Array)
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

        context.Reader.ReadNull();
        return null;
    }

    Image? IBsonSerializer<Image?>.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        if (context.Reader.CurrentBsonType == BsonType.ObjectId)
            return new Image
            {
                Id = context.Reader.ReadObjectId().ToString()
            };

        context.Reader.ReadNull();
        return null;
    }

    object? IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        if (context.Reader.CurrentBsonType == BsonType.ObjectId)
        {
            return new Image { Id = context.Reader.ReadObjectId().ToString() };
        }
        else if (context.Reader.CurrentBsonType == BsonType.Array)
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

        context.Reader.ReadNull();
        return null;
    }

    public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, List<Image>? value)
    {
        if(value == null)
        {
            context.Writer.WriteNull();
            return;
        }
        context.Writer.WriteStartArray();
        foreach(var image in value)
        {
            if(ObjectId.TryParse(image.Id, out var id))
                context.Writer.WriteObjectId(id);
        }
        context.Writer.WriteEndArray();
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

