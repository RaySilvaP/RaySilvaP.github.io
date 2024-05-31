using System.Buffers.Text;
using Backend.Data;
using Backend.Installers;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Antiforgery;
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
    return Results.Ok(projects);
});

app.MapGet("/projects/{id}", async (string id, IRepository repository) =>
{
    var project = await repository.GetProjectAsync(id);
    if (project == null)
        return Results.NotFound();
    else
        return Results.Ok(project);
});

app.MapPost("/projects", async (IRepository repository, IImageService service, ProjectRequestDto body) =>
{
    Project project;
    if (body.Base64Image == null)
    {
        project = new Project
        {
            Name = body.Name,
            Description = body.Description
        };
    }
    else
    {
        var isValid = await service.IsBase64Valid(body.Base64Image);
        if (!isValid)
            return Results.BadRequest("Invalid image.");

        var bytes = await service.CompressAsync(body.Base64Image);
        project = new Project
        {
            Name = body.Name,
            Description = body.Description,
            Base64Image = Convert.ToBase64String(bytes)
        };
    }
    await repository.InsertProjectAsync(project);
    return Results.Created();
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

app.MapPut("/projects", async (IRepository repository, IImageService service, Project project) =>
{
    if (project.Base64Image != null)
    {
        var isValid = await service.IsBase64Valid(project.Base64Image);
        if (!isValid)
            return Results.BadRequest("Invalid image.");
    }

    var wasModified = await repository.UpdateProjectAsync(project);
    if (wasModified)
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