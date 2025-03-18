using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NLog.Extensions.Logging;

using TTStreamer.Services;

namespace TTStreamer.Common.Extensions
{
    public static class ServiceCollectionExt
    {
        public static IServiceCollection AddAppServices(this IServiceCollection services)
        {
            services.AddSingleton<SpeechService>();
            services.AddSingleton<SoundService>();
            services.AddSingleton<GiftService>();
            services.AddLogging(b =>
            {
                b.ClearProviders();
                b.AddNLog();
            });

            return services;
        }
    }
}
