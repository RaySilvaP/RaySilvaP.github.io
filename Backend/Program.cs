using Backend.Data;
using Backend.Installers;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IConfigurationRoot>(builder.Configuration);
builder.Services.AddMongoDb();
builder.Services.AddScoped<IRepository, MongoDBRepository>();
builder.Services.AddTransient<IImageService, ImageSharpService>();
builder.Services.AddTransient<CredentialsService>();
builder.Services.AddJwtAuthorization();
builder.Services.AddAntiforgery();
// builder.Services.AddTransient<HtmlService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
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
    var project = new ProjectDto
    {
        Name = body.Name,
        Description = body.Description,
    };

    var url = $"{request.Scheme}://{request.Host.Value}/";

    var isSuccess = await repository.InsertProjectAsync(project);
    if (isSuccess)
        return Results.Created(url + $"projects/{project.Id}", project);
    else
        return Results.StatusCode(500);
})
.RequireAuthorization();

app.MapDelete("/projects/{id}", async (IRepository repository, string id) =>
{
    var wasDeleted = await repository.DeleteProjectAsync(id);
    if (wasDeleted)
        return Results.Ok();
    else
        return Results.NotFound();
})
.RequireAuthorization();

app.MapPut("/projects", async (IRepository repository, PutProjectDto body) =>
{
    var wasModified = await repository.UpdateProjectAsync(body);
    if (wasModified)
        return Results.Ok();
    else
        return Results.NotFound();
})
.RequireAuthorization();

app.MapGet("/images/{id}", async (IRepository repository, string id) =>
{
    var image = await repository.GetImageAsync(id);
    if (image == null)
        return Results.NotFound();
    else
        return Results.Ok(image);
});

app.MapPost("/images", async (IRepository repository, IImageService service, PostImageDto body) =>
{
    var isValid = await service.IsImageValidAsync(body.Image);
    if (!isValid)
        return Results.BadRequest("Image invalid.");

    if (body.Image.Format != "image/gif")
        await service.CompressAsync(body.Image);

    var isSuccess = await repository.InsertImageAsync(body.ProjectId, body.Image);
    if (isSuccess)
        return Results.Ok();
    else
        return Results.NotFound();
})
.RequireAuthorization();

app.MapPost("/images/thumbnails", async (IRepository repository, IImageService service, PostImageDto body) =>
{
    var isValid = await service.IsImageValidAsync(body.Image);
    if (!isValid)
        return Results.BadRequest("Image invalid.");

    var thumbnail = await service.CreateThumbnailAsync(body.Image);

    var isSuccess = await repository.InsertThumbnailAsync(body.ProjectId, thumbnail);
    if (isSuccess)
        return Results.Ok();
    else
        return Results.NotFound();
})
.RequireAuthorization();

app.MapDelete("/images/{id}", async (IRepository repository, string id) =>
{
    var wasDeleted = await repository.DeleteImageAsync(id);
    if (wasDeleted)
        return Results.Ok();
    else
        return Results.NotFound();
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
    return Results.Unauthorized();
});

app.MapGet("/auth", () => Results.Ok())
.RequireAuthorization();

app.Run();