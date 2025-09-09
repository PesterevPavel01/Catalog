using Microsoft.Extensions.DependencyInjection;
using TelegramService.Interfaces;

namespace TelegramService.DependencyInjection
{
    public static class DependencyInjection
    {
        public static void AddTelegramService(this IServiceCollection services)
        {

            InitServices(services);
        }

        private static void InitServices(this IServiceCollection services)
        {

            services.AddScoped<ITelegramService, TelegramService>();

        }
    }
}
