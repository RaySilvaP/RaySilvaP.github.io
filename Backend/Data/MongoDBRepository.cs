using Backend.Exceptions;
using Backend.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Shared.Models;

namespace Backend.Data;

public sealed class MongoDBRepository(IMongoClient client) : IRepository
{
    private readonly IMongoDatabase _database = client.GetDatabase("portifolio");
    private IMongoCollection<Project> Projects => _database.GetCollection<Project>("projects");
    private IMongoCollection<Image> Images => _database.GetCollection<Image>("images");
    private IMongoCollection<Admin> Admins => _database.GetCollection<Admin>("admins");

    public async Task<List<Project>> GetProjectsAsync(int skip, int take)
    {
        var projects = await Projects
        .AsQueryable()
        .Skip(skip)
        .Take(take)
        .ToListAsync();

        return projects;
    }

    public async Task<Project> GetProjectAsync(string id)
    {
        var filter = Builders<Project>.Filter.Eq(p => p.Id, id);
        var project = await Projects.Find(filter).FirstOrDefaultAsync();
        if (project == null)
            throw new ProjectNotFoundException($"Project {id} not found.");

        return project;
    }

    public async Task<int> GetProjectsCountAsync()
    {
        var filter = Builders<Project>.Filter.Empty;
        var count = await Projects.CountDocumentsAsync(filter);
        return (int)count;
    }

    public async Task<Project> InsertProjectAsync(PostProjectDto dto)
    {
        var project = new Project
        {
            Id = ObjectId.GenerateNewId().ToString(),
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
        var project = await GetProjectAsync(id);

        if (project.Thumbnail != null)
            await DeleteImageAsync(project.Thumbnail.Id);

        if (project.Images.Count > 0)
            await DeleteImagesAsync(project.Images.Select(i => i.Id));

        var filter = Builders<Project>.Filter.Eq(p => p.Id, id);
        await Projects.DeleteOneAsync(filter);
    }

    public async Task UpdateProjectAsync(UpdateProjectDto project)
    {
        var filter = Builders<Project>.Filter
        .Eq(p => p.Id, project.Id);

        var update = Builders<Project>.Update
        .Set(p => p.Name, project.Name)
        .Set(p => p.ShortDescription, project.ShortDescription)
        .Set(p => p.Description, project.Description)
        .Set(p => p.Github, project.Github);


        var result = await Projects.UpdateOneAsync(filter, update);
        if(result.ModifiedCount < 1)
            throw new ProjectNotFoundException($"Project {project.Id} not found.");
    }

    public async Task<Image?> GetProjectThumbnailAsync(string projectId)
    {
        var project = await GetProjectAsync(projectId);
        if (project.Thumbnail == null)
            return null;

        var imageFilter = Builders<Image>.Filter.Eq(i => i.Id, project.Thumbnail.Id);
        var thumbnail = await Images.Find(imageFilter).FirstOrDefaultAsync();
        if (thumbnail == null)
            throw new ImageNotFoundException($"Thumbnail {project.Thumbnail.Id} not found.");

        return thumbnail;
    }

    public async Task<List<Image>> GetProjectImagesAsync(string projectId)
    {
        var project = await GetProjectAsync(projectId);

        var imageIds = project.Images.Select(i => i.Id);
        var imageFilter = Builders<Image>.Filter.In(i => i.Id, imageIds);

        return await Images.Find(imageFilter).ToListAsync();
    }

    public async Task<Image> GetImageAsync(string id)
    {
        var filter = Builders<Image>.Filter.Eq(i => i.Id, id);
        var image = await Images.Find(filter).FirstOrDefaultAsync();
        if(image == null)
            throw new ImageNotFoundException();

        return image;
    }

    public async Task PushImageToProjectAsync(string projectId, Image image)
    {
        image.Id = ObjectId.GenerateNewId().ToString();
        await Images.InsertOneAsync(image);

        var filter = Builders<Project>.Filter.Eq(p => p.Id, projectId);
        var update = Builders<Project>.Update.Push(p => p.Images, image);
        var result = await Projects.UpdateOneAsync(filter, update);
        if(result.ModifiedCount < 1)
            throw new ProjectNotFoundException($"Project {projectId} not found.");
    }

    public async Task SetProjectThumbnailAsync(string projectId, Image thumbnail)
    {
        thumbnail.Id = ObjectId.GenerateNewId().ToString();
        await Images.InsertOneAsync(thumbnail);
        await DeleteThumbnailAsync(projectId);

        var filter = Builders<Project>.Filter.Eq(p => p.Id, projectId);
        var update = Builders<Project>.Update.Set(p => p.Thumbnail, thumbnail);
        var result = await Projects.UpdateOneAsync(filter, update);
        if(result.ModifiedCount < 1)
            throw new ProjectNotFoundException($"Project {projectId} not found.");

    }

    public async Task DeleteProjectImageAsync(string projectId, string imageId)
    {
        var project = await GetProjectAsync(projectId);
        var image = project.Images.Find(i => i.Id == imageId);
        if (image == null)
            throw new ImageNotFoundException($"Image {imageId} not found in project gallery.");

        project.Images.Remove(image);
        var update = Builders<Project>.Update.Set(p => p.Images, project.Images);
        var filter = Builders<Project>.Filter.Eq(p => p.Id, projectId);
        await Projects.UpdateOneAsync(filter, update);
        await DeleteImageAsync(imageId);
    }

    public async Task<Admin> GetAdminAsync(string username)
    {
        var filter = Builders<Admin>.Filter.Eq(a => a.Username, username);
        var admin = await Admins.Find(filter).FirstOrDefaultAsync();

        if (admin == null)
            throw new AdminNotFoundException();
        
        return admin;
    }

    public async Task CreateAdminAsync(Admin admin)
    {
        await Admins.InsertOneAsync(admin);
    }

    public async Task UpdateAdminPassword(Admin admin)
    {
        var filter = Builders<Admin>.Filter.Eq(a => a.Username, admin.Username);
        var update = Builders<Admin>.Update.Set(a => a.PasswordHash, admin.PasswordHash);
        var result = await Admins.UpdateOneAsync(filter, update);
        if(result.ModifiedCount < 1)
            throw new AdminNotFoundException();
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
}
