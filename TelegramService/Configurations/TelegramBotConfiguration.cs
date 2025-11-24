namespace TelegramService.Configurations
{
    public sealed class TelegramBotConfiguration
    {
        public required string Token { get; set; }
        public string ChatId { get; set; }
    }
}