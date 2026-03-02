using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Entities;
using Catalog.Contracts.Enum;
using Catalog.Contracts.Events.ApprovalEvents;
using Catalog.Contracts.Events.OrderEvents;
using Catalog.Contracts.Resources;
using Catalog.Domain.Entities;
using Microsoft.Extensions.Options;
using Rebus.Bus;
using Rebus.Handlers;
using System.Threading;
using TelegramService.Configurations;
using TelegramService.Interfaces;

namespace Catalog.OrderService.Application.QueueHandlers.ApprovalEventHandlers
{
    public class OrderApprovalWorkflowsRemoveEventHandler : IHandleMessages<OrderApprovalWorkflowsRemoveEvent>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBus _bus;
        private readonly ITelegramService _telegramService;

        public OrderApprovalWorkflowsRemoveEventHandler(IBus bus, IUnitOfWork unitOfWork,
            ITelegramService telegramService,
            IOptions<TelegramBotConfiguration> exceptionNotificationBot)
        {
            _bus = bus;
            _unitOfWork = unitOfWork;
            _telegramService = telegramService;
            _telegramService.Initialize(token: exceptionNotificationBot.Value.Token, chatId: exceptionNotificationBot.Value.ChatId);
        }

        public async Task Handle(OrderApprovalWorkflowsRemoveEvent message)
        {
            var orderRepository = _unitOfWork.GetRepository<Order>();

            var order = await orderRepository
                .GetFirstOrDefaultAsync(
                    predicate: x => x.Id == message.OrderId,
                    trackingType: TrackingType.Tracking,
                    include: Order.IncludeRequiredField()
                );

            if (order is null)
            {
                await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Event {message.GetType().Name}. order not found! Id: {message.OrderId}");
                return;
            }

            var cancelledResult = order.Cancel();

            if (!cancelledResult.Ok)
            {
                await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Event {message.GetType().Name}. {cancelledResult.Error}! Id: {message.OrderId}");
                return;
            }

            orderRepository.Update(order);

            var result = await _unitOfWork.SaveChangesAsync();

            result = await _unitOfWork.SaveChangesAsync();

            if (_unitOfWork.Result.Exception is not null)
            {
                await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Event {message.GetType().Name}. {_unitOfWork.Result.Exception.Message}! Id: {message.OrderId}");
                return;
            }

            await _bus.Publish(new UpdateOrderCacheCommand(order.Code));

            return;
        }
    }
}
