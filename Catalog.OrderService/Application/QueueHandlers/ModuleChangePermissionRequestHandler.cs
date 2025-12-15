using Calabonga.UnitOfWork;
using Catalog.Contracts.Interfaces;
using Catalog.Contracts.Request;
using Catalog.Contracts.Response;
using Catalog.Domain.Entities;
using Rebus.Bus;
using Rebus.Handlers;

namespace Catalog.OrderService.Application.QueueHandlers
{
    public class ModuleChangePermissionRequestHandler : IHandleMessages<ModuleChangePermissionRequest>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrderValidator _orderValidator;
        private readonly IOrderValidator _validator;
        private readonly IBus _bus;

        public ModuleChangePermissionRequestHandler(IUnitOfWork unitOfWork, IOrderValidator orderValidator, IOrderValidator validator, IBus bus)
        {
            _unitOfWork = unitOfWork;
            _orderValidator = orderValidator;
            _validator = validator;
            _bus = bus;
        }

        public async Task Handle(ModuleChangePermissionRequest message)
        {
            var module = message.ModuleDto;

            if (module is null)
            {
                await _bus.Reply(new ModuleChangePermissionResponse(null, false, "Module not found!"));
                return;
            }

            var orders = await _unitOfWork.GetRepository<Order>()
                .GetAllAsync(
                    predicate: x => x.OrderItems.Any(item => item.Module.Code == module.ModuleCode),
                    trackingType: TrackingType.NoTracking,
                    include: Order.IncludeRequiredField()
                );

            if (orders.Any())
                foreach (var order in orders)
                {
                    var orderDto = order.ConvertToDto();

                    var modifiedModule = order.OrderItems.FirstOrDefault(x => x.Module.Code == message.ModuleDto.ModuleCode);

                    if (modifiedModule is not null)
                        orderDto.Modules.First(x => x.Module.ModuleCode == modifiedModule.Module.Code).Module = message.ModuleDto;
                    else 
                        continue;

                    var validationResult = _validator.Validate(orderDto);

                    if (!validationResult.Ok)
                    {
                        await _bus.Reply(new ModuleChangePermissionResponse(module.ModuleCode, false, validationResult.Error));
                        return;
                    }
                }

            await _bus.Reply(new ModuleChangePermissionResponse(module.ModuleCode, true));
        }
    }
}
