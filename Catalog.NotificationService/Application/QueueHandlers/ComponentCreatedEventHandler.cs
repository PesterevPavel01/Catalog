using System.Text.Json;
using Catalog.Contracts.Events;
using Rebus.Handlers;
using TelegramService.Interfaces;

namespace Catalog.NotificationService.Application.QueueHandlers
{
    public sealed class ComponentCreatedEventHandler : IHandleMessages<ComponentCreatedEvent>
    {
        private readonly ILogger<ComponentCreatedEventHandler> _logger;
        private readonly ITelegramService _telegramService;

        public ComponentCreatedEventHandler(ILogger<ComponentCreatedEventHandler> logger, ITelegramService telegramService)
        {
            _logger = logger;
            _telegramService = telegramService;
        }

        public Task Handle(ComponentCreatedEvent message)
        {
            _logger.LogInformation("[{ServiceName}] Event {EventType} received successfully. ComponentCodes {ComponentCodes}",
                "NotificationService".ToUpper(),
                message.GetType().Name,
                message.ComponentCodes);

            _telegramService.SendMessageAsync($"{"NotificationService".ToUpper()} Event {message.GetType().Name} received successfully. ComponentCodes {JsonSerializer.Serialize(message.ComponentCodes)}").GetAwaiter().GetResult();

            return Task.CompletedTask;
        }
    }
}
