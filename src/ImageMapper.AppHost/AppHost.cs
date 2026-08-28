using Aspire.Hosting.Docker;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var isPublishMode = builder.ExecutionContext.IsPublishMode;
var composeDefaults = isPublishMode
    ? ResolveComposeDefaults(builder.Configuration)
    : new ComposeDefaults([], null);

builder.AddDockerComposeEnvironment("compose")
    .ConfigureEnvFile(envFile =>
    {
        AddComposeEnvFileEntries(envFile, composeDefaults);
    });

var webApp = builder.AddProject<Projects.ImageMapper_Web>("imagemapper-web");

if (isPublishMode)
{
    var imageFoldersParameters = composeDefaults.ImageFolders
        .Select((folder, index) => new
        {
            ConfigurationKey = $"ImageFolders__{index}",
            VariableName = $"IMAGEMAPPER_IMAGE_FOLDERS_{index}"
        })
        .ToArray();

    foreach (var imageFoldersParameter in imageFoldersParameters)
    {
        webApp.WithEnvironment(
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
            webApp.WithEnvironment(key, value);
        }
    }
}

webApp
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
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
    int? webPort = defaults.WebPort;

    return new ComposeDefaults(imageFolders, webPort);
}

static string ComposeVariableReference(string variableName) => $"${{{variableName}}}";

static void AddComposeEnvFileEntries(
    IDictionary<string, CapturedEnvironmentVariable> envFile,
    ComposeDefaults composeDefaults)
{
    for (var i = 0; i < composeDefaults.ImageFolders.Length; i++)
    {
        var variableName = $"IMAGEMAPPER_IMAGE_FOLDERS_{i}";
        envFile[variableName] = new CapturedEnvironmentVariable
        {
            Name = variableName,
            DefaultValue = composeDefaults.ImageFolders[i],
            Description = $"Default value for ImageFolders[{i}]"
        };
    }

    envFile["IMAGEMAPPER_WEB_PORT"] = new CapturedEnvironmentVariable
    {
        Name = "IMAGEMAPPER_WEB_PORT",
        DefaultValue = composeDefaults.WebPort?.ToString(),
        Description = "Default value for Web port"
    };

    var expectedFolderVariables = composeDefaults.ImageFolders
        .Select((_, index) => $"IMAGEMAPPER_IMAGE_FOLDERS_{index}")
        .ToHashSet(StringComparer.Ordinal);

    // Remove any existing keys no longer expected in the env file, including legacy keys
    foreach (var key in envFile.Keys.ToArray())
    {
        if (key.StartsWith("IMAGEMAPPER_IMAGE_FOLDERS_", StringComparison.Ordinal)
            && !expectedFolderVariables.Contains(key))
        {
            envFile.Remove(key);
        }

        if (key.StartsWith("IMAGEMAPPER_API_IMAGE_FOLDERS_", StringComparison.Ordinal))
        {
            envFile.Remove(key);
        }
    }

    envFile.Remove("IMAGEMAPPER_IMAGE_FOLDER");
    envFile.Remove("IMAGEMAPPER_API_IMAGE_FOLDER");
}

sealed record ComposeDefaults(string[] ImageFolders, int? WebPort);

sealed class ComposeDefaultsOptions
{
    public string[]? ImageFolders { get; init; }
    public int? WebPort { get; init; }
}
