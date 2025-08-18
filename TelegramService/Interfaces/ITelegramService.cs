using Calabonga.OperationResults;

namespace TelegramService.Interfaces
{
    public interface ITelegramService
    {
        /// <summary>
        /// Асинхронный метод для отправки сообщения в телеграм
        /// </summary>
        public Task<Operation<string, string>> SendMessageAsync(string message);
    }
}
