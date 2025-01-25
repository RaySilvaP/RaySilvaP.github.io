using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace Backend.Serializers.Mongo;

public class CustomIdSerializer : IBsonSerializer
{
    public Type ValueType => typeof(string);

    public object Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        return context.Reader.ReadObjectId().ToString();
    }

    public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
    {
        var objectId = ObjectId.TryParse((string)value, out ObjectId id) ? id : ObjectId.GenerateNewId(); 
        context.Writer.WriteObjectId(objectId);
    }
}