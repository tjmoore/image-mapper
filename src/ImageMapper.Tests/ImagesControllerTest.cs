using ImageMapper.Services;
using ImageMapper.Web.Client;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using NSubstitute;

namespace ImageMapper.Tests
{
    public class ImagesControllerTest
    {
        private const string ValidImageId = "test-image-id-123";
        private static readonly byte[] JpegMagicBytes = [0xFF, 0xD8, 0xFF, 0xE0];

        private static WebApplicationFactory<ImageMapper.Web.Program> CreateFactory(IImageService imageService)
        {
            return new WebApplicationFactory<ImageMapper.Web.Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        // Replace the registered IImageService with a mock and ensure ImageFetcher is available
                        services.AddSingleton(imageService);
                        services.AddScoped<ImageFetcher>();
                    });
                });
        }

        [Test]
        public async Task GetRawImage_ValidId_ReturnsOkWithOctetStream()
        {
            // Arrange
            var imageService = Substitute.For<IImageService>();
            imageService.GetImageBytesAsync(ValidImageId, Arg.Any<CancellationToken>())
                .Returns(JpegMagicBytes);

            using var factory = CreateFactory(imageService);
            using var client = factory.CreateClient();

            // Act
            using var response = await client.GetAsync($"/api/images/raw/{ValidImageId}");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/octet-stream"));

            var content = await response.Content.ReadAsByteArrayAsync();
            Assert.That(content, Has.Length.GreaterThan(0));
            Assert.That(content[0], Is.EqualTo(0xFF)); // JPEG magic byte
        }

        [Test]
        public async Task GetRawImage_NonExistentId_ReturnsNotFound()
        {
            // Arrange
            var imageService = Substitute.For<IImageService>();
            imageService.GetImageBytesAsync("nonexistent-id", Arg.Any<CancellationToken>())
                .Returns((byte[]?)null);

            using var factory = CreateFactory(imageService);
            using var client = factory.CreateClient();

            // Act
            using var response = await client.GetAsync("/api/images/raw/nonexistent-id");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task GetRawImage_EmptyPath_ReturnsNotFound()
        {
            // Arrange
            var imageService = Substitute.For<IImageService>();

            using var factory = CreateFactory(imageService);
            using var client = factory.CreateClient();

            // Act
            using var response = await client.GetAsync("/api/images/raw/");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }
    }
}
