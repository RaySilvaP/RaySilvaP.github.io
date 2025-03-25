using Backend.Data;
using Backend.Serializers.Mongo;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Shared.Models;

namespace Backend.Installers;

public static class MongoDBInstaller
{
    public static void AddMongoDb(this IServiceCollection services, IConfigurationRoot configuration)
    {
        var connectionString = configuration.GetConnectionString("MONGODB_URI");
        connectionString ??= Environment.GetEnvironmentVariable("MONGODB_URI");
        if (connectionString == null)
            throw new Exception("No Mongo connection string found.");
        AddBsonMaps();
        services.AddSingleton<IMongoClient>(new MongoClient(connectionString));
        services.AddScoped<IRepository, MongoDBRepository>();
    }

    private static void AddBsonMaps()
    {
        var customIdSerializer = new CustomIdSerializer();
        var imageSerializer = new ImageSerializer();

        BsonClassMap.RegisterClassMap<Project>(classMap =>
        {
            classMap.MapIdMember(p => p.Id)
            .SetSerializer(customIdSerializer)
            .SetDefaultValue("")
            .SetIgnoreIfDefault(true);
            classMap.MapMember(p => p.Name).SetElementName("name");
            classMap.MapMember(p => p.ShortDescription).SetElementName("shortDescription");
            classMap.MapMember(p => p.Github).SetElementName("github");
            classMap.MapMember(p => p.Description).SetElementName("description");
            classMap.MapMember(p => p.Thumbnail).SetElementName("thumbnailId").SetSerializer(imageSerializer);
        });

        BsonClassMap.RegisterClassMap<Image>(classMap =>
        {
            classMap.AutoMap();
            classMap.MapIdMember(p => p.Id).SetSerializer(customIdSerializer);
        });
    }
}
