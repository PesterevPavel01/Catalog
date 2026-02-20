using Calabonga.UnitOfWork;
using Catalog.Contracts.Commands;
using Catalog.Contracts.Entities.Approval;
using Catalog.Contracts.Enum;
using Catalog.Contracts.Events.ApprovalEvents;
using Catalog.Contracts.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Rebus.Bus;
using Rebus.Handlers;
using TelegramService.Configurations;
using TelegramService.Interfaces;

namespace Catalog.OrderService.Application.QueueHandlers.ApprovalEventHandlers
{
    public class CustomWorkflowChangedEventHandler : IHandleMessages<CustomWorkflowChangedEvent>
    {
        private readonly IBus _bus;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITelegramService _telegramService;

        public CustomWorkflowChangedEventHandler(IBus bus, IUnitOfWork unitOfWork, ITelegramService telegramService,
            IOptions<TelegramBotConfiguration> exceptionNotificationBot)
        {
            _bus = bus;
            _unitOfWork = unitOfWork;
            _telegramService = telegramService;
            _telegramService.Initialize(token: exceptionNotificationBot.Value.Token, chatId: exceptionNotificationBot.Value.ChatId);
        }

        public async Task Handle(CustomWorkflowChangedEvent message)
        {
            var workflow = await _unitOfWork
                .GetRepository<ApprovalWorkflow>()
                .GetFirstOrDefaultAsync(
                    trackingType: TrackingType.NoTracking,
                    include: query => query
                        .Include(x => x.OrderItem)
                            .ThenInclude(x => x.Order),
                    predicate: x => x.Id == message.WorkflowId);

            if (workflow is null) 
            {
                await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Event {message.GetType().Name}. workflow not found! ID: {message.WorkflowId}");
                return;
            }

            await _bus.Publish(new CreateOrderEventCommand(workflow.OrderItem.Order.Code, OrderEventType.CustomModuleModified, OrderEventTypeTitles.CustomModuleModified));

            return;
        }
    }
}