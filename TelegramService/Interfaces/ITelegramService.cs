using Calabonga.OperationResults;

namespace TelegramService.Interfaces
{
    public interface ITelegramService
    {
        /// <summary>
        /// Асинхронный метод для отправки сообщения в телеграмм
        /// </summary>
        public Task<Operation<string, string>> SendMessageAsync(string message);
        public void Initialize(string token, string chatId);
    }
}
