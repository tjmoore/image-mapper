using Serilog;

namespace ImageMapper.Api.Services
{
    // Very loosly based on sample https://learn.microsoft.com/en-us/dotnet/core/extensions/caching#photo-service-scenario
    // but caching is done in GetImagesAsync() as required and the worker service just calls that to update the cache at regular intervals instead of caching
    // in the worker and reading the cache in GetImagesAsync(). This simplifies synchronisation.

    public sealed class ImageWorkerService(
        IImageService _imageService) : BackgroundService
    {
        private readonly TimeSpan _updateInterval = TimeSpan.FromHours(3);

        /// <summary>
        /// Start the worker service
        /// </summary>
        /// <param name="ct">A cancellation token that can be used to cancel the operation. The default value is CancellationToken.None.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public override async Task StartAsync(CancellationToken ct)
        {
            await base.StartAsync(ct);
        }

        /// <summary>
        /// The main execution loop of the worker service. It updates the image cache at regular intervals and handles cancellation requests.
        /// </summary>
        /// <param name="ct">A cancellation token that can be used to cancel the operation. The default value is CancellationToken.None</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                Log.Information("Updating cache");

                try
                {
                    int totalImageCount = await _imageService.GetImagesAsync(reinitialise: true, ct: ct).CountAsync(ct);

                    Log.Information("Cache updated with {Count} images", totalImageCount);
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
