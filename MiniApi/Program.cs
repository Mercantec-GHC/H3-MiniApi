using Microsoft.EntityFrameworkCore;
using MiniApi.Data;
using MiniApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// Toggle data provider via configuration: set "UseInMemoryDatabase": true in appsettings
// to test the frontend/backend without a real PostgreSQL instance.
var useInMemory = builder.Configuration.GetValue<bool>("UseInMemoryDatabase");

if (useInMemory)
{
    builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("MiniApiDev"));
}
else
{
    var connectionString = builder.Configuration.GetConnectionString("Default")
        ?? throw new InvalidOperationException("Connection string 'Default' not found.");

    builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
}

builder.Services.AddOpenApi();

var app = builder.Build();

// Seed some sample data when using the in-memory database so the frontend has something to show.
if (useInMemory)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (!db.Countries.Any())
    {
        db.Countries.Add(new Country
        {
            ApiName = "restcountries",
            Choice = "Denmark",
            Cca3 = "DNK",
            Capital = "Copenhagen",
            Region = "Europe",
            Subregion = "Northern Europe",
            Population = 5831000,
            Area = 43094,
            Latitude = 56.0,
            Longitude = 10.0,
            FlagUrl = "https://flagcdn.com/dk.svg",
            CreatedAt = DateTime.UtcNow
        });
        db.SaveChanges();
    }
}

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();
