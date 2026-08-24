using Microsoft.Extensions.DependencyInjection;

namespace ImageMapper.Api.Services
{
    public static class ImageMapperServiceExtensions
    {
        public static IServiceCollection AddImageMapperServices(this IServiceCollection services)
        {
            services
                .AddMemoryCache()
                .AddSingleton<CacheActivityStatus>()
                .AddSingleton(typeof(CacheSignal<>))
                .AddTransient<ImageInfoFetcher>()
                .AddScoped<IImageService, ImageService>()
                .AddHostedService<ImageWorkerService>();

            return services;
        }
    }
}
