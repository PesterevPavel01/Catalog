using Calabonga.UnitOfWork;
using Catalog.Contracts.Commands;
using Catalog.Domain.Entities;
using Microsoft.Extensions.Options;
using Rebus.Handlers;
using TelegramService.Configurations;
using TelegramService.Interfaces;

namespace Catalog.OrderService.Application.QueueHandlers.OrderEventHandlers
{
    public class UpdateOrderCodeCommandHandler : IHandleMessages<UpdateOrderCodeCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITelegramService _telegramService;

        public UpdateOrderCodeCommandHandler(IUnitOfWork unitOfWork, ITelegramService telegramService,
            IOptions<TelegramBotConfiguration> exceptionNotificationBot)
        {
            _unitOfWork = unitOfWork;
            _telegramService = telegramService;
            _telegramService.Initialize(token: exceptionNotificationBot.Value.Token, chatId: exceptionNotificationBot.Value.ChatId);
        }

        public async Task Handle(UpdateOrderCodeCommand message)
        {
            var order = await _unitOfWork
                .GetRepository<Order>()
                .GetFirstOrDefaultAsync(
                    trackingType: TrackingType.Tracking,
                    predicate: x => x.Code == message.Code);

            if(order is null)
                await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Error: Order not found!");

            order.UpdateCode(message.NewCode);

            var result = await _unitOfWork.SaveChangesAsync();

            if (_unitOfWork.Result.Exception is not null)
            {
                await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Event {message.GetType().Name}. {_unitOfWork.Result.Exception.Message} OrderTitle: {order.Title}");
            }
        }
    }
}
