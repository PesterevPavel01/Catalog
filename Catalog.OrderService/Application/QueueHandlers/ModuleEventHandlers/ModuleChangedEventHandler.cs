using Calabonga.UnitOfWork;
using Catalog.Contracts.Events;
using Catalog.Contracts.Events.OrderEvents;
using Catalog.Domain.Entities;
using Microsoft.Extensions.Options;
using Rebus.Bus;
using Rebus.Handlers;
using TelegramService.Configurations;
using TelegramService.Interfaces;

namespace Catalog.OrderService.Application.QueueHandlers.ModuleEventHandlers
{
    public class ModuleChangedEventHandler : IHandleMessages<ModuleChangedEvent>
    {
        private readonly IBus _bus;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITelegramService _telegramService;

        public ModuleChangedEventHandler(IUnitOfWork unitOfWork, IBus bus,
            ITelegramService telegramService, IOptions<TelegramBotConfiguration> exceptionNotificationBot)
        {
            _unitOfWork = unitOfWork;
            _bus = bus;
            _telegramService = telegramService;
            _telegramService.Initialize(token: exceptionNotificationBot.Value.Token, chatId: exceptionNotificationBot.Value.ChatId);
        }

        public async Task Handle(ModuleChangedEvent message)
        {
            var orderRepository = _unitOfWork.GetRepository<Order>();
            var orders = await orderRepository
                .GetAllAsync(
                    predicate: x => x.OrderItems.Any(item => item.Module.Id == message.ModuleId),
                    trackingType: TrackingType.Tracking
                );

            if (!orders.Any())
            {
                await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Event {message.GetType().Name}. Order not found!");
                return;
            }

            foreach (var order in orders) {

                var orderItem = order.OrderItems.FirstOrDefault(x => x.ModuleId == message.ModuleId);

                if (orderItem is null)
                {
                    await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Event {message.GetType().Name}. Module not found!");
                    return;
                }

                if (orderItem.Module.IsCustom)
                { 
                    var changeResult = order.CustomModuleChange();

                    if (!changeResult.Ok)
                    {
                        await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Event {message.GetType().Name}. Error: {changeResult}. OrderCode: {order.Code}");
                        continue;
                    }

                    orderRepository.Update(order);

                    var result = await _unitOfWork.SaveChangesAsync();

                    if (_unitOfWork.Result.Exception is not null)
                    {
                        await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Event {message.GetType().Name}. Error: {_unitOfWork.Result.Exception.Message}. OrderCode: {order.Code}");
                        continue;
                    }

                    await _bus.Publish(new UpdateOrderCacheCommand(order.Code));
                }
            }

            return;
        }
    }
}
