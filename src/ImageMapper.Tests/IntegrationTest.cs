using Aspire.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace ImageMapper.Tests
{
    public class IntegrationTest
    {
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
        private string _testImagesDirectory = null!;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _testImagesDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, $"image-mapper-test-{Guid.NewGuid()}");
            Directory.CreateDirectory(_testImagesDirectory);
            
            // Create a test image file
            var testImagePath = Path.Combine(_testImagesDirectory, "test-image.jpg");
            File.WriteAllBytes(testImagePath, [0xFF, 0xD8, 0xFF, 0xE0]); // JPEG magic bytes
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            if (Directory.Exists(_testImagesDirectory))
                Directory.Delete(_testImagesDirectory, recursive: true);
        }

        private static async Task<DistributedApplication> BuildAndStartAppAsync(CancellationToken cancellationToken, string? imageFolderPath = null)
        {
            if (imageFolderPath != null)
                Environment.SetEnvironmentVariable("ImageFolder", imageFolderPath);

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
        public async Task GetRawImageReturnsExistingFile()
        {
            // Arrange
            using var cts = new CancellationTokenSource(DefaultTimeout);
            var cancellationToken = cts.Token;
            
            await using var app = await BuildAndStartAppAsync(cancellationToken, _testImagesDirectory);

            // Act
            using var httpClient = app.CreateHttpClient("imagemapper-web");
            await app.ResourceNotifications.WaitForResourceHealthyAsync("imagemapper-web", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
            using var response = await httpClient.GetAsync("/api/images/raw/test-image.jpg", cancellationToken);

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
            using var response = await httpClient.GetAsync("/api/images/raw/nonexistent-image.jpg", cancellationToken);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task GetRawImageReturnsBadRequestForEmptyPath()
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
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
    }
}
