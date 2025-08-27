using Catalog.Contracts.Events;
using Catalog.Domain.Entities;
using Rebus.Handlers;
using TelegramService.Interfaces;

namespace Catalog.NotificationService.Application.QueueHandlers
{
    public sealed class ApprovalCompletedEventHandler : IHandleMessages<ApprovalCompletedEvent>
    {
        private readonly ILogger<ComponentCreatedEventHandler> _logger;
        private readonly ITelegramService _telegramService;

        public ApprovalCompletedEventHandler(ILogger<ComponentCreatedEventHandler> logger, ITelegramService telegramService)
        {
            _logger = logger;
            _telegramService = telegramService;
        }

        public async Task Handle(ApprovalCompletedEvent message)
        {
            _logger
                .LogInformation("[{ServiceName}] Event {EventType} received successfully. Approved module ID: {ModuleId}, order ID: {OrderId}",
                    "NotificationService".ToUpper(),
                    message.GetType().Name,
                    message.ModuleId,
            message.OrderId);

            await _telegramService.SendMessageAsync($"{"NotificationService".ToUpper()} Event {message.GetType().Name} received successfully. Approved module ID: {message.ModuleId}, order ID: {message.OrderId}");

            return;
        }
    }
}
