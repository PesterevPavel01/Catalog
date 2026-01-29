using Catalog.Contracts.Events.CustomerEvents;
using Catalog.NotificationService.Application.Configurations;
using Microsoft.Extensions.Options;
using Rebus.Handlers;
using TelegramService.Configurations;
using TelegramService.Interfaces;

namespace Catalog.NotificationService.Application.QueueHandlers.CustomerHandlers
{
    public class CustomerCreatedEventHandler : IHandleMessages<CustomerCreatedEvent>
    {
        private readonly ITelegramService _telegramService;
        private readonly TelegramBotConfiguration _approvalNotificationBot;
        private readonly TelegramBotConfiguration _exceptionNotificationBot;

        public CustomerCreatedEventHandler(ILogger<CustomerCreatedEventHandler> logger, ITelegramService telegramService,
            IOptions<ApplicationConfiguration> applicationConfiguration, IOptions<TelegramBotConfiguration> exceptionNotificationBot)
        {
            _telegramService = telegramService;
            _approvalNotificationBot = applicationConfiguration.Value.ApprovalNotificationBot;
            _exceptionNotificationBot = exceptionNotificationBot.Value;
        }

        public async Task Handle(CustomerCreatedEvent message)
        {

            _telegramService.Initialize(token: _approvalNotificationBot.Token, chatId: _approvalNotificationBot.ChatId);

            await _telegramService.SendMessageAsync($"{"NotificationService".ToUpper()} Поступил запрос от пользователя {message.UserDto.UserName} на получение роли:{message.UserDto.Role}!");
        }
    }
}
