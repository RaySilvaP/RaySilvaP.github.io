using Backend.Data;
using MongoDB.Driver;

namespace Backend.Installers;

public static class MongoDBInstaller
{
    public static void AddMongoDb(this IServiceCollection services, IConfigurationRoot configuration)
    {
        var connectionString = configuration.GetConnectionString("MONGODB_URI");
        connectionString ??= Environment.GetEnvironmentVariable("MONGODB_URI");
        if (connectionString == null)
            throw new Exception("No Mongo connection string found.");

        services.AddSingleton<IMongoClient>(new MongoClient(connectionString));
        services.AddScoped<IRepository, MongoDBRepository>();
    }
}