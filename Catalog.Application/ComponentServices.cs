using Calabonga.UnitOfWork;
using Catalog.Domain.Entities;
using Catalog.Domain.Enum;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Application
{
    public class ComponentServices
    {
        private readonly IUnitOfWork _unitOfWork;

        public ComponentServices(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Component>> GetAllAsync(CancellationToken cancellationToken)
        {
            var moduleType = ModuleType.Create("Фасад", "00000000FSD").Result;

            var module = Module.Create("Фасад Матовый Вишня Оксфорд", Guid.NewGuid().ToString(), moduleType).Result;

            await _unitOfWork.GetRepository<Module>().InsertAsync(module, cancellationToken);

            var insertResult = await _unitOfWork.SaveChangesAsync();

            var componentType = ComponentType.Create("Пленка", "0000000PLNK").Result;
            
            var component = Component.Create("Вишня Оксфорд 2K073-05", "00080196743", componentType).Result;

            module.AddComponent(component);

            insertResult = await _unitOfWork.SaveChangesAsync();

            var parameterType = ParameterType.Create("Tone", "000000000TN", ParameterValueType.Text).Result;

            var textParameter = TextParameter.Create("Матовый", parameterType).Result;

            await _unitOfWork.GetRepository<TextParameter>().InsertAsync(textParameter, cancellationToken);

            component.AddTextParameter(textParameter);

            insertResult = await _unitOfWork.SaveChangesAsync();

            parameterType = ParameterType.Create("Width", "0000000WDTH", ParameterValueType.Numeric).Result;

            var numericParameter = NumericParameter.Create(150, parameterType).Result;

            await _unitOfWork.GetRepository<NumericParameter>().InsertAsync(numericParameter, cancellationToken);

            component.AddNumericParameter(numericParameter);

            insertResult = await _unitOfWork.SaveChangesAsync();

            parameterType = ParameterType.Create("Milling", "00000000MTR", ParameterValueType.Text).Result;
            
            textParameter = TextParameter.Create("МДФ", parameterType).Result;

            await _unitOfWork.GetRepository<TextParameter>().InsertAsync(textParameter, cancellationToken);

            component.AddTextParameter(textParameter);

            insertResult = await _unitOfWork.SaveChangesAsync();

            if (_unitOfWork.Result.Exception is not null)
                throw new Exception(_unitOfWork.Result.Exception.Message);
            
            var result = await _unitOfWork.GetRepository<Component>()
                .GetAllAsync(
                    trackingType: TrackingType.NoTracking, 
                    include: query => 
                        query
                        .Include(x => x.TextParameters)
                        .Include(x => x.NumericParameters)
                        .Include(x => x.ComponentType));


            return [.. result];
        } 
    }
}
