using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Message;
using Catalog.Contracts.Entities;

namespace Catalog.OrderService.Application.Handlers.QueryHandlers
{
    public sealed class OrderMessagesQueryHandler
    {

        private readonly IUnitOfWork _unitOfWork;

        public OrderMessagesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<Operation<List<MessageDto>, string>> HandleAsync(string orderCode, CancellationToken cancellationToken = default)
        {
            var messages = await _unitOfWork
                .GetRepository<Message>()
                .GetAllAsync(
                    predicate: x => x.OrderItem.Order.Code == orderCode,
                    include: Message.IncludeRequiredField(),
                    trackingType: TrackingType.NoTracking);

            if(!messages.Any())
                return new List<MessageDto>();

            return messages.Select(x => x.ConvertToDto()).OrderBy(x => x.CreatedAt).ToList();

        }
    }
}
