using Catalog.ApprovalService.Application.Processors;
using Catalog.Contracts.Events;
using Catalog.Contracts.Events.ApprovalEvents;
using Catalog.Contracts.Events.OrderEvents;
using Microsoft.Extensions.Options;
using Rebus.Bus;
using Rebus.Handlers;
using TelegramService.Configurations;
using TelegramService.Interfaces;

namespace Catalog.ApprovalService.Application.QueueHandlers.ModuleQueueHandlers
{
    public sealed class ModuleChangedEventHandler : IHandleMessages<ModuleChangedEvent>
    {        
        private readonly ModuleApprovalWorkflowRestartProcessor _processor;
        private readonly IBus _bus;
        private readonly ITelegramService _telegramService;

        public ModuleChangedEventHandler(ITelegramService telegramService, IBus bus, ModuleApprovalWorkflowRestartProcessor processor, IOptions<TelegramBotConfiguration> telegramBotConfiguration)
        {
            _processor = processor;
            _bus = bus;
            _telegramService = telegramService;
            _telegramService.Initialize(token: telegramBotConfiguration.Value.Token, chatId: telegramBotConfiguration.Value.ChatId);
        }

        public async Task Handle(ModuleChangedEvent message)
        {
            var result = await _processor.ProcessAsync(message.ModuleId, new CancellationToken());

            if(!result.Ok)
                await _telegramService.SendMessageAsync(result.Error);

            if(result.Result.IsCustom)
                foreach(var item in result.Result.OrderItems)
                    await _bus.Publish(new CustomWorkflowChangedEvent(item.ApprovalWorkflow.Id));

            return;
        }
    }
}
