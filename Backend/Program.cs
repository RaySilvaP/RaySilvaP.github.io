using Backend.Data;
using Backend.Installers;
using Backend.Models;
using Backend.Services;
using Backend.Utils;
using Shared.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMongoDb(builder.Configuration);
builder.Services.AddJwtAuthorization(builder.Configuration);
builder.Services.AddTransient<IImageService, ImageSharpService>();
builder.Services.AddTransient<CredentialsService>();
builder.Services.AddAntiforgery();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    await app.CreateAdmin();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapGet("/projects", async (IRepository repository, int page = 1, int pageSize = 10) =>
{
    page = page < 1 ? 1 : page;
    pageSize = pageSize < 1 ? 1 : pageSize;
    var projects = await repository.GetProjectsAsync((page - 1) * pageSize, pageSize);
    var projectsCount = await repository.GetProjectsCountAsync();
    var totalPages = projectsCount < pageSize ? 1 : (int)MathF.Ceiling(projectsCount / (float)pageSize);
    return Results.Ok(new { totalPages, projects });
});

app.MapGet("/projects/{id}", async (string id, IRepository repository) =>
{
    var project = await repository.GetProjectAsync(id);
    if (project == null)
        return Results.NotFound();
    else
        return Results.Ok(project);
});

app.MapPost("/projects", async (IRepository repository, IImageService service, HttpRequest request, PostProjectDto body) =>
{
    var url = $"{request.Scheme}://{request.Host.Value}/";
    var projectDto = await repository.InsertProjectAsync(body);
    if (projectDto != null)
        return Results.Created(url + $"projects/{projectDto.Id}", projectDto);
    else
        return Results.StatusCode(500);
})
.RequireAuthorization();

app.MapDelete("/projects/{id}", async (IRepository repository, string id) =>
{
    try
    {
        await repository.DeleteProjectAsync(id);
        return Results.Ok();
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        return Results.NotFound("Couldn't delete the project.");
    }
})
.RequireAuthorization();

app.MapPut("/projects", async (IRepository repository, UpdateProjectDto body) =>
{
    await repository.UpdateProjectAsync(body);
    return Results.Ok();
})
.RequireAuthorization();

app.MapGet("/projects/{id}/images", async (IRepository repository, string id) =>
{
    var images = await repository.GetProjectImagesAsync(id);
    foreach (var image in images)
    {
        Console.WriteLine(image.Id);
    }
    return Results.Ok(images);
});

app.MapPost("/projects/{id}/images", async (IRepository repository, IImageService service, string id, IFormFile file) =>
{
    var image = await ImageMapper.IFormFileToImageAsync(file);
    Console.WriteLine(file.Length);
    try
    {
        if (image.Format != "image/gif" || image.Size < 1024 * 1000)
            await service.CompressAsync(image);

        await repository.PushImageToProjectAsync(id, image);
        return Results.Ok();
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        return Results.BadRequest("Image invalid.");
    }
})
.RequireAuthorization()
.DisableAntiforgery();

app.MapPost("projects/{id}/thumbnail", async (IRepository repository, IImageService service, string id, IFormFile file) =>
{
    var image = await ImageMapper.IFormFileToImageAsync(file);
    try
    {
        var thumbnail = await service.CreateThumbnailAsync(image);

        await repository.SetProjectThumbnailAsync(id, thumbnail);
        return Results.Ok();
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        return Results.BadRequest("Image invalid.");
    }
})
.RequireAuthorization()
.DisableAntiforgery();

app.MapDelete("project/{projectId}images/{imageId}", async (IRepository repository, string projectId, string imageId) =>
{
    await repository.DeleteProjectImageAsync(projectId, imageId);
    return Results.Ok();
})
.RequireAuthorization();

app.MapPost("/login", async (
CredentialsService credentialsService,
ITokenService tokenService,
HttpContext context,
Login login) =>
{
    bool wasSucceeded = await credentialsService.VerifyCredentialsAsync(login);
    if (wasSucceeded)
    {
        var token = tokenService.GenerateToken();
        return Results.Ok(token);
    }
    return Results.Forbid();
});

var methods = new[] { "HEAD" };

app.MapMethods("/auth", methods, () => Results.Ok())
.RequireAuthorization();

app.MapMethods("/", methods, () => Results.Ok());

app.Run();
