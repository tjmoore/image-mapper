using ImageMapper.Models;
using ImageMapper.Services;

namespace ImageMapper.Tests;

public class CacheActivityStatusTest
{
    [Test]
    public void GetStatusReturnsInitialValues()
    {
        var sut = new CacheActivityStatus();

        var status = sut.GetStatus();

        Assert.That(status, Is.EqualTo(new CacheStatusInfo(false, 0, 0)));
        Assert.That(sut.IsCaching, Is.False);
    }

    [Test]
    public void MarkCachingStartedSetsCachingAndClampsNegativeTotal()
    {
        var sut = new CacheActivityStatus();

        sut.MarkCachingStarted(-5);

        var status = sut.GetStatus();
        Assert.That(sut.IsCaching, Is.True);
        Assert.That(status, Is.EqualTo(new CacheStatusInfo(true, 0, 0)));
    }

    [Test]
    public void MarkCachingStartedWhileAlreadyCachingDoesNotResetProgress()
    {
        var sut = new CacheActivityStatus();
        sut.MarkCachingStarted(10);
        sut.UpdateProgress(4, 10);

        sut.MarkCachingStarted(20);

        var status = sut.GetStatus();
        Assert.That(status, Is.EqualTo(new CacheStatusInfo(true, 4, 10)));
    }

    [Test]
    public void UpdateProgressIsIgnoredWhenNotCaching()
    {
        var sut = new CacheActivityStatus();

        sut.UpdateProgress(3, 7);

        Assert.That(sut.GetStatus(), Is.EqualTo(new CacheStatusInfo(false, 0, 0)));
    }

    [Test]
    public void UpdateProgressWhileCachingUpdatesAndClampsValues()
    {
        var sut = new CacheActivityStatus();
        sut.MarkCachingStarted(10);

        sut.UpdateProgress(-2, -9);

        Assert.That(sut.GetStatus(), Is.EqualTo(new CacheStatusInfo(true, 0, 0)));
    }

    [Test]
    public void MarkCachingStoppedDoesNotAllowNegativeActiveOperationCount()
    {
        var sut = new CacheActivityStatus();

        sut.MarkCachingStopped();
        sut.MarkCachingStopped();

        Assert.That(sut.IsCaching, Is.False);
        Assert.That(sut.GetStatus(), Is.EqualTo(new CacheStatusInfo(false, 0, 0)));
    }

    [Test]
    public async Task StreamStatusesEmitsInitialAndSubsequentPublishedStatuses()
    {
        var sut = new CacheActivityStatus();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        var stream = sut.StreamStatuses(cts.Token);

        var statuses = new List<CacheStatusInfo>();
        var readerTask = Task.Run(async () =>
        {
            await foreach (var status in stream.WithCancellation(cts.Token))
            {
                statuses.Add(status);
                if (statuses.Count >= 4)
                {
                    break;
                }
            }
        }, cts.Token);

        await Task.Delay(50, cts.Token);
        sut.MarkCachingStarted(3);
        sut.UpdateProgress(1, 3);
        sut.MarkCachingStopped();

        await readerTask;

        Assert.That(statuses[0], Is.EqualTo(new CacheStatusInfo(false, 0, 0)));
        Assert.That(statuses[1], Is.EqualTo(new CacheStatusInfo(true, 0, 3)));
        Assert.That(statuses[2], Is.EqualTo(new CacheStatusInfo(true, 1, 3)));
        Assert.That(statuses[3], Is.EqualTo(new CacheStatusInfo(false, 1, 3)));
    }
}
