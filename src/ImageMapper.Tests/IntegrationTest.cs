using Aspire.Hosting;
using ImageMapper.Models;
using ImageMapper.Services;
using ImageMapper.Web.Client;
using Microsoft.Extensions.Logging;
using Serilog;

namespace ImageMapper.Tests
{
    /// <summary>
    /// Tests running ImageMapper via Aspire and client / API calls
    /// </summary>
    public class IntegrationTest
    {
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
        private string _testImagesDirectory = null!;
        private string _testSubdirectory = null!;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _testImagesDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, $"image-mapper-test-{Guid.NewGuid()}");
            Directory.CreateDirectory(_testImagesDirectory);
            
            // Create a test image file
            var testImagePath = Path.Combine(_testImagesDirectory, "test-image.jpg");
            File.WriteAllBytes(testImagePath, [0xFF, 0xD8, 0xFF, 0xE0]); // JPEG magic bytes

            // Create a subdirectory with test images
            _testSubdirectory = Path.Combine(_testImagesDirectory, "subfolder");
            Directory.CreateDirectory(_testSubdirectory);
            
            var subImagePath = Path.Combine(_testSubdirectory, "nested-image.png");
            File.WriteAllBytes(subImagePath, [0x89, 0x50, 0x4E, 0x47]); // PNG magic bytes
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            if (Directory.Exists(_testImagesDirectory))
                Directory.Delete(_testImagesDirectory, recursive: true);
        }

        private static async Task<DistributedApplication> BuildAndStartAppAsync(CancellationToken cancellationToken, string? imageFolderPath = null)
        {
            // IGNORE values ensure existing appsettings values are overridden and ignored. This assumes no more than 5 image folders are configured in the appsettings.json file
            // in ImageMappier.Api in a development environment. A clean test environment wouldn't have any defined in appsettings.json,
            // so this is just a precaution for development environments.

            if (imageFolderPath != null)
            {
                Environment.SetEnvironmentVariable("ImageFolders__0", imageFolderPath);
                Environment.SetEnvironmentVariable("ImageFolders__1", "IGNORE");
                Environment.SetEnvironmentVariable("ImageFolders__2", "IGNORE");
                Environment.SetEnvironmentVariable("ImageFolders__3", "IGNORE");
                Environment.SetEnvironmentVariable("ImageFolders__4", "IGNORE");
            }
            else
            {
                Environment.SetEnvironmentVariable("ImageFolders__0", "IGNORE");
                Environment.SetEnvironmentVariable("ImageFolders__1", "IGNORE");
                Environment.SetEnvironmentVariable("ImageFolders__2", "IGNORE");
                Environment.SetEnvironmentVariable("ImageFolders__3", "IGNORE");
                Environment.SetEnvironmentVariable("ImageFolders__4", "IGNORE");
            }

            var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.ImageMapper_AppHost>();

            appHost.Services.AddLogging(logging =>
            {
                logging.SetMinimumLevel(LogLevel.Debug);
                logging.AddFilter(appHost.Environment.ApplicationName, LogLevel.Debug);
                logging.AddFilter("Aspire.", LogLevel.Debug);
                logging.AddSerilog(
                    new LoggerConfiguration()
                        .MinimumLevel.Debug()
                        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                        .CreateLogger());
            });

            appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
            {
                clientBuilder.AddStandardResilienceHandler();
            });

            var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
            await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

            return app;
        }

        [Test]
        public async Task GetWebResourceRootReturnsOkStatusCode()
        {
            // Arrange
            using var cts = new CancellationTokenSource(DefaultTimeout);
            var cancellationToken = cts.Token;
            
            await using var app = await BuildAndStartAppAsync(cancellationToken);

            // Act
            using var httpClient = app.CreateHttpClient("imagemapper-web");
            await app.ResourceNotifications.WaitForResourceHealthyAsync("imagemapper-web", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
            using var response = await httpClient.GetAsync("/", cancellationToken);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task GetImagesListReturnsAllImages()
        {
            // Arrange
            using var cts = new CancellationTokenSource(DefaultTimeout);
            var cancellationToken = cts.Token;
            
            await using var app = await BuildAndStartAppAsync(cancellationToken, _testImagesDirectory);            
            await app.ResourceNotifications.WaitForResourceHealthyAsync("imagemapper-web", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

            var imageService = app.Services.GetRequiredService<ImageService>();

            var fetcher = new ImageFetcher(imageService);

            // Act
            var images = new List<ImageInfo>();
            await foreach (var image in fetcher.FetchImageList(cancellationToken))
            {
                if (image != null)
                    images.Add(image);
            }

            // Assert
            Assert.That(images, Has.Count.EqualTo(2));
            Assert.That(images.Select(i => i.FileName), Does.Contain("test-image.jpg"));
            Assert.That(images.Select(i => i.FileName), Does.Contain("nested-image.png"));
        }

        [Test]
        public async Task GetCacheStatusStreamReturnsInitialStatus()
        {
            // Arrange
            using var cts = new CancellationTokenSource(DefaultTimeout);
            var cancellationToken = cts.Token;

            await using var app = await BuildAndStartAppAsync(cancellationToken, _testImagesDirectory);
            await app.ResourceNotifications.WaitForResourceHealthyAsync("imagemapper-web", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

            var cacheActivityStatus = app.Services.GetRequiredService<CacheActivityStatus>();
            var cacheStatus = new CacheStatus(cacheActivityStatus);

            // Act
            await foreach (var statusInfo in cacheStatus.StreamCacheStatus(cancellationToken))
            {
                // Assert
                Assert.That(statusInfo, Is.Not.Null);
                Assert.That(statusInfo.ProcessedFileCount, Is.GreaterThanOrEqualTo(0));
                Assert.That(statusInfo.TotalFileCount, Is.GreaterThanOrEqualTo(0));
                break; // Only check the first status update
            }
        }

        [Test]
        public async Task GetImagesListReturnsEmptyWhenNoImagesExist()
        {
            // Arrange
            using var cts = new CancellationTokenSource(DefaultTimeout);
            var cancellationToken = cts.Token;
            
            var emptyDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, $"empty-test-{Guid.NewGuid()}");
            Directory.CreateDirectory(emptyDirectory);

            try
            {
                await using var app = await BuildAndStartAppAsync(cancellationToken, emptyDirectory);                
                await app.ResourceNotifications.WaitForResourceHealthyAsync("imagemapper-web", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

                var imageService = app.Services.GetRequiredService<ImageService>();

                var fetcher = new ImageFetcher(imageService);

                // Act
                var images = new List<ImageInfo>();
                await foreach (var image in fetcher.FetchImageList(cancellationToken))
                {
                    if (image != null)
                        images.Add(image);
                }

                // Assert
                Assert.That(images, Is.Empty);
            }
            finally
            {
                if (Directory.Exists(emptyDirectory))
                    Directory.Delete(emptyDirectory, recursive: true);
            }
        }

        [Test]
        public async Task GetRawImageReturnsExistingFile()
        {
            // Arrange
            using var cts = new CancellationTokenSource(DefaultTimeout);
            var cancellationToken = cts.Token;

            await using var app = await BuildAndStartAppAsync(cancellationToken, _testImagesDirectory);

            // Act            
            await app.ResourceNotifications.WaitForResourceHealthyAsync("imagemapper-web", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

            var imageService = app.Services.GetRequiredService<ImageService>();

            var fetcher = new ImageFetcher(imageService);
            var images = new List<ImageInfo>();
            await foreach (var image in fetcher.FetchImageList(cancellationToken))
            {
                if (image != null)
                    images.Add(image);
            }

            var testImageId = images.FirstOrDefault(i => i.FileName == "test-image.jpg")?.Id;
            Assert.That(testImageId, Is.Not.Null, "test-image.jpg not found");

            using var httpClientWeb = app.CreateHttpClient("imagemapper-web");
            using var response = await httpClientWeb.GetAsync($"/api/images/raw/{testImageId}", cancellationToken);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/octet-stream"));

            var content = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            Assert.That(content, Has.Length.GreaterThan(0));
            Assert.That(content[0], Is.EqualTo(0xFF)); // JPEG magic byte
        }

        [Test]
        public async Task GetRawImageReturnsNotFoundForNonExistentFile()
        {
            // Arrange
            using var cts = new CancellationTokenSource(DefaultTimeout);
            var cancellationToken = cts.Token;

            await using var app = await BuildAndStartAppAsync(cancellationToken, _testImagesDirectory);

            // Act
            using var httpClient = app.CreateHttpClient("imagemapper-web");
            await app.ResourceNotifications.WaitForResourceHealthyAsync("imagemapper-web", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
            using var response = await httpClient.GetAsync("/api/images/raw/nonexistent-id", cancellationToken);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task GetRawImageReturnsNotFoundForEmptyPath()
        {
            // Arrange
            using var cts = new CancellationTokenSource(DefaultTimeout);
            var cancellationToken = cts.Token;

            await using var app = await BuildAndStartAppAsync(cancellationToken);

            // Act
            using var httpClient = app.CreateHttpClient("imagemapper-web");
            await app.ResourceNotifications.WaitForResourceHealthyAsync("imagemapper-web", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
            using var response = await httpClient.GetAsync("/api/images/raw/", cancellationToken);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }
    }
}
