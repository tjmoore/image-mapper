using Aspire.Hosting.Docker;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var isPublishMode = builder.ExecutionContext.IsPublishMode;
var composeDefaults = isPublishMode
    ? ResolveComposeDefaults(builder.Configuration)
    : new ComposeDefaults([]);

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
            VariableName = $"IMAGEMAPPER_API_IMAGE_FOLDERS_{index}",
            DefaultValue = folder,
            Parameter = builder.AddParameter(
                $"imagemapper-api-image-folders-{index}",
                folder,
                publishValueAsDefault: true,
                secret: false)
        })
        .ToArray();

    foreach (var imageFoldersParameter in imageFoldersParameters)
    {
        api.WithEnvironment(imageFoldersParameter.ConfigurationKey, imageFoldersParameter.Parameter);
        api.WithEnvironment(
            imageFoldersParameter.ConfigurationKey,
            ComposeVariableReference(imageFoldersParameter.VariableName, imageFoldersParameter.DefaultValue));
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
    //.WithReference(cache)
    //.WaitFor(cache)
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

    var imageFolders = defaults.ImageFolders ?? [];

    return new ComposeDefaults(imageFolders);
}

static string ComposeVariableReference(string variableName, string defaultValue)
{
    return string.IsNullOrWhiteSpace(defaultValue)
        ? $"${{{variableName}}}"
        : $"${{{variableName}:-{defaultValue}}}";
}

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
            Description = $"Optional override for API ImageFolders[{i}]"
        };
    }

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

sealed record ComposeDefaults(string[] ImageFolders);

sealed class ComposeDefaultsOptions
{
    public string[]? ImageFolders { get; init; }
}
