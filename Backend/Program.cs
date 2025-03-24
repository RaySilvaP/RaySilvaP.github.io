using Backend.Data;
using Backend.Installers;
using Backend.Models;
using Backend.Services;
using Utils;

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
    app.CreateAdmin();
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

app.MapPut("/projects", async (IRepository repository, PutProjectDto body) =>
{
    await repository.UpdateProjectAsync(body);
    return Results.Ok();
})
.RequireAuthorization();

app.MapGet("/projects/{id}/images", async (IRepository repository, string id) =>
{
    var images = await repository.GetProjectImagesAsync(id);
    return Results.Ok(images);
});

app.MapPost("/projects/images", async (IRepository repository, IImageService service, PostImageDto body) =>
{
    var isValid = await service.IsImageValidAsync(body.Image);
    if (!isValid)
        return Results.BadRequest("Image invalid.");

    if (body.Image.Format != "image/gif")
        await service.CompressAsync(body.Image);

    await repository.PushImageToProjectAsync(body.ProjectId, body.Image);
    return Results.Ok();
})
.RequireAuthorization();

app.MapPost("projects/thumbnails", async (IRepository repository, IImageService service, PostImageDto body) =>
{
    var isValid = await service.IsImageValidAsync(body.Image);
    if (!isValid)
        return Results.BadRequest("Image invalid.");

    var thumbnail = await service.CreateThumbnailAsync(body.Image);

    await repository.SetProjectThumbnailAsync(body.ProjectId, thumbnail);
    return Results.Ok();
})
.RequireAuthorization();

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
