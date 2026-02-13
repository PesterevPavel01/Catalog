using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Exchange;
using Catalog.Contracts.Entities.Exchange;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Catalog.ExchangeService.Application.Messaging.ComponentSyncMessages.Queries
{
    public sealed class GetComponentSyncSessionByCode
    { 
        public record Request(string SessionCode) : IRequest<Operation<SyncConfirmationDto, string>>;

        public class Handler(IUnitOfWork unitOfWork): IRequestHandler<Request, Operation<SyncConfirmationDto, string>>
        {
            public async Task<Operation<SyncConfirmationDto, string>> Handle(Request componentRequest, CancellationToken cancellationToken)
            {
                if (string.IsNullOrWhiteSpace(componentRequest.SessionCode))
                    return Operation.Error("Session Code not found!");

                var exchangeEventRepository = unitOfWork.GetRepository<ExchangeEvent>();

                var currentExchangeEvent = await exchangeEventRepository
                    .GetFirstOrDefaultAsync(
                        predicate: x => x.Code == componentRequest.SessionCode,
                        include: query => query.Include(x=> x.Entities));

                if (currentExchangeEvent is null)
                    return Operation.Error("Exchange event not found!");

                return currentExchangeEvent.ConvertToDto();
            }
        }
    }
}
