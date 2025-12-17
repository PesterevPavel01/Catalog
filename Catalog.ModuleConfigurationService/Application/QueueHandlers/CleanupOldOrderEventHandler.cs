using Calabonga.UnitOfWork;
using Catalog.Contracts.Events.OrderEvents;
using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Rebus.Handlers;
using TelegramService.Configurations;
using TelegramService.Interfaces;

namespace Catalog.ModuleConfigurationService.Application.QueueHandlers
{
    public class CleanupOldOrderEventHandler : IHandleMessages<CleanupOldOrderEvent>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITelegramService _telegramService;

        public CleanupOldOrderEventHandler(ITelegramService telegramService, IUnitOfWork unitOfWork, IOptions<TelegramBotConfiguration> telegramBotConfiguration)
        {
            _unitOfWork = unitOfWork;
            _telegramService = telegramService;
            _telegramService.Initialize(token: telegramBotConfiguration.Value.Token, chatId: telegramBotConfiguration.Value.ChatId);
        }

        public async Task Handle(CleanupOldOrderEvent message)
        {
            var modules = await _unitOfWork.GetRepository<Module>()
                .GetAllAsync(
                    predicate: x =>
                        (!x.OrderItems.Any() || !x.OrderItems.Any(item => item.ApprovalWorkflow != null))
                        && x.CreatedAt < DateTime.Now.AddDays(message.ArchiveStorageDays * -1),
                    include: query => query.Include(x => x.Components),
                    trackingType: TrackingType.Tracking
                );

            if (!modules.Any())
                return;

            _unitOfWork.GetRepository<Module>().Delete(modules);

            var result = await _unitOfWork.SaveChangesAsync();

            if (_unitOfWork.Result.Exception is not null)
            {
                await _telegramService.SendMessageAsync(_unitOfWork.Result.Exception.Message);
            }
        }
    }
}