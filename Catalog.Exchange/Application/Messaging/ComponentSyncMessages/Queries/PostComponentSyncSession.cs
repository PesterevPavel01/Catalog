using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Commands.Exchange;
using Catalog.Contracts.Dto.Components;
using Catalog.Contracts.Entities.Exchange;
using Catalog.Contracts.Enum;
using Catalog.Domain.Entities;
using MediatR;
using Rebus.Bus;

namespace Catalog.ExchangeService.Application.Messaging.ComponentMessages.Queries
{
    public sealed class PostComponentSyncSession
    {
        public record Request(IEnumerable<ComponentDto> Models) : IRequest<Operation<string, string>>;

        public class Handler(IUnitOfWork unitOfWork, IBus bus)
                : IRequestHandler<Request, Operation<string, string>>
        {
            public async Task<Operation<string, string>> Handle(Request componentRequest, CancellationToken cancellationToken)
            {
                if (componentRequest.Models is null || !componentRequest.Models.Any())
                    return Operation.Error("Components not found!");

                var exchangeEventRepository = unitOfWork.GetRepository<ExchangeEvent>();

                var currentExchangeEvent = ExchangeEvent.Create(ExchangeEventType.ImportComponent.ToRussianString(), DateTime.Now);

                if (!currentExchangeEvent.Ok)
                    return Operation.Error(currentExchangeEvent.Error);
                
                var importedComponent = componentRequest.Models.Select(x => ExportedEntity.Create(currentExchangeEvent.Result, typeof(Component).Name, x.ComponentCode));

                await unitOfWork.GetRepository<ExportedEntity>().InsertAsync(importedComponent, cancellationToken);
                
                await exchangeEventRepository.InsertAsync(currentExchangeEvent.Result, cancellationToken);

                await unitOfWork.SaveChangesAsync();

                if (unitOfWork.Result.Exception is not null)
                {
                    return Operation.Error(unitOfWork.Result.Exception.Message);
                }

                await bus.Publish(new ComponentSyncCommand(componentRequest.Models, currentExchangeEvent.Result.Code));

                return currentExchangeEvent.Result.Code;

            }
        }
    }
}
