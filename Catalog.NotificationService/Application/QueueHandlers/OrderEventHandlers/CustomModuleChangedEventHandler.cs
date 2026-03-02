using Calabonga.UnitOfWork;
using Catalog.Contracts.Events.OrderEvents;
using Catalog.NotificationService.Application.Configurations;
using Microsoft.Extensions.Options;
using Rebus.Handlers;
using TelegramService.Configurations;
using TelegramService.Interfaces;

namespace Catalog.NotificationService.Application.QueueHandlers.OrderEventHandlers
{
    public class CustomModuleChangedEventHandler : IHandleMessages<CustomModuleChangedEvent>
    {
        private readonly ITelegramService _telegramService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TelegramBotConfiguration _approvalNotificationBot;
        private readonly TelegramBotConfiguration _exceptionNotificationBot;

        public CustomModuleChangedEventHandler(ITelegramService telegramService, IUnitOfWork unitOfWork, 
            IOptions<ApplicationConfiguration> applicationConfiguration, IOptions<TelegramBotConfiguration> exceptionNotificationBot)
        {
            _approvalNotificationBot = applicationConfiguration.Value.ApprovalNotificationBot;
            _exceptionNotificationBot = exceptionNotificationBot.Value;
            _telegramService = telegramService;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(CustomModuleChangedEvent message)
        {

            if (message.Order is null)
            {
                _telegramService.Initialize(token: _exceptionNotificationBot.Token, chatId: _exceptionNotificationBot.ChatId);

                await _telegramService.SendMessageAsync($"{"NotificationService".ToUpper()} Event {message.GetType().Name}. Order not found!");

                return;
            }

            _telegramService.Initialize(token: _approvalNotificationBot.Token, chatId: _approvalNotificationBot.ChatId);

            await _telegramService.SendMessageAsync($"СОГЛАСОВАНИЕ ЗАКАЗА: у заказа \"{message.Order.Code}\" пользователя: \"{message.Order.User}\" произошли изменения нестандартного модуля!");

            return;
        }
    }
}