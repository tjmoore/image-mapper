using ImageMapper.Api.Services;
using Microsoft.Extensions.Configuration;

namespace ImageMapper.Tests
{
    public class ImagesApiTest
    {
        private string _testImagesDirectory = null!;
        private string _testSubdirectory = null!;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _testImagesDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, $"image-mapper-test-{Guid.NewGuid()}");
            Directory.CreateDirectory(_testImagesDirectory);
            
            // Create a test image file in root
            var testImagePath = Path.Combine(_testImagesDirectory, "test-image.jpg");
            File.WriteAllBytes(testImagePath, [0xFF, 0xD8, 0xFF, 0xE0]); // JPEG magic bytes

            // Create a subdirectory with test images
            _testSubdirectory = Path.Combine(_testImagesDirectory, "subfolder");
            Directory.CreateDirectory(_testSubdirectory);
            
            var subImagePath = Path.Combine(_testSubdirectory, "nested-image.jpg");
            File.WriteAllBytes(subImagePath, [0xFF, 0xD8, 0xFF, 0xE0]); // JPEG magic bytes
            
            var deepSubdirectory = Path.Combine(_testSubdirectory, "deep");
            Directory.CreateDirectory(deepSubdirectory);
            
            var deepImagePath = Path.Combine(deepSubdirectory, "deep-image.png");
            File.WriteAllBytes(deepImagePath, [0x89, 0x50, 0x4E, 0x47]); // PNG magic bytes
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            if (Directory.Exists(_testImagesDirectory))
                Directory.Delete(_testImagesDirectory, recursive: true);
        }

        [Test]
        [TestCase("../../etc/passwd")]
        [TestCase("../../../windows/system32/config/sam")]
        [TestCase("..\\..\\..\\windows\\system32\\config\\sam")]
        [TestCase("images/../../etc/passwd")]
        [TestCase("subfolder/../../../../sensitive.txt")]
        [TestCase("valid-image.jpg/../../../etc/passwd")]
        public void GetImageBytesAsyncRejectsPathTraversalAttempts(string traversalPath)
        {
            // Arrange
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { { "ImageFolder", _testImagesDirectory } })
                .Build();
            var service = new ImageService(config);

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(
                async () => await service.GetImageBytesAsync(traversalPath));
            
            Assert.That(ex.ParamName, Is.EqualTo("relativePath"));
            Assert.That(ex.Message, Contains.Substring("Path traversal detected"));
        }

        [Test]
        public async Task GetImageBytesAsyncReturnsValidImageBytes()
        {
            // Arrange
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { { "ImageFolder", _testImagesDirectory } })
                .Build();
            var service = new ImageService(config);

            // Act
            var bytes = await service.GetImageBytesAsync("test-image.jpg");

            // Assert
            Assert.That(bytes, Is.Not.Null);
            Assert.That(bytes, Has.Length.GreaterThan(0));
            Assert.That(bytes[0], Is.EqualTo(0xFF)); // JPEG magic byte
        }

        [Test]
        public async Task GetImageBytesAsyncReturnsNullForNonExistentFile()
        {
            // Arrange
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { { "ImageFolder", _testImagesDirectory } })
                .Build();
            var service = new ImageService(config);

            // Act
            var bytes = await service.GetImageBytesAsync("nonexistent.jpg");

            // Assert
            Assert.That(bytes, Is.Null);
        }

        [Test]
        [TestCase("subfolder/nested-image.jpg")]
        [TestCase("subfolder/deep/deep-image.png")]
        public async Task GetImageBytesAsyncReturnsValidImageBytesFromSubfolders(string relativePath)
        {
            // Arrange
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { { "ImageFolder", _testImagesDirectory } })
                .Build();
            var service = new ImageService(config);

            // Act
            var bytes = await service.GetImageBytesAsync(relativePath);

            // Assert
            Assert.That(bytes, Is.Not.Null);
            Assert.That(bytes, Has.Length.GreaterThan(0));
        }

        [Test]
        [TestCase("subfolder/nonexistent.jpg")]
        [TestCase("subfolder/deep/missing-image.png")]
        [TestCase("nonexistent-folder/image.jpg")]
        public async Task GetImageBytesAsyncReturnsNullForNonExistentFilesInSubfolders(string relativePath)
        {
            // Arrange
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { { "ImageFolder", _testImagesDirectory } })
                .Build();
            var service = new ImageService(config);

            // Act
            var bytes = await service.GetImageBytesAsync(relativePath);

            // Assert
            Assert.That(bytes, Is.Null);
        }
    }
}