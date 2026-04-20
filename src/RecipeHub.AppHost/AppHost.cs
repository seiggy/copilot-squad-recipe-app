var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.RecipeHub_Api>("api")
    .WithExternalHttpEndpoints();

builder.AddViteApp("web", "../RecipeHub.Web")
    .WithReference(api)
    .WithEnvironment("VITE_API_BASE_URL", api.GetEndpoint("http"));

builder.Build().Run();
