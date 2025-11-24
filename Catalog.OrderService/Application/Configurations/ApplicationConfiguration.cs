using TelegramService.Configurations;

namespace Catalog.OrderService.Application.Configurations
{
    public sealed record ApplicationConfiguration
    {
        public required TelegramBotConfiguration OrderTelegramBot { get; set; }
    }
}
