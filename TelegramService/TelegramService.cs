using Calabonga.OperationResults;
using Telegram.Bot;
using TelegramService.Interfaces;

namespace TelegramService
{
    public class TelegramService : ITelegramService
    {
        private string _token;
        private TelegramBotClient _telegramBotClient = null!;
        private string _chatId;

        public TelegramService()
        {
        }

        public void Initialize(string token, string chatId) 
        {
            _token = token;
            _chatId = chatId;
            _telegramBotClient = new(_token);
        }

        public async Task<Operation<string,string>> SendMessageAsync(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                try
                {
                    await _telegramBotClient.SendTextMessageAsync(_chatId, message);
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
                    _telegramBotClient.SendTextMessageAsync(_chatId, message).GetAwaiter().GetResult();
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
