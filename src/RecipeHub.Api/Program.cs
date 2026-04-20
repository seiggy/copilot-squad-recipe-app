using Microsoft.EntityFrameworkCore;
using RecipeHub.Api.Data;
using RecipeHub.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddDbContext<RecipeDbContext>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("RecipeDb") ?? "Data Source=recipes.db"));

const string DevCorsPolicy = "RecipeHubDevCors";

builder.Services.AddCors(options =>
{
    options.AddPolicy(DevCorsPolicy, policy =>
    {
        policy
            .SetIsOriginAllowed(_ => true)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RecipeDbContext>();
    db.Database.Migrate();
    SeedData.EnsureSeeded(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseCors(DevCorsPolicy);
}

app.MapDefaultEndpoints();

app.MapRecipeEndpoints();
app.MapTagEndpoints();

app.MapGet("/", () => "RecipeHub API");

app.Run();
