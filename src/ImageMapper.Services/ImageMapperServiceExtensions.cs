using Microsoft.Extensions.DependencyInjection;

namespace ImageMapper.Services
{
    public static class ImageMapperServiceExtensions
    {
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
