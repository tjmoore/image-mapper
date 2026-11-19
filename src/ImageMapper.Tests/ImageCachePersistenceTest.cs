using ImageMapper.Services;
using ImageMapper.Services.Caching;
using ImageMapper.Services.Models;
using ImageMapper.Services.Utils;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace ImageMapper.Tests
{
    public class ImageCachePersistenceTest
    {
        private string _directory = null!;

        [SetUp]
        public void SetUp()
        {
            _directory = Path.Combine(TestContext.CurrentContext.TestDirectory, $"image-cache-test-{Guid.NewGuid()}");
            Directory.CreateDirectory(_directory);
            File.WriteAllBytes(Path.Combine(_directory, "a.jpg"), [0xFF, 0xD8, 0xFF, 0xE0]);
            File.WriteAllBytes(Path.Combine(_directory, "b.jpg"), [0xFF, 0xD8, 0xFF, 0xE0]);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_directory))
                Directory.Delete(_directory, recursive: true);
        }

        [Test]
        public async Task UnchangedFilesAreNotReprocessed()
        {
            // Arrange
            var (fetcher, notifier) = CreateFetcher(TestCaches.Create());
            await fetcher.ProcessImagesAsync(CancellationToken.None);
            notifier.Published.Clear();

            // Act
            var count = await fetcher.ProcessImagesAsync(CancellationToken.None);

            // Assert
            Assert.That(count, Is.EqualTo(2));
            Assert.That(notifier.Published, Is.Empty);
        }

        [Test]
        public async Task ChangedFileIsReprocessed()
        {
            // Arrange
            var (fetcher, notifier) = CreateFetcher(TestCaches.Create());
            await fetcher.ProcessImagesAsync(CancellationToken.None);
            notifier.Published.Clear();

            File.WriteAllBytes(Path.Combine(_directory, "a.jpg"), [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x01]);

            // Act
            await fetcher.ProcessImagesAsync(CancellationToken.None);

            // Assert
            Assert.That(notifier.Published.Select(i => i.FileName), Is.EqualTo(new[] { "a.jpg" }));
        }

        [Test]
        public async Task DeletedFileIsRemovedFromCache()
        {
            // Arrange
            var (fetcher, _) = CreateFetcher(TestCaches.Create());
            await fetcher.ProcessImagesAsync(CancellationToken.None);
            var deletedId = (await fetcher.GetImageFilesAsync()).Single(f => f.FileName == "b.jpg").Id;
            File.Delete(Path.Combine(_directory, "b.jpg"));

            // Act
            var count = await fetcher.ProcessImagesAsync(CancellationToken.None);

            // Assert
            Assert.That(count, Is.EqualTo(1));
            Assert.That(await fetcher.GetImageInfoAsync(deletedId), Is.Null);
            Assert.That(await fetcher.GetImageCountAsync(), Is.EqualTo(1));
        }

        [Test]
        public async Task RestartLoadsFromDistributedCacheWithoutReprocessing()
        {
            // Arrange - first "process" populates the shared distributed cache
            IDistributedCache distributed = TestCaches.CreateDistributedCache();
            var (first, _) = CreateFetcher(TestCaches.Create(distributed));
            await first.ProcessImagesAsync(CancellationToken.None);
            Assert.That(distributed.Get(ImageInfoFetcher.FilesKey), Is.Not.Null, "L2 manifest");

            // Act - a new instance with an empty in-memory cache simulates a restart
            var (restarted, notifier) = CreateFetcher(TestCaches.Create(distributed));
            var loaded = await restarted.LoadFromCacheAsync(CancellationToken.None);
            var service = new ImageService(restarted, notifier);
            var images = new List<ImageInfo>();
            await foreach (var image in service.GetImagesAsync())
                images.Add(image);

            await restarted.ProcessImagesAsync(CancellationToken.None);

            // Assert
            Assert.That(loaded, Is.EqualTo(2));
            Assert.That(images, Has.Count.EqualTo(2));
            Assert.That(await restarted.GetImageCountAsync(), Is.EqualTo(2));
            Assert.That(notifier.Published, Is.Empty);
        }

        [Test]
        public void ImageInfoRoundTripsThroughJson()
        {
            // Arrange
            var original = new ImageInfo("id", "a.jpg", "/p/a.jpg", 10, 20, 51.5, -0.1,
                DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, "image/jpeg", DateTimeOffset.UtcNow,
                iso: 200, flashFired: true, fileSize: 1234);

            // Act
            var restored = JsonSerializer.Deserialize<ImageInfo>(JsonSerializer.Serialize(original));

            // Assert
            Assert.That(restored, Is.EqualTo(original));
        }

        private (ImageInfoFetcher Fetcher, RecordingNotifier Notifier) CreateFetcher(Microsoft.Extensions.Caching.Hybrid.HybridCache cache)
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection([new("ImageFolders:0", _directory)])
                .Build();
            var notifier = new RecordingNotifier();
            var fetcher = new ImageInfoFetcher(config, cache, new CacheSignal<ImageInfo>(), new CacheActivityStatus(), notifier);
            return (fetcher, notifier);
        }

        private sealed class RecordingNotifier : IImageCacheNotifier
        {
            public List<ImageInfo> Published { get; } = [];

            public void Publish(ImageInfo image) => Published.Add(image);

            public IAsyncEnumerable<ImageInfo> Subscribe(CancellationToken ct = default) => throw new NotSupportedException();
        }
    }
}
