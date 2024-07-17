using Backend.Models;
using Shared.Models;

namespace Backend.Data;

public interface IRepository
{
    Task<IEnumerable<ProjectDto>> GetProjectsAsync(int skip, int take);

    Task<ProjectDto?> GetProjectAsync(string id);

    Task<int> GetProjectsCountAsync();

    Task<ProjectDto?> InsertProjectAsync(PostProjectDto project);

    Task<bool> DeleteProjectAsync(string id);

    Task<bool> UpdateProjectAsync(PutProjectDto project);

    Task<Image?> GetImageAsync(string id);

    Task<bool> InsertImageAsync(string projectId, Image image);

    Task<bool> InsertThumbnailAsync(string projectId, Image thumbnail);

    Task<bool> DeleteImageAsync(string id);

    Task<Admin?> GetAdminAsync(string username);
}