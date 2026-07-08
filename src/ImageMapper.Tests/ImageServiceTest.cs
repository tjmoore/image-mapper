using ImageMapper.Api.Services;
using ImageMapper.Models;
using Microsoft.Extensions.Configuration;

namespace ImageMapper.Tests
{
    public class ImageServiceTest
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
        public async Task GetImageBytesAsyncReturnsValidImageBytes()
        {
            // Arrange
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection([new("ImageFolders:0", _testImagesDirectory)])
                .Build();

            var cache = new Microsoft.Extensions.Caching.Memory.MemoryCache(new Microsoft.Extensions.Caching.Memory.MemoryCacheOptions());
            var cacheSignal = new CacheSignal<ImageInfo>();
            var fetcher = new ImageInfoFetcher(config, cache, cacheSignal);
            var service = new ImageService(cache, fetcher);

            await fetcher.ProcessImagesAsync(CancellationToken.None);

            // Get the image ID first
            var images = new List<ImageInfo>();
            await foreach (var image in service.GetImagesAsync())
            {
                if (image.FileName == "test-image.jpg")
                    images.Add(image);
            }
            Assert.That(images, Has.Count.GreaterThan(0), "test-image.jpg not found");
            var testImageId = images[0].Id;

            Assert.That(testImageId, Is.Not.Empty.And.Not.WhiteSpace, "Image ID should not be empty or whitespace");

            // Act
            var bytes = await service.GetImageBytesAsync(testImageId);

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
                .AddInMemoryCollection([new("ImageFolders:0", _testImagesDirectory)])
                .Build();

            var cache = new Microsoft.Extensions.Caching.Memory.MemoryCache(new Microsoft.Extensions.Caching.Memory.MemoryCacheOptions());
            var cacheSignal = new CacheSignal<ImageInfo>();
            var fetcher = new ImageInfoFetcher(config, cache, cacheSignal);
            var service = new ImageService(cache, fetcher);

            await fetcher.ProcessImagesAsync(CancellationToken.None);

            // Act - use a fake ID that doesn't exist
            var bytes = await service.GetImageBytesAsync("nonexistent-id-12345");

            // Assert
            Assert.That(bytes, Is.Null);
        }

        [Test]
        [TestCase("nested-image.jpg")]
        [TestCase("deep-image.png")]
        public async Task GetImageBytesAsyncReturnsValidImageBytesFromSubfolders(string fileName)
        {
            // Arrange
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection([new("ImageFolders:0", _testImagesDirectory)])
                .Build();

            var cache = new Microsoft.Extensions.Caching.Memory.MemoryCache(new Microsoft.Extensions.Caching.Memory.MemoryCacheOptions());
            var cacheSignal = new CacheSignal<ImageInfo>();
            var fetcher = new ImageInfoFetcher(config, cache, cacheSignal);
            var service = new ImageService(cache, fetcher);

            await fetcher.ProcessImagesAsync(CancellationToken.None);

            // Get the image ID first
            var images = new List<ImageInfo>();
            await foreach (var image in service.GetImagesAsync())
            {
                if (image.FileName == fileName)
                    images.Add(image);
            }
            Assert.That(images, Has.Count.GreaterThan(0), $"{fileName} not found");
            var imageId = images[0].Id;

            Assert.That(imageId, Is.Not.Empty.And.Not.WhiteSpace, "Image ID should not be empty or whitespace");

            // Act
            var bytes = await service.GetImageBytesAsync(imageId);

            // Assert
            Assert.That(bytes, Is.Not.Null);
            Assert.That(bytes, Has.Length.GreaterThan(0));
        }

        [Test]
        public async Task GetImagesAsyncReturnsAllImagesFromDirectory()
        {
            // Arrange
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection([new("ImageFolders:0", _testImagesDirectory)])
                .Build();

            var cache = new Microsoft.Extensions.Caching.Memory.MemoryCache(new Microsoft.Extensions.Caching.Memory.MemoryCacheOptions());
            var cacheSignal = new CacheSignal<ImageInfo>();
            var fetcher = new ImageInfoFetcher(config, cache, cacheSignal);
            var service = new ImageService(cache, fetcher);

            await fetcher.ProcessImagesAsync(CancellationToken.None);

            // Act
            var images = new List<ImageInfo>();
            await foreach (var image in service.GetImagesAsync())
            {
                images.Add(image);
            }

            // Assert
            Assert.That(images, Has.Count.EqualTo(3));
            Assert.That(images.Select(i => i.FileName), Does.Contain("test-image.jpg"));
            Assert.That(images.Select(i => i.FileName), Does.Contain("nested-image.jpg"));
            Assert.That(images.Select(i => i.FileName), Does.Contain("deep-image.png"));
        }

        [Test]
        public async Task GetImagesAsyncReturnsEmptyListForNonExistentDirectory()
        {
            // Arrange
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection([new("ImageFolders:0", Path.Combine(_testImagesDirectory, "nonexistent"))])
                .Build();

            var cache = new Microsoft.Extensions.Caching.Memory.MemoryCache(new Microsoft.Extensions.Caching.Memory.MemoryCacheOptions());
            var cacheSignal = new CacheSignal<ImageInfo>();
            var fetcher = new ImageInfoFetcher(config, cache, cacheSignal);
            var service = new ImageService(cache, fetcher);

            await fetcher.ProcessImagesAsync(CancellationToken.None);

            // Act
            var images = new List<ImageInfo>();
            await foreach (var image in service.GetImagesAsync())
            {
                images.Add(image);
            }

            // Assert
            Assert.That(images, Is.Empty);
        }

        [Test]
        public async Task GetImagesAsyncIsCancellable()
        {
            // Arrange
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection([new("ImageFolders:0", _testImagesDirectory)])
                .Build();

            var cache = new Microsoft.Extensions.Caching.Memory.MemoryCache(new Microsoft.Extensions.Caching.Memory.MemoryCacheOptions());
            var cacheSignal = new CacheSignal<ImageInfo>();
            var fetcher = new ImageInfoFetcher(config, cache, cacheSignal);
            var service = new ImageService(cache, fetcher);

            await fetcher.ProcessImagesAsync(CancellationToken.None);

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            var ex = Assert.ThrowsAsync<OperationCanceledException>(
                async () =>
                {
                    await foreach (var image in service.GetImagesAsync(ct: cts.Token))
                    {
                        // This should throw due to cancellation
                    }
                });

            Assert.That(ex, Is.Not.Null);
        }

        [Test]
        public async Task GetImagesAsyncFiltersOnlyImageExtensions()
        {
            // Arrange
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection([new("ImageFolders:0", _testImagesDirectory)])
                .Build();
            
            // Create a non-image file
            var nonImagePath = Path.Combine(_testImagesDirectory, "readme.txt");
            File.WriteAllText(nonImagePath, "This is not an image");

            var cache = new Microsoft.Extensions.Caching.Memory.MemoryCache(new Microsoft.Extensions.Caching.Memory.MemoryCacheOptions());
            var cacheSignal = new CacheSignal<ImageInfo>();
            var fetcher = new ImageInfoFetcher(config, cache, cacheSignal);
            var service = new ImageService(cache, fetcher);

            await fetcher.ProcessImagesAsync(CancellationToken.None);

            // Act
            var images = new List<ImageInfo>();
            await foreach (var image in service.GetImagesAsync())
            {
                images.Add(image);
            }

            // Assert
            Assert.That(images, Has.Count.EqualTo(3)); // Still only 3 images, not 4
            Assert.That(images.Select(i => i.FileName), Does.Not.Contain("readme.txt"));

            // Cleanup
            File.Delete(nonImagePath);
        }
    }
}