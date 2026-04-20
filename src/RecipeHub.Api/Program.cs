using Microsoft.EntityFrameworkCore;
using RecipeHub.Api.Data;

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

if (app.Environment.IsDevelopment())
{
    app.UseCors(DevCorsPolicy);
}

app.MapDefaultEndpoints();

app.MapGet("/", () => "RecipeHub API");

app.Run();
