using Microsoft.Extensions.DependencyInjection;

namespace ImageMapper.Services
{
    public static class ImageMapperServiceExtensions
    {
        /// <summary>
        /// Adds the ImageMapper services to the specified IServiceCollection.
        /// </summary>
        /// <param name="services">The IServiceCollection to add the services to.</param>
        /// <returns>The updated IServiceCollection.</returns>
        public static IServiceCollection AddImageMapperServices(this IServiceCollection services)
        {
            services
                .AddMemoryCache()
                .AddSingleton<ICacheActivityStatus, CacheActivityStatus>()
                .AddSingleton(typeof(CacheSignal<>))
                .AddTransient<IImageInfoFetcher, ImageInfoFetcher>()
                .AddScoped<IImageService, ImageService>()
                .AddHostedService<ImageWorkerService>();

            return services;
        }
    }
}
