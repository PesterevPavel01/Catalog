using Catalog.Contracts.Events.ApprovalEvents;
using Catalog.Contracts.Events.OrderEvents;
using Catalog.NotificationService.Application.Configurations;
using Microsoft.Extensions.Options;
using Rebus.Handlers;
using TelegramService.Configurations;
using TelegramService.Interfaces;

namespace Catalog.NotificationService.Application.QueueHandlers.OrderEventHandlers;

public sealed class OrderRejectedEventHandler : IHandleMessages<OrderRejectedEvent>
{
    private readonly ITelegramService _telegramService;
    private readonly TelegramBotConfiguration _approvalNotificationBot;
    private readonly TelegramBotConfiguration _exceptionNotificationBot;

    public OrderRejectedEventHandler(ILogger<OrderRejectedEventHandler> logger, ITelegramService telegramService,
        IOptions<ApplicationConfiguration> applicationConfiguration, IOptions<TelegramBotConfiguration> exceptionNotificationBot)
    {
        _approvalNotificationBot = applicationConfiguration.Value.ApprovalNotificationBot;
        _exceptionNotificationBot = exceptionNotificationBot.Value;
        _telegramService = telegramService;
    }

    public async Task Handle(OrderRejectedEvent message)
    {
        if (message.Order is null)
        {
            _telegramService.Initialize(token: _exceptionNotificationBot.Token, chatId: _exceptionNotificationBot.ChatId);

            await _telegramService.SendMessageAsync($"{"NotificationService".ToUpper()} Event {message.GetType().Name}. Order is null!");

            return;
        }

        _telegramService.Initialize(token: _approvalNotificationBot.Token, chatId: _approvalNotificationBot.ChatId);

        await _telegramService.SendMessageAsync($"СОГЛАСОВАНИЕ ЗАКАЗА: у заказа \"{message.Order.Title}\" пользователя: \"{message.Order.User}\" не пройден процесс согласования модуля!");

        return;
    }
}
