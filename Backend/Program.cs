using Backend.Data;
using Backend.Exceptions;
using Backend.Installers;
using Backend.Models;
using Backend.Services;
using Backend.Utils;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMongoDb(builder.Configuration);
builder.Services.AddJwtAuthorization(builder.Configuration);
builder.Services.AddMemoryCache();
builder.Services.AddTransient<IImageService, ImageSharpService>();
builder.Services.AddTransient<CredentialsService>();
builder.Services.AddTransient<ICacheService, CacheService>();
builder.Services.AddAntiforgery();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

await app.CreateAdmin();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapGet("/projects", async (IRepository repository, ICacheService cache, int page = 1, int pageSize = 10) =>
{
    page = page < 1 ? 1 : page;
    pageSize = pageSize < 1 ? 1 : pageSize;
    var skip = (page - 1) * pageSize;

    var projects = await cache.GetProjectsAsync($"{skip}-{pageSize}");
    if (projects == null)
    {
        projects = await repository.GetProjectsAsync(skip, pageSize);
        await cache.SetProjectsAsync($"{skip}-{pageSize}", projects);
    }
    var projectsCount = await repository.GetProjectsCountAsync();
    var totalPages = projectsCount < pageSize ? 1 : (int)MathF.Ceiling(projectsCount / (float)pageSize);

    return Results.Ok(new { totalPages, projects });
});

app.MapGet("/projects/{id}", async (IRepository repository, ICacheService cache, string id) =>
{
    try
    {
        var project = await cache.GetProjectAsync(id);
        if (project == null)
        {
            project = await repository.GetProjectAsync(id);
            await cache.SetProjectAsync(project);
        }
        return Results.Ok(project);
    }
    catch (ProjectNotFoundException e)
    {
        return Results.NotFound(e.Message);
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        return Results.StatusCode(500);
    }
});

app.MapPost("/projects", async (IRepository repository, IImageService service, HttpRequest request, PostProjectDto body) =>
{
    var url = $"{request.Scheme}://{request.Host.Value}/";
    try
    {
        var project = await repository.InsertProjectAsync(body);
        return Results.Created(url + $"projects/{project.Id}", project);
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        return Results.StatusCode(500);
    }
})
.RequireAuthorization();

app.MapDelete("/projects/{id}", async (IRepository repository, string id) =>
{
    try
    {
        await repository.DeleteProjectAsync(id);
        return Results.Ok();
    }
    catch (ProjectNotFoundException e)
    {
        return Results.NotFound(e.Message);
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        return Results.StatusCode(500);
    }
})
.RequireAuthorization();

app.MapPut("/projects", async (IRepository repository, UpdateProjectDto body) =>
{
    try
    {
        await repository.UpdateProjectAsync(body);

        return Results.Ok();
    }
    catch (ProjectNotFoundException e)
    {
        return Results.NotFound(e.Message);
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        return Results.StatusCode(500);
    }
})
.RequireAuthorization();

app.MapGet("/images/{id}", async (IRepository repository, ICacheService cache, string id) =>
{
    try
    {
        var image = await cache.GetImageAsync(id);
        if (image == null)
        {
            image = await repository.GetImageAsync(id);
            await cache.SetImageAsync(image);
        }
        return Results.Ok(image);
    }
    catch (ImageNotFoundException e)
    {
        return Results.NotFound(e.Message);
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        return Results.StatusCode(500);
    }
});

app.MapGet("/projects/{id}/images", async (IRepository repository, ICacheService cache, string id) =>
{
    try
    {
        var images = await cache.GetProjectImagesAsync(id);
        if (images == null)
        {
            images = await repository.GetProjectImagesAsync(id);
            await cache.SetProjectImagesAsync(id, images);
        }
        return Results.Ok(images);
    }
    catch (ProjectNotFoundException e)
    {
        return Results.NotFound(e.Message);
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        return Results.StatusCode(500);
    }
});

app.MapPost("/projects/{id}/images", async (IRepository repository, IImageService service, string id, IFormFile file) =>
{
    const long MINIMUM_FILE_SIZE_B = 1024 * 1000;
    var image = await ImageMapper.IFormFileToImageAsync(file);
    try
    {
        if (image.Format != "image/gif" || image.Size > MINIMUM_FILE_SIZE_B)
            await service.CompressAsync(image);

        await repository.PushImageToProjectAsync(id, image);
        return Results.Ok();
    }
    catch (ProjectNotFoundException e)
    {
        return Results.NotFound(e.Message);
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        return Results.StatusCode(500);
    }
})
.RequireAuthorization()
.DisableAntiforgery();

app.MapGet("projects/{id}/thumbnail", async (IRepository repository, ICacheService cache, string id) =>
{
    try
    {
        var thumbnail = await cache.GetProjectThumbnailAsync(id);
        if (thumbnail == null)
        {
            thumbnail = await repository.GetProjectThumbnailAsync(id);
            if (thumbnail != null)
                await cache.SetProjectThumbnailAsync(id, thumbnail);
        }
        return Results.Ok(thumbnail);
    }
    catch (ProjectNotFoundException e)
    {
        return Results.NotFound(e.Message);
    }
    catch (ImageNotFoundException e)
    {
        return Results.NotFound(e.Message);
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        return Results.StatusCode(500);
    }
});

app.MapPost("projects/{id}/thumbnail", async (IRepository repository, IImageService service, string id, IFormFile file) =>
{
    var image = await ImageMapper.IFormFileToImageAsync(file);
    try
    {
        var thumbnail = await service.CreateThumbnailAsync(image);

        await repository.SetProjectThumbnailAsync(id, thumbnail);
        return Results.Ok();
    }
    catch (ProjectNotFoundException e)
    {
        return Results.NotFound(e.Message);
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        return Results.StatusCode(500);
    }
})
.RequireAuthorization()
.DisableAntiforgery();

app.MapDelete("project/{projectId}images/{imageId}", async (IRepository repository, string projectId, string imageId) =>
{
    try
    {
        await repository.DeleteProjectImageAsync(projectId, imageId);

        return Results.Ok();
    }
    catch (ProjectNotFoundException e)
    {
        return Results.NotFound(e.Message);
    }
    catch (ImageNotFoundException e)
    {
        return Results.NotFound(e.Message);
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        return Results.StatusCode(500);
    }
})
.RequireAuthorization();

app.MapPost("/login", async (
CredentialsService credentialsService,
ITokenService tokenService,
HttpContext context,
Login login) =>
{
    try
    {
        bool hasSucceeded = await credentialsService.VerifyCredentialsAsync(login);
        if (hasSucceeded)
        {
            var token = tokenService.GenerateToken();
            return Results.Ok(token);
        }
        return Results.Ok("Wrong credentials.");
    }
    catch (AdminNotFoundException e)
    {
        Console.WriteLine(e);
        return Results.Ok("Wrong credentials.");
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        return Results.StatusCode(500);
    }
});

var methods = new[] { "HEAD" };

app.MapMethods("/auth", methods, () => Results.Ok())
.RequireAuthorization();

app.MapMethods("/", methods, () => Results.Ok());

app.MapMethods("/cache", methods, async (ICacheService cache) => 
{
    await cache.ClearCache();
});

app.Run();
