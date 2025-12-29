using Calabonga.UnitOfWork;
using Catalog.Contracts.Commands;
using Catalog.Contracts.Dto.Events;
using Catalog.Contracts.Entities;
using Catalog.Domain.Entities;
using Catalog.OrderService.Application.Commands;
using Catalog.OrderService.Application.Handlers.QueryHandlers;
using Catalog.Redis;
using Microsoft.EntityFrameworkCore;
using Rebus.Bus;
using Rebus.Handlers;
using TelegramService.Interfaces;

namespace Catalog.OrderService.Application.QueueHandlers
{
    public class CreateOrderEventCommandHandler : IHandleMessages<CreateOrderEventCommand>
    {
        private readonly ConstructorOrderEventQueriesHandler _constructorCommandHandler;
        private readonly ApplicationUserOrderEventQueriesHandler _customerCommandHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBus _bus;
        private readonly RedisService<OrderEventDto> _redisService;

        public CreateOrderEventCommandHandler(ConstructorOrderEventQueriesHandler constructorCommandHandler, ApplicationUserOrderEventQueriesHandler customerCommandHandler,
            RedisServiceFactory redisServiceFactory, ITelegramService telegramService, IUnitOfWork unitOfWork, IBus bus)
        {
            _constructorCommandHandler = constructorCommandHandler;
            _customerCommandHandler = customerCommandHandler;
            _unitOfWork = unitOfWork;
            _bus = bus;
            _redisService = redisServiceFactory.GetService<OrderEventDto>();
        }

        public async Task Handle(CreateOrderEventCommand message)
        {
            var order = await _unitOfWork
                .GetRepository<Order>()
                .GetFirstOrDefaultAsync(
                    trackingType: TrackingType.Tracking,
                    include: query => query
                        .Include(x => x.ApplicationUser)
                        .ThenInclude(x=> x.Roles),
                    predicate: x => x.Code == message.OrderCode);

            if (order is null)
                throw new ArgumentException($"{"OrderService".ToUpper()} Event {message.GetType().Name}. Order not found! Code: {message.OrderCode}");

            var orderEvent = OrderEvent.Create(order, message.Note, null);

            if (!orderEvent.Ok)
                throw new ArgumentException($"{"OrderService".ToUpper()} Event {message.GetType().Name}. {orderEvent.Error} OrderTitle: {order.Title}");

            await _unitOfWork.GetRepository<OrderEvent>().InsertAsync(orderEvent.Result);

            var result = await _unitOfWork.SaveChangesAsync();

            if (_unitOfWork.Result.Exception is not null)
            {
                throw new ArgumentException($"{"OrderService".ToUpper()} Event {message.GetType().Name}. {_unitOfWork.Result.Exception.Message} OrderTitle: {order.Title}");
            }

            var queryResult = await _constructorCommandHandler.HandleAsync(default);

            if (!queryResult.Ok)
                throw new ArgumentException(queryResult.Error);

            await _bus.Send(new SetOrderEventsInCacheCommand(_redisService.GenerateCacheKey(("type", "constructor")), queryResult.Result.Items));

            if (order.ApplicationUser.Roles.Any(x => x.Code == "customer")) {

                queryResult = await _customerCommandHandler.HandleAsync(order.ApplicationUser.UserName);
                    if (!queryResult.Ok)
                        throw new ArgumentException(queryResult.Error);

                await _bus.Send(new SetOrderEventsInCacheCommand(_redisService.GenerateCacheKey(("type", order.ApplicationUser.UserName)), queryResult.Result.Items));
            }
        }
    }
}