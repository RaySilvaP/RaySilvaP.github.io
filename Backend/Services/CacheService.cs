using Shared.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Backend.Services;

public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly MemoryCacheEntryOptions _options;

    public CacheService(IMemoryCache cache)
    {
        _cache = cache;
        _options = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
        };
    }
        
    public Task ClearCache()
    {
        throw new NotImplementedException();
    }

    public Task<Image?> GetImageAsync(string id)
    {
        if(_cache.TryGetValue($"image_{id}", out var value) && value is Image image)
                return Task.FromResult<Image?>(image);

        return Task.FromResult<Image?>(null);
    }

    public Task<Project?> GetProjectAsync(string id)
    {
        if(_cache.TryGetValue($"project_{id}", out var value) && value is Project project)
            return Task.FromResult<Project?>(project);

        return Task.FromResult<Project?>(null);
    }

    public Task<List<Image>?> GetProjectImagesAsync(string projectId)
    {
        if(_cache.TryGetValue($"images_{projectId}", out var value) && value is List<Image> images)
            return Task.FromResult<List<Image>?>(images);

        return Task.FromResult<List<Image>?>(null);
    }

    public Task<List<Project>?> GetProjectsAsync(string interval)
    {
        if(_cache.TryGetValue($"projects_{interval}", out var value) && value is List<Project> projects)
            return Task.FromResult<List<Project>?>(projects);

        return Task.FromResult<List<Project>?>(null);
    }

    public Task<Image?> GetProjectThumbnailAsync(string projectId)
    {
        if(_cache.TryGetValue($"thumbnail_{projectId}", out var value) && value is Image thumbnail)
            return Task.FromResult<Image?>(thumbnail);

        return Task.FromResult<Image?>(null);
    }

    public Task SetImageAsync(Image image)
    {
        _cache.Set($"image_{image.Id}", image, _options);

        return Task.CompletedTask;
    }

    public Task SetProjectAsync(Project project)
    {
        _cache.Set($"project_{project.Id}", project, _options);

        return Task.CompletedTask;
    }

    public Task SetProjectImagesAsync(string projectId, List<Image> images)
    {
        _cache.Set($"images_{projectId}", images, _options);

        return Task.CompletedTask;
    }

    public Task SetProjectsAsync(string interval, List<Project> projects)
    {
        _cache.Set($"projects_{interval}", projects, _options);

        return Task.CompletedTask;
    }

    public Task SetProjectThumbnailAsync(string projectId, Image thumbnail)
    {
        _cache.Set($"thumbnail_{projectId}", thumbnail, _options);

        return Task.CompletedTask;
    }
}
