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

        return projects.Select(p => new Project
        {
            Id = p.Id.ToString()!,
            Name = p.Name,
            Description = p.Description,
            Base64Image = p.Base64Image
        });
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
                return new Project
                {
                    Id = project.Id.ToString(),
                    Name = project.Name,
                    Description = project.Description,
                    Base64Image = project.Base64Image
                };
            }
        }
        catch
        {
            return null;
        }
    }

    public async Task InsertProjectAsync(Project project)
    {
        var projectDto = new MongoProjectDto
        {
            Id = ObjectId.GenerateNewId(),
            Name = project.Name,
            Description = project.Description,
            Base64Image = project.Base64Image
        };
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

    public async Task<bool> UpdateProjectAsync(Project project)
    {
        try
        {
            var filter = Builders<MongoProjectDto>.Filter
            .Eq(p => p.Id, new ObjectId(project.Id));

            var update = Builders<MongoProjectDto>.Update
            .Set(p => p.Name, project.Name)
            .Set(p => p.Description, project.Description)
            .Set(p => p.Base64Image, project.Base64Image);

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