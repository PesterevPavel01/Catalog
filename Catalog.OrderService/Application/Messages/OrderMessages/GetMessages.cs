using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Message;
using Catalog.Contracts.Entities;
using MediatR;

namespace Catalog.OrderService.Application.Messages.OrderMessages
{
    public sealed class GetMessages
    {
        public record Request(string OrderCode) : IRequest<Operation<List<MessageDto>, string>>;

        public class Handler(IUnitOfWork unitOfWork)
            : IRequestHandler<Request, Operation<List<MessageDto>, string>>
        {
            /// <summary>Handles a request</summary>
            /// <param name="request">The request</param>
            /// <param name="cancellationToken">Cancellation token</param>
            /// <returns>Response from the request</returns>
            public async Task<Operation<List<MessageDto>, string>> Handle(Request request, CancellationToken cancellationToken)
            {
                var messages = await unitOfWork
                    .GetRepository<Message>()
                    .GetAllAsync(
                        predicate: x => x.OrderItem.Order.Code == request.OrderCode,
                        include: Message.IncludeRequiredField(),
                        trackingType: TrackingType.NoTracking);

                if (!messages.Any())
                    return new List<MessageDto>();

                return messages.Select(x => x.ConvertToDto()).OrderBy(x => x.CreatedAt).ToList();

            }
        }
    }
}
