using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace Backend.Serializers.Mongo;

public class CustomIdSerializer : IBsonSerializer<string>
{
    public Type ValueType => typeof(string);

    public object Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        return context.Reader.ReadObjectId().ToString();
    }

    public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
    {
        if (ObjectId.TryParse((string)value, out ObjectId id))
            context.Writer.WriteObjectId(id);
        else if(value is ObjectId)
            context.Writer.WriteObjectId((ObjectId)value);
        else
            throw new BsonSerializationException("Couln't convert id to objectId");
    }

    public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, string value)
    {
        if (ObjectId.TryParse(value, out ObjectId id))
            context.Writer.WriteObjectId(id);
        else
            throw new BsonSerializationException("Couldn't convert id to objectId");
    }

    string IBsonSerializer<string>.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        return context.Reader.ReadObjectId().ToString();
    }
}
