using Microsoft.Extensions.DependencyInjection;

namespace ImageMapper.RazorLib
{
    public static class ImageMapperRazorExtensions
    {
        /// <summary>
        /// Adds the ImageMapper Razor library services to the specified IServiceCollection.
        /// </summary>
        /// <param name="services">The IServiceCollection to add the services to.</param>
        /// <returns>The updated IServiceCollection.</returns>
        public static IServiceCollection AddImageMapperRazorLib(this IServiceCollection services)
        {
            services
                .AddScoped<Interops.MapSectionJsInterop>()
                .AddScoped<Interops.ImageModalJsInterop>()
                .AddScoped<Interops.ProgressSectionJsInterop>();

            return services;
        }
    }
}
