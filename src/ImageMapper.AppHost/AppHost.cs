var builder = DistributedApplication.CreateBuilder(args);

//var cache = builder.AddRedis("cache");

var api = builder.AddProject<Projects.ImageMapper_Api>("imagemapper-api")
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.ImageMapper_Web>("imagemapper-web")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    //.WithReference(cache)
    //.WaitFor(cache)
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();
