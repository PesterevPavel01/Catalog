using TelegramService.Configurations;

namespace Catalog.NotificationService.Application.Configurations
{
    public sealed record ApplicationConfiguration
    {
        public required TelegramBotConfiguration ApprovalNotificationBot { get; set; }
    }
}
