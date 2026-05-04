using backend.Data;
using backend.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var sqlConnectionString = builder.Configuration.GetConnectionString("ReleaseDb");

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "http://127.0.0.1:5173",
                "http://localhost:4173",
                "http://127.0.0.1:4173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddDbContext<ReleaseDbContext>(options =>
{
    if (!string.IsNullOrWhiteSpace(sqlConnectionString))
    {
        options.UseSqlServer(sqlConnectionString);
        return;
    }

    var dataDirectory = builder.Configuration["DataDirectory"];
    if (string.IsNullOrWhiteSpace(dataDirectory))
    {
        var homeDirectory = Environment.GetEnvironmentVariable("HOME");
        dataDirectory = !string.IsNullOrWhiteSpace(homeDirectory)
            ? Path.Combine(homeDirectory, "data", "game-release-dashboard")
            : Path.Combine(builder.Environment.ContentRootPath, "App_Data");
    }

    Directory.CreateDirectory(dataDirectory);
    options.UseSqlite($"Data Source={Path.Combine(dataDirectory, "releases.db")}");
});
builder.Services.AddScoped<IReleaseRepository, ReleaseRepository>();

var app = builder.Build();
var webRootPath = app.Environment.WebRootPath;
var hasFrontendBundle =
    !string.IsNullOrWhiteSpace(webRootPath) &&
    File.Exists(Path.Combine(webRootPath, "index.html"));

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (hasFrontendBundle)
{
    app.UseDefaultFiles();
    app.UseStaticFiles();
}

app.UseCors("Frontend");

app.UseAuthorization();

app.MapControllers();

if (hasFrontendBundle)
{
    app.MapFallbackToFile("index.html");
}

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ReleaseDbContext>();
    await ReleaseDbSeeder.SeedAsync(dbContext, CancellationToken.None);
}

app.Run();

public partial class Program;
