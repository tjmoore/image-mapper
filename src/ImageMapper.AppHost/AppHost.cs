using Aspire.Hosting.Docker;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var isPublishMode = builder.ExecutionContext.IsPublishMode;
var composeDefaults = isPublishMode
    ? ResolveComposeDefaults(builder.Configuration)
    : new ComposeDefaults([], null, null);

builder.AddDockerComposeEnvironment("compose")
    .ConfigureEnvFile(envFile =>
    {
        AddComposeEnvFileEntries(envFile, composeDefaults);
    });

var api = builder.AddProject<Projects.ImageMapper_Api>("imagemapper-api");

if (isPublishMode)
{
    var imageFoldersParameters = composeDefaults.ImageFolders
        .Select((folder, index) => new
        {
            ConfigurationKey = $"ImageFolders__{index}",
            VariableName = $"IMAGEMAPPER_API_IMAGE_FOLDERS_{index}"
        })
        .ToArray();

    foreach (var imageFoldersParameter in imageFoldersParameters)
    {
        api.WithEnvironment(
            imageFoldersParameter.ConfigurationKey,
            ComposeVariableReference(imageFoldersParameter.VariableName));
    }
}
else
{
    var imageFolders = Environment.GetEnvironmentVariables()
        .Keys
        .OfType<string>()
        .Where(key => key.StartsWith("ImageFolders__", StringComparison.Ordinal))
        .OrderBy(key => key, StringComparer.Ordinal);

    foreach (var key in imageFolders)
    {
        var value = Environment.GetEnvironmentVariable(key);
        if (!string.IsNullOrWhiteSpace(value))
        {
            api.WithEnvironment(key, value);
        }
    }
}

api.WithHttpHealthCheck("/health")
    .PublishAsDockerComposeService((_, service) =>
    {
        service.Name = "imagemapper-api";
    });

builder.AddProject<Projects.ImageMapper_Web>("imagemapper-web")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(api)
    .WaitFor(api)
    .PublishAsDockerComposeService((_, service) =>
    {
        service.Name = "imagemapper-web";
    });

builder.Build().Run();

static ComposeDefaults ResolveComposeDefaults(IConfiguration configuration)
{
    var composeDefaultsSection = configuration.GetSection("ComposeDefaults");
    var defaults = composeDefaultsSection.Get<ComposeDefaultsOptions>() ?? new ComposeDefaultsOptions();

    string[] imageFolders = defaults.ImageFolders ?? [];
    int? apiPort = defaults.ApiPort;
    int? webPort = defaults.WebPort;

    return new ComposeDefaults(imageFolders, apiPort, webPort);
}

static string ComposeVariableReference(string variableName) => $"${{{variableName}}}";

static void AddComposeEnvFileEntries(
    IDictionary<string, CapturedEnvironmentVariable> envFile,
    ComposeDefaults composeDefaults)
{
    for (var i = 0; i < composeDefaults.ImageFolders.Length; i++)
    {
        var variableName = $"IMAGEMAPPER_API_IMAGE_FOLDERS_{i}";
        envFile[variableName] = new CapturedEnvironmentVariable
        {
            Name = variableName,
            DefaultValue = composeDefaults.ImageFolders[i],
            Description = $"Default value for API ImageFolders[{i}]"
        };
    }

    envFile["IMAGEMAPPER_API_PORT"] = new CapturedEnvironmentVariable
    {
        Name = "IMAGEMAPPER_API_PORT",
        DefaultValue = composeDefaults.ApiPort?.ToString(),
        Description = "Default value for API port"
    };

    envFile["IMAGEMAPPER_WEB_PORT"] = new CapturedEnvironmentVariable
    {
        Name = "IMAGEMAPPER_WEB_PORT",
        DefaultValue = composeDefaults.WebPort?.ToString(),
        Description = "Default value for Web port"
    };

    var expectedFolderVariables = composeDefaults.ImageFolders
        .Select((_, index) => $"IMAGEMAPPER_API_IMAGE_FOLDERS_{index}")
        .ToHashSet(StringComparer.Ordinal);

    foreach (var key in envFile.Keys.ToArray())
    {
        if (key.StartsWith("IMAGEMAPPER_API_IMAGE_FOLDERS_", StringComparison.Ordinal)
            && !expectedFolderVariables.Contains(key))
        {
            envFile.Remove(key);
        }
    }

    envFile.Remove("IMAGEMAPPER_API_IMAGE_FOLDER");
}

sealed record ComposeDefaults(string[] ImageFolders, int? ApiPort, int? WebPort);

sealed class ComposeDefaultsOptions
{
    public string[]? ImageFolders { get; init; }
    public int? ApiPort { get; init; }
    public int? WebPort { get; init; }
}
