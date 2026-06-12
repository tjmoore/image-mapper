var builder = DistributedApplication.CreateBuilder(args);

var imageFolder = Environment.GetEnvironmentVariable("ImageFolder");

var api = builder.AddProject<Projects.ImageMapper_Api>("imagemapper-api")
    .WithEnvironment("ImageFolder", imageFolder)
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.ImageMapper_Web>("imagemapper-web")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    //.WithReference(cache)
    //.WaitFor(cache)
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();
