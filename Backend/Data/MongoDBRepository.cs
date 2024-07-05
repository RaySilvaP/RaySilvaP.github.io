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
    private IMongoCollection<MongoImageDto> Images => _database.GetCollection<MongoImageDto>("images");

    public async Task<IEnumerable<ProjectDto>> GetProjectsAsync(int skip, int take)
    {
        var projects = await Projects
        .AsQueryable()
        .Skip(skip)
        .Take(take)
        .ToListAsync();

        return projects.Select(p => (ProjectDto)p);
    }

    public async Task<ProjectDto?> GetProjectAsync(string id)
    {
        try
        {
            var filter = Builders<MongoProjectDto>.Filter.Eq(p => p.Id, new ObjectId(id));
            var project = await Projects.Find(filter).FirstOrDefaultAsync();
            if (project == null)
                return null;
            else
            {
                return (ProjectDto)project;
            }
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> InsertProjectAsync(ProjectDto project)
    {
        try
        {
            var projectDto = (MongoProjectDto)project;
            await Projects.InsertOneAsync(projectDto);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteProjectAsync(string id)
    {
        try
        {
            var filter = Builders<MongoProjectDto>.Filter.Eq(p => p.Id, new ObjectId(id));
            var project = await Projects.Find(filter).FirstOrDefaultAsync();
            await DeleteImageAsync(project.ThumbnailId);
            if (project.ImageIds != null)
            {
                foreach (var imageId in project.ImageIds)
                {
                    await DeleteImageAsync(imageId);
                }
            }
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

    public async Task<Image?> GetImageAsync(string id)
    {
        try
        {
            var filter = Builders<MongoImageDto>.Filter.Eq(i => i.Id, new ObjectId(id));

            var imageDto = await Images.Find(filter).FirstOrDefaultAsync();
            return (Image)imageDto;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> InsertImageAsync(string projectId, Image image)
    {
        try
        {
            var imageDto = (MongoImageDto)image;
            await Images.InsertOneAsync(imageDto);
            var isSuccess = await AddProjectImageAsync(projectId, imageDto.Id.ToString());
            return isSuccess;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteImageAsync(string id)
    {
        try
        {
            var filter = Builders<MongoImageDto>.Filter.Eq(i => i.Id, new ObjectId(id));
            var result = await Images.DeleteOneAsync(filter);
            return result.DeletedCount > 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateImageAsync(Image image)
    {
        try
        {
            var filter = Builders<MongoImageDto>.Filter
            .Eq(i => i.Id, new ObjectId(image.Id));

            var update = Builders<MongoImageDto>.Update
            .Set(i => i.Base64String, image.Base64String)
            .Set(i => i.Name, image.Name)
            .Set(i => i.Format, image.Format)
            .Set(i => i.Size, image.Size);

            var result = await Images.UpdateOneAsync(filter, update);
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

    private async Task<bool> AddProjectImageAsync(string projectId, string imageId)
    {
        try
        {
            var project = await GetProjectAsync(projectId);
            if (project != null)
            {
                if (project.ImageIds == null)
                    project.ImageIds = [imageId.ToString()];
                else
                    project.ImageIds.Add(imageId.ToString());

                var filter = Builders<MongoProjectDto>.Filter
                .Eq(p => p.Id, new ObjectId(project.Id));

                var update = Builders<MongoProjectDto>.Update
                .Set(p => p.ImageIds, project.ImageIds);

                await Projects.UpdateOneAsync(filter, update);
                return true;
            }
            else
                return false;
        }
        catch
        {
            return false;
        }
    }
}