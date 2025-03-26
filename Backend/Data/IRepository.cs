using Backend.Models;
using Shared.Models;

namespace Backend.Data;

public interface IRepository
{
    Task<IEnumerable<Project>> GetProjectsAsync(int skip, int take);

    Task<Project> GetProjectAsync(string id);

    Task<int> GetProjectsCountAsync();

    Task<Project> InsertProjectAsync(PostProjectDto project);

    Task DeleteProjectAsync(string id);

    Task UpdateProjectAsync(UpdateProjectDto project);

    Task<Image?> GetProjectThumbnailAsync(string projectId);

    Task<List<Image>> GetProjectImagesAsync(string projectId); 

    Task PushImageToProjectAsync(string projectId, Image image);

    Task SetProjectThumbnailAsync(string projectId, Image thumbnail);

    Task DeleteProjectImageAsync(string projectId, string imageId);

    Task<Admin> GetAdminAsync(string username);

    Task CreateAdminAsync(Admin admin);
}
