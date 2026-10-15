using ImageMapper.Services.Caching;
using ImageMapper.Services.Models;

namespace ImageMapper.Tests;

[TestFixture]
public class ImageCacheNotifierTest
{
    private static ImageInfo CreateImage(string id) =>
        new(id, id + ".jpg", id + ".jpg", 1, 1, 1, 1, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, "image/jpeg", DateTimeOffset.UtcNow);

    [Test]
    public async Task PublishedImagesAreDeliveredToSubscribers()
    {
        // Arrange
        var notifier = new ImageCacheNotifier();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var stream = notifier.Subscribe(cts.Token);

        // Act
        notifier.Publish(CreateImage("a"));
        notifier.Publish(CreateImage("b"));

        var received = new List<string>();
        await foreach (var image in stream)
        {
            received.Add(image.Id);
            if (received.Count == 2) break;
        }

        // Assert
        Assert.That(received, Is.EqualTo(new[] { "a", "b" }));
    }

    [Test]
    public async Task CancellationEndsSubscription()
    {
        // Arrange
        var notifier = new ImageCacheNotifier();
        using var cts = new CancellationTokenSource();
        var stream = notifier.Subscribe(cts.Token);
        cts.CancelAfter(50);

        // Act / Assert
        Assert.ThrowsAsync<OperationCanceledException>(async () =>
        {
            await foreach (var _ in stream) { }
        });
        await Task.CompletedTask;
    }
}
