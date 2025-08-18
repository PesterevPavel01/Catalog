using Calabonga.OperationResults;
using Telegram.Bot;
using TelegramService.Interfaces;

namespace TelegramService
{
    public class TelegramService : ITelegramService
    {
        private static string _token { get; set; } = "7562655253:AAEKJQnd1YXSXpEXtQJ0NvJSo3C1B-GEN8E";
        private static TelegramBotClient TelegramBotClient = null!;
        private static readonly string channelId = "-1002463757906";

        public TelegramService()
        {
            TelegramBotClient = new(_token);
        }

        public async Task<Operation<string,string>> SendMessageAsync(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                try
                {
                    await TelegramBotClient.SendTextMessageAsync(channelId, message);
                    return "Уведомление отправлено";
                }
                catch (Exception ex)
                {
                    return Operation.Error(ex.Message);
                }
            }
            else
            {
                return Operation.Error("Incorrect input message");
            }
        }

        public Operation<string,string> SendMessage(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                try
                {
                    TelegramBotClient.SendTextMessageAsync(channelId, message).GetAwaiter().GetResult();
                    return "Уведомление отправлено";
                }
                catch (Exception ex)
                {
                    return Operation.Error(ex.Message);
                }
            }
            else
            {
                return Operation.Error("Incorrect input message");
            }
        }
    }
}
