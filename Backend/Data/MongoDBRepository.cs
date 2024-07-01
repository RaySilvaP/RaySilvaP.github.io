using Backend.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Shared.Models;

namespace Backend.Data;

public sealed class MongoDBRepository(IMongoClient client) : IRepository
{
    private readonly IMongoDatabase _database = client.GetDatabase("db");
    private IMongoCollection<MongoProjectDto> Projects => _database.GetCollection<MongoProjectDto>("projects");

    public async Task<IEnumerable<Project>> GetProjectsAsync(int skip, int take)
    {
        var projects = await Projects
        .AsQueryable()
        .Skip(skip)
        .Take(take)
        .ToListAsync();

        return projects.Select(p => (Project)p);
    }

    public async Task<Project?> GetProjectAsync(string id)
    {
        try
        {
            var filter = Builders<MongoProjectDto>.Filter.Eq(p => p.Id, new ObjectId(id));
            var project = await Projects.Find(filter).FirstOrDefaultAsync();
            if (project == null)
                return null;
            else
            {
                return (Project)project;
            }
        }
        catch
        {
            return null;
        }
    }

    public async Task InsertProjectAsync(Project project)
    {
        var projectDto = (MongoProjectDto)project;
        await Projects.InsertOneAsync(projectDto);
    }

    public async Task<bool> DeleteProjectAsync(string id)
    {
        try
        {
            var filter = Builders<MongoProjectDto>.Filter.Eq(p => p.Id, new ObjectId(id));
            var result = await Projects.DeleteOneAsync(filter);
            return result.DeletedCount > 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateProjectAsync(PutProjectDto project)
    {
        try
        {
            var filter = Builders<MongoProjectDto>.Filter
            .Eq(p => p.Id, new ObjectId(project.Id));

            var update = Builders<MongoProjectDto>.Update
            .Set(p => p.Name, project.Name)
            .Set(p => p.Description, project.Description);

            var result = await Projects.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateProjectImageAsync(PutProjectImageDto project)
    {
        try
        {
            var filter = Builders<MongoProjectDto>.Filter
            .Eq(p => p.Id, new ObjectId(project.Id));

            var update = Builders<MongoProjectDto>.Update
            .Set(p => p.Image, project.Image);

            var result = await Projects.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }
        catch
        {
            return false;
        }
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
}