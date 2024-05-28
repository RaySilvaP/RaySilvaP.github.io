using Backend.Models;
using Shared.Models;

namespace Backend.Data;

public interface IRepository
{
    Task<IEnumerable<Project>> GetProjectsAsync(int skip, int take);

    Task<Project?> GetProjectAsync(string id);

    Task InsertProjectAsync(Project project);

    Task<bool> DeleteProjectAsync(string id);

    Task<bool> UpdateProjectAsync(Project project);

    Task<Admin?> GetAdminAsync(string username);
}