using MongoDB.Driver;

namespace Backend.Installers;

public static class MongoDBInstaller
{
    public static void AddMongoDb(this IServiceCollection services)
    {
        var configuration = services.BuildServiceProvider().GetRequiredService<IConfigurationRoot>();
        var connectionString = configuration.GetConnectionString("MONGODB_URI");
        services.AddSingleton<IMongoClient>(new MongoClient(connectionString));
    }
}