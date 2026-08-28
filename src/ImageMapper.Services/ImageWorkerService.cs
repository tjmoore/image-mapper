using Microsoft.Extensions.Hosting;
using Serilog;

namespace ImageMapper.Services
{
    // Very loosly based on sample https://learn.microsoft.com/en-us/dotnet/core/extensions/caching#photo-service-scenario

    public sealed class ImageWorkerService(
        ImageInfoFetcher _imageInfoFetcher) : BackgroundService
    {
        private readonly TimeSpan _updateInterval = TimeSpan.FromHours(3);

        /// <summary>
        /// Start the worker service
        /// </summary>
        /// <param name="ct">A cancellation token that can be used to cancel the operation</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public override async Task StartAsync(CancellationToken ct)
        {
            await base.StartAsync(ct);
        }

        /// <summary>
        /// The main execution loop of the worker service. It updates the image cache at regular intervals and handles cancellation requests
        /// </summary>
        /// <param name="ct">A cancellation token that can be used to cancel the operation</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                Log.Information("Updating cache");

                try
                {
                    int processedCount = await _imageInfoFetcher.ProcessImagesAsync(ct);

                    Log.Information("Cache updated with {Count} images", processedCount);
                }
                catch (OperationCanceledException)
                {
                    Log.Warning("Cancellation acknowledged: shutting down");
                    break;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error occurred while updating cache");
                }

                // TODO: Consider update schedule based on detected changes in the image folders, rather than a fixed interval

                try
                {
                    Log.Information(
                        "Will attempt to update the cache in {Hours} hours from now",
                        _updateInterval.Hours);

                    await Task.Delay(_updateInterval, ct);
                }
                catch (OperationCanceledException)
                {
                    Log.Warning("Cancellation acknowledged: shutting down");
                    break;
                }
            }
        }
    }
}
