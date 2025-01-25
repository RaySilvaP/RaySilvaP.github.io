using Backend.Models;
using Shared.Models;

namespace Backend.Data;

public interface IRepository
{
    Task<IEnumerable<Project>> GetProjectsAsync(int skip, int take);

    Task<Project?> GetProjectAsync(string id);

    Task<int> GetProjectsCountAsync();

    Task<Project?> InsertProjectAsync(PostProjectDto project);

    Task DeleteProjectAsync(string id);

    Task UpdateProjectAsync(PutProjectDto project);

    Task<Image?> GetProjectThumbnailAsync(string projectId);

    Task<List<Image>> GetProjectImagesAsync(string projectId); 

    Task PushImageToProjectAsync(string projectId, Image image);

    Task SetProjectThumbnailAsync(string projectId, Image thumbnail);

    Task DeleteProjectImageAsync(string projectId, string imageId);

    Task DeleteProjectImagesAsync(string projectId);

    Task<Admin?> GetAdminAsync(string username);
}