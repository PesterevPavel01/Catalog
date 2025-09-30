using Catalog.Contracts.Events.CustomerEvents;
using Catalog.NotificationService.Application.Configurations;
using Microsoft.Extensions.Options;
using Rebus.Handlers;
using TelegramService.Interfaces;

namespace Catalog.NotificationService.Application.QueueHandlers.CustomerHandlers
{
    public class CustomerCreatedEventHandler : IHandleMessages<CustomerCreatedEvent>
    {
        private readonly ILogger<CustomerCreatedEventHandler> _logger;
        private readonly ITelegramService _telegramService;

        public CustomerCreatedEventHandler(ILogger<CustomerCreatedEventHandler> logger, ITelegramService telegramService, IOptions<ApplicationConfiguration> applicationConfiguration)
        {
            _logger = logger;
            _telegramService = telegramService;
            var approvalBotConfiguration = applicationConfiguration.Value.ApprovalNotificationBot;
            _telegramService.Initialize(token: approvalBotConfiguration.Token, chatId: approvalBotConfiguration.ChatId);
        }

        public async Task Handle(CustomerCreatedEvent message)
        {
            _logger.LogInformation("[{ServiceName}] Event {EventType} received successfully. User code {UserCode}",
                "NotificationService".ToUpper(),
                message.GetType().Name,
                message.UserDto.UserName);

            await _telegramService.SendMessageAsync($"{"NotificationService".ToUpper()} Поступил запрос от пользователя {message.UserDto.UserName} на получение роли:{message.UserDto.Role}!");
        }
    }
}
