using Catalog.ApprovalService.Application.Processors;
using Catalog.Contracts.Events;
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
        private readonly ITelegramService _telegramService;

        public ModuleChangedEventHandler(ITelegramService telegramService, IBus bus, ModuleApprovalWorkflowRestartProcessor processor, IOptions<TelegramBotConfiguration> telegramBotConfiguration)
        {
            _processor = processor;
            _telegramService = telegramService;
            _telegramService.Initialize(token: telegramBotConfiguration.Value.Token, chatId: telegramBotConfiguration.Value.ChatId);
        }

        public async Task Handle(ModuleChangedEvent message)
        {
            var result = await _processor.ProcessAsync(message.ModuleId, new CancellationToken());

            if (!result.Ok && result.Error != "Information: Active ApprovalWorkflows not found!")
            {
                await _telegramService.SendMessageAsync(result.Error);
                return;
            }

            return;
        }
    }
}
