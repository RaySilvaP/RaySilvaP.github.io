using System.Security.Cryptography;
using Backend.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Shared.Models;

namespace Backend.Data;

public sealed class MongoDBRepository(IMongoClient client) : IRepository
{
    private readonly IMongoDatabase _database = client.GetDatabase("db");
    private IMongoCollection<Project> Projects => _database.GetCollection<Project>("projects");
    private IMongoCollection<BsonDocument> BsonProjects => _database.GetCollection<BsonDocument>("projects");
    private IMongoCollection<Image> Images => _database.GetCollection<Image>("images");

    public async Task<IEnumerable<Project>> GetProjectsAsync(int skip, int take)
    {
        var projects = await Projects
        .AsQueryable()
        .Skip(skip)
        .Take(take)
        .ToListAsync();

        return projects;
    }

    public async Task<Project?> GetProjectAsync(string id)
    {
        var filter = Builders<Project>.Filter.Eq(p => p.Id, id);
        return await Projects.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<int> GetProjectsCountAsync()
    {
        var filter = Builders<Project>.Filter.Empty;
        var count = await Projects.CountDocumentsAsync(filter);
        return (int)count;
    }

    public async Task<Project?> InsertProjectAsync(PostProjectDto dto)
    {
        var project = new Project
        {
            Name = dto.Name,
            ShortDescription = dto.ShortDescription,
            Description = dto.Description,
            Github = dto.Github

        };
        await Projects.InsertOneAsync(project);
        return project;
    }

    public async Task DeleteProjectAsync(string id)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(id));
        var project = await BsonProjects.Find(filter).FirstAsync();

        if(project.TryGetElement("thumbnailId", out BsonElement thumbnailId))
            await DeleteImageAsync(thumbnailId.Value.AsString);

        if(project.TryGetElement("imageIds", out BsonElement imageIds))
            await DeleteImagesAsync(imageIds.Value.AsBsonArray.Values.Select(v => v.AsString));

        await BsonProjects.DeleteOneAsync(filter);
    }

    public async Task UpdateProjectAsync(PutProjectDto project)
    {
        var filter = Builders<Project>.Filter
        .Eq(p => p.Id, project.Id);

        var update = Builders<Project>.Update
        .Set(p => p.Name, project.Name)
        .Set(p => p.ShortDescription, project.ShortDescription)
        .Set(p => p.Description, project.Description)
        .Set(p => p.Github, project.Github);


        await Projects.UpdateOneAsync(filter, update);
    }

    public async Task<Image?> GetProjectThumbnailAsync(string projectId)
    {
        var projectFilter = Builders<BsonDocument>.Filter.Eq("_id", projectId);
        var project = await BsonProjects.Find(projectFilter).FirstAsync();
        var thumbnailId = project.GetElement("thumbnailId").Value.AsString;
        var imageFilter = Builders<Image>.Filter.Eq(i => i.Id, thumbnailId);
        var thumbnail = await Images.Find(imageFilter).FirstOrDefaultAsync();
        return thumbnail;
    }

    public async Task<List<Image>> GetProjectImagesAsync(string projectId)
    {
        var projectFilter = Builders<BsonDocument>.Filter.Eq("_id", projectId);
        var project = await BsonProjects.Find(projectFilter).FirstAsync();
        var imageIds = project.GetElement("imageIds").Value.AsBsonArray.Values.Select(v => v.ToString());
        var imageFilter = Builders<Image>.Filter.In(i => i.Id, imageIds);
        return await Images.Find(imageFilter).ToListAsync();
    }

    public async Task PushImageToProjectAsync(string projectId, Image image)
    {
        await Images.InsertOneAsync(image);

        var filter = Builders<BsonDocument>.Filter.Eq("_id", projectId);
        var update = Builders<BsonDocument>.Update.Push("imageIds", image.Id);
        await BsonProjects.UpdateOneAsync(filter, update);
    }

    public async Task SetProjectThumbnailAsync(string projectId, Image thumbnail)
    {
        await Images.InsertOneAsync(thumbnail);
        await DeleteThumbnailAsync(projectId);
        var filter = Builders<BsonDocument>.Filter.Eq("_id", projectId);
        var update = Builders<BsonDocument>.Update.Set("thumbnailId", thumbnail.Id);
        await BsonProjects.UpdateOneAsync(filter, update);

    }

    public async Task DeleteProjectImageAsync(string projectId, string imageId)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("_id", projectId);
        var project = await BsonProjects.Find(filter).FirstAsync();
        var imageIds = project.GetElement("imageIds").Value.AsBsonArray.Values
        .Select(v => v.AsString)
        .ToList()
        .Remove(imageId);
        var update = Builders<BsonDocument>.Update.Set("imageIds", imageIds);
        await BsonProjects.UpdateOneAsync(filter, update);
        await DeleteImageAsync(imageId);
    }

    public async Task DeleteProjectImagesAsync(string projectId)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("_id", projectId);
        var project = await BsonProjects.Find(filter).FirstAsync();
        var imageIds = project.GetElement("imageIds").Value.AsBsonArray.Values.Select(v => v.AsString);
        await DeleteImagesAsync(imageIds);
    }

    public async Task<Admin?> GetAdminAsync(string username)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("username", username);
        var result = await _database
            .GetCollection<BsonDocument>("admin")
            .Find(filter)
            .FirstOrDefaultAsync();

        if (result == null)
            return null;
        else
            return new Admin
            {
                Username = result.GetElement("username").Value.ToString()!,
                PasswordHash = result.GetElement("passwordHash").Value.ToString()!
            };
    }

    private async Task DeleteThumbnailAsync(string projectId)
    {
        var thumbnail = await GetProjectThumbnailAsync(projectId);
        if (thumbnail == null)
            return;

        await DeleteImageAsync(thumbnail.Id);
    }

    private async Task DeleteImageAsync(string id)
    {
        var filter = Builders<Image>.Filter.Eq(i => i.Id, id);
        await Images.DeleteOneAsync(filter);
    }

    private async Task DeleteImagesAsync(IEnumerable<string> imageIds)
    {
        var filter = Builders<Image>.Filter.In(i => i.Id, imageIds);
        await Images.DeleteManyAsync(filter);
    }

    public async Task CreateAdminAsync(string username, string passwordHash)
    {
        var document = new BsonDocument
        {
            {"username", username},
            {"passwordHash", passwordHash}
        };
        await _database.GetCollection<BsonDocument>("admin").InsertOneAsync(document);
    }
}
