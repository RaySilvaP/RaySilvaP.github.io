using Shared.Models;

namespace Backend.Services;

public interface ICacheService
{
    Task<List<Project>?> GetProjectsAsync(string interval);

    Task SetProjectsAsync(string interval, List<Project> projects);

    Task<Project?> GetProjectAsync(string id);

    Task SetProjectAsync(Project project);

    Task<Image?> GetProjectThumbnailAsync(string projectId);

    Task SetProjectThumbnailAsync(string projectId, Image thumbnail);

    Task<List<Image>?> GetProjectImagesAsync(string projectId);

    Task SetProjectImagesAsync(string projectId, List<Image> images);

    Task<Image?> GetImageAsync(string id);

    Task SetImageAsync(Image image);

    Task ClearCache();
}
